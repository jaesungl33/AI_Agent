"""
Requirement → code coverage matching via lightweight RAG.

This module evaluates how well codebase implementations match GDD requirements
by using semantic search to find relevant code chunks and LLM-based classification
to determine implementation status (implemented, partially_implemented, not_implemented).
"""

from __future__ import annotations

# Standard library imports
import asyncio
import json
import logging
from pathlib import Path
from typing import Any, Dict, Iterable, List, Optional, Sequence

logger = logging.getLogger(__name__)

# Project imports
from gdd_rag_backbone.gdd.schemas import GddRequirement, BehaviorRequirement, CodeBehavior
from gdd_rag_backbone.gdd.extraction import convert_to_behavior_requirement
from gdd_rag_backbone.gdd.behavior_indexing import index_code_behaviors, load_behavior_index, save_behavior_index
from gdd_rag_backbone.gdd.behavior_matching import find_matching_behaviors, batch_find_matching_behaviors
from gdd_rag_backbone.llm_providers import QwenProvider, make_llm_model_func, make_embedding_func
from gdd_rag_backbone.rag_backend.chunk_qa import get_top_chunks, load_doc_chunks

DEFAULT_REPORT_DIR = Path("reports/coverage_checks")

# In-memory cache for semantic results within a process.
# Keyed by (requirement_id, code_index_key, top_k, retrieval_version)
_SEMANTIC_CACHE: Dict[str, Dict[str, Any]] = {}


def generate_code_queries(requirement: GddRequirement) -> List[str]:
    # Prefer semantic description/summary + triggers/effects over names
    queries: List[str] = []
    for field in (
        getattr(requirement, "summary", None),
        requirement.description,
        requirement.acceptance_criteria,
        requirement.title,
    ):
        if field:
            queries.append(field)
    # Add triggers/effects as separate signals
    for trig in getattr(requirement, "triggers", []) or []:
        if trig:
            queries.append(trig)
    for eff in getattr(requirement, "effects", []) or []:
        if eff:
            queries.append(eff)
    if requirement.related_systems:
        queries.append("; ".join(requirement.related_systems))
    return [q for q in queries if q]


async def search_code_chunks(
    queries: Sequence[str],
    code_index_id: str | Sequence[str],  # Support single or multiple code indices
    *,
    provider: Optional[QwenProvider] = None,
    top_k: int = 8,
) -> List[Dict[str, Any]]:
    if not queries:
        return []

    active_provider = provider or QwenProvider()
    
    # Normalize to list of code indices
    if isinstance(code_index_id, str):
        code_indices = [code_index_id]
    else:
        code_indices = list(code_index_id)

    async def _run_query(query: str) -> List[Dict[str, Any]]:
        def _load():
            # Search across all code indices
            return get_top_chunks(code_indices, query, provider=active_provider, top_k=top_k)

        return await asyncio.to_thread(_load)

    # Run all queries in parallel instead of sequentially
    query_tasks = [_run_query(query) for query in queries]
    all_query_results = await asyncio.gather(*query_tasks)
    
    # Merge results, keeping best score per chunk
    seen: Dict[str, Dict[str, Any]] = {}
    for query, chunks in zip(queries, all_query_results):
        for chunk in chunks:
            chunk_id = chunk["chunk_id"]
            existing = seen.get(chunk_id)
            if existing is None or chunk.get("score", 0) > existing.get("score", 0):
                seen[chunk_id] = {**chunk, "query": query}
    return sorted(seen.values(), key=lambda item: item.get("score", 0), reverse=True)


