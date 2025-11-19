"""
RAG Backend - Integration with RAG-Anything for document indexing and querying.
"""

from gdd_rag_backbone.rag_backend.indexing import index_document
from gdd_rag_backbone.rag_backend.query_engine import ask_question, debug_query
from gdd_rag_backbone.rag_backend.rag_config import get_rag_instance, RAGAnythingConfig

__all__ = [
    "index_document",
    "ask_question",
    "debug_query",
    "get_rag_instance",
    "RAGAnythingConfig",
]

