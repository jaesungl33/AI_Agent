# Backend Architecture and Refactor Notes

This document highlights major issues in the previous codebase and outlines the refactored structure.

## Key Problems Identified

- **Monolithic `main.py`** mixed API wiring, business logic, and background processing, making it hard to test or extend.
- **Implicit global state**: service objects were instantiated at import time without clear dependency boundaries or lifecycle management.
- **Weak configuration handling**: environment variables were read ad-hoc with no validation, increasing runtime failure risk.
- **Coupled routes and services**: retrieval, ingestion, and generation logic lived directly inside route handlers, preventing reuse and unit testing.
- **Background job ambiguity**: indexing jobs lacked a centralized processor and coherent routing.

## New Structure (FastAPI)

```
backend/
├─ app/
│  ├─ application.py   # App factory, CORS, router registration, lifespan
│  ├─ config.py        # Pydantic settings + env validation
│  ├─ dependencies.py  # Service singletons (DB, storage, indexer, retriever, generator)
│  ├─ schemas.py       # Shared Pydantic models
│  └─ routes/
│     ├─ health.py     # / and /health
│     ├─ ingest.py     # /ingest/docs, /ingest/code + background indexing
│     ├─ admin.py      # /admin/run-job, /admin/jobs
│     └─ chat.py       # /chat, /extract/* and search-mode detection
├─ database.py         # Supabase database wrapper
├─ storage.py          # Supabase storage wrapper
├─ indexing.py         # PDF/code indexing pipelines
├─ retrieval.py        # Hybrid retrieval + reranking
├─ generation.py       # Answer generation + grounding
└─ main.py             # Entry point using the app factory
```

## Dependency and Lifecycle Improvements

- **App factory** (`backend.app.application.create_app`) centralizes middleware, routers, and startup/shutdown.
- **Explicit dependencies** (`backend.app.dependencies`) ensure services are shared consistently and can be mocked.
- **Validated configuration** (`backend.app.config.Settings`) enforces required environment variables at startup.

## Next Steps

- Add unit tests per router and service layer with fixtures for Supabase and embedding/model mocks.
- Implement SHA-256 computation for uploads and de-duplication.
- Replace placeholder embedding vectors when the model is unavailable with a deterministic stub for testing.
- Add structured logging and request IDs across routers.
- Harden background job processing with a task queue (e.g., Dramatiq/RQ) and retries.