async def classify_requirement_coverage(
    requirement: GddRequirement,
    code_chunks: Sequence[Dict[str, Any]],
    llm_func,
) -> Dict[str, Any]:
    system_prompt = (
        "You are a senior gameplay engineer. Evaluate whether the provided code implements the requirement. "
        "Do NOT guess. If there is insufficient evidence, respond with 'not_implemented'."
    )

    requirement_payload = json.dumps(requirement.to_dict(), indent=2)
    if code_chunks:
        snippet_lines = []
        for idx, chunk in enumerate(code_chunks[:8]):
            snippet = chunk.get("content", "")
            snippet_lines.append(
                f"[Chunk {idx + 1}] id={chunk.get('chunk_id')} score={chunk.get('score', 0):.3f}\n{snippet[:1200]}"
            )
        code_context = "\n\n".join(snippet_lines)
    else:
        code_context = "No relevant code chunks were retrieved."

    user_prompt = f"""
Requirement:
{requirement_payload}

Candidate Code:
{code_context}

Classify the implementation status. Possible statuses: "implemented", "partially_implemented", "not_implemented".
Provide specific evidence (file path and short reason) when available.

Return ONLY JSON:
{{
  "requirement_id": "{requirement.id}",
  "status": "implemented/partially_implemented/not_implemented",
  "evidence": [
    {{
      "file": "path/to/file.ext",
      "reason": "How this code satisfies or fails the requirement"
    }}
  ]
}}
"""

    response = await llm_func(prompt=user_prompt, system_prompt=system_prompt, temperature=0.1)
    text = response.strip()
    if text.startswith("```"):
        lines = text.split("\n")
        text = "\n".join(lines[1:-1]) if lines[-1].strip() == "```" else "\n".join(lines[1:])

    try:
        payload = json.loads(text)
    except json.JSONDecodeError:
        return {
            "requirement_id": requirement.id,
            "status": "error",
            "evidence": [
                {
                    "file": None,
                    "reason": "LLM response could not be parsed",
                }
            ],
        }
    payload.setdefault("requirement_id", requirement.id)
    payload.setdefault("evidence", [])
    return payload


def fast_symbol_coverage(requirement: GddRequirement, symbol_index: Dict[str, list]) -> Dict[str, Any]:
    """
    Cheap O(1) symbol lookup. Does NOT call LLM.
    requirement may optionally include:
      - expected_symbol (Class.Method)
      - or expected_class + expected_method
    """
    expected_symbol = getattr(requirement, "expected_symbol", None)
    expected_class = getattr(requirement, "expected_class", None)
    expected_method = getattr(requirement, "expected_method", None)

    used_symbol = None
    if not symbol_index:
        return {
            "requirement_id": requirement.id,
            "status": "unknown",
            "coverage_type": "fast",
            "used_symbol": used_symbol,
            "matches": [],
        }

    candidates = []
    if expected_symbol:
        candidates.append(expected_symbol)
    if expected_class and expected_method:
        candidates.append(f"{expected_class}.{expected_method}")
    anchors = getattr(requirement, "expected_code_anchors", []) or []
    candidates.extend(anchors)

    for symbol in candidates:
        locations = symbol_index.get(symbol, [])
        if locations:
            return {
                "requirement_id": requirement.id,
                "status": "implemented",
                "coverage_type": "fast",
                "used_symbol": symbol,
                "matches": locations,
            }

    return {
        "requirement_id": requirement.id,
        "status": "not_implemented",
        "coverage_type": "fast",
        "used_symbol": None,
        "matches": [],
    }


def semantic_retrieve_candidates(
    requirement: GddRequirement,
    code_index_id: str | Sequence[str],
    provider: QwenProvider,
    top_k: int = 12,
) -> List[Dict[str, Any]]:
    """
    Retrieve candidate code chunks via vector search using requirement summary/description.
    """
    query = requirement.description or requirement.title
    if not query:
        return []
    try:
        return get_top_chunks(
            [code_index_id] if isinstance(code_index_id, str) else code_index_id,
            query,
            provider=provider,
            top_k=top_k,
        )
    except Exception:
        return []


async def llm_semantic_judgement(requirement: GddRequirement, candidate: Dict[str, Any], llm_model, timeout: float = 25.0) -> Dict[str, Any]:
    """
    Use LLM to classify how well a code chunk implements the requirement.
    Reduced timeout from 30s to 25s to prevent hanging.
    """
    system_prompt = (
        "You are a senior gameplay engineer. "
        "Determine whether this code implements the described requirement. "
        "Classify as 'implemented', 'partially_implemented', or 'not_related'. "
        "Keep reasoning to one short sentence."
    )
    # Truncate code content to prevent overly long prompts
    code_content = candidate.get('content', '')
    if len(code_content) > 2000:
        code_content = code_content[:2000] + "... [truncated]"
    
    user_prompt = f"""
Requirement:
{requirement.description or requirement.title}

Code:
{code_content}

Respond ONLY JSON:
{{
  "classification": "implemented|partially_implemented|not_related",
  "reason": "one sentence"
}}
"""
    try:
        resp_text = await asyncio.wait_for(
            llm_model(prompt=user_prompt, system_prompt=system_prompt, temperature=0.1),
            timeout=timeout,
        )
    except asyncio.TimeoutError:
        return {
            "candidate": candidate,
            "classification": "not_related",
            "reason": "LLM timeout (exceeded 25s)",
        }
    except Exception as e:
        # Catch any other LLM errors and continue
        return {
            "candidate": candidate,
            "classification": "not_related",
            "reason": f"LLM error: {str(e)[:50]}",
        }
    text = resp_text.strip()
    if text.startswith("```"):
        lines = text.split("\n")
        text = "\n".join(lines[1:-1]) if lines[-1].strip() == "```" else "\n".join(lines[1:])
    try:
        payload = json.loads(text)
    except json.JSONDecodeError:
        payload = {"classification": "not_related", "reason": "Could not parse LLM response"}
    return {
        "candidate": candidate,
        "classification": payload.get("classification", "not_related"),
        "reason": payload.get("reason", "No reason provided"),
    }


