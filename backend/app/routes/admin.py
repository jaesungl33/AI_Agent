"""Administrative job management routes."""

from fastapi import APIRouter, Depends, HTTPException

from backend.app.dependencies import get_database, get_indexer, get_storage
from backend.app.routes.ingest import process_indexing_job
from backend.app.schemas import JobResponse
from backend.database import Database
from backend.indexing import Indexer
from backend.storage import Storage

router = APIRouter(prefix="/admin", tags=["admin"])


@router.post("/run-job", response_model=JobResponse, summary="Run next queued job")
async def run_job(
    db: Database = Depends(get_database),
    storage: Storage = Depends(get_storage),
    indexer: Indexer = Depends(get_indexer),
):
    """Run the next queued indexing job."""
    try:
        job = await db.get_next_queued_job()
        if not job:
            return JobResponse(status="empty")

        await db.update_job_status(job["id"], "running")
        try:
            await process_indexing_job(job["id"], db, storage, indexer, update_status=True)
            return JobResponse(
                status="done", job_id=job["id"], document_id=job["document_id"]
            )
        except Exception as exc:
            return JobResponse(
                status="failed",
                job_id=job["id"],
                document_id=job["document_id"],
                error=str(exc),
            )
    except Exception as exc:
        raise HTTPException(status_code=500, detail=f"Failed to run job: {exc}")


@router.get("/jobs", summary="List recent jobs")
async def get_jobs(limit: int = 20, db: Database = Depends(get_database)):
    """Get recent jobs."""
    try:
        jobs = await db.get_recent_jobs(limit)
        return {"jobs": jobs}
    except Exception as exc:
        raise HTTPException(status_code=500, detail=f"Failed to get jobs: {exc}")


__all__ = ["router"]
