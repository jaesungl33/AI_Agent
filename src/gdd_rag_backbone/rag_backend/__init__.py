"""
RAG Backend - Integration with RAG-Anything for document indexing and querying.
"""

import os

# Apply lightrag patch unless explicitly disabled (helps avoid native deps in constrained envs)
if not os.environ.get("LIGHTRAG_PATCH_DISABLE"):
    from gdd_rag_backbone.rag_backend import lightrag_patch  # noqa: F401

from gdd_rag_backbone.rag_backend.indexing import index_document
from gdd_rag_backbone.rag_backend.query_engine import ask_question, debug_query
from gdd_rag_backbone.rag_backend.rag_config import get_rag_instance

__all__ = [
    "index_document",
    "ask_question",
    "debug_query",
    "get_rag_instance",
]

