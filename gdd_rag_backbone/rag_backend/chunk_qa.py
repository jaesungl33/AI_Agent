"""Chunk-level QA helpers shared between CLIs and the Streamlit UI."""

from __future__ import annotations

import json
import math
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, List, Optional, Sequence, Tuple, Iterable

from gdd_rag_backbone.config import DEFAULT_WORKING_DIR

STATUS_PATH = DEFAULT_WORKING_DIR / "kv_store_doc_status.json"
CHUNKS_PATH = DEFAULT_WORKING_DIR / "kv_store_text_chunks.json"
VDB_CHUNKS_PATH = DEFAULT_WORKING_DIR / "vdb_chunks.json"


class ChunkStoreError(RuntimeError):
    """Raised when the persisted chunk stores cannot be read."""


@dataclass
class ChunkRecord:
    chunk_id: str
    doc_id: str
    content: str


# ---------------------------------------------------------------------------
# Storage helpers
# ---------------------------------------------------------------------------

def _load_json(path: Path) -> dict:
    if not path.exists():
        raise ChunkStoreError(f"Expected store not found: {path}")
    try:
        return json.loads(path.read_text())
    except json.JSONDecodeError as exc:  # pragma: no cover - defensive
        raise ChunkStoreError(f"Could not parse {path}: {exc}") from exc


def load_doc_status() -> Dict[str, dict]:
    try:
        data = _load_json(STATUS_PATH)
    except ChunkStoreError:
        return {}
    return {
        doc_id: meta
        for doc_id, meta in data.items()
        if isinstance(meta, dict)
    }


def list_indexed_docs() -> List[dict]:
    status = load_doc_status()
    docs: List[dict] = []
    for doc_id, meta in status.items():
        docs.append(
            {
                "doc_id": doc_id,
                "file_path": meta.get("file_path", ""),
                "updated_at": meta.get("updated_at"),
                "status": meta.get("status", ""),
            }
        )
    docs.sort(key=lambda item: item["doc_id"])
    return docs


def get_doc_metadata(doc_id: str) -> Optional[dict]:
    return load_doc_status().get(doc_id)


def load_doc_chunks(doc_id: str) -> List[ChunkRecord]:
    try:
        data = _load_json(CHUNKS_PATH)
    except ChunkStoreError:
        return []
    records: List[ChunkRecord] = []
    for chunk_id, payload in data.items():
        if (
            isinstance(payload, dict)
            and payload.get("full_doc_id") == doc_id
            and payload.get("content")
        ):
            records.append(
                ChunkRecord(
                    chunk_id=chunk_id,
                    doc_id=doc_id,
                    content=payload["content"],
                )
            )
    return records


def preview_chunks(doc_id: str, limit: int = 3) -> List[ChunkRecord]:
    return load_doc_chunks(doc_id)[:limit]


# ---------------------------------------------------------------------------
# Embedding + scoring helpers
# ---------------------------------------------------------------------------

def _cosine_similarity(vec_a: Sequence[float], vec_b: Sequence[float]) -> float:
    dot = sum(a * b for a, b in zip(vec_a, vec_b))
    norm_a = math.sqrt(sum(a * a for a in vec_a))
    norm_b = math.sqrt(sum(b * b for b in vec_b))
    if norm_a == 0 or norm_b == 0:
        return 0.0
    return dot / (norm_a * norm_b)


def _ensure_float_vector(raw: Iterable) -> Optional[List[float]]:
    vector: List[float] = []
    for value in raw:
        if isinstance(value, (int, float)):
            vector.append(float(value))
        elif isinstance(value, str):
            value = value.strip()
            if not value:
                continue
            try:
                vector.append(float(value))
            except ValueError:
                return None
        else:
            return None
    return vector if vector else None


def _embed_texts(provider, texts: Sequence[str]) -> List[List[float]]:
    raw_embeddings = provider.embed(list(texts))
    floats: List[List[float]] = []
    for embedding in raw_embeddings:
        if embedding is None:
            raise ValueError("Embedding provider returned None vector")
        float_embedding = _ensure_float_vector(embedding)
        if not float_embedding:
            raise ValueError("Embedding provider returned invalid vector values")
        floats.append(float_embedding)
    return floats


def _load_chunk_vectors(doc_ids: Sequence[str]) -> Dict[str, List[float]]:
    if not VDB_CHUNKS_PATH.exists():
        return {}
    try:
        data = json.loads(VDB_CHUNKS_PATH.read_text())
    except json.JSONDecodeError:  # pragma: no cover - defensive
        return {}
    allowed = set(doc_ids)
    vectors: Dict[str, List[float]] = {}
    for entry in data.get("data", []):
        doc_id = entry.get("full_doc_id")
        chunk_id = entry.get("__id__") or entry.get("id")
        vector = entry.get("vector") or entry.get("embedding")
        if chunk_id and vector and doc_id in allowed:
            float_vector = _ensure_float_vector(vector)
            if float_vector:
                vectors[chunk_id] = float_vector
    return vectors


def _score_chunks(
    question_embedding: List[float],
    chunks: List[ChunkRecord],
    vectors: Dict[str, List[float]],
    provider,
) -> List[Tuple[float, ChunkRecord]]:
    missing_records = [record for record in chunks if record.chunk_id not in vectors]
    if missing_records:
        contents = [record.content for record in missing_records]
        embeddings = _embed_texts(provider, contents)
        for record, embedding in zip(missing_records, embeddings):
            vectors[record.chunk_id] = embedding

    scored: List[Tuple[float, ChunkRecord]] = []
    for record in chunks:
        embedding = vectors.get(record.chunk_id)
        if embedding is None:
            embedding = _embed_texts(provider, [record.content])[0]
            vectors[record.chunk_id] = embedding
        score = _cosine_similarity(question_embedding, embedding)
        scored.append((score, record))
    scored.sort(key=lambda item: item[0], reverse=True)
    return scored