async def semantic_coverage(
    requirement: GddRequirement,
    code_index_id: str | Sequence[str],
    provider: QwenProvider,
    llm_model,
    top_k: int = 12,
) -> Dict[str, Any]:
    # Cache key
    code_key = code_index_id if isinstance(code_index_id, str) else "_".join(code_index_id)
    cache_key = f"{requirement.id}::{code_key}::top{top_k}"
    cached = _SEMANTIC_CACHE.get(cache_key)
    if cached:
        return cached

    candidates = semantic_retrieve_candidates(requirement, code_index_id, provider, top_k=top_k)
    if not candidates:
        result = {
            "requirement_id": requirement.id,
            "status": "not_implemented",
            "coverage_type": "semantic",
            "best_match": None,
            "reason": "No candidates retrieved",
            "retrieved_chunks": [],
        }
        _SEMANTIC_CACHE[cache_key] = result
        return result

    # Evaluate candidates sequentially with early exit optimization
    # If we find an "implemented" match, we can stop early
    judgements: List[Dict[str, Any]] = []
    for idx, cand in enumerate(candidates):
        try:
            judgement = await llm_semantic_judgement(requirement, cand, llm_model)
            judgements.append(judgement)
            
            # Early exit: if we found a clear "implemented" match, stop evaluating
            if judgement.get("classification") == "implemented":
                logger.info(f"[Semantic] Found implemented match for requirement {requirement.id} at candidate {idx+1}/{len(candidates)}")
                break
        except Exception as e:
            # Log but continue with next candidate
            logger.warning(f"[Semantic] Error evaluating candidate {idx+1} for requirement {requirement.id}: {e}")
            judgements.append({
                "candidate": cand,
                "classification": "not_related",
                "reason": f"Evaluation error: {str(e)[:50]}",
            })

    status = "not_implemented"
    best_match = None
    for j in judgements:
        if j["classification"] == "implemented":
            status = "implemented"
            best_match = j
            break
        if j["classification"] == "partially_implemented" and status != "implemented":
            status = "partially_implemented"
            best_match = j

    result = {
        "requirement_id": requirement.id,
        "status": status,
        "coverage_type": "semantic",
        "best_match": best_match,
        "retrieved_chunks": candidates,
        "reason": best_match["reason"] if best_match else "No matching candidates",
    }
    _SEMANTIC_CACHE[cache_key] = result
    return result


def build_symbol_index(code_index_id: str | Sequence[str]) -> Dict[str, list]:
    """
    Build a lightweight symbol index from code chunks:
    - Detect 'class X' lines and subsequent 'def y' lines -> 'X.y'
    - Detect standalone 'def func' -> 'func'
    """
    if isinstance(code_index_id, str):
        code_ids = [code_index_id]
    else:
        code_ids = list(code_index_id)

    index: Dict[str, list] = {}

    for code_id in code_ids:
        for chunk in load_doc_chunks(code_id):
            lines = chunk.content.splitlines()
            current_class = None
            for line in lines:
                stripped = line.strip()
                if stripped.startswith("class "):
                    parts = stripped.split()
                    if len(parts) >= 2:
                        name = parts[1].split("(")[0].strip().strip(":")
                        current_class = name or current_class
                if stripped.startswith("def "):
                    fn = stripped[4:].split("(")[0].strip().strip(":")
                    if fn:
                        if current_class:
                            symbol = f"{current_class}.{fn}"
                            index.setdefault(symbol, []).append(
                                {"chunk_id": chunk.chunk_id, "doc_id": chunk.doc_id}
                            )
                        symbol = fn
                        index.setdefault(symbol, []).append(
                            {"chunk_id": chunk.chunk_id, "doc_id": chunk.doc_id}
                        )
    return index


