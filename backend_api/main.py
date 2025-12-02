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

from fastapi import FastAPI, UploadFile, File, Form
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
from pydantic import BaseModel

# Ensure project root is on PYTHONPATH so we can import existing modules later
PROJECT_ROOT = Path(__file__).resolve().parents[1]
if str(PROJECT_ROOT) not in sys.path:
    sys.path.insert(0, str(PROJECT_ROOT))

from gdd_rag_backbone.config import DEFAULT_DOCS_DIR  # type: ignore


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


@app.post("/documents/gdd")
async def upload_gdd(
    file: UploadFile = File(...),
    docId: Optional[str] = Form(default=None),
) -> JSONResponse:
    """
    Upload a GDD document.

    For now this just saves the file into `docs/` and returns a status.
    Later you can plug in `indexing.index_document` and spec extraction.
    """
    DEFAULT_DOCS_DIR.mkdir(parents=True, exist_ok=True)

    # Determine doc_id
    if docId and docId.strip():
        doc_id = docId.strip()
    else:
        doc_id = Path(file.filename).stem.replace(" ", "_")

    # Save file
    suffix = Path(file.filename).suffix or ".pdf"
    target_path = DEFAULT_DOCS_DIR / f"{doc_id}{suffix}"
    contents = await file.read()
    target_path.write_bytes(contents)

    # Placeholder: we don't run full indexing here yet
    return JSONResponse(
        {
            "docId": doc_id,
            "status": "uploaded",  # or "indexing"/"indexed" when wired to RAG
            "message": f'File "{file.filename}" saved as "{target_path.name}". (Indexing not yet implemented.)',
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
    Simple echo-style chat endpoint.

    This replaces the mock client with a real HTTP endpoint, so the frontend
    sees it as a real backend. Later you can:
    - Call `ask_across_docs` or `ask_with_chunks` here
    - Use `workspaceId` and `docIds` to scope queries
    """
    now = datetime.utcnow().isoformat() + "Z"
    doc_ids = payload.docIds or []
    content = (
        f'You said: "{payload.message}".\n\n'
        "This response is coming from the FastAPI backend, not the mock client.\n"
        "Once wired to the RAG pipeline, this endpoint will answer using your indexed GDDs and code."
    )
    response_message = {
        "id": now,
        "role": "assistant",
        "content": content,
        "timestamp": now,
        "context": {"docIds": doc_ids, "chunks": []},
    }
    return JSONResponse({"message": response_message})


if __name__ == "__main__":
    import uvicorn

    uvicorn.run("backend_api.main:app", host="0.0.0.0", port=8000, reload=True)


