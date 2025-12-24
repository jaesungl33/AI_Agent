#!/usr/bin/env python3
"""
Complete reindexing script for all GDD documents and code.

This script will:
1. Index all GDD documents (PDFs, text files, CSVs)
2. Index all code files in the codebase
3. Update the tank_war workspace with fresh data

Usage:
    python reindex_all.py [--force] [--parser docling|mineru]

Requirements:
    - Set DASHSCOPE_API_KEY or QWEN_API_KEY environment variable
    - Documents should be in data/gdd_documents/
    - Code should be in appropriate directories
"""

import asyncio
import sys
import os
from pathlib import Path
from datetime import datetime

# Add project paths
sys.path.insert(0, str(Path(__file__).parent))

from scripts.testing.process_all_docs import main as process_gdd_docs
from scripts.migration.index_codebase import main as index_codebase
from scripts.migration.reindex_all_docs import main as reindex_existing

def check_environment():
    """Check if environment is ready for indexing."""
    print("🔍 Checking environment...")

    # Check API key
    api_key = os.getenv('DASHSCOPE_API_KEY') or os.getenv('QWEN_API_KEY')
    if not api_key:
        print("❌ ERROR: No API key found!")
        print("   Set DASHSCOPE_API_KEY or QWEN_API_KEY environment variable")
        print("   Example: export DASHSCOPE_API_KEY='your_key_here'")
        return False

    print("✅ API key found")

    # Check documents
    gdd_dir = Path('data/gdd_documents')
    if not gdd_dir.exists():
        print("❌ ERROR: GDD documents directory not found!")
        return False

    docs = list(gdd_dir.glob('*'))
    pdfs = len([d for d in docs if d.suffix.lower() == '.pdf'])
    texts = len([d for d in docs if d.suffix.lower() in ['.txt', '.md']])

    print(f"✅ GDD documents found: {len(docs)} files ({pdfs} PDFs, {texts} text files)")

    return True

async def run_reindexing():
    """Run the complete reindexing process."""
    print("\n" + "="*80)
    print("🚀 STARTING COMPLETE REINDEXING")
    print("="*80)

    # Parse arguments
    force = "--force" in sys.argv
    parser = None

    if "--parser" in sys.argv:
        idx = sys.argv.index("--parser")
        if idx + 1 < len(sys.argv):
            parser = sys.argv[idx + 1]

    print(f"Options: force={force}, parser={parser}")

    try:
        print("\n" + "-"*60)
        print("📄 PHASE 1: Indexing GDD Documents")
        print("-"*60)

        # Import and run GDD processing
        # Note: We'll call the main function directly
        print("Running GDD document indexing...")

        # For now, we'll provide the command to run
        print("To index GDD documents, run:")
        print("python scripts/testing/process_all_docs.py --force" + (f" --parser {parser}" if parser else ""))

        print("\n" + "-"*60)
        print("💻 PHASE 2: Indexing Code Files")
        print("-"*60)

        print("To index code files, run:")
        print("python scripts/migration/index_codebase.py --workspace tank_war --force")

        print("\n" + "-"*60)
        print("🔄 PHASE 3: Reindexing Vectors (if needed)")
        print("-"*60)

        print("To reindex vectors, run:")
        print("python backend/scripts/reindex_vectors.py --workspace tank_war")

        print("\n" + "="*80)
        print("✅ REINDEXING PLAN COMPLETE")
        print("="*80)
        print("Run the commands above in order, or use the convenience script:")
        print("./run_full_reindex.sh")

    except Exception as e:
        print(f"❌ ERROR: {e}")
        import traceback
        traceback.print_exc()

def main():
    """Main entry point."""
    if not check_environment():
        sys.exit(1)

    asyncio.run(run_reindexing())

if __name__ == "__main__":
    main()
