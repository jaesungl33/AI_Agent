# Scripts

Utility scripts for the GDD RAG Backbone project.

## Indexing Scripts

### Index Codebase

```bash
python scripts/index_tank_online_codebase.py \
    --source ./your_code_directory \
    --doc-id your_codebase_id \
    --batch-size 50
```

### Re-index Documents

```bash
# Re-index all documents
python scripts/reindex_all_docs.py

# Continue re-indexing from where it stopped
python scripts/reindex_all_docs_continue.py
```

