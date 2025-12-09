# Step-by-Step Guide: Restoring RAG Data

This guide is for your partner to restore the processed RAG data you've shared.

## Prerequisites

Your partner needs:
1. ✅ Python 3.10 or higher installed
2. ✅ The AI_Agent project cloned/set up
3. ✅ Dependencies installed (`pip install -r requirements.txt`)
4. ✅ The `rag_data_backup.tar.gz` file you shared

## Step-by-Step Process

### Step 1: Get the Project Ready

```bash
# Navigate to the project directory
cd /path/to/AI_Agent

# Verify you're in the right place (should see these folders)
ls
# Should show: docs/, rag_storage/, scripts/, gdd_rag_backbone/, etc.
```

### Step 2: Download the Archive File

Download `rag_data_backup.tar.gz` from wherever you shared it (Google Drive, Dropbox, etc.) and place it in the project root directory:

```bash
# Make sure the file is in the project root
cd /path/to/AI_Agent
ls rag_data_backup.tar.gz
# Should show: rag_data_backup.tar.gz
```

### Step 3: Run the Restore Script

```bash
# Make sure you're in the project root
cd /path/to/AI_Agent

# Run the restore script
python3 scripts/restore_rag_data.py rag_data_backup.tar.gz
```

**What you'll see:**
```
================================================================================
📦 RESTORING RAG DATA
================================================================================
📁 Archive: rag_data_backup.tar.gz

📋 Archive metadata:
   Created: 2025-11-28T...
   Total size: 161.78 MB

📂 Extracting files...
   ✓ Restored: kv_store_doc_status.json
   ✓ Restored: kv_store_text_chunks.json
   ✓ Restored: kv_store_entity_chunks.json
   ... (more files)
   ✓ Restored: tank_online_codebase_codebase_batch001.txt
   ... (more codebase files)

================================================================================
✅ RESTORATION COMPLETE
================================================================================
✓ Restored: 42 file(s)
⏭️  Skipped: 0 file(s) (already exist)

🎉 RAG data restored! You can now use the system without reindexing.
```

### Step 4: Verify It Worked

Test that the data was restored correctly:

```bash
# Check that files exist
ls rag_storage/
# Should show: kv_store_*.json, vdb_*.json, graph_*.graphml

ls docs/*codebase*.txt
# Should show: tank_online_codebase_codebase_batch*.txt files

# Verify indexed documents (optional test)
python3 -c "from gdd_rag_backbone.rag_backend.chunk_qa import list_indexed_docs; docs = list_indexed_docs(); print(f'✅ Found {len(docs)} indexed documents')"
```

Expected output: `✅ Found 44+ indexed documents` (or similar number)

### Step 5: Test the System

Try querying a document to make sure everything works:

```bash
# Test asking a question about a document
python3 gdd_rag_backbone/scripts/ask_doc.py codebase "What is the main game loop?"
```

Or use the Gradio UI:
```bash
python3 ui/app_gradio.py
```

## Troubleshooting

### Problem: "File already exists" error

**Solution:** Use the `--overwrite` flag:
```bash
python3 scripts/restore_rag_data.py rag_data_backup.tar.gz --overwrite
```

### Problem: "Archive file not found"

**Solution:** Make sure:
1. The file is named exactly `rag_data_backup.tar.gz`
2. You're in the project root directory
3. The file path is correct

```bash
# Check current directory
pwd
# Should end with: AI_Agent

# Check if file exists
ls -lh rag_data_backup.tar.gz
```

### Problem: "Module not found" errors

**Solution:** Install dependencies:
```bash
pip install -r requirements.txt
```

### Problem: Files extracted but system doesn't work

**Solution:** Check file permissions and locations:
```bash
# Check rag_storage directory
ls -la rag_storage/
# Should show JSON and graphml files

# Check docs directory
ls -la docs/*codebase*.txt
# Should show codebase snapshot files
```

## What Gets Restored

The script automatically extracts files to:

- **`rag_storage/`** - All RAG storage files:
  - `kv_store_*.json` - Chunks, entities, relations, status
  - `vdb_*.json` - Vector embeddings
  - `graph_*.graphml` - Knowledge graph

- **`docs/`** - Codebase snapshot files:
  - `tank_online_codebase_codebase_batch*.txt` - Code snapshots

## After Restoration

Once restored, your partner can:
- ✅ Query documents using the RAG system
- ✅ Run coverage evaluations
- ✅ Use the Gradio UI
- ✅ Ask questions about the codebase

**No reindexing needed!** 🎉

## Quick Reference

```bash
# Full restore process (copy-paste ready)
cd /path/to/AI_Agent
python3 scripts/restore_rag_data.py rag_data_backup.tar.gz

# If files already exist and you want to overwrite
python3 scripts/restore_rag_data.py rag_data_backup.tar.gz --overwrite

# Verify it worked
python3 -c "from gdd_rag_backbone.rag_backend.chunk_qa import list_indexed_docs; print(f'Indexed docs: {len(list_indexed_docs())}')"
```



