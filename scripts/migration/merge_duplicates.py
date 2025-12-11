#!/usr/bin/env python3
"""
Merge duplicate indexed documents to reduce storage size.

This script:
1. Identifies duplicates (same file path, different doc_ids)
2. Keeps the newest version (most recent updated_at)
3. Merges chunks/entities from duplicates into the kept version
4. Removes duplicate entries from all storage files
5. Cleans up duplicate output directories
"""

import json
import shutil
from pathlib import Path
from collections import defaultdict
from datetime import datetime
from typing import Dict, List, Tuple

from gdd_rag_backbone.config import DEFAULT_WORKING_DIR, DEFAULT_OUTPUT_DIR
from gdd_rag_backbone.rag_backend.chunk_qa import load_doc_status, load_doc_chunks

# Storage file paths
STATUS_PATH = DEFAULT_WORKING_DIR / "kv_store_doc_status.json"
CHUNKS_PATH = DEFAULT_WORKING_DIR / "kv_store_text_chunks.json"
FULL_DOCS_PATH = DEFAULT_WORKING_DIR / "kv_store_full_docs.json"
ENTITY_CHUNKS_PATH = DEFAULT_WORKING_DIR / "kv_store_entity_chunks.json"
VDB_CHUNKS_PATH = DEFAULT_WORKING_DIR / "vdb_chunks.json"
VDB_ENTITIES_PATH = DEFAULT_WORKING_DIR / "vdb_entities.json"
VDB_RELATIONS_PATH = DEFAULT_WORKING_DIR / "vdb_relationships.json"

def load_json(path: Path) -> dict:
    """Load JSON file."""
    if not path.exists():
        return {}
    with open(path, 'r', encoding='utf-8') as f:
        return json.load(f)

def save_json(path: Path, data: dict) -> None:
    """Save JSON file with backup."""
    # Create backup
    if path.exists():
        backup_path = path.with_suffix(f'.json.backup_{datetime.now().strftime("%Y%m%d_%H%M%S")}')
        shutil.copy2(path, backup_path)
        print(f"  Created backup: {backup_path.name}")
    
    path.parent.mkdir(parents=True, exist_ok=True)
    with open(path, 'w', encoding='utf-8') as f:
        json.dump(data, f, indent=2, ensure_ascii=False)

def find_duplicates() -> List[Tuple[str, List[Tuple[str, dict]]]]:
    """Find duplicate documents grouped by file path."""
    status = load_doc_status()
    by_file = defaultdict(list)
    
    for doc_id, meta in status.items():
        file_path = meta.get('file_path', '')
        if file_path:
            by_file[file_path].append((doc_id, meta))
    
    # Return only groups with duplicates
    duplicates = []
    for file_path, docs in by_file.items():
        if len(docs) > 1:
            # Sort by updated_at (newest first)
            docs.sort(key=lambda x: x[1].get('updated_at', ''), reverse=True)
            duplicates.append((file_path, docs))
    
    return duplicates

def choose_keep_doc(docs: List[Tuple[str, dict]]) -> str:
    """Choose which document to keep (newest one)."""
    # Already sorted by updated_at (newest first)
    return docs[0][0]

