"""
Code behavior indexing - Step 2 of QA-style approach.

This module extracts behavior descriptions from code methods/functions.
Instead of embedding entire code files, we extract:
- All methods
- All comments
- All event handlers
- All key logic blocks

Then convert them to lightweight behavior descriptions via LLM.
"""

from __future__ import annotations

import asyncio
import hashlib
import json
import logging
import re
from typing import Iterable
from dataclasses import asdict
from pathlib import Path
from typing import Any, Dict, List, Optional, Sequence

logger = logging.getLogger(__name__)

from gdd_rag_backbone.gdd.schemas import CodeBehavior
from gdd_rag_backbone.llm_providers import QwenProvider, make_llm_model_func
from gdd_rag_backbone.rag_backend.chunk_qa import load_doc_chunks

BEHAVIOR_EXTRACTION_PROMPT = """
Analyze this code method/function and extract its behavior in QA-style format.

Extract:
- Description: What does this code do? (one sentence)
- Trigger patterns: What events, method calls, or conditions activate this? (e.g., "OnTriggerEnter", "player enters", "button click")
- Effect patterns: What happens when this executes? (e.g., "player invisible", "damage dealt", "state changed")
- Entities: What objects, classes, or systems are involved? (e.g., "Player", "Grass", "HidingSystem")

Return ONLY JSON:
{{
  "description": "Brief description of what this code does",
  "trigger_patterns": ["pattern1", "pattern2"],
  "effect_patterns": ["effect1", "effect2"],
  "entities": ["Entity1", "Entity2"]
}}

Code:
{{CODE}}

Focus on behavior, not implementation details. Extract what the code DOES, not how it does it.
"""


def extract_methods_from_chunk(chunk_content: str, chunk_id: str) -> List[Dict[str, Any]]:
    """
    Extract method/function definitions from a code chunk.
    Returns list of {symbol, code, chunk_id} dicts.
    """
    methods = []
    lines = chunk_content.splitlines()
    
    current_class = None
    current_method = None
    method_start = None
    method_lines = []
    brace_count = 0
    in_method = False
    
    for idx, line in enumerate(lines):
        stripped = line.strip()
        
        # Detect class definition
        if stripped.startswith("class "):
            parts = stripped.split()
            if len(parts) >= 2:
                class_name = parts[1].split("(")[0].strip().rstrip(":")
                current_class = class_name
        
        # Detect method/function definition
        if stripped.startswith("def ") or stripped.startswith("public ") or stripped.startswith("private ") or stripped.startswith("protected "):
            # Save previous method if exists
            if current_method and method_lines:
                method_code = "\n".join(method_lines)
                symbol = f"{current_class}.{current_method}" if current_class else current_method
                methods.append({
                    "symbol": symbol,
                    "code": method_code,
                    "chunk_id": chunk_id,
                })
            
            # Start new method
            if stripped.startswith("def "):
                method_name = stripped[4:].split("(")[0].strip().rstrip(":")
            else:
                # C# style: public void MethodName(...)
                parts = stripped.split()
                method_name = None
                for i, part in enumerate(parts):
                    if i > 0 and part and not part in ("public", "private", "protected", "static", "void", "int", "bool", "string", "float"):
                        method_name = part.split("(")[0].strip().rstrip(":")
                        break
                if not method_name:
                    continue
            
            current_method = method_name
            method_start = idx
            method_lines = [line]
            in_method = True
            
            # Count opening braces (net count: +1 for {, -1 for })
            brace_count = line.count("{") - line.count("}")
        elif in_method:
            method_lines.append(line)
            brace_count += line.count("{") - line.count("}")
            
            # Method ends when braces are balanced (for C#) or we hit a dedent (for Python)
            if brace_count <= 0 and (stripped.startswith("def ") or stripped.startswith("class ") or (stripped and not line.startswith(" ") and not line.startswith("\t"))):
                if current_method and method_lines:
                    method_code = "\n".join(method_lines)
                    symbol = f"{current_class}.{current_method}" if current_class else current_method
                    methods.append({
                        "symbol": symbol,
                        "code": method_code,
                        "chunk_id": chunk_id,
                    })
                in_method = False
                current_method = None
                method_lines = []
                brace_count = 0
    
    # Save last method if exists
    if current_method and method_lines:
        method_code = "\n".join(method_lines)
        symbol = f"{current_class}.{current_method}" if current_class else current_method
        methods.append({
            "symbol": symbol,
            "code": method_code,
            "chunk_id": chunk_id,
        })
    
    return methods


