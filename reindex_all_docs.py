#!/usr/bin/env python3
"""
Script to re-index all currently indexed documents.

This script will:
1. Get the list of all indexed documents
2. Re-index each document
3. Automatically extract the Game Spec for each document after indexing
"""
import asyncio
import sys
from pathlib import Path
from datetime import datetime

# Add project root to path
PROJECT_ROOT = Path(__file__).parent
sys.path.insert(0, str(PROJECT_ROOT))

from gdd_rag_backbone.config import DEFAULT_DOCS_DIR
from gdd_rag_backbone.llm_providers import (
    QwenProvider,
    make_llm_model_func,
    make_embedding_func,
)
from gdd_rag_backbone.rag_backend import indexing
from gdd_rag_backbone.rag_backend.chunk_qa import list_indexed_docs
from gdd_rag_backbone.gdd.extraction import extract_all_requirements
import json


def _game_spec_path(doc_id: str) -> Path:
    """Get the file path for storing the unified Game Spec."""
    checklist_dir = Path("checklists")
    checklist_dir.mkdir(parents=True, exist_ok=True)
    return checklist_dir / f"{doc_id}_game_spec.json"


def _save_game_spec(doc_id: str, payload: dict, metadata: dict = None) -> Path:
    """Save the unified Game Spec with metadata."""
    spec = {
        "doc_id": doc_id,
        "metadata": {
            "created_at": datetime.now().isoformat(),
            "version": "1.0",
            **(metadata or {})
        },
        "spec": payload
    }
    path = _game_spec_path(doc_id)
    path.write_text(json.dumps(spec, indent=2, ensure_ascii=False))
    return path


async def reindex_and_extract(doc_id: str, file_path: str, llm_func, embedding_func):
    """Re-index a document and automatically extract Game Spec."""
    doc_path = DEFAULT_DOCS_DIR / file_path
    
    if not doc_path.exists():
        print(f"  ⚠️  File not found: {doc_path}")
        return False
    
    try:
        # Step 1: Re-index document
        print(f"  📝 Re-indexing document...")
        await indexing.index_document(
            doc_path=doc_path,
            doc_id=doc_id,
            llm_func=llm_func,
            embedding_func=embedding_func,
        )
        print(f"  ✅ Re-indexed successfully")
        
        # Step 2: Auto-extract Game Spec
        print(f"  🔍 Extracting Game Spec...")
        payload = await extract_all_requirements(doc_id)
        save_path = _save_game_spec(doc_id, payload, metadata={"auto_extracted": True})
        print(f"  ✅ Game Spec saved to {save_path}")
        
        return True
    except Exception as e:
        print(f"  ❌ Error: {e}")
        import traceback
        traceback.print_exc()
        return False


async def main():
    """Main function to re-index all documents."""
    print("=" * 80)
    print("🔄 RE-INDEXING ALL DOCUMENTS")
    print("=" * 80)
    print()
    
    # Initialize providers
    print("Initializing LLM providers...")
    provider = QwenProvider()
    llm_func = make_llm_model_func(provider)
    embedding_func = make_embedding_func(provider)
    print("✅ Providers initialized\n")
    
    # Get all indexed documents
    docs = list_indexed_docs()
    total_docs = len(docs)
    
    if total_docs == 0:
        print("❌ No indexed documents found.")
        return
    
    print(f"Found {total_docs} indexed document(s) to re-index\n")
    
    # Process each document
    successful = 0
    failed = 0
    
    for idx, doc in enumerate(docs, 1):
        doc_id = doc["doc_id"]
        file_path = doc.get("file_path", "")
        
        print(f"[{idx}/{total_docs}] Processing: {doc_id}")
        print(f"  File: {file_path}")
        
        success = await reindex_and_extract(doc_id, file_path, llm_func, embedding_func)
        
        if success:
            successful += 1
        else:
            failed += 1
        
        print()  # Empty line between documents
    
    # Summary
    print("=" * 80)
    print("📊 SUMMARY")
    print("=" * 80)
    print(f"✅ Successful: {successful}/{total_docs}")
    print(f"❌ Failed: {failed}/{total_docs}")
    print("=" * 80)


if __name__ == "__main__":
    asyncio.run(main())














