"""Health and root routes."""

from datetime import datetime

from fastapi import APIRouter, Depends

from backend.app.dependencies import get_database, get_storage
from backend.app.schemas import HealthResponse
from backend.database import Database
from backend.storage import Storage

router = APIRouter()


@router.get("/", summary="Root health ping")
async def root() -> dict:
    """Lightweight health response."""
    return {"status": "ok", "service": "AI Agent RAG Backend"}


@router.get("/health", response_model=HealthResponse, summary="Detailed health check")
async def health(
    db: Database = Depends(get_database), storage: Storage = Depends(get_storage)
) -> HealthResponse:
    """Return status of dependent services."""
    return HealthResponse(
        status="ok",
        timestamp=datetime.utcnow(),
        version="1.0.0",
        services={"database": await db.health_check(), "storage": storage.health_check()},
    )


__all__ = ["router"]