def _gap_analysis(requirement: BehaviorRequirement, code_behavior: CodeBehavior) -> Dict[str, List[str]]:
    """
    Compare requirement triggers/effects against code behavior triggers/effects and
    return missing items. This is lightweight and keeps the gap explicit.
    """
    req_triggers = set((requirement.triggers or []))
    req_effects = set((requirement.effects or []))
    code_triggers = set((code_behavior.trigger_patterns or []))
    code_effects = set((code_behavior.effect_patterns or []))
    missing_triggers = [t for t in req_triggers if t and t not in code_triggers]
    missing_effects = [e for e in req_effects if e and e not in code_effects]
    return {"missing_triggers": missing_triggers, "missing_effects": missing_effects}


async def evaluate_requirement(
    requirement: GddRequirement,
    code_index_id: str | Sequence[str],
    *,
    provider: Optional[QwenProvider] = None,
    llm_func=None,
    embedding_func=None,
    symbol_index: Optional[Dict[str, list]] = None,
    top_k: int = 5,
    code_behaviors: Optional[List[CodeBehavior]] = None,
) -> Dict[str, Any]:
    """
    Behavior-based evaluation pipeline:
    1) Fast symbol check
    2) Behavior → behavior semantic match (embeddings)
    3) LLM verification on top matches
    4) Gap analysis for partials
    """
    active_provider = provider or QwenProvider()
    llm_model = llm_func or make_llm_model_func(active_provider)
    embed_func = embedding_func or make_embedding_func(active_provider)
    symbol_index = symbol_index or {}

    # Step 1: Convert to behavior requirement (cached in extraction layer)
    try:
        behavior_req = await convert_to_behavior_requirement(requirement, llm_func=llm_model)
    except Exception as e:  # pragma: no cover - defensive
        logger.warning(f"[Behavior] Failed to convert requirement {requirement.id}: {e}")
        return {
            "requirement_id": requirement.id,
            "status": "error",
            "coverage_type": "behavior",
            "reason": f"Behavior conversion failed: {str(e)[:120]}",
        }

    # Step 2: Fast symbol check (if anchors exist)
    fast_result = fast_symbol_coverage(requirement, symbol_index)
    if fast_result.get("status") == "implemented":
        return {
            "requirement_id": requirement.id,
            "status": "implemented",
            "coverage_type": "fast",
            "used_symbol": fast_result.get("used_symbol"),
            "matches": fast_result.get("matches", []),
            "behavior_requirement": behavior_req.to_dict(),
        }

    # Step 3: Build/load code behaviors once (cached by behavior_indexing)
    # We rely on behavior_indexing to do one-time method extraction and avoid raw chunk scanning.
    if code_behaviors is None:
        code_behaviors = await index_code_behaviors(
            code_index_id,
            provider=active_provider,
            llm_func=llm_model,
            max_methods=120,  # cap for fast interactive runs
        )
    if not code_behaviors:
        return {
            "requirement_id": requirement.id,
            "status": "not_implemented",
            "coverage_type": "behavior",
            "reason": "No code behaviors available to match",
            "behavior_requirement": behavior_req.to_dict(),
        }

    # Step 4: Behavior-to-behavior semantic match (fast embedding similarity)
    matches = await find_matching_behaviors(
        behavior_req,
        code_behaviors,
        provider=active_provider,
        embedding_func=embed_func,
        top_k=top_k,
    )
    if not matches:
        return {
            "requirement_id": requirement.id,
            "status": "not_implemented",
            "coverage_type": "behavior",
            "reason": "No matching behaviors found",
            "behavior_requirement": behavior_req.to_dict(),
        }

    # Step 5: LLM verification on top candidates only
    classification = await classify_behavior_implementation(behavior_req, matches[:5], llm_model)
    status = classification.get("status", "not_implemented")
    best_match_symbol = None
    best_similarity = None
    if matches:
        best_match_symbol = matches[0][0].symbol
        best_similarity = float(matches[0][1])

    missing = {}
    if status == "partially_implemented" and matches:
        missing = _gap_analysis(behavior_req, matches[0][0])

    return {
        "requirement_id": requirement.id,
        "status": status,
        "coverage_type": "behavior",
        "best_match": best_match_symbol,
        "similarity": best_similarity,
        "llm_reason": classification.get("reason"),
        "matched_symbols": classification.get("matched_symbols", []),
        **missing,
        "behavior_requirement": behavior_req.to_dict(),
        "top_matches": [
            {
                "symbol": cb.symbol,
                "similarity": float(score),
                "description": cb.description,
            }
            for cb, score in matches[:top_k]
        ],
    }


