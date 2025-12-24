"""Document ingestion endpoints."""

import uuid

from fastapi import APIRouter, BackgroundTasks, Depends, File, HTTPException, UploadFile

from backend.app.dependencies import get_database, get_indexer, get_storage
from backend.app.schemas import DocumentResponse
from backend.database import Database
from backend.indexing import Indexer
from backend.storage import Storage

router = APIRouter(prefix="/ingest", tags=["ingest"])


@router.post("/docs", response_model=DocumentResponse, summary="Ingest PDF document")
async def ingest_docs(
    background_tasks: BackgroundTasks,
    file: UploadFile = File(...),
    db: Database = Depends(get_database),
    storage: Storage = Depends(get_storage),
    indexer: Indexer = Depends(get_indexer),
):
    """Upload and queue a PDF document for indexing."""
    if not file.filename.lower().endswith(".pdf"):
        raise HTTPException(status_code=400, detail="Only PDF files are supported")

    document_id = str(uuid.uuid4())
    job_id = str(uuid.uuid4())

    try:
        file_content = await file.read()
        storage_path = await storage.upload_file(
            file_content, f"docs/{document_id}/{file.filename}"
        )

        await db.create_document(
            document_id=document_id,
            doc_type="docs",
            filename=file.filename,
            storage_path=storage_path,
            sha256="",  # TODO: compute SHA256
        )

        await db.create_job(job_id=job_id, job_type="index_docs", document_id=document_id)

        background_tasks.add_task(process_indexing_job, job_id, db, storage, indexer)

        return DocumentResponse(document_id=document_id, job_id=job_id, status="queued")
    except Exception as exc:
        raise HTTPException(status_code=500, detail=f"Failed to ingest document: {exc}")


@router.post("/code", response_model=DocumentResponse, summary="Ingest code repository")
async def ingest_code(
    background_tasks: BackgroundTasks,
    file: UploadFile = File(...),
    db: Database = Depends(get_database),
    storage: Storage = Depends(get_storage),
    indexer: Indexer = Depends(get_indexer),
):
    """Upload and queue a code repository (ZIP) for indexing."""
    if not file.filename.lower().endswith(".zip"):
        raise HTTPException(status_code=400, detail="Only ZIP files are supported")

    document_id = str(uuid.uuid4())
    job_id = str(uuid.uuid4())

    try:
        file_content = await file.read()
        storage_path = await storage.upload_file(
            file_content, f"code/{document_id}/{file.filename}"
        )

        await db.create_document(
            document_id=document_id,
            doc_type="code",
            filename=file.filename,
            storage_path=storage_path,
            sha256="",  # TODO: compute SHA256
        )

        await db.create_job(job_id=job_id, job_type="index_code", document_id=document_id)

        background_tasks.add_task(process_indexing_job, job_id, db, storage, indexer)

        return DocumentResponse(document_id=document_id, job_id=job_id, status="queued")
    except Exception as exc:
        raise HTTPException(status_code=500, detail=f"Failed to ingest code: {exc}")


async def process_indexing_job(
    job_id: str, db: Database, storage: Storage, indexer: Indexer, update_status: bool = True
):
    """Background processor for indexing jobs."""
    try:
        job = await db.get_job(job_id)
        if not job:
            return

        document = await db.get_document(job["document_id"])
        if not document:
            return

        file_content = await storage.download_file(document["storage_path"])

        if job["job_type"] == "index_docs":
            await indexer.index_pdf(document, file_content)
        elif job["job_type"] == "index_code":
            await indexer.index_zip(document, file_content)

        if update_status:
            await db.update_job_status(job_id, "done")
    except Exception as exc:
        if update_status:
            await db.update_job_status(job_id, "failed", str(exc))
        raise


__all__ = ["router", "process_indexing_job"]
