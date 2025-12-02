#!/usr/bin/env python3
"""
Script to index the tank_online_1-dev Unity codebase into the RAG system.

This script:
1. Scans the tank_online_1-dev directory for code files (C#, shaders, JSON, etc.)
2. Creates a flattened plain-text snapshot preserving file boundaries
3. Indexes the snapshot via the RAG pipeline so it can be queried

Usage:
    python index_tank_online_codebase.py [--doc-id tank_online_codebase]
"""
import asyncio
import glob
import sys
from pathlib import Path

# Ensure project root is on PYTHONPATH
PROJECT_ROOT = Path(__file__).resolve().parent
if str(PROJECT_ROOT) not in sys.path:
    sys.path.insert(0, str(PROJECT_ROOT))

from gdd_rag_backbone.config import DEFAULT_DOCS_DIR
from gdd_rag_backbone.llm_providers import (
    QwenProvider,
    make_embedding_func,
    make_llm_model_func,
)
from gdd_rag_backbone.rag_backend import index_document

# Unity-specific file extensions to include
UNITY_INCLUDE_EXTS = {
    ".cs",           # C# scripts
    ".shader",       # Shader files
    ".compute",      # Compute shaders
    ".cginc",        # Shader includes
    ".hlsl",         # HLSL shaders
    ".json",         # JSON config files
    ".txt",          # Text files
    ".asmdef",       # Assembly definition files
    ".asmref",       # Assembly reference files
}

# Directories to exclude (Unity-specific)
UNITY_EXCLUDE_DIRS = {
    ".git",
    ".idea",
    ".vscode",
    ".vs",
    ".svn",
    "Library",
    "Logs",
    "Temp",
    "Build",
    "Builds",
    "obj",
    "DerivedDataCache",
    "__pycache__",
    "node_modules",
    "Packages",      # Unity package cache
    "UserSettings",  # Unity user settings
    "ProjectSettings",  # Unity project settings (can be large)
}


def should_skip_path(path: Path, root: Path, exclude_dirs: set) -> bool:
    """Check if a path should be skipped based on directory exclusions."""
    try:
        rel_parts = path.relative_to(root).parts[:-1]  # Exclude filename
        for part in rel_parts:
            if part.lower() in exclude_dirs:
                return True
    except ValueError:
        # Path is not relative to root, skip it
        return True
    return False


def find_code_files(root: Path, include_exts: set, exclude_dirs: set):
    """Find all code files in the directory tree."""
    code_files = []
    root_str = str(root)
    
    print(f"🔍 Scanning {root} for code files...")
    
    for file_path in root.rglob("*"):
        if not file_path.is_file():
            continue
        
        # Check extension
        if file_path.suffix.lower() not in include_exts:
            continue
        
        # Check if path should be excluded
        if should_skip_path(file_path, root, exclude_dirs):
            continue
        
        code_files.append(file_path)
    
    return sorted(code_files)


def create_code_snapshot_batch(source_dir: Path, code_files: list, output_path: Path, batch_size: int = 50):
    """Create code snapshots in batches to avoid memory issues."""
    from datetime import datetime, timezone
    
    output_path.parent.mkdir(parents=True, exist_ok=True)
    timestamp = datetime.now(timezone.utc).isoformat()
    
    total_files = len(code_files)
    num_batches = (total_files + batch_size - 1) // batch_size
    
    print(f"📝 Creating {num_batches} snapshot batches ({batch_size} files per batch)...")
    
    snapshot_paths = []
    
    for batch_idx in range(num_batches):
        start_idx = batch_idx * batch_size
        end_idx = min(start_idx + batch_size, total_files)
        batch_files = code_files[start_idx:end_idx]
        
        # Create batch-specific output path
        if num_batches > 1:
            batch_output = output_path.parent / f"{output_path.stem}_batch{batch_idx + 1:03d}{output_path.suffix}"
        else:
            batch_output = output_path
        
        header = [
            f"# Code Snapshot for {source_dir.name} - Batch {batch_idx + 1}/{num_batches}",
            f"Source: {source_dir}",
            f"Generated: {timestamp}",
            f"Files in this batch: {len(batch_files)}",
            f"Total files: {total_files}",
            "",
            "Each section below represents a file from the source tree.",
            "Lines are captured verbatim to help semantic retrieval.",
            "",
            "=" * 80,
            "",
        ]
        
        # Write batch snapshot
        with batch_output.open("w", encoding="utf-8") as f:
            f.write("\n".join(header))
            
            for local_idx, file_path in enumerate(batch_files, 1):
                global_idx = start_idx + local_idx
                try:
                    rel_path = file_path.relative_to(source_dir).as_posix()
                    f.write(f"\n\n{'=' * 80}\n")
                    f.write(f"FILE {global_idx}/{total_files}: {rel_path}\n")
                    f.write(f"{'=' * 80}\n\n")
                    
                    # Try to read as UTF-8, fallback to ignore errors
                    try:
                        content = file_path.read_text(encoding="utf-8")
                    except UnicodeDecodeError:
                        content = file_path.read_text(encoding="utf-8", errors="ignore")
                    
                    # Limit file size to prevent huge files (e.g., 50KB per file max)
                    max_file_size = 50 * 1024  # 50KB
                    if len(content) > max_file_size:
                        content = content[:max_file_size] + f"\n\n[TRUNCATED: File too large ({len(content)} bytes)]"
                    
                    f.write(content.rstrip())
                    f.write("\n")
                    
                except Exception as e:
                    print(f"  ⚠️  Warning: Could not read {file_path}: {e}")
                    f.write(f"\n[ERROR: Could not read file - {e}]\n")
        
        snapshot_paths.append(batch_output)
        print(f"  ✅ Batch {batch_idx + 1}/{num_batches} created: {batch_output.name} ({len(batch_files)} files)")
    
    return snapshot_paths