ALLOWED_CODE_EXTS = {
    ".cs",
    ".js",
    ".jsx",
    ".ts",
    ".tsx",
    ".py",
    ".cpp",
    ".cc",
    ".c",
    ".h",
    ".hpp",
    ".java",
}


def should_skip_file(file_path: Optional[str]) -> bool:
    """
    Heuristic to skip non-code files (e.g., Unity .meta) and non-allowed extensions.
    """
    if not file_path:
        return False
    lowered = file_path.lower()
    if lowered.endswith(".meta"):
        return True
    # Allow only known code extensions; skip everything else if an extension exists
    ext = Path(lowered).suffix
    if ext and ext not in ALLOWED_CODE_EXTS:
        return True
    return False


async def extract_code_behavior(
    symbol: str,
    code: str,
    chunk_id: str,
    *,
    llm_func,
    file_path: Optional[str] = None,
) -> CodeBehavior:
    """
    Convert a code method to a behavior description using LLM.
    """
    # Truncate code if too long
    code_snippet = code[:2000] if len(code) > 2000 else code
    
    prompt = BEHAVIOR_EXTRACTION_PROMPT.replace("{{CODE}}", code_snippet)
    system_prompt = (
        "You are a QA engineer analyzing code to extract behavior descriptions. "
        "Focus on WHAT the code does, not HOW. Return ONLY JSON."
    )
    
    try:
        response_text = await asyncio.wait_for(
            llm_func(prompt=prompt, system_prompt=system_prompt, temperature=0.1),
            timeout=15.0,
        )
    except asyncio.TimeoutError:
        logger.warning(f"Timeout extracting behavior for {symbol}")
        # Fallback: create basic behavior from symbol name
        return CodeBehavior(
            symbol=symbol,
            description=f"Method {symbol}",
            trigger_patterns=[],
            effect_patterns=[],
            entities=[],
            file_path=file_path,
            chunk_id=chunk_id,
        )
    
    # Parse response
    text = response_text.strip()
    if text.startswith("```"):
        lines = text.split("\n")
        text = "\n".join(lines[1:-1]) if lines[-1].strip() == "```" else "\n".join(lines[1:])
    
    try:
        payload = json.loads(text)
    except json.JSONDecodeError:
        logger.warning(f"Failed to parse behavior extraction for {symbol}")
        return CodeBehavior(
            symbol=symbol,
            description=f"Method {symbol}",
            trigger_patterns=[],
            effect_patterns=[],
            entities=[],
            file_path=file_path,
            chunk_id=chunk_id,
        )
    
    return CodeBehavior(
        symbol=symbol,
        description=payload.get("description", f"Method {symbol}"),
        trigger_patterns=payload.get("trigger_patterns", []),
        effect_patterns=payload.get("effect_patterns", []),
        entities=payload.get("entities", []),
        file_path=file_path,
        chunk_id=chunk_id,
    )


def _compute_code_signature(
    code_index_id: str | Sequence[str],
    *,
    workspace_id: Optional[str] = None,
) -> Dict[str, Any]:
    """
    Compute a lightweight signature for a code index based on chunk contents.
    Used to decide if the behavior index can be reused.
    """
    if isinstance(code_index_id, str):
        code_indices = [code_index_id]
    else:
        code_indices = list(code_index_id)

    hasher = hashlib.sha256()
    total_chunks = 0
    for code_id in code_indices:
        chunks = load_doc_chunks(code_id, workspace_id=workspace_id)
        total_chunks += len(chunks)
        for chunk in chunks:
            hasher.update(chunk.chunk_id.encode("utf-8", errors="ignore"))
            hasher.update(chunk.content.encode("utf-8", errors="ignore"))

    return {
        "code_indices": code_indices,
        "workspace_id": workspace_id,
        "chunk_count": total_chunks,
        "hash": hasher.hexdigest(),
    }


def _should_skip_method(method_code: str, *, min_lines: int, max_lines: int) -> bool:
    """Chunk pruning: skip trivial or extremely large methods."""
    line_count = len(method_code.splitlines())
    return line_count < min_lines or line_count > max_lines