def merge_documents(keep_id: str, remove_ids: List[str], dry_run: bool = False) -> Dict[str, int]:
    """Merge duplicate documents."""
    stats = {
        'chunks_merged': 0,
        'entities_merged': 0,
        'vectors_merged': 0,
        'status_removed': 0,
        'output_dirs_removed': 0,
    }
    
    print(f"\n  Merging into: {keep_id}")
    print(f"  Removing: {', '.join(remove_ids)}")
    
    if dry_run:
        print("  [DRY RUN - no changes made]")
        return stats
    
    # 1. Merge chunks
    chunks_data = load_json(CHUNKS_PATH)
    for remove_id in remove_ids:
        for chunk_id, chunk_data in chunks_data.items():
            if isinstance(chunk_data, dict) and chunk_data.get('full_doc_id') == remove_id:
                # Update doc_id to keep_id
                chunk_data['full_doc_id'] = keep_id
                stats['chunks_merged'] += 1
    # Update chunks file
    if stats['chunks_merged'] > 0:
        save_json(CHUNKS_PATH, chunks_data)
        print(f"  ✓ Merged {stats['chunks_merged']} chunks")
    
    # 2. Merge entity chunks
    entity_chunks_data = load_json(ENTITY_CHUNKS_PATH)
    for remove_id in remove_ids:
        for chunk_id, chunk_data in entity_chunks_data.items():
            if isinstance(chunk_data, dict) and chunk_data.get('full_doc_id') == remove_id:
                chunk_data['full_doc_id'] = keep_id
                stats['entities_merged'] += 1
    if stats['entities_merged'] > 0:
        save_json(ENTITY_CHUNKS_PATH, entity_chunks_data)
        print(f"  ✓ Merged {stats['entities_merged']} entity chunks")
    
    # 3. Merge vectors (vdb_chunks)
    vdb_chunks_data = load_json(VDB_CHUNKS_PATH)
    if vdb_chunks_data:
        # vdb_chunks is a list of [vector, metadata] pairs
        if isinstance(vdb_chunks_data, list):
            for item in vdb_chunks_data:
                if isinstance(item, list) and len(item) >= 2:
                    metadata = item[1] if len(item) > 1 else {}
                    if isinstance(metadata, dict) and metadata.get('full_doc_id') in remove_ids:
                        metadata['full_doc_id'] = keep_id
                        stats['vectors_merged'] += 1
            if stats['vectors_merged'] > 0:
                save_json(VDB_CHUNKS_PATH, vdb_chunks_data)
                print(f"  ✓ Merged {stats['vectors_merged']} vectors")
    
    # 4. Remove from status
    status_data = load_json(STATUS_PATH)
    for remove_id in remove_ids:
        if remove_id in status_data:
            del status_data[remove_id]
            stats['status_removed'] += 1
    if stats['status_removed'] > 0:
        save_json(STATUS_PATH, status_data)
        print(f"  ✓ Removed {stats['status_removed']} status entries")
    
    # 5. Remove from full_docs (if exists)
    full_docs_data = load_json(FULL_DOCS_PATH)
    for remove_id in remove_ids:
        if remove_id in full_docs_data:
            del full_docs_data[remove_id]
    if full_docs_data:
        save_json(FULL_DOCS_PATH, full_docs_data)
    
    # 6. Clean up output directories
    for remove_id in remove_ids:
        output_dir = DEFAULT_OUTPUT_DIR / remove_id
        if output_dir.exists() and output_dir.is_dir():
            try:
                shutil.rmtree(output_dir)
                stats['output_dirs_removed'] += 1
                print(f"  ✓ Removed output directory: {output_dir.name}")
            except Exception as e:
                print(f"  ⚠ Failed to remove {output_dir}: {e}")
    
    return stats

def main(dry_run: bool = True):
    """Main merge function."""
    print("=== Duplicate Document Merger ===\n")
    
    if dry_run:
        print("🔍 DRY RUN MODE - No changes will be made\n")
    else:
        print("⚠️  LIVE MODE - Changes will be saved\n")
        response = input("Continue? (yes/no): ")
        if response.lower() != 'yes':
            print("Aborted.")
            return
    
    # Find duplicates
    duplicates = find_duplicates()
    
    if not duplicates:
        print("✅ No duplicates found!")
        return
    
    print(f"Found {len(duplicates)} files with duplicates\n")
    
    total_stats = {
        'chunks_merged': 0,
        'entities_merged': 0,
        'vectors_merged': 0,
        'status_removed': 0,
        'output_dirs_removed': 0,
    }
    
    # Process each duplicate group
    for file_path, docs in duplicates:
        keep_id = choose_keep_doc(docs)
        remove_ids = [doc_id for doc_id, _ in docs[1:]]  # All except the first (kept one)
        
        print(f"\n📄 {Path(file_path).name}")
        print(f"   Keeping: {keep_id}")
        print(f"   Removing: {len(remove_ids)} duplicate(s)")
        
        stats = merge_documents(keep_id, remove_ids, dry_run=dry_run)
        for key in total_stats:
            total_stats[key] += stats[key]
    
    # Summary
    print("\n" + "="*50)
    print("MERGE SUMMARY")
    print("="*50)
    print(f"Files processed: {len(duplicates)}")
    print(f"Chunks merged: {total_stats['chunks_merged']}")
    print(f"Entity chunks merged: {total_stats['entities_merged']}")
    print(f"Vectors merged: {total_stats['vectors_merged']}")
    print(f"Status entries removed: {total_stats['status_removed']}")
    print(f"Output directories removed: {total_stats['output_dirs_removed']}")
    
    if not dry_run:
        print("\n✅ Merge complete! Backups created for all modified files.")
    else:
        print("\n💡 Run with dry_run=False to apply changes")

if __name__ == "__main__":
    import sys
    dry_run = '--live' not in sys.argv
    main(dry_run=dry_run)

