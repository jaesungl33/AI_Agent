#!/usr/bin/env python3
"""Entry point for the FastAPI backend."""

import os

import uvicorn

from backend.app.application import create_app
from backend.app.config import get_settings

app = create_app()


def main() -> None:
    """Launch the FastAPI server."""
    settings = get_settings()
    uvicorn.run(
        "backend.main:app",
        host="0.0.0.0",
        port=settings.port,
        reload=settings.debug,
    )


if __name__ == "__main__":
    main()
