#!/usr/bin/env python3
"""
Retrieval system with hybrid search and reranking
"""

import os
import re
from typing import List, Dict, Any, Optional
from collections import defaultdict

# Embeddings for query encoding
try:
    from sentence_transformers import SentenceTransformer
    EMBEDDINGS_AVAILABLE = True
except ImportError:
    EMBEDDINGS_AVAILABLE = False

# LLM for reranking
try:
    from openai import OpenAI
    OPENAI_AVAILABLE = True
except ImportError:
    OPENAI_AVAILABLE = False

from .database import Database

class Retriever:
    """Handles document and code retrieval with hybrid search"""

    def __init__(self, db: Database):
        self.db = db

        # Initialize embedding model for queries
        if EMBEDDINGS_AVAILABLE:
            self.embedding_model = SentenceTransformer('all-MiniLM-L6-v2')
        else:
            self.embedding_model = None

        # Initialize LLM for reranking
        if OPENAI_AVAILABLE:
            self.llm_client = OpenAI(api_key=os.getenv("OPENAI_API_KEY"))
        else:
            self.llm_client = None

    async def retrieve(self, query: str, mode: str = "docs",
                      code_document_id: str = None, docs_document_id: str = None,
                      top_k: int = 10) -> List[Dict[str, Any]]:
        """Main retrieval function with hybrid search and reranking"""
        # Determine which document types to search
        search_code = mode in ["code", "both"]
        search_docs = mode in ["docs", "both"]

        all_candidates = []

        # Get embeddings for query
        if self.embedding_model:
            query_embedding = self.embedding_model.encode(query).tolist()
        else:
            query_embedding = [0.1] * 384  # Placeholder

        # Vector search
        if search_code and code_document_id:
            code_chunks = await self.db.search_chunks_vector(
                embedding=query_embedding,
                chunk_type="code",
                document_id=code_document_id,
                limit=20
            )
            all_candidates.extend(code_chunks)

        if search_docs and docs_document_id:
            docs_chunks = await self.db.search_chunks_vector(
                embedding=query_embedding,
                chunk_type="docs",
                document_id=docs_document_id,
                limit=20
            )
            all_candidates.extend(docs_chunks)

        # Lexical search
        lexical_candidates = []
        if search_code and code_document_id:
            code_lexical = await self.db.search_chunks_lexical(
                query=query,
                chunk_type="code",
                document_id=code_document_id,
                limit=20
            )
            lexical_candidates.extend(code_lexical)

        if search_docs and docs_document_id:
            docs_lexical = await self.db.search_chunks_lexical(
                query=query,
                chunk_type="docs",
                document_id=docs_document_id,
                limit=20
            )
            lexical_candidates.extend(docs_lexical)

        # Merge and deduplicate candidates
        merged_candidates = self._merge_candidates(all_candidates, lexical_candidates)

        # Apply diversity cap (max 4 chunks per file)
        diverse_candidates = self._apply_diversity_cap(merged_candidates)

        # Rerank if LLM available
        if self.llm_client and len(diverse_candidates) > top_k:
            reranked_candidates = await self._rerank_candidates(query, diverse_candidates[:20])
            final_candidates = reranked_candidates[:top_k]
        else:
            final_candidates = diverse_candidates[:top_k]

        return final_candidates

    def _merge_candidates(self, vector_results: List[Dict[str, Any]],
                         lexical_results: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """Merge vector and lexical results, removing duplicates"""
        seen_ids = set()
        merged = []

        # Add vector results first (they have similarity scores)
        for chunk in vector_results:
            chunk_id = chunk.get('id')
            if chunk_id not in seen_ids:
                chunk['_source'] = 'vector'
                merged.append(chunk)
                seen_ids.add(chunk_id)

        # Add lexical results if not already present
        for chunk in lexical_results:
            chunk_id = chunk.get('id')
            if chunk_id not in seen_ids:
                chunk['_source'] = 'lexical'
                merged.append(chunk)
                seen_ids.add(chunk_id)

        return merged

    def _apply_diversity_cap(self, candidates: List[Dict[str, Any]],
                           max_per_file: int = 4) -> List[Dict[str, Any]]:
        """Apply diversity cap to prevent one file from dominating results"""
        file_counts = defaultdict(int)
        diverse = []

        for candidate in candidates:
            metadata = candidate.get('metadata', {})
            file_path = metadata.get('path', 'unknown')

            if file_counts[file_path] < max_per_file:
                diverse.append(candidate)
                file_counts[file_path] += 1

        return diverse

    async def _rerank_candidates(self, query: str, candidates: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """Rerank candidates using LLM"""
        if not self.llm_client:
            return candidates

        try:
            # Prepare reranking prompt
            rerank_prompt = self._build_rerank_prompt(query, candidates)

            # Call LLM
            response = self.llm_client.chat.completions.create(
                model="gpt-4o-mini",
                messages=[{"role": "user", "content": rerank_prompt}],
                temperature=0.1
            )

            # Parse reranking results
            reranked_indices = self._parse_rerank_response(response.choices[0].message.content)

            # Reorder candidates based on reranking
            reranked_candidates = []
            for idx in reranked_indices:
                if idx < len(candidates):
                    reranked_candidates.append(candidates[idx])

            # Add any remaining candidates
            for i, candidate in enumerate(candidates):
                if i not in reranked_indices:
                    reranked_candidates.append(candidate)

            return reranked_candidates

        except Exception as e:
            print(f"Reranking failed: {e}")
            return candidates

    def _build_rerank_prompt(self, query: str, candidates: List[Dict[str, Any]]) -> str:
        """Build reranking prompt for LLM"""
        prompt = f"""Given the query: "{query}"

Please rank these document chunks by relevance (most relevant first).
Score each chunk 0-3 where:
- 3 = Highly relevant, directly answers the query
- 2 = Moderately relevant, contains useful information
- 1 = Somewhat relevant, tangentially related
- 0 = Not relevant

Return only a comma-separated list of indices in order of relevance (e.g., "2,0,1,3").

Chunks:
"""

        for i, chunk in enumerate(candidates):
            content_preview = chunk.get('content', '')[:200] + "..."
            metadata = chunk.get('metadata', {})
            source_info = ""

            if chunk.get('chunk_type') == 'code':
                source_info = f"Code: {metadata.get('path', 'unknown')}:{metadata.get('start_line', 0)}"
            else:
                source_info = f"Doc: page {metadata.get('page', 0)}"

            prompt += f"\n[{i}] {source_info}\n{content_preview}\n"

        return prompt

    def _parse_rerank_response(self, response: str) -> List[int]:
        """Parse reranking response from LLM"""
        try:
            # Extract indices from response
            indices = []
            for part in response.split(','):
                part = part.strip()
                if part.isdigit():
                    indices.append(int(part))

            return indices

        except Exception:
            # Return original order if parsing fails
            return list(range(len(response.split(','))))

    async def extract_code(self, document_id: str, symbol_name: str,
                          symbol_type: str) -> Dict[str, Any]:
        """Extract a specific function/class/method from code"""
        try:
            # Try to find in symbols table first
            symbols = await self.db.find_symbol(document_id, symbol_name, symbol_type)

            if symbols:
                # Get the most relevant symbol (first match)
                symbol = symbols[0]

                # Get the chunk containing this symbol
                chunks = await self.db.search_chunks_lexical(
                    query=f"{symbol_type} {symbol_name}",
                    chunk_type="code",
                    document_id=document_id,
                    limit=1
                )

                if chunks:
                    chunk = chunks[0]
                    return {
                        "found": True,
                        "extract": chunk['content'],
                        "citations": [{
                            "type": "code",
                            "path": symbol['path'],
                            "start_line": symbol['start_line'],
                            "end_line": symbol['end_line']
                        }],
                        "notes": f"Extracted {symbol_type} '{symbol_name}'"
                    }

            # Fallback: lexical search
            chunks = await self.db.search_chunks_lexical(
                query=f"def {symbol_name}" if symbol_type == "function" else f"class {symbol_name}",
                chunk_type="code",
                document_id=document_id,
                limit=1
            )

            if chunks:
                chunk = chunks[0]
                metadata = chunk.get('metadata', {})
                return {
                    "found": False,  # Not found in symbols table
                    "extract": chunk['content'],
                    "citations": [{
                        "type": "code",
                        "path": metadata.get('path', 'unknown'),
                        "start_line": metadata.get('start_line', 0),
                        "end_line": metadata.get('end_line', 0)
                    }],
                    "notes": f"Found via lexical search - may not be exact {symbol_type}"
                }

            return {
                "found": False,
                "extract": None,
                "citations": [],
                "notes": f"Symbol '{symbol_name}' not found"
            }

        except Exception as e:
            return {
                "found": False,
                "extract": None,
                "citations": [],
                "notes": f"Error during extraction: {str(e)}"
            }

    async def extract_docs(self, document_id: str, query: str,
                          mode: str = "phrase") -> Dict[str, Any]:
        """Extract a specific section or phrase from documents"""
        try:
            # Search for chunks containing the query
            chunks = await self.db.search_chunks_lexical(
                query=query,
                chunk_type="docs",
                document_id=document_id,
                limit=5
            )

            if not chunks:
                return {
                    "found": False,
                    "extracts": None,
                    "citations": [],
                    "notes": f"No content found matching '{query}'"
                }

            extracts = []
            citations = []

            for chunk in chunks:
                # Find the specific phrase/section in the chunk
                content_lower = chunk['content'].lower()
                query_lower = query.lower()

                if query_lower in content_lower:
                    # Extract context around the match
                    start_idx = content_lower.find(query_lower)
                    end_idx = start_idx + len(query_lower)

                    # Get broader context (up to 500 chars around match)
                    context_start = max(0, start_idx - 250)
                    context_end = min(len(chunk['content']), end_idx + 250)

                    extract_text = chunk['content'][context_start:context_end]
                    if context_start > 0:
                        extract_text = "..." + extract_text
                    if context_end < len(chunk['content']):
                        extract_text = extract_text + "..."

                    extracts.append({
                        "text": extract_text,
                        "page": chunk.get('metadata', {}).get('page', 0),
                        "heading": chunk.get('metadata', {}).get('heading')
                    })

                    citations.append({
                        "type": "docs",
                        "page": chunk.get('metadata', {}).get('page', 0),
                        "filename": chunk.get('metadata', {}).get('filename', 'unknown')
                    })

            return {
                "found": len(extracts) > 0,
                "extracts": extracts if extracts else None,
                "citations": citations,
                "notes": f"Found {len(extracts)} matching extracts"
            }

        except Exception as e:
            return {
                "found": False,
                "extracts": None,
                "citations": [],
                "notes": f"Error during extraction: {str(e)}"
            }
