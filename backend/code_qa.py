"""
Main code QA functionality integrating all components.
Based on code_qa implementation.
"""

import os
import time
import logging
from typing import List, Dict, Any, Optional
from pathlib import Path
import json

from .indexing import code_indexer
from .reranking import colbert_reranker
from .prompts import prompt_manager
from .llm_providers import get_llm_provider

logger = logging.getLogger(__name__)


class CodeQA:
    """Main Code QA system integrating indexing, search, and chat."""

    def __init__(self):
        self.provider = get_llm_provider()
        self.indexed_codebases = set()

    def index_codebase(self, codebase_path: str, force_reindex: bool = False) -> Dict[str, Any]:
        """Index a codebase for search."""
        try:
            result = code_indexer.index_codebase(codebase_path, force_reindex)
            if result["status"] == "indexed":
                self.indexed_codebases.add(codebase_path)
            return result
        except Exception as e:
            return {"status": "error", "error": str(e)}

    def search_codebase(self, query: str, top_k: int = 10, use_reranking: bool = True) -> Dict[str, Any]:
        """Search the codebase with optional reranking."""
        start_time = time.time()

        try:
            # Basic semantic search
            results = code_indexer.search_codebase(query, top_k=top_k * 2)  # Get more for reranking

            # Apply reranking if enabled
            if use_reranking and results:
                try:
                    results = colbert_reranker.rerank(query, results, top_k=top_k)
                except Exception as e:
                    print(f"Reranking failed, using original results: {e}")

            # Limit to top_k
            results = results[:top_k]

            search_time = time.time() - start_time

            return {
                "status": "success",
                "query": query,
                "results": results,
                "count": len(results),
                "search_time": round(search_time, 2),
                "reranking_used": use_reranking
            }

        except Exception as e:
            return {
                "status": "error",
                "query": query,
                "error": str(e),
                "search_time": round(time.time() - start_time, 2)
            }

    def answer_question(self, query: str, use_codebase: bool = True) -> Dict[str, Any]:
        """Answer a question using the codebase."""
        start_time = time.time()

        try:
            # Check if query contains @codebase
            is_codebase_query = "@codebase" in query.lower()
            if is_codebase_query:
                query = query.lower().replace("@codebase", "").strip()

            # Search codebase if enabled or if @codebase is used
            contexts = []
            if use_codebase or is_codebase_query:
                search_result = self.search_codebase(query, top_k=5, use_reranking=True)
                if search_result["status"] == "success" and search_result["results"]:
                    contexts = search_result["results"]

            # Generate answer
            if contexts:
                # Use contexts from codebase
                prompt = prompt_manager.get_answer_generation_prompt(query, contexts)
                logger.info(f"[CodeQA] Calling LLM with prompt length: {len(prompt)}")
                answer = self.provider.llm(prompt=prompt)
                
                if not answer or not answer.strip():
                    logger.warning("[CodeQA] LLM returned empty answer, using fallback")
                    answer = "I found relevant code contexts but received an empty response. Please try rephrasing your question."

                return {
                    "status": "success",
                    "answer": answer.strip() if answer else "Empty response from LLM",
                    "contexts_used": len(contexts),
                    "contexts": contexts,
                    "query_type": "codebase",
                    "response_time": round(time.time() - start_time, 2)
                }
            else:
                # General answer without codebase context
                system_prompt = "You are a helpful AI assistant with expertise in software development and game design."
                logger.info(f"[CodeQA] Calling LLM for general question: {query[:50]}...")
                answer = self.provider.llm(prompt=query, system_prompt=system_prompt)
                
                if not answer or not answer.strip():
                    logger.warning("[CodeQA] LLM returned empty answer for general question")
                    answer = "I received an empty response. Please try rephrasing your question."

                return {
                    "status": "success",
                    "answer": answer.strip() if answer else "Empty response from LLM",
                    "contexts_used": 0,
                    "contexts": [],
                    "query_type": "general",
                    "response_time": round(time.time() - start_time, 2)
                }

        except Exception as e:
            return {
                "status": "error",
                "error": str(e),
                "response_time": round(time.time() - start_time, 2)
            }

    def get_stats(self) -> Dict[str, Any]:
        """Get system statistics."""
        return {
            "indexed_codebases": list(self.indexed_codebases),
            "index_stats": code_indexer.get_stats(),
            "provider": str(type(self.provider).__name__)
        }

    def clear_index(self) -> Dict[str, Any]:
        """Clear all indexed data."""
        try:
            code_indexer.clear_index()
            self.indexed_codebases.clear()
            return {"status": "success", "message": "Index cleared"}
        except Exception as e:
            return {"status": "error", "error": str(e)}


# Global CodeQA instance
code_qa = CodeQA()