async def evaluate_all_requirements(
    doc_id: str,
    code_index_id: str | Sequence[str],  # Support single or multiple code indices
    requirements: Sequence[GddRequirement],
    *,
    output_dir: Optional[Path] = None,
    provider: Optional[QwenProvider] = None,
    top_k: int = 5,
    symbol_index: Optional[Dict[str, list]] = None,
) -> Path:
    if not requirements:
        raise ValueError("No requirements provided for coverage evaluation.")

    out_dir = output_dir or DEFAULT_REPORT_DIR
    out_dir.mkdir(parents=True, exist_ok=True)
    # Create a safe filename from code_index_id (handle both str and list)
    if isinstance(code_index_id, str):
        code_id_str = code_index_id
    else:
        code_id_str = "_".join(code_index_id[:3])  # Use first 3 indices for filename
    report_path = out_dir / f"{doc_id}_{code_id_str}_coverage.json"

    active_provider = provider or QwenProvider()
    llm = make_llm_model_func(active_provider)
    embed_func = make_embedding_func(active_provider)
    # Build symbol index once if not provided
    symbol_index = symbol_index or build_symbol_index(code_index_id)
    # Build behavior index once for the whole run
    code_behaviors = await index_code_behaviors(
        code_index_id,
        provider=active_provider,
        llm_func=llm,
        max_methods=120,  # cap for faster runs
    )

    results: List[Dict[str, Any]] = []
    total = len(requirements)
    for idx, requirement in enumerate(requirements, 1):
        try:
            logger.info(f"[Coverage] Evaluating requirement {idx}/{total}: {requirement.id} - {requirement.title[:50]}")
            result = await evaluate_requirement(
                requirement,
                code_index_id,
                provider=active_provider,
                llm_func=llm,
                embedding_func=embed_func,
                top_k=top_k,
                symbol_index=symbol_index,
                code_behaviors=code_behaviors,
            )
            results.append(result)
            status = result.get("status", "unknown")
            logger.info(f"[Coverage] Requirement {idx}/{total} completed: {status}")
        except Exception as e:
            logger.error(f"[Coverage] Error evaluating requirement {idx}/{total} ({requirement.id}): {e}", exc_info=True)
            # Add error result so evaluation can continue
            results.append({
                "requirement_id": requirement.id,
                "status": "error",
                "coverage_type": "error",
                "reason": f"Evaluation error: {str(e)[:100]}",
            })

    report_payload = {
        "doc_id": doc_id,
        "code_index_id": code_index_id,
        "results": results,
    }
    report_path.write_text(json.dumps(report_payload, indent=2, ensure_ascii=False))
    return report_path


# ============================================================================
# BEHAVIOR-BASED EVALUATION (QA-Style Approach)
# ============================================================================

BEHAVIOR_INDEX_CACHE_DIR = Path("rag_storage/behavior_indices")


