#!/usr/bin/env python3
"""
Migrate all existing indexed documents and storage to tank_war workspace.
"""
import sys
import json
import shutil
from pathlib import Path

# Add src to path
PROJECT_ROOT = Path(__file__).resolve().parents[2]
SRC_DIR = PROJECT_ROOT / "src"
sys.path.insert(0, str(SRC_DIR))

from gdd_rag_backbone.workspace import WorkspaceManager, WorkspaceStorage
from gdd_rag_backbone.config import PROJECT_ROOT

def migrate_all_to_tank_war():
    """Migrate all legacy storage to tank_war workspace."""
    print("=" * 70)
    print("Migration: Legacy Storage → tank_war Workspace")
    print("=" * 70)
    print()
    
    # Get or create tank_war workspace
    manager = WorkspaceManager()
    workspaces = manager.list_workspaces()
    tank_war = next((w for w in workspaces if w.id == "tank_war"), None)
    
    if not tank_war:
        print("Creating tank_war workspace...")
        tank_war_id = manager.create_workspace(
            name="Tank War",
            description="Main workspace for Tank War game project - all GDDs and code",
            workspace_id="tank_war"
        )
        print(f"✅ Created workspace: {tank_war_id}")
    else:
        print(f"✅ Found workspace: {tank_war.id}")
    
    # Set as default workspace
    manager.set_default_workspace("tank_war")
    print("✅ Set tank_war as default workspace")
    
    storage = WorkspaceStorage("tank_war")
    legacy_storage = PROJECT_ROOT / "data" / "rag_storage"
    
    # Files to migrate
    storage_files = {
        "kv_store_doc_status.json": storage.get_status_file(),
        "kv_store_text_chunks.json": storage.get_indices_dir() / "chunks.json",
        "kv_store_entity_chunks.json": storage.get_indices_dir() / "entities.json",
        "kv_store_relation_chunks.json": storage.get_indices_dir() / "relation_chunks.json",
        "kv_store_full_docs.json": storage.get_indices_dir() / "full_docs.json",
        "kv_store_full_entities.json": storage.get_indices_dir() / "full_entities.json",
        "kv_store_full_relations.json": storage.get_indices_dir() / "full_relations.json",
        "vdb_chunks.json": storage.get_vectors_dir() / "chunks.json",
        "vdb_entities.json": storage.get_vectors_dir() / "entities.json",
        "vdb_relationships.json": storage.get_vectors_dir() / "relations.json",
        "kv_store_llm_response_cache.json": storage.get_cache_dir() / "llm_cache.json",
        "kv_store_parse_cache.json": storage.get_cache_dir() / "parse_cache.json",
    }
    
    print("\nMigrating storage files...")
    migrated_count = 0
    for source_file, target_path in storage_files.items():
        source_path = legacy_storage / source_file
        if source_path.exists():
            target_path.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(source_path, target_path)
            migrated_count += 1
            print(f"  ✅ {source_file} → {target_path.relative_to(storage.workspace_dir)}")
        else:
            print(f"  ⚠️  {source_file} not found (skipping)")
    
    print(f"\n✅ Migrated {migrated_count} storage files")
    
    # Migrate graph file if exists
    graph_source = legacy_storage / "graph_chunk_entity_relation.graphml"
    if graph_source.exists():
        graph_target = storage.get_indices_dir() / "graph.graphml"
        shutil.copy2(graph_source, graph_target)
        print(f"  ✅ graph_chunk_entity_relation.graphml → {graph_target.relative_to(storage.workspace_dir)}")
    
    # Migrate behavior cache if exists
    behavior_cache_source = PROJECT_ROOT / "rag_storage" / "behavior_indices"
    if behavior_cache_source.exists():
        for cache_file in behavior_cache_source.glob("*.json"):
            target_cache = storage.get_behavior_cache_dir() / cache_file.name
            shutil.copy2(cache_file, target_cache)
            print(f"  ✅ {cache_file.name} → behavior_cache/")
    
    # Migrate reports
    reports_source = PROJECT_ROOT / "data" / "reports"
    if reports_source.exists():
        reports_target = storage.get_reports_dir()
        for report_file in reports_source.rglob("*.json"):
            rel_path = report_file.relative_to(reports_source)
            target_report = reports_target / rel_path
            target_report.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(report_file, target_report)
            print(f"  ✅ {rel_path} → reports/")
    
    print("\n" + "=" * 70)
    print("Migration Complete!")
    print("=" * 70)
    print(f"Workspace: tank_war")
    print(f"Storage: {storage.get_storage_dir()}")
    print(f"Documents: {storage.get_documents_dir()}")
    print(f"\n✅ All data has been migrated to tank_war workspace")
    print("✅ tank_war is now the default workspace")

if __name__ == "__main__":
    migrate_all_to_tank_war()

