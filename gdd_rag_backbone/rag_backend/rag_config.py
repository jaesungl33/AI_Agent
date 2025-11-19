"""
RAG-Anything configuration and instance factory.
"""
from pathlib import Path
from typing import Callable, Optional
from raganything import RAGAnything, RAGAnythingConfig
from gdd_rag_backbone.config import (
    DEFAULT_WORKING_DIR,
    DEFAULT_PARSER,
    DEFAULT_PARSE_METHOD,
    DEFAULT_ENABLE_IMAGE_PROCESSING,
    DEFAULT_ENABLE_TABLE_PROCESSING,
    DEFAULT_ENABLE_EQUATION_PROCESSING,
)


def get_rag_instance(
    llm_func: Optional[Callable] = None,
    embedding_func: Optional[Callable] = None,
    vision_model_func: Optional[Callable] = None,
    working_dir: Optional[Path | str] = None,
    parser: Optional[str] = None,
    parse_method: Optional[str] = None,
    enable_image_processing: Optional[bool] = None,
    enable_table_processing: Optional[bool] = None,
    enable_equation_processing: Optional[bool] = None,
    **lightrag_kwargs
) -> RAGAnything:
    """
    Create and configure a RAGAnything instance.
    
    Args:
        llm_func: LLM function for text generation (compatible with RAG-Anything)
        embedding_func: Embedding function for vector generation (compatible with RAG-Anything)
        vision_model_func: Optional vision model function for image understanding
        working_dir: Working directory for RAG storage (defaults to DEFAULT_WORKING_DIR)
        parser: Parser choice - "mineru" or "docling" (defaults to DEFAULT_PARSER)
        parse_method: Parse method - "auto", "layout", "ocr", etc. (defaults to DEFAULT_PARSE_METHOD)
        enable_image_processing: Whether to process images (defaults to DEFAULT_ENABLE_IMAGE_PROCESSING)
        enable_table_processing: Whether to process tables (defaults to DEFAULT_ENABLE_TABLE_PROCESSING)
        enable_equation_processing: Whether to process equations (defaults to DEFAULT_ENABLE_EQUATION_PROCESSING)
        **lightrag_kwargs: Additional keyword arguments passed to LightRAG initialization
    
    Returns:
        Configured RAGAnything instance
    """
    # Convert working_dir to Path if string
    if working_dir is None:
        working_dir = DEFAULT_WORKING_DIR
    elif isinstance(working_dir, str):
        working_dir = Path(working_dir)
    
    # Ensure working directory exists
    working_dir.mkdir(parents=True, exist_ok=True)
    
    # Create RAGAnythingConfig with defaults
    config = RAGAnythingConfig(
        working_dir=str(working_dir),
        parser=parser or DEFAULT_PARSER,
        parse_method=parse_method or DEFAULT_PARSE_METHOD,
        enable_image_processing=enable_image_processing if enable_image_processing is not None else DEFAULT_ENABLE_IMAGE_PROCESSING,
        enable_table_processing=enable_table_processing if enable_table_processing is not None else DEFAULT_ENABLE_TABLE_PROCESSING,
        enable_equation_processing=enable_equation_processing if enable_equation_processing is not None else DEFAULT_ENABLE_EQUATION_PROCESSING,
    )
    
    # Create RAGAnything instance
    rag = RAGAnything(
        llm_model_func=llm_func,
        embedding_func=embedding_func,
        vision_model_func=vision_model_func,
        config=config,
        lightrag_kwargs=lightrag_kwargs,
    )
    
    return rag