async def classify_behavior_implementation(
    requirement: BehaviorRequirement,
    code_behaviors: Sequence[Tuple[CodeBehavior, float]],
    llm_func,
) -> Dict[str, Any]:
    """
    Step 4: Use LLM to classify if top-matched code behaviors implement the requirement.
    Only called on top 3-5 matches (fast, lightweight).
    """
    system_prompt = (
        "You are a QA engineer comparing a behavior requirement to code behavior descriptions. "
        "Determine if the code behaviors implement the required behavior. "
        "Classify as 'implemented', 'partially_implemented', or 'not_implemented'. "
        "Be strict - only mark as implemented if the behavior fully matches."
    )
    
    # Format requirement
    req_text = requirement.to_behavior_text()
    
    # Format top code behaviors (limit to top 5)
    code_descriptions = []
    for idx, (code_behavior, similarity) in enumerate(code_behaviors[:5], 1):
        code_text = code_behavior.to_behavior_text()
        code_descriptions.append(
            f"[Match {idx}] Similarity: {similarity:.3f}\n{code_text}\n"
            f"Symbol: {code_behavior.symbol}\n"
        )
    
    code_context = "\n\n".join(code_descriptions) if code_descriptions else "No matching code behaviors found."
    
    user_prompt = f"""
Behavior Requirement:
{req_text}

Top Matching Code Behaviors:
{code_context}

Compare the requirement to the code behaviors. Does the code implement the required behavior?

Return ONLY JSON:
{{
  "requirement_id": "{requirement.id}",
  "status": "implemented/partially_implemented/not_implemented",
  "reason": "Brief explanation",
  "matched_symbols": ["symbol1", "symbol2"]
}}
"""
    
    try:
        response = await asyncio.wait_for(
            llm_func(prompt=user_prompt, system_prompt=system_prompt, temperature=0.1),
            timeout=20.0,
        )
    except asyncio.TimeoutError:
        return {
            "requirement_id": requirement.id,
            "status": "error",
            "reason": "LLM timeout",
            "matched_symbols": [],
        }
    
    text = response.strip()
    if text.startswith("```"):
        lines = text.split("\n")
        text = "\n".join(lines[1:-1]) if lines[-1].strip() == "```" else "\n".join(lines[1:])
    
    try:
        payload = json.loads(text)
    except json.JSONDecodeError:
        return {
            "requirement_id": requirement.id,
            "status": "error",
            "reason": "Could not parse LLM response",
            "matched_symbols": [],
        }
    
    payload.setdefault("requirement_id", requirement.id)
    payload.setdefault("matched_symbols", [cb.symbol for cb, _ in code_behaviors[:3]])
    return payload


async def evaluate_requirement_behavior(
    requirement: GddRequirement,
    code_index_id: str | Sequence[str],
    code_behaviors: Optional[List[CodeBehavior]] = None,
    *,
    provider: Optional[QwenProvider] = None,
    llm_func=None,
    embedding_func=None,
    behavior_index_path: Optional[Path] = None,
    top_k: int = 5,
    workspace_id: Optional[str] = None,
) -> Dict[str, Any]:
    """
    Evaluate requirement using behavior-based QA approach.
    
    Steps:
    1. Convert GDD requirement → Behavior Requirement
    2. Match to code behaviors (via embeddings)
    3. LLM classification on top 3-5 matches only
    
    This is fast, scalable, and avoids massive LLM calls.
    """
    active_provider = provider or QwenProvider()
    llm = llm_func or make_llm_model_func(active_provider)
    embed_func = embedding_func or make_embedding_func(active_provider)
    
    # Step 1: Convert to behavior requirement
    try:
        behavior_req = await convert_to_behavior_requirement(requirement, llm_func=llm)
    except Exception as e:
        logger.warning(f"Failed to convert requirement {requirement.id} to behavior: {e}")
        return {
            "requirement_id": requirement.id,
            "status": "error",
            "reason": f"Behavior conversion failed: {str(e)[:100]}",
            "coverage_type": "behavior",
        }
    
    # Step 2: Load or get code behaviors
    if code_behaviors is None:
        if behavior_index_path and behavior_index_path.exists():
            logger.info(f"Loading behavior index from {behavior_index_path}")
            code_behaviors = load_behavior_index(behavior_index_path)
        else:
            logger.info(f"Indexing code behaviors for {code_index_id}...")
            code_behaviors = await index_code_behaviors(
                code_index_id,
                provider=active_provider,
                llm_func=llm,
                workspace_id=workspace_id,
            )
            # Save for future use
            if behavior_index_path:
                save_behavior_index(code_behaviors, behavior_index_path)
    
    if not code_behaviors:
        return {
            "requirement_id": requirement.id,
            "status": "not_implemented",
            "reason": "No code behaviors found",
            "coverage_type": "behavior",
        }
    
    # Step 3: Find matching behaviors
    try:
        matches = await find_matching_behaviors(
            behavior_req,
            code_behaviors,
            provider=active_provider,
            embedding_func=embed_func,
            top_k=top_k,
        )
    except Exception as e:
        logger.warning(f"Error finding behavior matches for {requirement.id}: {e}")
        return {
            "requirement_id": requirement.id,
            "status": "error",
            "reason": f"Behavior matching failed: {str(e)[:100]}",
            "coverage_type": "behavior",
        }
    
    if not matches:
        return {
            "requirement_id": requirement.id,
            "status": "not_implemented",
            "reason": "No matching code behaviors found",
            "coverage_type": "behavior",
            "behavior_requirement": behavior_req.to_dict(),
        }
    
    # Step 4: LLM classification on top matches only
    try:
        classification = await classify_behavior_implementation(behavior_req, matches, llm)
        classification["coverage_type"] = "behavior"
        classification["behavior_requirement"] = behavior_req.to_dict()
        classification["top_matches"] = [
            {
                "symbol": cb.symbol,
                "similarity": float(score),
                "description": cb.description,
            }
            for cb, score in matches[:top_k]
        ]
        return classification
    except Exception as e:
        logger.warning(f"Error classifying behavior implementation for {requirement.id}: {e}")
        return {
            "requirement_id": requirement.id,
            "status": "error",
            "reason": f"Classification failed: {str(e)[:100]}",
            "coverage_type": "behavior",
            "behavior_requirement": behavior_req.to_dict(),
        }


