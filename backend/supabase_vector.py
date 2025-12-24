#!/usr/bin/env python3
"""
Supabase Vector Search Integration
Handles vector storage and retrieval for code search functionality
"""

import os
import json
import logging
from typing import List, Dict, Any, Optional
from supabase import create_client, Client

logger = logging.getLogger(__name__)

class SupabaseVectorStore:
    """Supabase vector store for code embeddings and search."""

    def __init__(self):
        """Initialize Supabase client."""
        self.supabase_url = os.getenv('SUPABASE_URL')
        self.supabase_key = os.getenv('SUPABASE_ANON_KEY')
        self.supabase_service_key = os.getenv('SUPABASE_SERVICE_ROLE_KEY')

        if not all([self.supabase_url, self.supabase_key]):
            raise ValueError("Supabase credentials not found. Set SUPABASE_URL and SUPABASE_ANON_KEY")

        # Use service role key for admin operations if available
        key = self.supabase_service_key or self.supabase_key
        self.client: Client = create_client(self.supabase_url, key)

        # Test connection
        try:
            self.client.table('code_chunks').select('count').limit(1).execute()
            logger.info("Supabase vector store connected")
        except Exception as e:
            logger.warning(f"Supabase table not found or error: {e}")

    def store_code_chunks(self, chunks: List[Dict[str, Any]], workspace_id: str = "tank_war") -> bool:
        """Store code chunks with embeddings in Supabase."""
        try:
            # Prepare data for Supabase
            records = []
            for chunk in chunks:
                record = {
                    'workspace_id': workspace_id,
                    'file_path': chunk.get('file_path', ''),
                    'content': chunk.get('content', ''),
                    'start_line': chunk.get('start_line', 0),
                    'end_line': chunk.get('end_line', 0),
                    'language': chunk.get('language', 'unknown'),
                    'embedding': chunk.get('embedding', []),
                    'metadata': json.dumps(chunk.get('metadata', {})),
                    'created_at': 'now()'
                }
                records.append(record)

            # Insert in batches to avoid payload size limits
            batch_size = 100
            for i in range(0, len(records), batch_size):
                batch = records[i:i + batch_size]
                self.client.table('code_chunks').insert(batch).execute()

            logger.info(f"Stored {len(records)} code chunks in Supabase")
            return True

        except Exception as e:
            logger.error(f"Error storing code chunks: {e}")
            return False

    def search_similar(self, query_embedding: List[float], workspace_id: str = "tank_war",
                      top_k: int = 5, threshold: float = 0.1) -> List[Dict[str, Any]]:
        """Search for similar code chunks using vector similarity."""
        try:
            # Use Supabase's vector similarity search
            # Note: This assumes you have pgvector extension and proper function setup
            query = f"""
            SELECT file_path, content, start_line, end_line, language, metadata,
                   1 - (embedding <=> '{query_embedding}') as similarity
            FROM code_chunks
            WHERE workspace_id = '{workspace_id}'
            ORDER BY embedding <=> '{query_embedding}'
            LIMIT {top_k}
            """

            result = self.client.rpc('vector_search', {
                'query_embedding': query_embedding,
                'workspace_id': workspace_id,
                'top_k': top_k
            }).execute()

            # Format results
            hits = []
            for row in result.data:
                hit = {
                    'file_path': row['file_path'],
                    'content': row['content'],
                    'start_line': row['start_line'],
                    'end_line': row['end_line'],
                    'language': row['language'],
                    'metadata': json.loads(row['metadata']) if row['metadata'] else {},
                    'score': float(row.get('similarity', 0))
                }
                hits.append(hit)

            return hits

        except Exception as e:
            logger.error(f"Error searching vectors: {e}")
            return []

    def store_gdd_chunks(self, chunks: List[Dict[str, Any]], workspace_id: str = "tank_war") -> bool:
        """Store GDD document chunks with embeddings."""
        try:
            records = []
            for chunk in chunks:
                record = {
                    'workspace_id': workspace_id,
                    'doc_id': chunk.get('doc_id', ''),
                    'doc_name': chunk.get('doc_name', ''),
                    'content': chunk.get('content', ''),
                    'page_number': chunk.get('page_number', 0),
                    'chunk_index': chunk.get('chunk_index', 0),
                    'embedding': chunk.get('embedding', []),
                    'metadata': json.dumps(chunk.get('metadata', {})),
                    'created_at': 'now()'
                }
                records.append(record)

            # Insert in batches
            batch_size = 100
            for i in range(0, len(records), batch_size):
                batch = records[i:i + batch_size]
                self.client.table('gdd_chunks').insert(batch).execute()

            logger.info(f"Stored {len(records)} GDD chunks in Supabase")
            return True

        except Exception as e:
            logger.error(f"Error storing GDD chunks: {e}")
            return False

    def search_gdd_similar(self, query_embedding: List[float], workspace_id: str = "tank_war",
                          top_k: int = 5) -> List[Dict[str, Any]]:
        """Search for similar GDD chunks."""
        try:
            result = self.client.rpc('gdd_vector_search', {
                'query_embedding': query_embedding,
                'workspace_id': workspace_id,
                'top_k': top_k
            }).execute()

            hits = []
            for row in result.data:
                hit = {
                    'doc_id': row['doc_id'],
                    'doc_name': row['doc_name'],
                    'content': row['content'],
                    'page_number': row['page_number'],
                    'metadata': json.loads(row['metadata']) if row['metadata'] else {},
                    'score': float(row.get('similarity', 0))
                }
                hits.append(hit)

            return hits

        except Exception as e:
            logger.error(f"Error searching GDD vectors: {e}")
            return []

    def get_workspace_stats(self, workspace_id: str = "tank_war") -> Dict[str, int]:
        """Get workspace statistics."""
        try:
            # Get code chunks count
            code_result = self.client.table('code_chunks').select('count', count='exact').eq('workspace_id', workspace_id).execute()
            code_count = code_result.count

            # Get GDD chunks count
            gdd_result = self.client.table('gdd_chunks').select('count', count='exact').eq('workspace_id', workspace_id).execute()
            gdd_count = gdd_result.count

            # Get unique documents
            docs_result = self.client.table('gdd_chunks').select('doc_id', count='exact').eq('workspace_id', workspace_id).execute()
            doc_count = len(set(row['doc_id'] for row in docs_result.data))

            return {
                'code_chunks': code_count,
                'gdd_chunks': gdd_count,
                'documents': doc_count
            }

        except Exception as e:
            logger.error(f"Error getting workspace stats: {e}")
            return {'code_chunks': 0, 'gdd_chunks': 0, 'documents': 0}


# Global instance
supabase_store = None

def get_supabase_store() -> Optional[SupabaseVectorStore]:
    """Get or create Supabase vector store instance."""
    global supabase_store
    if supabase_store is None:
        try:
            supabase_store = SupabaseVectorStore()
        except Exception as e:
            logger.warning(f"Could not initialize Supabase store: {e}")
            return None
    return supabase_store
