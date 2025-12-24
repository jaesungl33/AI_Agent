#!/usr/bin/env python3
"""
Database operations for Supabase PostgreSQL
"""

import json
import os
import uuid
from datetime import datetime
from typing import Any, Dict, List, Optional

from supabase import Client, create_client

from backend.app.config import get_settings

class Database:
    """Supabase database operations"""

    def __init__(self):
        """Initialize Supabase client"""
        settings = get_settings()
        self.supabase_url = settings.supabase_url
        self.supabase_key = settings.supabase_service_role_key or settings.supabase_anon_key

        if not self.supabase_url or not self.supabase_key:
            raise ValueError("Missing SUPABASE_URL or SUPABASE_SERVICE_ROLE_KEY/SUPABASE_ANON_KEY")

        self.client: Client = create_client(self.supabase_url, self.supabase_key)

    async def connect(self):
        """Connect to database (Supabase handles connection pooling)"""
        pass

    async def disconnect(self):
        """Disconnect from database"""
        pass

    async def health_check(self) -> bool:
        """Check database connectivity"""
        try:
            result = self.client.table('documents').select('count').limit(1).execute()
            return True
        except Exception:
            return False

    # Document operations
    async def create_document(self, document_id: str, doc_type: str, filename: str,
                            storage_path: str, sha256: str):
        """Create a document record"""
        data = {
            "id": document_id,
            "doc_type": doc_type,
            "filename": filename,
            "storage_path": storage_path,
            "sha256": sha256,
            "status": "uploaded"
        }

        result = self.client.table('documents').insert(data).execute()
        return result.data[0] if result.data else None

    async def get_document(self, document_id: str) -> Optional[Dict[str, Any]]:
        """Get a document by ID"""
        result = self.client.table('documents').select('*').eq('id', document_id).execute()
        return result.data[0] if result.data else None

    async def update_document_status(self, document_id: str, status: str, error: str = None):
        """Update document status"""
        data = {"status": status}
        if error:
            data["error"] = error

        self.client.table('documents').update(data).eq('id', document_id).execute()

    # File operations
    async def create_file(self, document_id: str, path: str, sha256: str,
                         language: str = None, size_bytes: int = None) -> str:
        """Create a file record"""
        file_id = str(uuid.uuid4())
        data = {
            "id": file_id,
            "document_id": document_id,
            "path": path,
            "sha256": sha256,
            "language": language,
            "size_bytes": size_bytes
        }

        self.client.table('files').insert(data).execute()
        return file_id

    async def get_file_by_path(self, document_id: str, path: str) -> Optional[Dict[str, Any]]:
        """Get a file by document ID and path"""
        result = self.client.table('files').select('*').eq('document_id', document_id).eq('path', path).execute()
        return result.data[0] if result.data else None

    # Chunk operations
    async def create_chunk(self, document_id: str, chunk_type: str, content: str,
                          embedding: List[float], metadata: Dict[str, Any],
                          file_id: str = None):
        """Create a chunk record"""
        data = {
            "document_id": document_id,
            "file_id": file_id,
            "chunk_type": chunk_type,
            "content": content,
            "embedding": embedding,
            "metadata": json.dumps(metadata)
        }

        result = self.client.table('chunks').insert(data).execute()
        return result.data[0] if result.data else None

    async def search_chunks_vector(self, embedding: List[float], chunk_type: str = None,
                                 document_id: str = None, limit: int = 20) -> List[Dict[str, Any]]:
        """Search chunks using vector similarity"""
        query = self.client.table('chunks').select('*')

        if chunk_type:
            query = query.eq('chunk_type', chunk_type)

        if document_id:
            query = query.eq('document_id', document_id)

        # Vector similarity search
        # Note: This requires pgvector extension and proper function setup in Supabase
        try:
            # Use RPC function for vector search (would need to be created in Supabase)
            result = self.client.rpc('vector_search', {
                'query_embedding': embedding,
                'chunk_type': chunk_type,
                'document_id': document_id,
                'limit': limit
            }).execute()
            return result.data
        except Exception:
            # Fallback to basic select if RPC function doesn't exist
            result = query.limit(limit).execute()
            return result.data

    async def search_chunks_lexical(self, query: str, chunk_type: str = None,
                                  document_id: str = None, limit: int = 20) -> List[Dict[str, Any]]:
        """Search chunks using lexical matching"""
        query = self.client.table('chunks').select('*')

        if chunk_type:
            query = query.eq('chunk_type', chunk_type)

        if document_id:
            query = query.eq('document_id', document_id)

        # Lexical search on content and metadata
        search_query = f"%{query}%"
        result = query.or_(f"content.ilike.{search_query},metadata->>'path'.ilike.{search_query}").limit(limit).execute()

        return result.data

    # Symbol operations
    async def create_symbol(self, document_id: str, file_id: str, path: str,
                           language: str, symbol_type: str, symbol_name: str,
                           start_line: int, end_line: int, signature: str = None):
        """Create a symbol record"""
        data = {
            "document_id": document_id,
            "file_id": file_id,
            "path": path,
            "language": language,
            "symbol_type": symbol_type,
            "symbol_name": symbol_name,
            "start_line": start_line,
            "end_line": end_line,
            "signature": signature
        }

        result = self.client.table('symbols').insert(data).execute()
        return result.data[0] if result.data else None

    async def find_symbol(self, document_id: str, symbol_name: str,
                         symbol_type: str = None) -> List[Dict[str, Any]]:
        """Find symbols by name and type"""
        query = self.client.table('symbols').select('*').eq('document_id', document_id).eq('symbol_name', symbol_name)

        if symbol_type:
            query = query.eq('symbol_type', symbol_type)

        result = query.execute()
        return result.data

    # Job operations
    async def create_job(self, job_id: str, job_type: str, document_id: str):
        """Create a job record"""
        data = {
            "id": job_id,
            "job_type": job_type,
            "document_id": document_id,
            "status": "queued"
        }

        self.client.table('jobs').insert(data).execute()

    async def get_job(self, job_id: str) -> Optional[Dict[str, Any]]:
        """Get a job by ID"""
        result = self.client.table('jobs').select('*').eq('id', job_id).execute()
        return result.data[0] if result.data else None

    async def get_next_queued_job(self) -> Optional[Dict[str, Any]]:
        """Get the next queued job"""
        result = self.client.table('jobs').select('*').eq('status', 'queued').order('created_at').limit(1).execute()
        return result.data[0] if result.data else None

    async def update_job_status(self, job_id: str, status: str, error: str = None):
        """Update job status"""
        data = {"status": status}
        if error:
            data["error"] = error

        self.client.table('jobs').update(data).eq('id', job_id).execute()

    async def get_recent_jobs(self, limit: int = 20) -> List[Dict[str, Any]]:
        """Get recent jobs"""
        result = self.client.table('jobs').select('*').order('created_at', desc=True).limit(limit).execute()
        return result.data
