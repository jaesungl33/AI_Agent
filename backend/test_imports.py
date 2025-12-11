#!/usr/bin/env python3
"""
Test script to verify all imports work after reorganization.
"""
import sys
from pathlib import Path

# Add project root and src to path (same as main.py)
PROJECT_ROOT = Path(__file__).resolve().parents[1]
SRC_DIR = PROJECT_ROOT / "src"
if str(PROJECT_ROOT) not in sys.path:
    sys.path.insert(0, str(PROJECT_ROOT))
if str(SRC_DIR) not in sys.path:
    sys.path.insert(0, str(SRC_DIR))

print("Testing imports...")
print(f"PROJECT_ROOT: {PROJECT_ROOT}")
print(f"SRC_DIR: {SRC_DIR}")
print()

# Test core imports
try:
    from gdd_rag_backbone.config import DEFAULT_DOCS_DIR, DEFAULT_WORKING_DIR, PROJECT_ROOT as CONFIG_ROOT
    print("✅ gdd_rag_backbone.config")
    print(f"   DEFAULT_DOCS_DIR: {DEFAULT_DOCS_DIR}")
    print(f"   DEFAULT_WORKING_DIR: {DEFAULT_WORKING_DIR}")
except ImportError as e:
    print(f"❌ gdd_rag_backbone.config: {e}")
    sys.exit(1)

try:
    from gdd_rag_backbone.llm_providers import QwenProvider, make_llm_model_func, make_embedding_func
    print("✅ gdd_rag_backbone.llm_providers")
except ImportError as e:
    print(f"❌ gdd_rag_backbone.llm_providers: {e}")
    sys.exit(1)

try:
    from gdd_rag_backbone.rag_backend import indexing
    from gdd_rag_backbone.rag_backend.chunk_qa import load_doc_status, ask_with_chunks
    print("✅ gdd_rag_backbone.rag_backend")
except ImportError as e:
    print(f"❌ gdd_rag_backbone.rag_backend: {e}")
    sys.exit(1)

try:
    from gdd_rag_backbone.gdd import extract_all_requirements, evaluate_all_requirements_behavior
    print("✅ gdd_rag_backbone.gdd")
except ImportError as e:
    print(f"❌ gdd_rag_backbone.gdd: {e}")
    sys.exit(1)

try:
    from gdd_rag_backbone.workspace import WorkspaceManager, WorkspaceStorage
    print("✅ gdd_rag_backbone.workspace")
except ImportError as e:
    print(f"❌ gdd_rag_backbone.workspace: {e}")
    sys.exit(1)

print()
print("✅ All imports successful!")

