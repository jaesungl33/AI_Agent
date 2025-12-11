"""
Document indexing functionality using RAG-Anything.
"""
from __future__ import annotations

import asyncio
import json
from pathlib import Path
from typing import Optional, Callable, Union
from gdd_rag_backbone.config import DEFAULT_OUTPUT_DIR, DEFAULT_WORKING_DIR
from gdd_rag_backbone.rag_backend.rag_config import get_rag_instance


# Global registry to store RAG instances per doc_id
_rag_instances: dict[str, object] = {}


def _get_workspace_storage(workspace_id: Optional[str] = None):
    """Get workspace storage if workspace_id provided, else None."""
    if workspace_id:
        from gdd_rag_backbone.workspace.storage import WorkspaceStorage
        return WorkspaceStorage(workspace_id)
    return None


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
    workspace_id: Optional[str] = None,
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
    
    # Get workspace storage if workspace_id provided
    workspace_storage = _get_workspace_storage(workspace_id)
    
    # Set up output directory
    if output_dir is None:
        if workspace_storage:
            output_dir = workspace_storage.get_output_dir() / doc_id
        else:
            output_dir = DEFAULT_OUTPUT_DIR / doc_id
    elif isinstance(output_dir, str):
        output_dir = Path(output_dir)
    
    output_dir.mkdir(parents=True, exist_ok=True)
    
    # Create or get RAG instance for this document
    # Use the same working_dir to ensure we can query it later
    if working_dir is None:
        if workspace_storage:
            working_dir = workspace_storage.get_storage_dir()
        else:
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
    
    # Determine appropriate parser based on file extension
    file_ext = doc_path.suffix.lower()
    
    # For text and CSV files, use generic parser explicitly
    # For MindMap files (.mm), skip as they're not well supported
    if file_ext == '.mm':
        raise ValueError(
            f"MindMap (.mm) files are not currently supported. "
            f"Please convert '{doc_path.name}' to PDF or another supported format."
        )
    
    # For plain-text code or data files, force generic parser and avoid PDF-specific parsing
    if file_ext in ['.txt', '.csv', '.cs', '.csx', '.csharp']:
        # Use generic parser method for text-based files
        parse_method = parse_method or "auto"
        # Don't pass parser="mineru" or parser="docling" for text/code files
        parser = None
    
    try:
        await rag.process_document_complete(
            file_path=str(doc_path.absolute()),
            output_dir=str(output_dir.absolute()),
            parse_method=parse_method,
            doc_id=doc_id,
            **parser_kwargs
        )
    except Exception as e:
        error_msg = str(e)
        # Provide more helpful error messages
        if "EOF marker not found" in error_msg:
            raise RuntimeError(
                f"Failed to parse '{doc_path.name}': The file may be corrupted, "
                f"incomplete, or in an unsupported format. "
                f"File type: {file_ext}. "
                f"Original error: {error_msg}"
            ) from e
        elif "parser" in error_msg.lower():
            raise RuntimeError(
                f"Parser error for '{doc_path.name}': {error_msg}. "
                f"File type: {file_ext}. "
                f"Try converting to PDF or another supported format."
            ) from e
        else:
            raise RuntimeError(
                f"Failed to index '{doc_path.name}': {error_msg}"
            ) from e
    
    print(f"Document {doc_id} indexed successfully. Output: {output_dir}")