async def evaluate_all_requirements_behavior(
    doc_id: str,
    code_index_id: str | Sequence[str],
    requirements: Sequence[GddRequirement],
    *,
    output_dir: Optional[Path] = None,
    provider: Optional[QwenProvider] = None,
    top_k: int = 5,
    use_behavior_index_cache: bool = True,
    workspace_id: Optional[str] = None,
) -> Path:
    """
    Evaluate all requirements using behavior-based QA approach.
    This is the new scalable method that avoids massive LLM calls.
    """
    if not requirements:
        raise ValueError("No requirements provided for coverage evaluation.")
    
    out_dir = output_dir or DEFAULT_REPORT_DIR
    out_dir.mkdir(parents=True, exist_ok=True)
    
    code_id_str = code_index_id if isinstance(code_index_id, str) else "_".join(code_index_id[:3])
    report_path = out_dir / f"{doc_id}_{code_id_str}_behavior_coverage.json"
    
    active_provider = provider or QwenProvider()
    llm = make_llm_model_func(active_provider)
    embed_func = make_embedding_func(active_provider)
    
    # Load or create behavior index (workspace-scoped if workspace_id provided)
    if workspace_id:
        from gdd_rag_backbone.workspace.storage import WorkspaceStorage
        storage = WorkspaceStorage(workspace_id)
        behavior_cache_dir = storage.get_behavior_cache_dir()
    else:
        behavior_cache_dir = BEHAVIOR_INDEX_CACHE_DIR
    
    behavior_cache_dir.mkdir(parents=True, exist_ok=True)
    behavior_index_path = behavior_cache_dir / f"{code_id_str}_behaviors.json"
    
    code_behaviors = None
    if use_behavior_index_cache and behavior_index_path.exists():
        logger.info(f"Loading cached behavior index from {behavior_index_path}")
        code_behaviors = load_behavior_index(behavior_index_path)
    else:
        logger.info(f"Creating behavior index for {code_index_id}...")
        code_behaviors = await index_code_behaviors(
            code_index_id,
            provider=active_provider,
            llm_func=llm,
            workspace_id=workspace_id,
        )
        save_behavior_index(code_behaviors, behavior_index_path)
        logger.info(f"Saved behavior index with {len(code_behaviors)} behaviors")
    
    # Evaluate each requirement
    results: List[Dict[str, Any]] = []
    total = len(requirements)
    
    for idx, requirement in enumerate(requirements, 1):
        try:
            logger.info(f"[Behavior Coverage] Evaluating requirement {idx}/{total}: {requirement.id} - {requirement.title[:50]}")
            result = await evaluate_requirement_behavior(
                requirement,
                code_index_id,
                code_behaviors=code_behaviors,
                provider=active_provider,
                llm_func=llm,
                workspace_id=workspace_id,
                embedding_func=embed_func,
                top_k=top_k,
            )
            results.append(result)
            status = result.get("status", "unknown")
            logger.info(f"[Behavior Coverage] Requirement {idx}/{total} completed: {status}")
        except Exception as e:
            logger.error(f"[Behavior Coverage] Error evaluating requirement {idx}/{total} ({requirement.id}): {e}", exc_info=True)
            results.append({
                "requirement_id": requirement.id,
                "status": "error",
                "coverage_type": "behavior",
                "reason": f"Evaluation error: {str(e)[:100]}",
            })
    
    report_payload = {
        "doc_id": doc_id,
        "code_index_id": code_index_id,
        "evaluation_method": "behavior_based",
        "results": results,
    }
    report_path.write_text(json.dumps(report_payload, indent=2, ensure_ascii=False))
    logger.info(f"Saved behavior-based coverage report to {report_path}")
    return report_path

