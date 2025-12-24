"""Pydantic models shared across API routes."""

from datetime import datetime
from typing import Any, Dict, List, Optional

from pydantic import BaseModel, Field


class DocumentScope(BaseModel):
    code_document_id: Optional[str] = None
    docs_document_id: Optional[str] = None


class ChatRequest(BaseModel):
    message: str = Field(..., description="User query message")
    document_scope: Optional[DocumentScope] = None


class Citation(BaseModel):
    id: str
    type: str  # 'code' or 'docs'
    path: Optional[str] = None
    page: Optional[int] = None
    start_line: Optional[int] = None
    end_line: Optional[int] = None
    heading: Optional[str] = None
    filename: Optional[str] = None


class Evidence(BaseModel):
    citation_id: str
    quote: str
    why: str


class ChatResponse(BaseModel):
    mode: str  # 'code', 'docs', or 'both'
    answer: str
    evidence: List[Evidence]
    citations: List[Citation]


class ExtractCodeRequest(BaseModel):
    document_id: str
    symbol_name: str
    symbol_type: str = Field(..., description="function, class, or method")


class ExtractDocsRequest(BaseModel):
    document_id: str
    query: str
    mode: str = "phrase"  # "phrase" or "section"


class ExtractResponse(BaseModel):
    found: bool
    extract: Optional[str] = None
    extracts: Optional[List[Dict[str, Any]]] = None
    citations: List[Citation]
    notes: Optional[str] = None


class JobResponse(BaseModel):
    status: str
    job_id: Optional[str] = None
    document_id: Optional[str] = None
    error: Optional[str] = None


class DocumentResponse(BaseModel):
    document_id: str
    job_id: str
    status: str


class HealthResponse(BaseModel):
    status: str
    timestamp: datetime
    version: str
    services: Dict[str, Any]


__all__ = [
    "ChatRequest",
    "ChatResponse",
    "Citation",
    "DocumentResponse",
    "DocumentScope",
    "Evidence",
    "ExtractCodeRequest",
    "ExtractDocsRequest",
    "ExtractResponse",
    "HealthResponse",
    "JobResponse",
]
