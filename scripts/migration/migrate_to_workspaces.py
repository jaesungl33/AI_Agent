#!/usr/bin/env python3
"""
Migration script to convert existing global storage to workspace-based structure.

This script:
1. Creates a "default" workspace
2. Moves all existing data to workspaces/default/
3. Preserves all documents, indices, and cache
"""

import json
import shutil
from pathlib import Path
from datetime import datetime
from typing import Dict, Any

from gdd_rag_backbone.config import PROJECT_ROOT, DEFAULT_WORKING_DIR, DEFAULT_DOCS_DIR, DEFAULT_OUTPUT_DIR
from gdd_rag_backbone.workspace import WorkspaceManager, WorkspaceStorage


def migrate_storage_files(source_dir: Path, target_dir: Path) -> Dict[str, int]:
    """Migrate storage files from source to target."""
    stats = {"files_moved": 0, "files_skipped": 0, "errors": 0}
    
    # Storage files to migrate
    storage_files = [
        "kv_store_doc_status.json",
        "kv_store_text_chunks.json",
        "kv_store_entity_chunks.json",
        "kv_store_relation_chunks.json",
        "kv_store_full_docs.json",
        "kv_store_full_entities.json",
        "kv_store_full_relations.json",
        "vdb_chunks.json",
        "vdb_entities.json",
        "vdb_relationships.json",
        "graph_chunk_entity_relation.graphml",
        "kv_store_llm_response_cache.json",
        "kv_store_parse_cache.json",
    ]
    
    target_dir.mkdir(parents=True, exist_ok=True)
    indices_dir = target_dir / "indices"
    vectors_dir = target_dir / "indices" / "vectors"
    cache_dir = target_dir / "cache"
    
    indices_dir.mkdir(parents=True, exist_ok=True)
    vectors_dir.mkdir(parents=True, exist_ok=True)
    cache_dir.mkdir(parents=True, exist_ok=True)
    
    # File mapping: source -> target
    file_mapping = {
        "kv_store_doc_status.json": target_dir / "status.json",
        "kv_store_text_chunks.json": indices_dir / "chunks.json",
        "kv_store_entity_chunks.json": indices_dir / "entities.json",
        "kv_store_relation_chunks.json": indices_dir / "relation_chunks.json",
        "kv_store_full_docs.json": indices_dir / "full_docs.json",
        "kv_store_full_entities.json": indices_dir / "full_entities.json",
        "kv_store_full_relations.json": indices_dir / "full_relations.json",
        "vdb_chunks.json": vectors_dir / "chunks.json",
        "vdb_entities.json": vectors_dir / "entities.json",
        "vdb_relationships.json": vectors_dir / "relations.json",
        "graph_chunk_entity_relation.graphml": indices_dir / "graph.graphml",
        "kv_store_llm_response_cache.json": cache_dir / "llm_cache.json",
        "kv_store_parse_cache.json": cache_dir / "parse_cache.json",
    }
    
    for source_file in storage_files:
        source_path = source_dir / source_file
        if source_path.exists():
            target_path = file_mapping[source_file]
            try:
                shutil.copy2(source_path, target_path)
                stats["files_moved"] += 1
                print(f"  ✅ {source_file} → {target_path.relative_to(target_dir)}")
            except Exception as e:
                print(f"  ❌ Error copying {source_file}: {e}")
                stats["errors"] += 1
        else:
            stats["files_skipped"] += 1
    
    return stats


def migrate_documents(source_dir: Path, target_dir: Path) -> int:
    """Migrate documents from source to target."""
    count = 0
    target_dir.mkdir(parents=True, exist_ok=True)
    
    if not source_dir.exists():
        return 0
    
    for file_path in source_dir.iterdir():
        if file_path.is_file():
            try:
                target_path = target_dir / file_path.name
                shutil.copy2(file_path, target_path)
                count += 1
                print(f"  ✅ {file_path.name}")
            except Exception as e:
                print(f"  ❌ Error copying {file_path.name}: {e}")
    
    return count


def migrate_output_dirs(source_dir: Path, target_dir: Path) -> int:
    """Migrate output directories."""
    count = 0
    target_dir.mkdir(parents=True, exist_ok=True)
    
    if not source_dir.exists():
        return 0
    
    for output_subdir in source_dir.iterdir():
        if output_subdir.is_dir():
            try:
                target_path = target_dir / output_subdir.name
                if target_path.exists():
                    shutil.rmtree(target_path)
                shutil.copytree(output_subdir, target_path)
                count += 1
                print(f"  ✅ {output_subdir.name}/")
            except Exception as e:
                print(f"  ❌ Error copying {output_subdir.name}: {e}")
    
    return count


