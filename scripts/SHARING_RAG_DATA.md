# Sharing RAG Data with Team Members

This guide explains how to share your processed RAG data (chunks, vectors, embeddings) with team members so they don't need to reindex everything.

## Quick Start

### For You (Sharing the Data)

1. **Package all RAG data:**
   ```bash
   python scripts/package_rag_data.py
   ```
   
   This creates `rag_data_backup.tar.gz` containing:
   - All RAG storage files (chunks, vectors, graph, status)
   - Codebase snapshot files (if indexed)
   - Metadata about what's included

2. **Optional: Include LLM cache (can be large):**
   ```bash
   python scripts/package_rag_data.py --include-cache
   ```

3. **Share the archive file** (`rag_data_backup.tar.gz`) with your partner via:
   - Cloud storage (Google Drive, Dropbox, etc.)
   - File sharing service
   - Direct transfer

### For Your Partner (Restoring the Data)

**Important:** Your partner must run the restore script from the **project root directory** (where the `scripts/` folder is located).

1. **Make sure they have the project set up:**
   - They should have cloned/pulled the AI_Agent repository
   - They should be in the project root directory: `/path/to/AI_Agent/`

2. **Download the archive file** (`rag_data_backup.tar.gz`) to the project root directory

3. **Restore the data:**
   ```bash
   cd /path/to/AI_Agent
   python3 scripts/restore_rag_data.py rag_data_backup.tar.gz
   ```

4. **If files already exist and you want to overwrite:**
   ```bash
   python3 scripts/restore_rag_data.py rag_data_backup.tar.gz --overwrite
   ```

5. **Done!** The script automatically extracts files to:
   - `rag_storage/` - All RAG storage files (chunks, vectors, graph)
   - `docs/` - Codebase snapshot files

Your partner can now use the RAG system without reindexing.

## What Gets Shared

### RAG Storage Files (Required)
- `kv_store_doc_status.json` - Document processing status
- `kv_store_text_chunks.json` - Text chunks from documents
- `kv_store_entity_chunks.json` - Entity chunks
- `kv_store_relation_chunks.json` - Relation chunks
- `kv_store_full_docs.json` - Full document metadata
- `kv_store_full_entities.json` - Full entity data
- `kv_store_full_relations.json` - Full relation data
- `vdb_chunks.json` - Vector embeddings for chunks
- `vdb_entities.json` - Vector embeddings for entities
- `vdb_relationships.json` - Vector embeddings for relationships
- `graph_chunk_entity_relation.graphml` - Knowledge graph
- `kv_store_parse_cache.json` - Parse cache (speeds up re-parsing)

### Optional Files
- `kv_store_llm_response_cache.json` - LLM response cache (can be large, use `--include-cache`)
- Codebase snapshot files (`*codebase*.txt`, `*batch*.txt`) - If you indexed code

## Archive Size

Typical archive sizes:
- **Without cache**: ~50-200 MB (depending on number of documents)
- **With cache**: ~200-500 MB (can be larger with many queries)
- **With codebase**: +100-500 MB (depends on codebase size)

## Troubleshooting

### "File already exists" error
Use `--overwrite` flag when restoring:
```bash
python scripts/restore_rag_data.py rag_data_backup.tar.gz --overwrite
```

### Archive is too large
- Exclude LLM cache: Don't use `--include-cache` flag
- Exclude codebase: Use `--no-codebase` flag when packaging
- Compress further: The archive is already gzipped, but you can use additional compression

### Missing files after restore
- Check that the archive was created with all files
- Verify your partner has the same directory structure
- Check file permissions

## Notes

- The archive preserves the directory structure (`rag_storage/` and `docs/`)
- Original PDF documents are NOT included (only processed chunks)
- Your partner still needs the original PDFs in the `docs/` folder if they want to reindex
- API keys are NOT included (your partner needs their own)

## Example Workflow

```bash
# On your machine (sharing)
python scripts/package_rag_data.py --output my_rag_backup.tar.gz

# Upload my_rag_backup.tar.gz to cloud storage

# On partner's machine (receiving)
# Download my_rag_backup.tar.gz
python scripts/restore_rag_data.py my_rag_backup.tar.gz

# Verify it worked
python -c "from gdd_rag_backbone.rag_backend.chunk_qa import list_indexed_docs; print(f'Indexed docs: {len(list_indexed_docs())}')"
```

