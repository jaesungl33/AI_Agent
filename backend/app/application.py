"""FastAPI application factory."""

from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from backend.app.config import get_settings
from backend.app.dependencies import get_database
from backend.app.routes import admin, chat, health, ingest
from backend.database import Database


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Handle application startup and shutdown."""
    db: Database = get_database()
    await db.connect()
    yield
    await db.disconnect()


def create_app() -> FastAPI:
    """Create and configure the FastAPI app."""
    settings = get_settings()

    app = FastAPI(
        title="AI Agent - Chat-First RAG",
        description="Retrieval-Augmented Generation system for code and documentation analysis",
        version="1.0.0",
        lifespan=lifespan,
    )

    app.add_middleware(
        CORSMiddleware,
        allow_origins=settings.allowed_origins,
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    app.include_router(health.router)
    app.include_router(ingest.router)
    app.include_router(admin.router)
    app.include_router(chat.router)

    return app


__all__ = ["create_app"]
