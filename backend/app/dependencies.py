"""Shared dependency providers for FastAPI routes."""

from functools import lru_cache

from backend.database import Database
from backend.generation import Generator
from backend.indexing import Indexer
from backend.retrieval import Retriever
from backend.storage import Storage


@lru_cache(maxsize=1)
def get_database() -> Database:
    """Return a singleton Database instance."""
    return Database()


@lru_cache(maxsize=1)
def get_storage() -> Storage:
    """Return a singleton Storage instance."""
    return Storage()


@lru_cache(maxsize=1)
def get_indexer() -> Indexer:
    """Return a singleton Indexer instance."""
    return Indexer(get_database(), get_storage())


@lru_cache(maxsize=1)
def get_retriever() -> Retriever:
    """Return a singleton Retriever instance."""
    return Retriever(get_database())


@lru_cache(maxsize=1)
def get_generator() -> Generator:
    """Return a singleton Generator instance."""
    return Generator()


__all__ = [
    "get_database",
    "get_storage",
    "get_indexer",
    "get_retriever",
    "get_generator",
]
