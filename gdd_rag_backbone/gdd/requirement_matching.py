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
from pathlib import Path
from typing import Any, Dict, Iterable, List, Optional, Sequence

# Project imports
from gdd_rag_backbone.gdd.schemas import GddRequirement
from gdd_rag_backbone.llm_providers import QwenProvider, make_llm_model_func
from gdd_rag_backbone.rag_backend.chunk_qa import get_top_chunks

DEFAULT_REPORT_DIR = Path("reports/coverage_checks")


def generate_code_queries(requirement: GddRequirement) -> List[str]:
    queries = [requirement.title, f"{requirement.category or ''} {requirement.title}".strip()]
    for field in (requirement.description, requirement.acceptance_criteria):
        if field:
            queries.append(field)
    if requirement.related_systems:
        queries.append("; ".join(requirement.related_systems))
    return [query for query in queries if query]


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


async def evaluate_requirement(
    requirement: GddRequirement,
    code_index_id: str | Sequence[str],  # Support single or multiple code indices
    *,
    provider: Optional[QwenProvider] = None,
    llm_func=None,
    top_k: int = 8,
) -> Dict[str, Any]:
    active_provider = provider or QwenProvider()
    chunks = await search_code_chunks(
        generate_code_queries(requirement),
        code_index_id,
        provider=active_provider,
        top_k=top_k,
    )
    llm = llm_func or make_llm_model_func(active_provider)
    result = await classify_requirement_coverage(requirement, chunks, llm)
    result["retrieved_chunks"] = chunks
    return result


async def evaluate_all_requirements(
    doc_id: str,
    code_index_id: str | Sequence[str],  # Support single or multiple code indices
    requirements: Sequence[GddRequirement],
    *,
    output_dir: Optional[Path] = None,
    provider: Optional[QwenProvider] = None,
    top_k: int = 8,
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

    results: List[Dict[str, Any]] = []
    for requirement in requirements:
        result = await evaluate_requirement(
            requirement,
            code_index_id,
            provider=active_provider,
            llm_func=llm,
            top_k=top_k,
        )
        results.append(result)

    report_payload = {
        "doc_id": doc_id,
        "code_index_id": code_index_id,
        "results": results,
    }
    report_path.write_text(json.dumps(report_payload, indent=2, ensure_ascii=False))
    return report_path


