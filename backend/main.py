#!/usr/bin/env python3
"""
Chat-First RAG Backend - FastAPI Implementation
Based on technical specification for Render + Supabase deployment
"""

import os
import uuid
import asyncio
from typing import Optional, List, Dict, Any
from contextlib import asynccontextmanager
from datetime import datetime

import uvicorn
from fastapi import FastAPI, UploadFile, File, HTTPException, Depends, BackgroundTasks
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
from dotenv import load_dotenv

# Load environment variables (ignore if file doesn't exist or permission denied)
try:
    load_dotenv()
except (PermissionError, OSError):
    print("Warning: Could not load .env file, using environment variables only")

# Import our modules
from database import Database
from storage import Storage
from indexing import Indexer
from retrieval import Retriever
from generation import Generator

# Initialize components
db = Database()
storage = Storage()
indexer = Indexer(db, storage)
retriever = Retriever(db)
generator = Generator()

@asynccontextmanager
async def lifespan(app: FastAPI):
    """Application lifespan manager"""
    # Startup
    await db.connect()
    yield
    # Shutdown
    await db.disconnect()

app = FastAPI(
    title="AI Agent - Chat-First RAG",
    description="Retrieval-Augmented Generation system for code and documentation analysis",
    version="1.0.0",
    lifespan=lifespan
)

# CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:3000", "http://127.0.0.1:3000"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Pydantic models
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

# Routes
@app.get("/")
async def root():
    """Health check endpoint"""
    return {"status": "ok", "service": "AI Agent RAG Backend"}

@app.get("/health")
async def health():
    """Detailed health check"""
    return {
        "status": "ok",
        "timestamp": datetime.utcnow().isoformat(),
        "version": "1.0.0",
        "services": {
            "database": await db.health_check(),
            "storage": storage.health_check()
        }
    }

@app.post("/ingest/docs", response_model=DocumentResponse)
async def ingest_docs(
    background_tasks: BackgroundTasks,
    file: UploadFile = File(...)
):
    """Upload and queue a PDF document for indexing"""
    if not file.filename.lower().endswith('.pdf'):
        raise HTTPException(400, "Only PDF files are supported")

    try:
        # Generate IDs
        document_id = str(uuid.uuid4())
        job_id = str(uuid.uuid4())

        # Upload file to storage
        file_content = await file.read()
        storage_path = await storage.upload_file(
            file_content,
            f"docs/{document_id}/{file.filename}"
        )

        # Create document record
        await db.create_document(
            document_id=document_id,
            doc_type="docs",
            filename=file.filename,
            storage_path=storage_path,
            sha256=""  # TODO: compute SHA256
        )

        # Create indexing job
        await db.create_job(
            job_id=job_id,
            job_type="index_docs",
            document_id=document_id
        )

        # Queue background indexing
        background_tasks.add_task(process_indexing_job, job_id)

        return DocumentResponse(
            document_id=document_id,
            job_id=job_id,
            status="queued"
        )

    except Exception as e:
        raise HTTPException(500, f"Failed to ingest document: {str(e)}")

@app.post("/ingest/code", response_model=DocumentResponse)
async def ingest_code(
    background_tasks: BackgroundTasks,
    file: UploadFile = File(...)
):
    """Upload and queue a code repository (ZIP) for indexing"""
    if not file.filename.lower().endswith('.zip'):
        raise HTTPException(400, "Only ZIP files are supported")

    try:
        # Generate IDs
        document_id = str(uuid.uuid4())
        job_id = str(uuid.uuid4())

        # Upload file to storage
        file_content = await file.read()
        storage_path = await storage.upload_file(
            file_content,
            f"code/{document_id}/{file.filename}"
        )

        # Create document record
        await db.create_document(
            document_id=document_id,
            doc_type="code",
            filename=file.filename,
            storage_path=storage_path,
            sha256=""  # TODO: compute SHA256
        )

        # Create indexing job
        await db.create_job(
            job_id=job_id,
            job_type="index_code",
            document_id=document_id
        )

        # Queue background indexing
        background_tasks.add_task(process_indexing_job, job_id)

        return DocumentResponse(
            document_id=document_id,
            job_id=job_id,
            status="queued"
        )

    except Exception as e:
        raise HTTPException(500, f"Failed to ingest code: {str(e)}")

