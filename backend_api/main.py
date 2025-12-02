"""
Minimal FastAPI backend for the GDD RAG frontend.

This provides REST endpoints that the Next.js app can call:
- GET  /health                -> simple health check
- POST /documents/gdd         -> upload a GDD file
- POST /documents/code        -> upload a code archive (placeholder)
- GET  /gdd/{doc_id}/summary  -> placeholder GDD summary
- POST /chat                  -> simple echo-style chat

This is intentionally lightweight so you can get a real backend
running quickly. Later, you can wire these endpoints into the full
`gdd_rag_backbone` RAG pipeline.
"""

from __future__ import annotations

import sys
from pathlib import Path
from datetime import datetime
from typing import Optional, List
import asyncio

from fastapi import FastAPI, UploadFile, File, Form
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
from pydantic import BaseModel

# Ensure project root is on PYTHONPATH so we can import existing modules later
PROJECT_ROOT = Path(__file__).resolve().parents[1]
if str(PROJECT_ROOT) not in sys.path:
    sys.path.insert(0, str(PROJECT_ROOT))

from gdd_rag_backbone.config import DEFAULT_DOCS_DIR  # type: ignore
from gdd_rag_backbone.llm_providers import (  # type: ignore
    QwenProvider,
    make_llm_model_func,
    make_embedding_func,
)
from gdd_rag_backbone.rag_backend import indexing  # type: ignore
from gdd_rag_backbone.rag_backend.chunk_qa import (  # type: ignore
    ask_with_chunks,
    ask_across_docs,
    load_doc_status,
)


app = FastAPI(title="GDD RAG Backend", version="0.1.0")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/health")
async def health() -> dict:
    """Simple health check used by the frontend to detect backend."""
    return {"status": "ok", "time": datetime.utcnow().isoformat() + "Z"}


@app.get("/documents")
async def list_documents() -> JSONResponse:
    """
    Placeholder documents list endpoint.

    The frontend calls this to populate the Documents page. For now it just
    returns an empty list instead of 404 so the UI doesn't error.
    """
    return JSONResponse([])


@app.post("/documents/gdd")
async def upload_gdd(
    file: UploadFile = File(...),
    docId: Optional[str] = Form(default=None),
) -> JSONResponse:
    """
    Upload and index a GDD document using the existing RAG pipeline.
    """
    DEFAULT_DOCS_DIR.mkdir(parents=True, exist_ok=True)

    # Determine doc_id
    if docId and docId.strip():
        doc_id = docId.strip()
    else:
        doc_id = Path(file.filename).stem.replace(" ", "_")

    # Save file to docs/
    suffix = Path(file.filename).suffix or ".pdf"
    target_path = DEFAULT_DOCS_DIR / f"{doc_id}{suffix}"
    contents = await file.read()
    target_path.write_bytes(contents)
    status: str = "uploaded"
    message: str = f'File "{file.filename}" saved as "{target_path.name}".'

    # Index the document with RAG-Anything so it can be queried in chat
    try:
        provider = QwenProvider()
        llm_func = make_llm_model_func(provider)
        embedding_func = make_embedding_func(provider)

        await indexing.index_document(
            doc_path=target_path,
            doc_id=doc_id,
            llm_func=llm_func,
            embedding_func=embedding_func,
        )
        status = "indexed"
        message = f'File "{file.filename}" saved and indexed as "{target_path.name}".'
    except Exception as exc:  # pragma: no cover - runtime safety
        status = "error"
        message = (
            f'File "{file.filename}" was saved as "{target_path.name}", '
            f"but indexing failed: {exc}"
        )

    return JSONResponse(
        {
            "docId": doc_id,
            "status": status,
            "message": message,
        }
    )


@app.post("/documents/code")
async def upload_code(
    file: UploadFile = File(...),
    indexId: Optional[str] = Form(default=None),
) -> JSONResponse:
    """
    Upload a game code archive (e.g. ZIP).

    Currently just saves the file to `docs/` as a placeholder.
    """
    DEFAULT_DOCS_DIR.mkdir(parents=True, exist_ok=True)

    if indexId and indexId.strip():
        idx_id = indexId.strip()
    else:
        idx_id = Path(file.filename).stem.replace(" ", "_")

    suffix = Path(file.filename).suffix or ".zip"
    target_path = DEFAULT_DOCS_DIR / f"{idx_id}{suffix}"
    contents = await file.read()
    target_path.write_bytes(contents)

    return JSONResponse(
        {
            "indexId": idx_id,
            "status": "uploaded",
            "message": f'Code file "{file.filename}" saved as "{target_path.name}". (Indexing not yet implemented.)',
            "batchCount": 1,
        }
    )


