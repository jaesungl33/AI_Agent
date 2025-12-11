# Code Reorganization Plan

## Current Issues
1. Root directory cluttered with many files
2. Scripts scattered across multiple locations
3. Data directories at root level
4. Documentation files mixed with code
5. Test codebase at root

## Proposed Structure

```
AI_Agent/
├── src/                          # Core library (rename from gdd_rag_backbone)
│   ├── gdd_rag_backbone/
│   │   ├── gdd/
│   │   ├── llm_providers/
│   │   ├── rag_backend/
│   │   └── workspace/
│   └── tests/
│
├── backend/                      # Backend API (rename from backend_api)
│   ├── main.py
│   └── requirements.txt
│
├── frontend/                      # Frontend (keep as is)
│
├── scripts/                      # All scripts consolidated
│   ├── migration/
│   ├── indexing/
│   ├── testing/
│   └── utilities/
│
├── data/                         # Runtime data (move from root)
│   ├── rag_storage/             # Legacy storage
│   ├── output/                  # Processing output
│   ├── reports/                  # Coverage reports
│   └── workspaces/              # Workspace data
│
├── docs/                         # Documentation
│   ├── architecture/
│   ├── guides/
│   └── api/
│
├── tests/                        # Test codebase
│   └── tank_online_1-dev/
│
├── .github/                      # CI/CD workflows
│
├── README.md
├── requirements.txt
└── .env.example
```

## Migration Steps
1. Rename `gdd_rag_backbone/` → `src/gdd_rag_backbone/`
2. Rename `backend_api/` → `backend/`
3. Consolidate scripts into `scripts/` with subdirectories
4. Move data directories to `data/`
5. Move documentation to `docs/`
6. Move test codebase to `tests/`
7. Update all imports and references
8. Update README and documentation

