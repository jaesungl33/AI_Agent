"""
Behavior matching - Step 3 of QA-style approach.

This module matches behavior requirements to code behaviors using semantic similarity.
Instead of matching GDD directly to code, we match:
- Requirement Behaviors (triggers, effects, entities)
- Code Behaviors (trigger patterns, effect patterns, entities)

This is fast, lightweight, and highly aligned with QA workflow.
"""

from __future__ import annotations

import asyncio
import logging
import inspect
from typing import Any, List, Optional, Sequence, Tuple

import math

logger = logging.getLogger(__name__)

from gdd_rag_backbone.gdd.schemas import BehaviorRequirement, CodeBehavior
from gdd_rag_backbone.llm_providers import QwenProvider, make_embedding_func

# In-memory cache for behavior embeddings
_BEHAVIOR_EMBEDDING_CACHE: dict[str, List[float]] = {}


def _flatten_to_float_list(value: Any) -> List[float]:
    """Flatten nested embeddings into a 1D float list."""
    result: List[float] = []

    def _walk(item: Any) -> None:
        if isinstance(item, (list, tuple)):
            for sub in item:
                _walk(sub)
        elif isinstance(item, (int, float)):
            result.append(float(item))
        elif isinstance(item, str):
            stripped = item.strip()
            if stripped:
                try:
                    result.append(float(stripped))
                except ValueError:
                    pass
        # Ignore other types silently (defensive)

    _walk(value)
    return result


async def embed_behavior_text(
    text: str,
    *,
    provider: Optional[QwenProvider] = None,
    embedding_func=None,
) -> np.ndarray:
    """
    Get embedding for a behavior text description.
    Uses cache to avoid recomputing.
    """
    cache_key = f"behavior::{text[:200]}"  # Use first 200 chars as key
    
    if cache_key in _BEHAVIOR_EMBEDDING_CACHE:
        return _BEHAVIOR_EMBEDDING_CACHE[cache_key]
    
    active_provider = provider or QwenProvider()
    embed_func = embedding_func or make_embedding_func(active_provider)
    
    # Support both sync and async embedding functions
    def _embed():
        result = embed_func(text)
        return result

    if inspect.iscoroutinefunction(embed_func):
        embedding = await embed_func(text)  # type: ignore
    else:
        embedding = await asyncio.to_thread(_embed)

    # If the embedder returned a coroutine for any reason, await it once more (defensive)
    if inspect.iscoroutine(embedding):
        embedding = await embedding  # type: ignore

    embedding_array = _flatten_to_float_list(embedding)
    _BEHAVIOR_EMBEDDING_CACHE[cache_key] = embedding_array
    return embedding_array


def cosine_similarity(a: Sequence[float], b: Sequence[float]) -> float:
    """Compute cosine similarity between two vectors."""
    if not a or not b:
        return 0.0
    # Align lengths
    length = min(len(a), len(b))
    if length == 0:
        return 0.0
    dot_product = sum(a[i] * b[i] for i in range(length))
    norm_a = math.sqrt(sum(x * x for x in a[:length]))
    norm_b = math.sqrt(sum(x * x for x in b[:length]))
    if norm_a == 0.0 or norm_b == 0.0:
        return 0.0
    return float(dot_product / (norm_a * norm_b))


async def find_matching_behaviors(
    requirement: BehaviorRequirement,
    code_behaviors: Sequence[CodeBehavior],
    *,
    provider: Optional[QwenProvider] = None,
    embedding_func=None,
    top_k: int = 6,
    similarity_threshold: float = 0.3,
) -> List[Tuple[CodeBehavior, float]]:
    """
    Find top-k code behaviors that match a requirement behavior.
    
    Uses semantic similarity on behavior descriptions (triggers, effects, entities).
    This is Step 3: Compare Requirement Behaviors to Code Behaviors.
    
    Returns:
        List of (CodeBehavior, similarity_score) tuples, sorted by score descending.
    """
    # Get embedding for requirement behavior
    req_text = requirement.to_behavior_text()
    req_embedding = await embed_behavior_text(req_text, provider=provider, embedding_func=embedding_func)
    
    # Get embeddings for all code behaviors and compute similarities
    similarities: List[Tuple[CodeBehavior, float]] = []
    
    for code_behavior in code_behaviors:
        code_text = code_behavior.to_behavior_text()
        code_embedding = await embed_behavior_text(code_text, provider=provider, embedding_func=embedding_func)
        
        similarity = cosine_similarity(req_embedding, code_embedding)
        similarities.append((code_behavior, similarity))
    
    # Filter by threshold and return top-k above threshold
    filtered_similarities = [(cb, sim) for cb, sim in similarities if sim >= similarity_threshold]
    filtered_similarities.sort(key=lambda x: x[1], reverse=True)
    return filtered_similarities[:top_k]


async def batch_find_matching_behaviors(
    requirements: Sequence[BehaviorRequirement],
    code_behaviors: Sequence[CodeBehavior],
    *,
    provider: Optional[QwenProvider] = None,
    embedding_func=None,
    top_k: int = 6,
    similarity_threshold: float = 0.3,
) -> dict[str, List[Tuple[CodeBehavior, float]]]:
    """
    Find matching behaviors for multiple requirements.
    More efficient than calling find_matching_behaviors multiple times.
    
    Returns:
        Dict mapping requirement_id -> list of (CodeBehavior, similarity_score) tuples
    """
    active_provider = provider or QwenProvider()
    embed_func = embedding_func or make_embedding_func(active_provider)
    
    # Pre-compute embeddings for all code behaviors
    logger.info(f"Pre-computing embeddings for {len(code_behaviors)} code behaviors...")
    code_embeddings: dict[str, List[float]] = {}
    
    for code_behavior in code_behaviors:
        code_text = code_behavior.to_behavior_text()
        code_embeddings[code_behavior.symbol] = await embed_behavior_text(
            code_text, provider=active_provider, embedding_func=embed_func
        )
    
    logger.info(f"Finding matches for {len(requirements)} requirements...")
    results: dict[str, List[Tuple[CodeBehavior, float]]] = {}
    
    for requirement in requirements:
        req_text = requirement.to_behavior_text()
        req_embedding = await embed_behavior_text(
            req_text, provider=active_provider, embedding_func=embed_func
        )
        
        # Compute similarities
        similarities: List[Tuple[CodeBehavior, float]] = []
        for code_behavior in code_behaviors:
            code_embedding = code_embeddings[code_behavior.symbol]
            similarity = cosine_similarity(req_embedding, code_embedding)
            similarities.append((code_behavior, similarity))
        
        # Filter by threshold and take top-k above threshold
        filtered_similarities = [(cb, sim) for cb, sim in similarities if sim >= similarity_threshold]
        filtered_similarities.sort(key=lambda x: x[1], reverse=True)
        results[requirement.id] = filtered_similarities[:top_k]
    
    return results