def main(dry_run: bool = False):
    """Main migration function."""
    print("=" * 70)
    print("Migration: Global Storage → Workspace-Based Structure")
    print("=" * 70)
    print()
    
    if dry_run:
        print("🔍 DRY RUN MODE - No changes will be made")
        print()
    else:
        print("⚠️  LIVE MODE - Changes will be made")
        response = input("Continue? (yes/no): ")
        if response.lower() != 'yes':
            print("Migration cancelled.")
            return
    
    # Step 1: Create default workspace
    print("\n[1/4] Creating 'default' workspace...")
    manager = WorkspaceManager()
    
    if manager.workspace_exists("default"):
        print("  ⚠️  'default' workspace already exists")
        if not dry_run:
            response = input("  Overwrite? (yes/no): ")
            if response.lower() != 'yes':
                print("  Migration cancelled.")
                return
            manager.delete_workspace("default")
    
    if not dry_run:
        workspace_id = manager.create_workspace(
            name="Default Workspace",
            description="Migrated from global storage",
            workspace_id="default"
        )
        print(f"  ✅ Created workspace: {workspace_id}")
    else:
        workspace_id = "default"
        print(f"  ✅ Would create workspace: {workspace_id}")
    
    # Step 2: Migrate storage files
    print("\n[2/4] Migrating storage files...")
    storage = WorkspaceStorage(workspace_id)
    source_storage = DEFAULT_WORKING_DIR
    target_storage = storage.get_storage_dir()
    
    if not dry_run:
        stats = migrate_storage_files(source_storage, target_storage)
        print(f"  ✅ Moved {stats['files_moved']} files")
        if stats['errors'] > 0:
            print(f"  ⚠️  {stats['errors']} errors")
    else:
        print("  ✅ Would migrate storage files")
    
    # Step 3: Migrate documents
    print("\n[3/4] Migrating documents...")
    source_docs = DEFAULT_DOCS_DIR
    target_docs_gdd = storage.get_documents_dir("gdd")
    target_docs_code = storage.get_documents_dir("code")
    
    if not dry_run:
        # Move GDD files (PDF, DOCX, etc.)
        gdd_count = 0
        code_count = 0
        for file_path in source_docs.iterdir():
            if file_path.is_file():
                ext = file_path.suffix.lower()
                if ext in {".pdf", ".docx", ".doc", ".txt", ".md", ".csv", ".xml", ".json", ".mm"}:
                    target = target_docs_gdd / file_path.name
                    shutil.copy2(file_path, target)
                    gdd_count += 1
                elif ext in {".cs", ".js", ".py", ".cpp", ".java", ".zip", ".tar", ".tar.gz"}:
                    target = target_docs_code / file_path.name
                    shutil.copy2(file_path, target)
                    code_count += 1
        
        print(f"  ✅ Migrated {gdd_count} GDD files, {code_count} code files")
    else:
        print("  ✅ Would migrate documents")
    
    # Step 4: Migrate output directories
    print("\n[4/4] Migrating output directories...")
    source_output = DEFAULT_OUTPUT_DIR
    target_output = storage.get_output_dir()
    
    if not dry_run:
        count = migrate_output_dirs(source_output, target_output)
        print(f"  ✅ Migrated {count} output directories")
    else:
        print("  ✅ Would migrate output directories")
    
    # Summary
    print("\n" + "=" * 70)
    print("Migration Summary")
    print("=" * 70)
    print(f"Workspace: {workspace_id}")
    print(f"Storage: {target_storage}")
    print(f"Documents: {target_docs_gdd.parent}")
    print(f"Output: {target_output}")
    
    if not dry_run:
        print("\n✅ Migration complete!")
        print("\nNext steps:")
        print("1. Test the workspace system")
        print("2. Update frontend to use workspace API")
        print("3. (Optional) Remove old global storage after verification")
    else:
        print("\n💡 Run without --dry-run to apply changes")


if __name__ == "__main__":
    import sys
    dry_run = '--live' not in sys.argv
    main(dry_run=dry_run)