async def index_code_snapshot(snapshot_path: Path, doc_id: str):
    """Index the code snapshot using the RAG system."""
    print(f"\n🚀 Starting RAG indexing for doc_id='{doc_id}'...")
    
    provider = QwenProvider()
    llm_func = make_llm_model_func(provider)
    embedding_func = make_embedding_func(provider)
    
    await index_document(
        doc_path=snapshot_path,
        doc_id=doc_id,
        llm_func=llm_func,
        embedding_func=embedding_func,
    )
    
    print(f"✅ Codebase indexed successfully as '{doc_id}'")


async def index_code_snapshots_batch(snapshot_paths: list, base_doc_id: str):
    """Index multiple snapshot files as separate documents."""
    print(f"\n🚀 Starting batch RAG indexing for {len(snapshot_paths)} snapshots...")
    
    provider = QwenProvider()
    llm_func = make_llm_model_func(provider)
    embedding_func = make_embedding_func(provider)
    
    indexed_doc_ids = []
    
    for idx, snapshot_path in enumerate(snapshot_paths, 1):
        if len(snapshot_paths) > 1:
            doc_id = f"{base_doc_id}_batch{idx:03d}"
        else:
            doc_id = base_doc_id
        
        print(f"\n  📦 Indexing batch {idx}/{len(snapshot_paths)}: {snapshot_path.name} as '{doc_id}'...")
        
        try:
            await index_document(
                doc_path=snapshot_path,
                doc_id=doc_id,
                llm_func=llm_func,
                embedding_func=embedding_func,
            )
            indexed_doc_ids.append(doc_id)
            print(f"  ✅ Batch {idx} indexed successfully")
        except Exception as e:
            print(f"  ❌ Error indexing batch {idx}: {e}")
            import traceback
            traceback.print_exc()
    
    return indexed_doc_ids