async def index_document_raw_text(
    doc_path: Union[str, Path],
    doc_id: str,
    *,
    llm_func: Optional[Callable] = None,
    embedding_func: Optional[Callable] = None,
    workspace_id: Optional[str] = None,
) -> None:
    """
    Minimal raw-text ingestion bypassing raganything parser.
    - Reads the file as UTF-8 text
    - Splits into chunks (simple length-based)
    - Embeds and writes into workspace indices
    """
    doc_path = Path(doc_path)
    if not doc_path.exists():
        raise FileNotFoundError(f"Document not found: {doc_path}")
    if not doc_id:
        raise ValueError("doc_id cannot be empty")

    workspace_storage = _get_workspace_storage(workspace_id)
    if workspace_storage:
        working_dir = workspace_storage.get_storage_dir()
        output_dir = workspace_storage.get_output_dir() / doc_id
    else:
        working_dir = DEFAULT_WORKING_DIR
        output_dir = DEFAULT_OUTPUT_DIR / doc_id
    output_dir.mkdir(parents=True, exist_ok=True)

    # Load text
    text = doc_path.read_text(encoding="utf-8", errors="ignore")
    if not text.strip():
        raise RuntimeError(f"Empty text in {doc_path.name}")

    # Simple chunking by size
    max_len = 1200
    chunks = []
    start = 0
    while start < len(text):
        end = min(start + max_len, len(text))
        chunk_text = text[start:end]
        chunk_id = f"chunk-{hash((doc_id, start, end)) & 0xffffffff:x}"
        chunks.append({"chunk_id": chunk_id, "doc_id": doc_id, "content": chunk_text})
        start = end

    # Embed chunks
    rag = get_rag_instance(
        llm_func=llm_func,
        embedding_func=embedding_func,
        working_dir=working_dir,
        parser=None,
        parse_method="auto",
    )
    texts = [c["content"] for c in chunks]

    embeddings: List[List[float]] = []
    if embedding_func is not None:
        if asyncio.iscoroutinefunction(embedding_func):
            embeddings = [await embedding_func(t) for t in texts]  # type: ignore
        else:
            embeddings = [embedding_func(t) for t in texts]  # type: ignore
    elif hasattr(rag, "embed"):
        maybe = rag.embed(texts)  # type: ignore
        if asyncio.iscoroutine(maybe):  # type: ignore
            maybe = await maybe  # type: ignore
        embeddings = maybe  # type: ignore
    else:
        raise RuntimeError("No embedding function available for raw text ingest")

    # Resolve any coroutine embeddings
    resolved_embeddings = []
    for e in embeddings:
        if asyncio.iscoroutine(e):
            e = await e
        resolved_embeddings.append(e)
    embeddings = resolved_embeddings

    def _to_float_list(vec):
        try:
            if isinstance(vec, dict) and "embedding" in vec:
                vec = vec["embedding"]
            # handle nested lists e.g. [[...]]
            if len(vec) == 1 and isinstance(vec[0], (list, tuple)):
                vec = vec[0]
            return [float(x) for x in vec]
        except Exception as exc:
            raise RuntimeError(f"Invalid embedding vector: {exc}")

    embeddings = [_to_float_list(e) for e in embeddings]

    # Persist to kv stores / vector stores
    # We reuse the same paths as raganything expects
    from gdd_rag_backbone.rag_backend.chunk_qa import _get_storage_paths
    paths = _get_storage_paths(workspace_id)
    chunks_path = paths["chunks"]
    vdb_chunks_path = paths["vdb_chunks"]

    chunks_data = {}
    if chunks_path.exists():
        chunks_data = json.loads(chunks_path.read_text())
    for c in chunks:
        chunks_data[c["chunk_id"]] = {
            "doc_id": c["doc_id"],
            "full_doc_id": c["doc_id"],
            "file_path": doc_path.name,
            "content": c["content"],
            "chunk_order_index": 0,
            "create_time": 0,
            "update_time": 0,
        }
    chunks_path.parent.mkdir(parents=True, exist_ok=True)
    chunks_path.write_text(json.dumps(chunks_data, ensure_ascii=False))

    # Write vectors
    vectors = []
    for c, emb in zip(chunks, embeddings):
        vectors.append({
            "id": c["chunk_id"],
            "vector": emb,
            "metadata": {
                "doc_id": c["doc_id"],
                "chunk_id": c["chunk_id"],
            },
        })
    vdb_chunks_path.parent.mkdir(parents=True, exist_ok=True)
    vdb_payload = {
        "data": [
            {
                "__id__": v["id"],
                "id": v["id"],
                "full_doc_id": v["metadata"]["doc_id"],
                "doc_id": v["metadata"]["doc_id"],
                "chunk_id": v["metadata"]["chunk_id"],
                "vector": v["vector"],
            }
            for v in vectors
        ]
    }
    vdb_chunks_path.write_text(json.dumps(vdb_payload, ensure_ascii=False))

    # Update status
    status_path = paths["status"]
    status_data = {}
    if status_path.exists():
        status_data = json.loads(status_path.read_text())
    status_data[doc_id] = {
        "doc_id": doc_id,
        "file_path": doc_path.name,
        "doc_type": "code",
        "status": "indexed",
        "chunks_list": [c["chunk_id"] for c in chunks],
        "updated_at": "",
    }
    status_path.write_text(json.dumps(status_data, ensure_ascii=False))

    print(f"Raw-text document {doc_id} indexed successfully with {len(chunks)} chunks. Output: {output_dir}")

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

