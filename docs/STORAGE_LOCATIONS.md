# Index Storage Locations

## Overview

Indexed data is stored in multiple locations depending on whether you're using the legacy global storage or the new workspace-based storage.

## Storage Locations

### 1. Legacy Global Storage (Default)

**Location:** `data/rag_storage/`

This is the default storage location used when no workspace is specified. Contains:

```
data/rag_storage/
├── kv_store_doc_status.json          # Document metadata and status
├── kv_store_text_chunks.json         # Text chunks from documents
├── kv_store_entity_chunks.json       # Entity chunks
├── kv_store_relation_chunks.json     # Relation chunks
├── kv_store_full_docs.json           # Full document content
├── kv_store_full_entities.json       # Full entity data
├── kv_store_full_relations.json     # Full relation data
├── kv_store_llm_response_cache.json  # LLM response cache
├── kv_store_parse_cache.json         # Parse cache
├── vdb_chunks.json                   # Vector embeddings for chunks
├── vdb_entities.json                  # Vector embeddings for entities
└── vdb_relationships.json             # Vector embeddings for relations
```

**Configuration:** Set in `src/gdd_rag_backbone/config.py`:
```python
DEFAULT_WORKING_DIR = PROJECT_ROOT / "data" / "rag_storage"
```

### 2. Workspace-Scoped Storage

**Location:** `data/workspaces/{workspace_id}/storage/`

Each workspace has its own isolated storage:

```
data/workspaces/{workspace_id}/
├── documents/                         # Source documents
│   ├── gdd/                          # GDD documents
│   └── code/                         # Code files
├── storage/                          # RAG storage
│   ├── status.json                   # Document status (workspace-scoped)
│   ├── indices/                      # Index files
│   │   ├── chunks.json
│   │   ├── entities.json
│   │   ├── full_docs.json
│   │   └── ...
│   ├── vectors/                     # Vector embeddings
│   │   ├── chunks.json
│   │   ├── entities.json
│   │   └── relations.json
│   └── cache/                       # Cache files
│       ├── llm_cache.json
│       └── parse_cache.json
├── behavior_cache/                   # Behavior index cache
│   └── {code_id}_behaviors.json
├── reports/                          # Coverage reports
│   └── coverage_checks/
└── output/                           # Processing output
```

**Configuration:** Managed by `src/gdd_rag_backbone/workspace/storage.py`

### 3. Behavior Index Cache

**Location:** 
- Legacy: `rag_storage/behavior_indices/` (if using old system)
- Workspace: `data/workspaces/{workspace_id}/behavior_cache/`

Contains behavior descriptions extracted from code:
```
{code_index_id}_behaviors.json
```

### 4. Coverage Reports

**Location:**
- Legacy: `data/reports/coverage_checks/`
- Workspace: `data/workspaces/{workspace_id}/reports/coverage_checks/`

Contains coverage evaluation results:
```
{gdd_id}_{code_id}_behavior_coverage.json
```

## File Sizes (Current)

Based on current storage:

| File | Size | Description |
|------|------|-------------|
| `kv_store_llm_response_cache.json` | ~48 MB | LLM response cache |
| `vdb_chunks.json` | ~12 MB | Vector embeddings for chunks |
| `kv_store_text_chunks.json` | ~4 MB | Text chunks |
| `kv_store_parse_cache.json` | ~3.9 MB | Parse cache |
| `kv_store_full_docs.json` | ~2.2 MB | Full document content |
| `kv_store_entity_chunks.json` | ~1.5 MB | Entity chunks |
| `kv_store_relation_chunks.json` | ~1.7 MB | Relation chunks |
| `kv_store_doc_status.json` | ~482 KB | Document status |
| `kv_store_full_relations.json` | ~804 KB | Full relations |
| `kv_store_full_entities.json` | ~134 KB | Full entities |

**Total:** ~75 MB of indexed data

## How to Find Your Indexes

### Check Legacy Storage
```bash
ls -lh data/rag_storage/*.json
```

### Check Workspace Storage
```bash
# List all workspaces
ls data/workspaces/

# Check specific workspace
ls -lh data/workspaces/{workspace_id}/storage/
```

### Check Document Status
```python
from gdd_rag_backbone.rag_backend.chunk_qa import load_doc_status

# Legacy storage
status = load_doc_status()

# Workspace storage
status = load_doc_status(workspace_id="your_workspace_id")
```

## Migration

If you want to migrate from legacy storage to workspace storage:

```bash
python scripts/migration/migrate_to_workspaces.py --live
```

This will:
1. Create a "default" workspace
2. Move all data from `data/rag_storage/` to `data/workspaces/default/storage/`
3. Preserve all indexes and cache

## Notes

- **Legacy storage** is still used if no workspace is specified (backward compatibility)
- **Workspace storage** provides isolation between projects
- Both storage systems use the same file structure
- Index files are JSON format for easy inspection and backup