async def main():
    """Main function to index the tank_online_1-dev codebase."""
    import argparse
    
    parser = argparse.ArgumentParser(
        description="Index the tank_online_1-dev Unity codebase into RAG"
    )
    parser.add_argument(
        "--source",
        type=Path,
        default=PROJECT_ROOT / "tank_online_1-dev",
        help="Path to tank_online_1-dev directory (default: ./tank_online_1-dev)",
    )
    parser.add_argument(
        "--doc-id",
        type=str,
        default="tank_online_codebase",
        help="Document ID for the indexed codebase (default: tank_online_codebase)",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=None,
        help="Optional snapshot output path (default: docs/{doc_id}_codebase.txt)",
    )
    parser.add_argument(
        "--skip-snapshot",
        action="store_true",
        help="Skip creating snapshot and index existing snapshot file",
    )
    parser.add_argument(
        "--batch-size",
        type=int,
        default=50,
        help="Number of files per batch (default: 50). Smaller batches use less memory.",
    )
    parser.add_argument(
        "--single-file",
        action="store_true",
        help="Create a single snapshot file (may cause memory issues with large codebases)",
    )
    
    args = parser.parse_args()
    
    source_dir = args.source.expanduser().resolve()
    if not source_dir.exists():
        print(f"❌ Error: Source directory not found: {source_dir}")
        sys.exit(1)
    
    # Determine snapshot path
    if args.output:
        snapshot_path = args.output.expanduser().resolve()
    else:
        snapshot_name = f"{args.doc_id}_codebase.txt"
        snapshot_path = DEFAULT_DOCS_DIR / snapshot_name
    
    print("=" * 80)
    print("📦 Indexing Tank Online Codebase")
    print("=" * 80)
    print(f"📁 Source: {source_dir}")
    print(f"🆔 Doc ID: {args.doc_id}")
    print(f"📝 Snapshot: {snapshot_path}")
    print("=" * 80)
    
    # Step 1: Create snapshot(s) (unless skipped)
    if not args.skip_snapshot:
        code_files = find_code_files(source_dir, UNITY_INCLUDE_EXTS, UNITY_EXCLUDE_DIRS)
        
        if not code_files:
            print("❌ Error: No code files found!")
            sys.exit(1)
        
        print(f"\n📊 Found {len(code_files)} code files")
        
        if args.single_file:
            # Create single snapshot (may cause memory issues)
            print("⚠️  Warning: Creating single file snapshot. This may cause memory issues with large codebases.")
            from datetime import datetime, timezone
            snapshot_path.parent.mkdir(parents=True, exist_ok=True)
            timestamp = datetime.now(timezone.utc).isoformat()
            
            header = [
                f"# Code Snapshot for {source_dir.name}",
                f"Source: {source_dir}",
                f"Generated: {timestamp}",
                f"Total files: {len(code_files)}",
                "",
                "Each section below represents a file from the source tree.",
                "Lines are captured verbatim to help semantic retrieval.",
                "",
                "=" * 80,
                "",
            ]
            
            print(f"📝 Creating single snapshot with {len(code_files)} files...")
            
            with snapshot_path.open("w", encoding="utf-8") as f:
                f.write("\n".join(header))
                
                for idx, file_path in enumerate(code_files, 1):
                    try:
                        rel_path = file_path.relative_to(source_dir).as_posix()
                        f.write(f"\n\n{'=' * 80}\n")
                        f.write(f"FILE {idx}/{len(code_files)}: {rel_path}\n")
                        f.write(f"{'=' * 80}\n\n")
                        
                        try:
                            content = file_path.read_text(encoding="utf-8")
                        except UnicodeDecodeError:
                            content = file_path.read_text(encoding="utf-8", errors="ignore")
                        
                        # Limit file size
                        max_file_size = 50 * 1024
                        if len(content) > max_file_size:
                            content = content[:max_file_size] + f"\n\n[TRUNCATED: File too large]"
                        
                        f.write(content.rstrip())
                        f.write("\n")
                        
                        if idx % 50 == 0:
                            print(f"  Processed {idx}/{len(code_files)} files...")
                            
                    except Exception as e:
                        print(f"  ⚠️  Warning: Could not read {file_path}: {e}")
                        f.write(f"\n[ERROR: Could not read file - {e}]\n")
            
            print(f"✅ Snapshot created: {snapshot_path}")
            snapshot_paths = [snapshot_path]
        else:
            # Create batched snapshots (memory-efficient)
            snapshot_paths = create_code_snapshot_batch(
                source_dir, code_files, snapshot_path, batch_size=args.batch_size
            )
    else:
        # Use existing snapshot(s)
        if snapshot_path.exists():
            snapshot_paths = [snapshot_path]
        else:
            # Try to find batch files
            pattern = snapshot_path.parent / f"{snapshot_path.stem}_batch*{snapshot_path.suffix}"
            found = sorted(glob.glob(str(pattern)))
            if found:
                snapshot_paths = [Path(f) for f in found]
                print(f"⏭️  Found {len(snapshot_paths)} existing batch snapshot(s)")
            else:
                print(f"❌ Error: Snapshot file(s) not found: {snapshot_path}")
                sys.exit(1)
    
    # Step 2: Index the snapshot(s)
    print("\n" + "=" * 80)
    
    if len(snapshot_paths) > 1:
        # Index multiple batches
        indexed_doc_ids = await index_code_snapshots_batch(snapshot_paths, args.doc_id)
        
        print("\n" + "=" * 80)
        print("🎉 Successfully indexed codebase in batches!")
        print(f"💡 Indexed {len(indexed_doc_ids)} batch document(s):")
        for doc_id in indexed_doc_ids:
            print(f"   - {doc_id}")
        print(f"\n💡 You can query all batches by selecting multiple doc_ids, or query individually.")
        print("=" * 80)
    else:
        # Index single snapshot
        await index_code_snapshot(snapshot_paths[0], args.doc_id)
        
        print("\n" + "=" * 80)
        print("🎉 Successfully indexed codebase!")
        print(f"💡 You can now query it using doc_id='{args.doc_id}' in the RAG system")
        print("=" * 80)


if __name__ == "__main__":
    asyncio.run(main())


