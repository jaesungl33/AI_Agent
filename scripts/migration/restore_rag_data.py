#!/usr/bin/env python3
"""
Script to restore RAG data from a packaged archive.

This script extracts all RAG storage files and codebase snapshots
from a packaged archive created by package_rag_data.py.

Usage:
    python scripts/restore_rag_data.py <archive_file> [--overwrite]
    
Options:
    --overwrite    Overwrite existing files (default: skip existing files)
"""
import argparse
import json
import tarfile
from pathlib import Path
import sys

# Add project root to path
PROJECT_ROOT = Path(__file__).parent.parent
sys.path.insert(0, str(PROJECT_ROOT))

from gdd_rag_backbone.config import DEFAULT_WORKING_DIR, DEFAULT_DOCS_DIR


def restore_rag_data(archive_file: Path, overwrite: bool = False):
    """Restore RAG data from archive."""
    if not archive_file.exists():
        print(f"❌ Error: Archive file not found: {archive_file}")
        return False
    
    rag_storage_dir = Path(DEFAULT_WORKING_DIR)
    docs_dir = Path(DEFAULT_DOCS_DIR)
    
    # Ensure directories exist
    rag_storage_dir.mkdir(parents=True, exist_ok=True)
    docs_dir.mkdir(parents=True, exist_ok=True)
    
    print("=" * 80)
    print("📦 RESTORING RAG DATA")
    print("=" * 80)
    print(f"📁 Archive: {archive_file}")
    print()
    
    # Read metadata if available
    metadata = None
    try:
        with tarfile.open(archive_file, "r:gz") as tar:
            # Try to extract metadata first
            try:
                metadata_file = tar.extractfile("metadata.json")
                if metadata_file:
                    metadata = json.loads(metadata_file.read().decode("utf-8"))
                    print("📋 Archive metadata:")
                    print(f"   Created: {metadata.get('created_at', 'Unknown')}")
                    print(f"   Total size: {metadata.get('total_size_mb', 0):.2f} MB")
                    print()
            except KeyError:
                print("⚠️  No metadata found in archive (older format)")
                print()
    except Exception as e:
        print(f"⚠️  Could not read metadata: {e}")
        print()
    
    # Extract files
    print("📂 Extracting files...")
    restored_count = 0
    skipped_count = 0
    
    with tarfile.open(archive_file, "r:gz") as tar:
        for member in tar.getmembers():
            if member.name == "metadata.json":
                continue  # Skip metadata, already read
            
            # Determine destination
            if member.name.startswith("rag_storage/"):
                dest_path = rag_storage_dir / Path(member.name).name
            elif member.name.startswith("docs/"):
                dest_path = docs_dir / Path(member.name).name
            else:
                print(f"   ⚠️  Skipping unknown path: {member.name}")
                continue
            
            # Check if file exists
            if dest_path.exists() and not overwrite:
                print(f"   ⏭️  Skipping (exists): {dest_path.name}")
                skipped_count += 1
                continue
            
            # Extract file
            try:
                tar.extract(member, dest_path.parent)
                # Move to correct location if needed
                extracted_path = dest_path.parent / member.name
                if extracted_path != dest_path:
                    extracted_path.rename(dest_path)
                print(f"   ✓ Restored: {dest_path.name}")
                restored_count += 1
            except Exception as e:
                print(f"   ❌ Error extracting {member.name}: {e}")
    
    print()
    print("=" * 80)
    print("✅ RESTORATION COMPLETE")
    print("=" * 80)
    print(f"✓ Restored: {restored_count} file(s)")
    if skipped_count > 0:
        print(f"⏭️  Skipped: {skipped_count} file(s) (already exist)")
        print("   Use --overwrite to replace existing files")
    print()
    print("🎉 RAG data restored! You can now use the system without reindexing.")
    
    return True


def main():
    parser = argparse.ArgumentParser(description="Restore RAG data from archive")
    parser.add_argument(
        "archive_file",
        type=str,
        help="Path to the archive file (e.g., rag_data_backup.tar.gz)",
    )
    parser.add_argument(
        "--overwrite",
        action="store_true",
        help="Overwrite existing files",
    )
    
    args = parser.parse_args()
    
    archive_file = Path(args.archive_file)
    restore_rag_data(archive_file, overwrite=args.overwrite)


if __name__ == "__main__":
    main()



