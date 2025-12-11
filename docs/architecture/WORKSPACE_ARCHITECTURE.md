# Workspace Architecture - Implementation Guide

## Visual Structure

```
┌─────────────────────────────────────────────────────────────┐
│                    User Interface Layer                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │  Workspace   │  │  Documents  │  │  Coverage   │       │
│  │  Manager     │  │  Upload     │  │  Evaluation │       │
│  └──────────────┘  └──────────────┘  └──────────────┘       │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    API Layer (FastAPI)                      │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  /workspaces/{id}/documents                          │  │
│  │  /workspaces/{id}/coverage/evaluate                  │  │
│  │  /workspaces/{id}/chat                               │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│              Workspace Manager Layer                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  Create      │  │  Get         │  │  Delete      │      │
│  │  Workspace   │  │  Workspace   │  │  Workspace   │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│              Workspace Storage Layer                          │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  workspace_id → isolated storage paths                │  │
│  │  - documents/                                         │  │
│  │  - storage/indices/                                   │  │
│  │  - behavior_cache/                                    │  │
│  │  - reports/                                           │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│              RAG Processing Layer                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  Indexing    │  │  Querying    │  │  Evaluation  │      │
│  │  (scoped)   │  │  (scoped)    │  │  (scoped)    │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
```

## Data Flow

### Upload Document Flow
```
User Uploads File
    ↓
API: POST /workspaces/{id}/documents/gdd
    ↓
WorkspaceStorage.get_documents_dir() → workspace/{id}/documents/gdd/
    ↓
Save file to workspace documents/
    ↓
WorkspaceStorage.get_storage_dir() → workspace/{id}/storage/
    ↓
Index document (scoped to workspace storage)
    ↓
Update workspace/{id}/storage/status.json
```

### Query Flow
```
User Queries
    ↓
API: POST /workspaces/{id}/chat
    ↓
WorkspaceStorage.get_storage_dir() → workspace/{id}/storage/
    ↓
Load chunks from workspace storage
    ↓
Query RAG (scoped to workspace)
    ↓
Return results
```

## Workspace Isolation

Each workspace has:
- **Own documents**: `workspaces/{id}/documents/`
- **Own indices**: `workspaces/{id}/storage/indices/`
- **Own cache**: `workspaces/{id}/storage/cache/`
- **Own reports**: `workspaces/{id}/reports/`
- **Own config**: `workspaces/{id}/workspace.json`

No cross-contamination between workspaces.

## Example Workspace Structure

```
workspaces/
├── tank_war_v1/
│   ├── workspace.json
│   ├── documents/
│   │   ├── gdd/
│   │   │   ├── garage_design.pdf
│   │   │   └── combat_system.pdf
│   │   └── code/
│   │       └── codebase_batch001.zip
│   ├── storage/
│   │   ├── indices/
│   │   │   ├── chunks.json
│   │   │   └── vectors/
│   │   ├── cache/
│   │   └── status.json
│   ├── behavior_cache/
│   └── reports/
│
└── tank_war_v2/
    ├── workspace.json
    ├── documents/
    └── storage/
        └── ...
```

## API Usage Examples

### Create Workspace
```bash
POST /workspaces
{
  "name": "Tank War v2",
  "description": "New version of Tank War"
}

Response:
{
  "id": "tank_war_v2",
  "name": "Tank War v2",
  "created_at": "2025-12-10T10:00:00Z"
}
```

### Upload to Workspace
```bash
POST /workspaces/tank_war_v2/documents/gdd
FormData:
  - file: garage_design.pdf
  - docId: garage_design

Response:
{
  "workspaceId": "tank_war_v2",
  "docId": "garage_design",
  "status": "indexed"
}
```

### Query Workspace
```bash
POST /workspaces/tank_war_v2/chat
{
  "message": "What is the garage system?",
  "useAllDocs": true
}

Response:
{
  "message": {
    "content": "...",
    "context": [...]
  }
}
```

## Migration from Current System

1. **Create default workspace** from existing `rag_storage/`
2. **Move all documents** from `docs/` to `workspaces/default/documents/`
3. **Move all indices** from `rag_storage/` to `workspaces/default/storage/`
4. **Update all references** to use workspace context
5. **Keep old paths** as symlinks for backward compatibility (optional)