async def index_code_behaviors(
    code_index_id: str | Sequence[str],
    *,
    provider: Optional[QwenProvider] = None,
    llm_func=None,
    max_methods: Optional[int] = None,
    force_rebuild: bool = False,
    workspace_id: Optional[str] = None,
    min_method_lines: int = 3,
    max_method_lines: int = 400,
) -> List[CodeBehavior]:
    """
    Extract behavior descriptions from all methods in a codebase.
    This is Step 2: Codebase → Behavior Index.
    
    Args:
        code_index_id: Single code index ID or list of IDs
        provider: LLM provider (optional)
        llm_func: LLM function (optional)
        max_methods: Limit number of methods to process (for testing)
    
    Returns:
        List of CodeBehavior objects
    """
    active_provider = provider or QwenProvider()
    llm = llm_func or make_llm_model_func(active_provider)
    
    # Normalize to list
    if isinstance(code_index_id, str):
        code_indices = [code_index_id]
    else:
        code_indices = list(code_index_id)
    
    # Extract all methods from all chunks (skip non-code files like .meta)
    all_methods: List[Dict[str, Any]] = []
    for code_id in code_indices:
        logger.info(f"Extracting methods from code index: {code_id}")
        chunks = load_doc_chunks(code_id, workspace_id=workspace_id)
        
        for chunk in chunks:
            file_path = getattr(chunk, "file_path", None)
            if should_skip_file(file_path):
                continue
            methods = extract_methods_from_chunk(chunk.content, chunk.chunk_id)
            # Add file path if available in chunk metadata
            for method in methods:
                method["file_path"] = file_path
                if _should_skip_method(method["code"], min_lines=min_method_lines, max_lines=max_method_lines):
                    continue
                all_methods.append(method)
    
    if max_methods:
        all_methods = all_methods[:max_methods]
    
    logger.info(f"Found {len(all_methods)} methods to process")
    
    # Convert each method to behavior description
    behaviors: List[CodeBehavior] = []
    total = len(all_methods)
    
    for idx, method in enumerate(all_methods, 1):
        try:
            if idx % 10 == 0:
                logger.info(f"Processing method {idx}/{total}: {method['symbol']}")
            
            behavior = await extract_code_behavior(
                symbol=method["symbol"],
                code=method["code"],
                chunk_id=method["chunk_id"],
                llm_func=llm,
                file_path=method.get("file_path"),
            )
            behaviors.append(behavior)
        except Exception as e:
            logger.warning(f"Error processing method {method['symbol']}: {e}")
            # Create fallback behavior
            behaviors.append(CodeBehavior(
                symbol=method["symbol"],
                description=f"Method {method['symbol']}",
                trigger_patterns=[],
                effect_patterns=[],
                entities=[],
                file_path=method.get("file_path"),
                chunk_id=method["chunk_id"],
            ))
    
    logger.info(f"Extracted {len(behaviors)} code behaviors")
    return behaviors


def save_behavior_index(behaviors: List[CodeBehavior], output_path: Path) -> None:
    """Save behavior index to JSON file."""
    output_path.parent.mkdir(parents=True, exist_ok=True)
    data = [behavior.to_dict() for behavior in behaviors]
    output_path.write_text(json.dumps(data, indent=2, ensure_ascii=False))
    logger.info(f"Saved {len(behaviors)} behaviors to {output_path}")


def load_behavior_index(input_path: Path) -> List[CodeBehavior]:
    """Load behavior index from JSON file."""
    data = json.loads(input_path.read_text())
    return [CodeBehavior(**item) for item in data]


def save_behavior_index_with_meta(
    behaviors: List[CodeBehavior],
    output_path: Path,
    signature: Dict[str, Any],
) -> None:
    """Save behavior index and its signature sidecar file."""
    save_behavior_index(behaviors, output_path)
    meta_path = output_path.with_suffix(output_path.suffix + ".meta.json")
    meta_payload = {
        "signature": signature,
        "count": len(behaviors),
    }
    meta_path.write_text(json.dumps(meta_payload, indent=2, ensure_ascii=False))
    logger.info(f"Saved behavior index metadata to {meta_path}")


def load_behavior_index_meta(input_path: Path) -> Optional[Dict[str, Any]]:
    """Load behavior index signature if present."""
    meta_path = input_path.with_suffix(input_path.suffix + ".meta.json")
    if not meta_path.exists():
        return None
    try:
        return json.loads(meta_path.read_text())
    except json.JSONDecodeError:
        logger.warning(f"Failed to parse behavior index meta: {meta_path}")
        return None


__all__ = [
    "index_code_behaviors",
    "load_behavior_index",
    "save_behavior_index",
    "save_behavior_index_with_meta",
    "load_behavior_index_meta",
    "_compute_code_signature",
]


