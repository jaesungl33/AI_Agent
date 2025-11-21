"""
Document indexing functionality using RAG-Anything.
"""
from __future__ import annotations

import asyncio
from pathlib import Path
from typing import Optional, Callable, Union
from gdd_rag_backbone.config import DEFAULT_OUTPUT_DIR, DEFAULT_WORKING_DIR
from gdd_rag_backbone.rag_backend.rag_config import get_rag_instance


# Global registry to store RAG instances per doc_id
_rag_instances: dict[str, object] = {}


async def index_document(
    doc_path: Union[str, Path],
    doc_id: str,
    *,
    llm_func: Optional[Callable] = None,
    embedding_func: Optional[Callable] = None,
    working_dir: Optional[Union[Path, str]] = None,
    output_dir: Optional[Union[Path, str]] = None,
    parser: Optional[str] = None,
    parse_method: Optional[str] = None,
    **parser_kwargs
) -> None:
    """
    Index a document using RAG-Anything.
    
    This function parses, chunks, embeds, and indexes the document,
    storing the results in the specified output directory.
    
    Args:
        doc_path: Path to the document file (PDF, DOCX, etc.)
        doc_id: Unique identifier for the document
        llm_func: Optional LLM function for RAG (required for querying later)
        embedding_func: Optional embedding function (required for indexing and querying)
        working_dir: Working directory for RAG storage (defaults to DEFAULT_WORKING_DIR)
        output_dir: Output directory for parsed content (defaults to DEFAULT_OUTPUT_DIR/{doc_id})
        parser: Parser choice - "mineru" or "docling" (defaults to config default)
        parse_method: Parse method - "auto", "layout", "ocr", etc. (defaults to config default)
        **parser_kwargs: Additional parser parameters (lang, device, start_page, end_page, etc.)
    
    Raises:
        FileNotFoundError: If doc_path does not exist
        ValueError: If doc_id is empty
    """
    doc_path = Path(doc_path)
    
    if not doc_path.exists():
        raise FileNotFoundError(f"Document not found: {doc_path}")
    
    if not doc_id:
        raise ValueError("doc_id cannot be empty")
    
    # Set up output directory
    if output_dir is None:
        output_dir = DEFAULT_OUTPUT_DIR / doc_id
    elif isinstance(output_dir, str):
        output_dir = Path(output_dir)
    
    output_dir.mkdir(parents=True, exist_ok=True)
    
    # Create or get RAG instance for this document
    # Use the same working_dir to ensure we can query it later
    if working_dir is None:
        working_dir = DEFAULT_WORKING_DIR
    
    rag = get_rag_instance(
        llm_func=llm_func,
        embedding_func=embedding_func,
        working_dir=working_dir,
        parser=parser,
        parse_method=parse_method,
    )
    
    # Store the instance for later querying
    _rag_instances[doc_id] = rag
    
    # Process the document completely (parse, chunk, embed, index)
    print(f"Indexing document {doc_id} from {doc_path}...")
    
    await rag.process_document_complete(
        file_path=str(doc_path.absolute()),
        output_dir=str(output_dir.absolute()),
        parse_method=parse_method,
        doc_id=doc_id,
        **parser_kwargs
    )
    
    print(f"Document {doc_id} indexed successfully. Output: {output_dir}")


def get_rag_instance_for_doc(doc_id: str) -> Optional[object]:
    """
    Get the RAG instance for a specific document ID.
    
    Args:
        doc_id: Document ID
    
    Returns:
        RAGAnything instance if found, None otherwise
    """
    return _rag_instances.get(doc_id)


def clear_rag_instance(doc_id: str) -> None:
    """
    Clear a RAG instance from the registry.
    
    Args:
        doc_id: Document ID
    """
    if doc_id in _rag_instances:
        del _rag_instances[doc_id]

