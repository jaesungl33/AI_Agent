#!/usr/bin/env python3
"""
Script to package all RAG data for sharing with team members.

This script creates a compressed archive containing:
- All RAG storage files (chunks, vectors, graph, status)
- Codebase snapshot files (if they exist)
- Metadata about what's included

Usage:
    python scripts/package_rag_data.py [--output OUTPUT_FILE] [--include-cache]
    
Options:
    --output OUTPUT_FILE    Output archive path (default: rag_data_backup.tar.gz)
    --include-cache         Include LLM response cache (can be large, optional)
    --include-codebase      Include codebase snapshot files (default: True)
"""
import argparse
import json
import tarfile
from pathlib import Path
from datetime import datetime
import sys

# Add project root to path
PROJECT_ROOT = Path(__file__).parent.parent
sys.path.insert(0, str(PROJECT_ROOT))

from gdd_rag_backbone.config import DEFAULT_WORKING_DIR, DEFAULT_DOCS_DIR


def get_file_size_mb(file_path: Path) -> float:
    """Get file size in MB."""
    if not file_path.exists():
        return 0.0
    return file_path.stat().st_size / (1024 * 1024)


def create_metadata(rag_storage_dir: Path, docs_dir: Path, include_cache: bool, include_codebase: bool) -> dict:
    """Create metadata about what's being packaged."""
    metadata = {
        "created_at": datetime.now().isoformat(),
        "rag_storage_files": [],
        "codebase_files": [],
        "total_size_mb": 0.0,
        "include_cache": include_cache,
        "include_codebase": include_codebase,
    }
    
    # RAG storage files
    rag_files = [
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
        "kv_store_parse_cache.json",
    ]
    
    if include_cache:
        rag_files.append("kv_store_llm_response_cache.json")
    
    for filename in rag_files:
        file_path = rag_storage_dir / filename
        if file_path.exists():
            size_mb = get_file_size_mb(file_path)
            metadata["rag_storage_files"].append({
                "filename": filename,
                "size_mb": round(size_mb, 2),
                "exists": True,
            })
            metadata["total_size_mb"] += size_mb
        else:
            metadata["rag_storage_files"].append({
                "filename": filename,
                "size_mb": 0.0,
                "exists": False,
            })
    
    # Codebase snapshot files
    if include_codebase:
        codebase_patterns = [
            "*codebase*.txt",
            "*batch*.txt",
        ]
        
        for pattern in codebase_patterns:
            for file_path in docs_dir.glob(pattern):
                size_mb = get_file_size_mb(file_path)
                metadata["codebase_files"].append({
                    "filename": file_path.name,
                    "size_mb": round(size_mb, 2),
                })
                metadata["total_size_mb"] += size_mb
    
    metadata["total_size_mb"] = round(metadata["total_size_mb"], 2)
    return metadata


def package_rag_data(output_file: Path, include_cache: bool = False, include_codebase: bool = True):
    """Package all RAG data into a compressed archive."""
    rag_storage_dir = Path(DEFAULT_WORKING_DIR)
    docs_dir = Path(DEFAULT_DOCS_DIR)
    
    print("=" * 80)
    print("📦 PACKAGING RAG DATA FOR SHARING")
    print("=" * 80)
    print()
    
    # Create metadata
    print("📋 Analyzing files...")
    metadata = create_metadata(rag_storage_dir, docs_dir, include_cache, include_codebase)
    
    print(f"   RAG storage files: {sum(1 for f in metadata['rag_storage_files'] if f['exists'])}")
    print(f"   Codebase files: {len(metadata['codebase_files'])}")
    print(f"   Total size: {metadata['total_size_mb']:.2f} MB")
    print()
    
    # Create archive
    print(f"📦 Creating archive: {output_file}")
    with tarfile.open(output_file, "w:gz") as tar:
        # Add RAG storage files
        print("\n📁 Adding RAG storage files...")
        for file_info in metadata["rag_storage_files"]:
            if file_info["exists"]:
                file_path = rag_storage_dir / file_info["filename"]
                print(f"   ✓ {file_info['filename']} ({file_info['size_mb']:.2f} MB)")
                tar.add(file_path, arcname=f"rag_storage/{file_info['filename']}")
        
        # Add codebase snapshot files
        if include_codebase and metadata["codebase_files"]:
            print("\n📁 Adding codebase snapshot files...")
            for file_info in metadata["codebase_files"]:
                file_path = docs_dir / file_info["filename"]
                print(f"   ✓ {file_info['filename']} ({file_info['size_mb']:.2f} MB)")
                tar.add(file_path, arcname=f"docs/{file_info['filename']}")
        
        # Add metadata
        print("\n📄 Adding metadata...")
        metadata_json = json.dumps(metadata, indent=2, ensure_ascii=False)
        metadata_path = Path("/tmp/rag_data_metadata.json")
        metadata_path.write_text(metadata_json, encoding="utf-8")
        tar.add(metadata_path, arcname="metadata.json")
        metadata_path.unlink()  # Clean up temp file
    
    output_size_mb = get_file_size_mb(output_file)
    print()
    print("=" * 80)
    print("✅ PACKAGING COMPLETE")
    print("=" * 80)
    print(f"📦 Archive: {output_file}")
    print(f"📊 Archive size: {output_size_mb:.2f} MB")
    print(f"📋 Contents: {len([f for f in metadata['rag_storage_files'] if f['exists']])} RAG files")
    if include_codebase:
        print(f"📋 Contents: {len(metadata['codebase_files'])} codebase files")
    print()
    print("📤 Share this file with your partner!")
    print("   They can restore it using: python scripts/restore_rag_data.py <archive_file>")


def main():
    parser = argparse.ArgumentParser(description="Package RAG data for sharing")
    parser.add_argument(
        "--output",
        type=str,
        default="rag_data_backup.tar.gz",
        help="Output archive file path (default: rag_data_backup.tar.gz)",
    )
    parser.add_argument(
        "--include-cache",
        action="store_true",
        help="Include LLM response cache (can be large)",
    )
    parser.add_argument(
        "--no-codebase",
        action="store_true",
        help="Exclude codebase snapshot files",
    )
    
    args = parser.parse_args()
    
    output_file = Path(args.output)
    if output_file.exists():
        response = input(f"⚠️  File {output_file} already exists. Overwrite? (y/N): ")
        if response.lower() != "y":
            print("❌ Cancelled.")
            return
    
    package_rag_data(
        output_file=output_file,
        include_cache=args.include_cache,
        include_codebase=not args.no_codebase,
    )


if __name__ == "__main__":
    main()



