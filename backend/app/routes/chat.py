"""Chat and extraction endpoints."""

from fastapi import APIRouter, Depends, HTTPException

from backend.app.dependencies import get_generator, get_retriever
from backend.app.schemas import (
    ChatRequest,
    ChatResponse,
    ExtractCodeRequest,
    ExtractDocsRequest,
    ExtractResponse,
)
from backend.generation import Generator
from backend.retrieval import Retriever

router = APIRouter(tags=["chat"])


@router.post("/chat", response_model=ChatResponse, summary="Chat with RAG")
async def chat(
    request: ChatRequest,
    retriever: Retriever = Depends(get_retriever),
    generator: Generator = Depends(get_generator),
):
    """Main chat endpoint with RAG retrieval and generation."""
    try:
        mode = determine_search_mode(request.message)

        code_doc_id = request.document_scope.code_document_id if request.document_scope else None
        docs_doc_id = request.document_scope.docs_document_id if request.document_scope else None

        chunks = await retriever.retrieve(
            query=request.message,
            mode=mode,
            code_document_id=code_doc_id,
            docs_document_id=docs_doc_id,
        )

        if not chunks:
            return ChatResponse(
                mode=mode,
                answer=(
                    "I couldn't find sufficient information to answer your question. "
                    "Please try rephrasing or ensure relevant documents have been indexed."
                ),
                evidence=[],
                citations=[],
            )

        return await generator.generate_answer(query=request.message, chunks=chunks, mode=mode)
    except Exception as exc:
        raise HTTPException(status_code=500, detail=f"Chat processing failed: {exc}")


@router.post("/extract/code", response_model=ExtractResponse, summary="Extract code symbol")
async def extract_code(
    request: ExtractCodeRequest, retriever: Retriever = Depends(get_retriever)
):
    """Extract a specific function/class/method from code."""
    try:
        result = await retriever.extract_code(
            document_id=request.document_id,
            symbol_name=request.symbol_name,
            symbol_type=request.symbol_type,
        )
        return ExtractResponse(**result)
    except Exception as exc:
        raise HTTPException(status_code=500, detail=f"Code extraction failed: {exc}")


@router.post("/extract/docs", response_model=ExtractResponse, summary="Extract docs content")
async def extract_docs(
    request: ExtractDocsRequest, retriever: Retriever = Depends(get_retriever)
):
    """Extract a specific section or phrase from documents."""
    try:
        result = await retriever.extract_docs(
            document_id=request.document_id, query=request.query, mode=request.mode
        )
        return ExtractResponse(**result)
    except Exception as exc:
        raise HTTPException(status_code=500, detail=f"Document extraction failed: {exc}")


def determine_search_mode(message: str) -> str:
    """Determine search mode from message tags or content."""
    message_lower = message.lower()

    if "@codebase" in message_lower or "@code" in message_lower:
        return "code"
    if "@docs" in message_lower or "@gdd" in message_lower:
        return "docs"
    if "@both" in message_lower:
        return "both"

    code_signals = [
        "function",
        "class",
        "method",
        "import",
        "def ",
        "class ",
        "try:",
        "except:",
        "if __name__",
        "async def",
        ".py",
        ".js",
        ".ts",
        ".java",
        ".go",
        ".rs",
    ]
    code_score = sum(1 for signal in code_signals if signal in message_lower)

    return "code" if code_score >= 2 else "docs"


__all__ = ["router"]
