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
from typing import Optional, List, Union
import asyncio

from fastapi import FastAPI, UploadFile, File, Form, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse, StreamingResponse
from pydantic import BaseModel
import json

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
from gdd_rag_backbone.gdd import (  # type: ignore
    extract_all_requirements,
    evaluate_all_requirements,
)
from gdd_rag_backbone.gdd.schemas import GddRequirement  # type: ignore


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
    Return all indexed documents from the RAG status store.
    """
    status = load_doc_status()
    documents = []

    for doc_id, meta in status.items():
        file_path = meta.get("file_path", "")
        file_name = meta.get("file_name") or Path(file_path).name or doc_id

        # -------------------------
        # Normalize document type
        # -------------------------
        # Heuristic:
        # - Explicit meta["doc_type"] wins if it is "code" or "gdd"
        # - IDs starting with "code_" are code
        # - Tank Online codebase batches (tank_online_codebase_batchXXX) are code
        # - Otherwise treat as GDD (design docs, maps, systems, etc.)
        raw_type = (meta.get("doc_type") or "").lower()
        if raw_type in {"code", "gdd"}:
            doc_type = raw_type
        elif doc_id.startswith("code_"):
            doc_type = "code"
        elif doc_id.startswith("tank_online_codebase_batch"):
            doc_type = "code"
        else:
            doc_type = "gdd"

        # -------------------------
        # Normalize status
        # -------------------------
        # Frontend expects:
        # - "uploaded" | "indexing" | "indexed" | "error"
        # Many RAG docs are stored as "processed" → map to "indexed".
        raw_status = (meta.get("status") or "").lower()
        if raw_status in {"indexed", "processed"}:
            norm_status = "indexed"
        elif raw_status in {"uploading", "indexing"}:
            norm_status = "indexing"
        elif raw_status in {"error", "failed"}:
            norm_status = "error"
        elif raw_status == "uploaded":
            norm_status = "uploaded"
        else:
            # Default to indexed so existing docs show up in the UI
            norm_status = "indexed"

        chunks = meta.get("chunks_list") or meta.get("chunks") or []

        documents.append(
            {
                "id": doc_id,
                "name": file_name,
                "type": doc_type,
                "filePath": file_path,
                "status": norm_status,
                "indexedAt": meta.get("updated_at") or meta.get("created_at"),
                "chunksCount": len(chunks),
            }
        )

    # Sort alphabetically for consistent UI
    documents.sort(key=lambda doc: doc["id"])
    return JSONResponse(documents)


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


@app.get("/gdd/{doc_id}/spec")
async def get_gdd_spec(doc_id: str) -> JSONResponse:
    """
    Extract all requirements, systems, objects, and logic rules from a GDD.
    This is the main function that summarizes what needs to be built.
    """
    status = load_doc_status()
    if doc_id not in status:
        raise HTTPException(status_code=404, detail=f"Document {doc_id} not found or not indexed")
    
    provider = QwenProvider()
    llm_func = make_llm_model_func(provider)
    
    # Extract requirements directly (extract_all_requirements is async)
    spec_data = await extract_all_requirements(doc_id, llm_func=llm_func)
    
    now = datetime.utcnow().isoformat() + "Z"
    spec = {
        "docId": doc_id,
        "objects": spec_data.get("objects", []),
        "systems": spec_data.get("systems", []),
        "logicRules": spec_data.get("logic_rules", []),
        "requirements": spec_data.get("requirements", []),
        "extractedAt": now,
    }
    return JSONResponse(spec)


class CoverageEvaluateRequest(BaseModel):
    docId: Union[str, List[str]]  # Single GDD or list of GDDs to extract requirements from
    codeIndexId: Union[str, List[str]]  # Single code index or list of code indices (all batches)
    topK: Optional[int] = 8


@app.post("/coverage/evaluate")
async def evaluate_coverage(payload: CoverageEvaluateRequest) -> JSONResponse:
    """
    Compare GDD requirements against codebase to see what's implemented.
    
    This is the main coverage evaluation function:
    1. Extracts requirements from GDD(s) - can be single or multiple GDDs
    2. Searches the entire codebase (all code batches) for each requirement
    3. Classifies each as implemented/not_implemented/partially_implemented
    
    Supports:
    - Multiple GDDs: Extract requirements from all selected GDDs
    - Multiple code batches: Search across entire codebase
    """
    status = load_doc_status()
    
    # Normalize to lists
    gdd_ids = payload.docId if isinstance(payload.docId, list) else [payload.docId]
    code_indices = payload.codeIndexId if isinstance(payload.codeIndexId, list) else [payload.codeIndexId]
    
    # Verify all GDD documents exist
    for gdd_id in gdd_ids:
        if gdd_id not in status:
            raise HTTPException(status_code=404, detail=f"GDD document {gdd_id} not found")
    
    # Verify all code indices exist
    for code_id in code_indices:
        if code_id not in status:
            raise HTTPException(status_code=404, detail=f"Code index {code_id} not found")
    
    provider = QwenProvider()
    llm_func = make_llm_model_func(provider)
    
    # Step 1: Extract requirements from ALL GDDs and merge them
    all_requirements_data: List[dict] = []
    skipped_gdds: List[str] = []
    for gdd_id in gdd_ids:
        gdd_meta = status.get(gdd_id, {})
        gdd_chunks = gdd_meta.get("chunks_list") or gdd_meta.get("chunks") or []
        if not gdd_chunks:
            skipped_gdds.append(f"{gdd_id}: document has not been indexed yet")
            continue
        try:
            spec_data = await extract_all_requirements(gdd_id, llm_func=llm_func)
        except ValueError as exc:  # e.g. no context found
            skipped_gdds.append(f"{gdd_id}: {exc}")
            continue

        requirements_data = spec_data.get("requirements", [])
        # Tag each requirement with its source GDD
        for req in requirements_data:
            req["source_gdd"] = gdd_id
        all_requirements_data.extend(requirements_data)
    
    # Convert dicts to GddRequirement objects (from all GDDs)
    requirements = []
    seen_ids = set()  # Deduplicate requirements with same ID
    for req_dict in all_requirements_data:
        try:
            req_id = req_dict.get("id", "")
            # Skip duplicates (same requirement from multiple GDDs)
            if req_id in seen_ids:
                continue
            seen_ids.add(req_id)
            
            req = GddRequirement(
                id=req_id,
                title=req_dict.get("title", req_dict.get("summary", "")),
                description=req_dict.get("description", req_dict.get("details", "")),
                category=req_dict.get("category"),
                priority=req_dict.get("priority"),
                status=req_dict.get("status"),
                acceptance_criteria=req_dict.get("acceptance_criteria"),
                related_objects=req_dict.get("related_objects", []),
                related_systems=req_dict.get("related_systems", []),
                source_note=req_dict.get("source_note"),
            )
            requirements.append(req)
        except Exception as e:
            # Skip invalid requirements
            continue
    
    # ------------------------------------------------------------------
    # TEMP: Limit number of requirements for faster demo runs
    # ------------------------------------------------------------------
    # To keep evaluation responsive on a laptop, we cap the number of
    # requirements we evaluate per request. This still gives a realistic
    # coverage sample without waiting many minutes.
    MAX_REQUIREMENTS_FOR_DEMO = 20
    if len(requirements) > MAX_REQUIREMENTS_FOR_DEMO:
        requirements = requirements[:MAX_REQUIREMENTS_FOR_DEMO]

    if not requirements:
        gdd_list_str = ", ".join(skipped_gdds) if skipped_gdds else ", ".join(gdd_ids)
        raise HTTPException(
            status_code=400,
            detail=f"No requirements found in GDD(s): {gdd_list_str}. Make sure the documents have been indexed."
        )
    
    # Step 2: Evaluate each requirement against the ENTIRE codebase (all batches)
    # Use the first GDD ID for report naming, but search across all code batches
    primary_gdd_id = gdd_ids[0]
    report_path = await evaluate_all_requirements(
        primary_gdd_id,
        code_indices,  # Pass all code batches
        requirements,
        provider=provider,
        top_k=payload.topK or 8,
    )
    
    # Step 3: Load and format the report
    report_data = json.loads(report_path.read_text())
    results = report_data.get("results", [])
    
    # Convert to frontend format
    coverage_results = []
    implemented_count = 0
    partially_implemented_count = 0
    not_implemented_count = 0
    error_count = 0
    
    for result in results:
        req_id = result.get("requirement_id", "")
        status = result.get("status", "error")
        
        # Find the original requirement for name/type
        req = next((r for r in requirements if r.id == req_id), None)
        item_name = req.title if req else req_id
        item_type = "requirement"
        
        if status == "implemented":
            implemented_count += 1
        elif status == "partially_implemented":
            partially_implemented_count += 1
        elif status == "not_implemented":
            not_implemented_count += 1
        else:
            error_count += 1
        
        evidence = result.get("evidence", [])
        retrieved_chunks = result.get("retrieved_chunks", [])
        
        coverage_results.append({
            "itemId": req_id,
            "itemType": item_type,
            "itemName": item_name,
            "status": status,
            "evidence": evidence,
            "retrievedChunks": [
                {
                    "chunkId": str(chunk.get("chunk_id", "")),
                    "content": chunk.get("content", ""),
                    "score": float(chunk.get("score", 0.0)),
                    "filePath": chunk.get("file_path"),
                }
                for chunk in retrieved_chunks
            ],
        })
    
    now = datetime.utcnow().isoformat() + "Z"
    # Format IDs for display
    gdd_id_display = ", ".join(gdd_ids) if len(gdd_ids) > 1 else gdd_ids[0]
    code_id_display = ", ".join(code_indices) if len(code_indices) > 1 else code_indices[0]
    report = {
        "docId": gdd_id_display,
        "codeIndexId": code_id_display,
        "generatedAt": now,
        "summary": {
            "totalItems": len(coverage_results),
            "implemented": implemented_count,
            "partiallyImplemented": partially_implemented_count,
            "notImplemented": not_implemented_count,
            "errors": error_count,
        },
        "results": coverage_results,
    }
    
    response_payload = {"report": report}
    if skipped_gdds:
        response_payload["warnings"] = skipped_gdds

    return JSONResponse(response_payload)


class ChatRequestModel(BaseModel):
    workspaceId: str
    message: str
    useAllDocs: Optional[bool] = False
    docIds: Optional[List[str]] = None
    topK: Optional[int] = 6


@app.post("/chat/stream")
async def chat_stream(payload: ChatRequestModel):
    """
    Streaming RAG-powered chat endpoint.
    Returns Server-Sent Events (SSE) stream of tokens.
    """
    try:
        from openai import OpenAI
    except ImportError:
        raise HTTPException(status_code=500, detail="OpenAI package required for streaming")
    
    from gdd_rag_backbone.config import QWEN_API_KEY, QWEN_BASE_URL, DEFAULT_LLM_MODEL
    
    status = load_doc_status()
    all_doc_ids = list(status.keys())
    
    # Determine which documents to query
    doc_ids: List[str] = []
    if payload.useAllDocs:
        doc_ids = all_doc_ids[:10] if len(all_doc_ids) > 10 else all_doc_ids
    elif payload.docIds:
        doc_ids = [doc_id for doc_id in payload.docIds if doc_id in status]
    else:
        if payload.workspaceId in status:
            doc_ids = [payload.workspaceId]
    
    if not doc_ids:
        async def error_stream():
            yield f"data: {json.dumps({'type': 'error', 'content': 'No indexed documents found.'})}\n\n"
        return StreamingResponse(error_stream(), media_type="text/event-stream")
    
    # Get context chunks (non-streaming part)
    provider = QwenProvider()
    top_k = payload.topK or 4
    
    def _get_context():
        if len(doc_ids) == 1:
            from gdd_rag_backbone.rag_backend.chunk_qa import ask_with_chunks
            result = ask_with_chunks(
                doc_ids[0],
                payload.message,
                provider=provider,
                top_k=top_k,
            )
        else:
            from gdd_rag_backbone.rag_backend.chunk_qa import ask_across_docs
            result = ask_across_docs(
                doc_ids,
                payload.message,
                provider=provider,
                top_k=top_k,
            )
        return result
    
    # Get context in background
    result = await asyncio.to_thread(_get_context)
    context_chunks = result.get("context", [])
    
    # Build prompt with context
    context_text = "\n\n".join([chunk.get("content", "") for chunk in context_chunks[:top_k]])
    system_prompt = "You are a helpful assistant answering questions about game design documents. Use only the provided context to answer."
    user_prompt = f"Context from game design documents:\n\n{context_text}\n\nQuestion: {payload.message}\n\nAnswer:"
    
    # Stream LLM response
    async def stream_response():
        try:
            client = OpenAI(
                api_key=QWEN_API_KEY,
                base_url=QWEN_BASE_URL,
            )
            
            messages = [
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": user_prompt}
            ]
            
            # Send context info first
            chunks_payload = [
                {
                    "chunkId": str(chunk.get("chunk_id") or chunk.get("id") or idx),
                    "content": chunk.get("content", ""),
                    "score": float(chunk.get("score", 0.0)),
                }
                for idx, chunk in enumerate(context_chunks)
            ]
            yield f"data: {json.dumps({'type': 'context', 'docIds': doc_ids, 'chunks': chunks_payload})}\n\n"
            
            # Stream LLM tokens
            stream = client.chat.completions.create(
                model=DEFAULT_LLM_MODEL,
                messages=messages,
                stream=True,
                temperature=0.3,
            )
            
            for chunk in stream:
                if chunk.choices[0].delta.content:
                    content = chunk.choices[0].delta.content
                    yield f"data: {json.dumps({'type': 'token', 'content': content})}\n\n"
            
            # Send completion
            now = datetime.utcnow().isoformat() + "Z"
            yield f"data: {json.dumps({'type': 'done', 'timestamp': now})}\n\n"
            
        except Exception as e:
            yield f"data: {json.dumps({'type': 'error', 'content': str(e)})}\n\n"
    
    return StreamingResponse(stream_response(), media_type="text/event-stream")


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
        # Limit to first 10 docs for faster responses when querying "all"
        # Users can specify specific docIds if they want more
        doc_ids = all_doc_ids[:10] if len(all_doc_ids) > 10 else all_doc_ids
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
    top_k = payload.topK or 4  # Default to 4 for faster responses

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