@app.post("/admin/run-job", response_model=JobResponse)
async def run_job():
    """Run the next queued indexing job"""
    try:
        job = await db.get_next_queued_job()
        if not job:
            return JobResponse(status="empty")

        # Mark job as running
        await db.update_job_status(job["id"], "running")

        try:
            # Process the job
            await process_indexing_job(job["id"])

            # Mark as done
            await db.update_job_status(job["id"], "done")

            return JobResponse(
                status="done",
                job_id=job["id"],
                document_id=job["document_id"]
            )

        except Exception as e:
            # Mark as failed
            await db.update_job_status(job["id"], "failed", str(e))

            return JobResponse(
                status="failed",
                job_id=job["id"],
                document_id=job["document_id"],
                error=str(e)
            )

    except Exception as e:
        raise HTTPException(500, f"Failed to run job: {str(e)}")

@app.get("/jobs")
async def get_jobs(limit: int = 20):
    """Get recent jobs"""
    try:
        jobs = await db.get_recent_jobs(limit)
        return {"jobs": jobs}
    except Exception as e:
        raise HTTPException(500, f"Failed to get jobs: {str(e)}")

@app.post("/chat", response_model=ChatResponse)
async def chat(request: ChatRequest):
    """Main chat endpoint with RAG retrieval and generation"""
    try:
        # Determine search mode
        mode = determine_search_mode(request.message)

        # Get document scope
        code_doc_id = request.document_scope.code_document_id if request.document_scope else None
        docs_doc_id = request.document_scope.docs_document_id if request.document_scope else None

        # Retrieve relevant chunks
        chunks = await retriever.retrieve(
            query=request.message,
            mode=mode,
            code_document_id=code_doc_id,
            docs_document_id=docs_doc_id
        )

        if not chunks:
            return ChatResponse(
                mode=mode,
                answer="I couldn't find sufficient information to answer your question. Please try rephrasing or ensure relevant documents have been indexed.",
                evidence=[],
                citations=[]
            )

        # Generate grounded response
        response = await generator.generate_answer(
            query=request.message,
            chunks=chunks,
            mode=mode
        )

        return response

    except Exception as e:
        raise HTTPException(500, f"Chat processing failed: {str(e)}")

@app.post("/extract/code", response_model=ExtractResponse)
async def extract_code(request: ExtractCodeRequest):
    """Extract a specific function/class/method from code"""
    try:
        result = await retriever.extract_code(
            document_id=request.document_id,
            symbol_name=request.symbol_name,
            symbol_type=request.symbol_type
        )

        return ExtractResponse(**result)

    except Exception as e:
        raise HTTPException(500, f"Code extraction failed: {str(e)}")

@app.post("/extract/docs", response_model=ExtractResponse)
async def extract_docs(request: ExtractDocsRequest):
    """Extract a specific section or phrase from documents"""
    try:
        result = await retriever.extract_docs(
            document_id=request.document_id,
            query=request.query,
            mode=request.mode
        )

        return ExtractResponse(**result)

    except Exception as e:
        raise HTTPException(500, f"Document extraction failed: {str(e)}")

# Helper functions
def determine_search_mode(message: str) -> str:
    """Determine search mode from message tags or content"""
    message_lower = message.lower()

    # Check for explicit tags
    if "@codebase" in message_lower or "@code" in message_lower:
        return "code"
    elif "@docs" in message_lower or "@gdd" in message_lower:
        return "docs"
    elif "@both" in message_lower:
        return "both"

    # Auto-detect based on content
    code_signals = [
        "function", "class", "method", "import", "def ", "class ",
        "try:", "except:", "if __name__", "async def",
        ".py", ".js", ".ts", ".java", ".go", ".rs"
    ]

    code_score = sum(1 for signal in code_signals if signal in message_lower)

    if code_score >= 2:
        return "code"
    else:
        return "docs"

async def process_indexing_job(job_id: str):
    """Process an indexing job in the background"""
    try:
        # Get job details
        job = await db.get_job(job_id)
        if not job:
            return

        document = await db.get_document(job["document_id"])
        if not document:
            return

        # Download file from storage
        file_content = await storage.download_file(document["storage_path"])

        # Index based on type
        if job["job_type"] == "index_docs":
            await indexer.index_pdf(document, file_content)
        elif job["job_type"] == "index_code":
            await indexer.index_zip(document, file_content)

    except Exception as e:
        # Update job with error
        await db.update_job_status(job_id, "failed", str(e))
        raise

if __name__ == "__main__":
    port = int(os.getenv("PORT", 8000))
    uvicorn.run(
        "main:app",
        host="0.0.0.0",
        port=port,
        reload=os.getenv("DEBUG", "false").lower() == "true"
    )