@app.get("/gdd/{doc_id}/summary")
async def get_gdd_summary(doc_id: str) -> JSONResponse:
    """
    Return a placeholder GDD summary.

    Later this should call into the real RAG-based analysis.
    """
    now = datetime.utcnow().isoformat() + "Z"
    summary = {
        "docId": doc_id,
        "genre": "Action Strategy (placeholder)",
        "coreLoop": "Players engage in battles, upgrade units, and progress through matches. (placeholder)",
        "majorSystems": ["Combat", "Progression", "Matchmaking"],
        "playerInteractions": ["PvP", "Team Play"],
        "keyObjects": ["Tank", "Map"],
        "mapsAndModes": ["Deathmatch"],
        "specialMechanics": ["Placeholder mechanic"],
        "extractedAt": now,
    }
    return JSONResponse({"summary": summary})


class ChatRequestModel(BaseModel):
    workspaceId: str
    message: str
    useAllDocs: Optional[bool] = False
    docIds: Optional[List[str]] = None
    topK: Optional[int] = 6


@app.post("/chat")
async def chat(payload: ChatRequestModel) -> JSONResponse:
    """
    RAG-powered chat endpoint over indexed GDD documents.

    - If payload.useAllDocs is True, query across all indexed docs.
    - Else if payload.docIds is provided, query across those docs.
    - Else, fall back to using workspaceId as a single doc_id (if indexed).
    """
    status = load_doc_status()
    all_doc_ids = list(status.keys())

    # Determine which documents to query
    doc_ids: List[str] = []
    if payload.useAllDocs:
        doc_ids = all_doc_ids
    elif payload.docIds:
        # Filter to only known doc_ids
        doc_ids = [doc_id for doc_id in payload.docIds if doc_id in status]
    else:
        # Fallback: treat workspaceId as a doc_id if it has been indexed
        if payload.workspaceId in status:
            doc_ids = [payload.workspaceId]

    if not doc_ids:
        # No indexed documents found for this query; return helpful message
        now = datetime.utcnow().isoformat() + "Z"
        content = (
            "I couldn't find any indexed GDD documents to answer your question.\n\n"
            "- First, go to the Upload page and upload a GDD.\n"
            "- Give it a doc ID (for example: `default`).\n"
            "- After it finishes indexing, come back to Chat and ask again."
        )
        response_message = {
            "id": now,
            "role": "assistant",
            "content": content,
            "timestamp": now,
            "context": {"docIds": [], "chunks": []},
        }
        return JSONResponse({"message": response_message})

    provider = QwenProvider()
    top_k = payload.topK or 6

    # Run the chunk-based QA in a thread to avoid blocking the event loop
    def _run_qa():
        if len(doc_ids) == 1:
            return ask_with_chunks(
                doc_ids[0],
                payload.message,
                provider=provider,
                top_k=top_k,
            )
        else:
            return ask_across_docs(
                doc_ids,
                payload.message,
                provider=provider,
                top_k=top_k,
            )

    result = await asyncio.to_thread(_run_qa)

    now = datetime.utcnow().isoformat() + "Z"
    answer_text = result.get("answer", "").strip()
    context_chunks = result.get("context", [])

    # Map chunk records to the frontend's CodeChunk shape
    chunks_payload = [
        {
            "chunkId": str(chunk.get("chunk_id") or chunk.get("id") or idx),
            "content": chunk.get("content", ""),
            "score": float(chunk.get("score", 0.0)),
            "filePath": chunk.get("file_path") or result.get("file_path"),
        }
        for idx, chunk in enumerate(context_chunks)
    ]

    response_message = {
        "id": now,
        "role": "assistant",
        "content": answer_text or "I could not generate an answer from the indexed GDD.",
        "timestamp": now,
        "context": {"docIds": doc_ids, "chunks": chunks_payload},
    }
    return JSONResponse({"message": response_message})


if __name__ == "__main__":
    import uvicorn

    uvicorn.run("backend_api.main:app", host="0.0.0.0", port=8000, reload=True)


