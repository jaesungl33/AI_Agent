# Project Structure

## Overview

The codebase has been reorganized for better maintainability and clarity.

## Directory Structure

```
AI_Agent/
├── src/                          # Core library source code
│   └── gdd_rag_backbone/
│       ├── gdd/                  # GDD processing modules
│       │   ├── analysis.py
│       │   ├── behavior_indexing.py
│       │   ├── behavior_matching.py
│       │   ├── extraction.py
│       │   ├── requirement_matching.py
│       │   └── schemas.py
│       ├── llm_providers/        # LLM provider implementations
│       │   ├── base.py
│       │   ├── qwen_provider.py
│       │   └── vertex_provider.py
│       ├── rag_backend/          # RAG indexing and querying
│       │   ├── chunk_qa.py
│       │   ├── indexing.py
│       │   ├── rag_config.py
│       │   └── query_engine.py
│       ├── workspace/            # Workspace management
│       │   ├── manager.py
│       │   └── storage.py
│       └── config.py             # Global configuration
│
├── backend/                      # FastAPI backend server
│   ├── main.py                   # Main API application
│   └── requirements.txt         # Backend dependencies
│
├── frontend/                     # Next.js frontend application
│   ├── app/                     # Next.js app router
│   │   ├── api/                 # API routes (proxies)
│   │   ├── chat/                # Chat page
│   │   ├── coverage/            # Coverage page
│   │   ├── documents/           # Documents page
│   │   ├── upload/              # Upload page
│   │   └── workspace/          # Workspace page
│   ├── components/              # React components
│   │   ├── chat/               # Chat components
│   │   ├── documents/          # Document components
│   │   ├── layout/             # Layout components
│   │   └── ui/                 # UI components
│   └── lib/                     # Utilities
│       ├── api/                # API client
│       └── contexts/           # React contexts
│
├── scripts/                      # Utility scripts
│   ├── migration/              # Data migration scripts
│   │   ├── migrate_to_workspaces.py
│   │   ├── merge_duplicates.py
│   │   └── reindex_all_docs.py
│   ├── indexing/               # Indexing utilities
│   ├── testing/                # Test scripts
│   │   ├── test_qwen.py
│   │   └── test_all_docs.py
│   └── utilities/              # Helper scripts
│       └── start_backend.sh
│
├── data/                         # Runtime data (gitignored)
│   ├── rag_storage/            # RAG indices and vectors
│   ├── output/                 # Processing output
│   ├── reports/                # Coverage reports
│   ├── workspaces/             # Workspace data
│   └── gdd_documents/          # GDD source files
│
├── docs/                         # Documentation
│   ├── architecture/            # Architecture documentation
│   │   ├── ARCHITECTURE_PROPOSAL.md
│   │   ├── BEHAVIOR_BASED_APPROACH.md
│   │   └── WORKSPACE_*.md
│   ├── guides/                  # User guides
│   └── api/                     # API documentation
│
├── tests/                        # Test files
│   ├── test_extraction.py
│   ├── test_parsing.py
│   ├── test_retrieval.py
│   ├── test_schemas.py
│   └── tank_online_1-dev/       # Test codebase
│
├── README.md                     # Main README
├── requirements.txt              # Python dependencies
├── Render.yaml                   # Render deployment config
└── .gitignore                   # Git ignore rules
```

## Key Changes

### 1. Source Code Organization
- Core library moved to `src/gdd_rag_backbone/`
- Backend API renamed from `backend_api/` to `backend/`
- All source code is now clearly separated from data and scripts

### 2. Data Organization
- All runtime data moved to `data/` directory
- Subdirectories for different data types:
  - `rag_storage/` - RAG indices and vectors
  - `output/` - Processing output
  - `reports/` - Coverage reports
  - `workspaces/` - Workspace data
  - `gdd_documents/` - GDD source files

### 3. Scripts Organization
- All scripts consolidated in `scripts/`
- Organized by purpose:
  - `migration/` - Data migration
  - `indexing/` - Indexing utilities
  - `testing/` - Test scripts
  - `utilities/` - Helper scripts

### 4. Documentation
- All documentation in `docs/`
- Organized by type:
  - `architecture/` - Architecture docs
  - `guides/` - User guides
  - `api/` - API documentation

## Import Paths

All imports remain the same:
```python
from gdd_rag_backbone.config import ...
from gdd_rag_backbone.gdd import ...
```

The `src/` directory is automatically added to PYTHONPATH in:
- `backend/main.py` - For the FastAPI server
- Scripts should add it manually if needed

## Configuration

### Path Updates
All paths in `src/gdd_rag_backbone/config.py` have been updated:
- `DEFAULT_WORKING_DIR` → `data/rag_storage/`
- `DEFAULT_OUTPUT_DIR` → `data/output/`
- `DEFAULT_DOCS_DIR` → `data/gdd_documents/`

### Environment Variables
No changes needed - all environment variables remain the same.

## Deployment

### Render
- `Render.yaml` updated to use `backend/` instead of `backend_api/`
- Build commands updated accordingly

### Vercel
- Frontend structure unchanged
- No deployment changes needed

## Benefits

1. **Clear Separation**: Source code, data, scripts, and docs are clearly separated
2. **Better Navigation**: Easier to find files
3. **Scalability**: Structure supports future growth
4. **Maintainability**: Easier to understand and modify
5. **Standard Structure**: Follows Python/Node.js best practices