def _select_top_chunks(
    scored: List[Tuple[float, ChunkRecord]],
    *,
    top_k: int,
    per_doc_limit: Optional[int] = None,
) -> List[Tuple[float, ChunkRecord]]:
    selected: List[Tuple[float, ChunkRecord]] = []
    counts: Dict[str, int] = {}
    if per_doc_limit and per_doc_limit > 0:
        for score, record in scored:
            if counts.get(record.doc_id, 0) >= per_doc_limit:
                continue
            selected.append((score, record))
            counts[record.doc_id] = counts.get(record.doc_id, 0) + 1
            if len(selected) >= top_k:
                break
    if len(selected) < top_k:
        for score, record in scored:
            if (score, record) in selected:
                continue
            selected.append((score, record))
            if len(selected) >= top_k:
                break
    return selected


# ---------------------------------------------------------------------------
# Public API
# ---------------------------------------------------------------------------

def get_top_chunks(
    doc_ids: Sequence[str],
    question: str,
    *,
    provider,
    top_k: int = 4,
    per_doc_limit: Optional[int] = None,
) -> List[Dict[str, object]]:
    if not doc_ids:
        raise ValueError("At least one doc_id is required.")

    unique_ids = list(dict.fromkeys(doc_ids))
    all_chunks: List[ChunkRecord] = []
    for doc_id in unique_ids:
        all_chunks.extend(load_doc_chunks(doc_id))
    if not all_chunks:
        raise ValueError("No chunks found for the selected documents. Verify they were indexed.")

    question_embedding = _embed_texts(provider, [question])[0]
    vectors = _load_chunk_vectors(unique_ids)
    scored = _score_chunks(question_embedding, all_chunks, vectors, provider)
    selected = _select_top_chunks(
        scored,
        top_k=top_k,
        per_doc_limit=per_doc_limit or (1 if len(unique_ids) > 1 else None),
    )
    return [
        {
            "doc_id": record.doc_id,
            "chunk_id": record.chunk_id,
            "content": record.content,
            "score": score,
        }
        for score, record in selected
    ]


def _build_prompt(doc_title: str, context_blocks: List[str], question: str) -> str:
    context = "\n\n".join(f"[Chunk {idx + 1}]\n{block}" for idx, block in enumerate(context_blocks))
    return (
        "You are an expert assistant answering questions about a Game Design Document. "
        "Use ONLY the provided context. If the answer is missing, say you don't know.\n"
        f"Document: {doc_title}\n\n"
        f"Context:\n{context}\n\n"
        f"Question: {question}\nAnswer:"
    )


def ask_with_chunks(
    doc_id: str,
    question: str,
    *,
    provider,
    top_k: int = 4,
) -> Dict[str, object]:
    metadata = get_doc_metadata(doc_id)
    if metadata is None:
        raise ValueError(f"Document '{doc_id}' not found. Please index it before asking questions.")

    chunks = load_doc_chunks(doc_id)
    if not chunks:
        raise ValueError(f"No chunks found for '{doc_id}'. Try re-indexing the document first.")

    question_embedding = _embed_texts(provider, [question])[0]
    vectors = _load_chunk_vectors([doc_id])
    scored = _score_chunks(question_embedding, chunks, vectors, provider)
    top_records = [record for _, record in scored[:top_k]]
    prompt = _build_prompt(metadata.get("file_path", doc_id), [r.content for r in top_records], question)
    answer_text = provider.llm(prompt=prompt)

    context_payload = [
        {
            "chunk_id": record.chunk_id,
            "doc_id": record.doc_id,
            "content": record.content,
            "score": score,
        }
        for score, record in scored[:top_k]
    ]

    return {
        "answer": answer_text.strip(),
        "doc_id": doc_id,
        "file_path": metadata.get("file_path"),
        "context": context_payload,
    }


def ask_across_docs(
    doc_ids: Sequence[str],
    question: str,
    *,
    provider,
    top_k: int = 6,
    per_doc_limit: Optional[int] = 2,
) -> Dict[str, object]:
    if not doc_ids:
        raise ValueError("At least one doc_id is required.")

    unique_ids = list(dict.fromkeys(doc_ids))
    metadata_map = {doc_id: get_doc_metadata(doc_id) or {} for doc_id in unique_ids}

    all_chunks: List[ChunkRecord] = []
    for doc_id in unique_ids:
        all_chunks.extend(load_doc_chunks(doc_id))
    if not all_chunks:
        raise ValueError("No chunks found for the selected documents. Verify they were indexed.")

    question_embedding = _embed_texts(provider, [question])[0]
    vectors = _load_chunk_vectors(unique_ids)
    scored = _score_chunks(question_embedding, all_chunks, vectors, provider)
    selected = _select_top_chunks(
        scored,
        top_k=top_k,
        per_doc_limit=per_doc_limit or (1 if len(unique_ids) > 1 else None),
    )

    prompt_docs = ", ".join(
        metadata_map.get(doc_id, {}).get("file_path", doc_id) or doc_id
        for doc_id in unique_ids
    )
    prompt = _build_prompt(prompt_docs, [record.content for _, record in selected], question)
    answer_text = provider.llm(prompt=prompt)
    context_payload = [
        {
            "doc_id": record.doc_id,
            "chunk_id": record.chunk_id,
            "content": record.content,
            "score": score,
        }
        for score, record in selected
    ]
    return {
        "answer": answer_text.strip(),
        "doc_ids": unique_ids,
        "context": context_payload,
    }

