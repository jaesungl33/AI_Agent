#!/usr/bin/env python3
"""
Grounded answer generation with citations
"""

from typing import Any, Dict, List

# LLM client
try:
    from openai import OpenAI
    OPENAI_AVAILABLE = True
except ImportError:
    OPENAI_AVAILABLE = False

class Generator:
    """Handles LLM-based answer generation with proper grounding"""

    def __init__(self):
        """Initialize LLM client"""
        if OPENAI_AVAILABLE:
            from backend.app.config import get_settings

            settings = get_settings()
            self.client = OpenAI(api_key=settings.openai_api_key)
        else:
            self.client = None

    async def generate_answer(self, query: str, chunks: List[Dict[str, Any]],
                            mode: str) -> Dict[str, Any]:
        """Generate a grounded answer with citations"""
        if not chunks:
            return self._create_empty_response(mode)

        if not self.client:
            return self._create_fallback_response(query, chunks, mode)

        try:
            # Build context from chunks
            context = self._build_context(chunks)

            # Generate answer
            prompt = self._build_generation_prompt(query, context, mode)

            response = self.client.chat.completions.create(
                model="gpt-4o-mini",
                messages=[{"role": "user", "content": prompt}],
                temperature=0.1,
                max_tokens=1000
            )

            answer_text = response.choices[0].message.content

            # Extract citations from the answer
            citations = self._extract_citations_from_answer(answer_text, chunks)

            # Build evidence from citations
            evidence = self._build_evidence(answer_text, citations, chunks)

            return {
                "mode": mode,
                "answer": answer_text,
                "evidence": evidence,
                "citations": citations
            }

        except Exception as e:
            print(f"Generation error: {e}")
            return self._create_fallback_response(query, chunks, mode)

    def _build_context(self, chunks: List[Dict[str, Any]]) -> str:
        """Build context string from retrieved chunks"""
        context_parts = []

        for i, chunk in enumerate(chunks):
            metadata = chunk.get('metadata', {})
            source_info = ""

            if chunk.get('chunk_type') == 'code':
                source_info = f"[SOURCE code {metadata.get('path', 'unknown')}:{metadata.get('start_line', 0)}-{metadata.get('end_line', 0)}]"
            else:
                source_info = f"[SOURCE docs {metadata.get('filename', 'unknown')} page {metadata.get('page', 0)}]"

            context_parts.append(f"{source_info}\n{chunk.get('content', '')}")

        return "\n\n".join(context_parts)

    def _build_generation_prompt(self, query: str, context: str, mode: str) -> str:
        """Build the generation prompt with grounding instructions"""
        return f"""You are a helpful AI assistant that answers questions about code and documentation.

IMPORTANT RULES:
1. Use ONLY the provided context to answer the question
2. Every claim you make MUST be backed by a citation from the sources
3. If the context doesn't contain enough information, say "Insufficient context" and explain what's missing
4. Do NOT make up information or use external knowledge
5. Cite sources using the format: [SOURCE:X] where X is the source number
6. For code questions, focus on the actual implementation details
7. For documentation questions, provide direct quotes when relevant

QUESTION: {query}

CONTEXT:
{context}

Provide a clear, direct answer with citations. If you cannot answer based on the provided context, say so explicitly."""

    def _extract_citations_from_answer(self, answer: str, chunks: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """Extract citation references from the generated answer"""
        citations = []

        # Find all [SOURCE:X] references in the answer
        import re
        source_refs = re.findall(r'\[SOURCE:(\d+)\]', answer)

        for ref in source_refs:
            try:
                idx = int(ref)
                if idx < len(chunks):
                    chunk = chunks[idx]
                    metadata = chunk.get('metadata', {})

                    citation = {
                        "id": f"c{idx}",
                        "type": chunk.get('chunk_type', 'unknown')
                    }

                    if citation['type'] == 'code':
                        citation.update({
                            "path": metadata.get('path', 'unknown'),
                            "start_line": metadata.get('start_line'),
                            "end_line": metadata.get('end_line')
                        })
                    else:
                        citation.update({
                            "page": metadata.get('page'),
                            "filename": metadata.get('filename', 'unknown')
                        })

                    citations.append(citation)

            except (ValueError, IndexError):
                continue

        return citations

    def _build_evidence(self, answer: str, citations: List[Dict[str, Any]],
                       chunks: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """Build evidence list linking answer parts to citations"""
        evidence = []

        for citation in citations:
            citation_id = citation['id']

            # Find the corresponding chunk
            chunk_idx = int(citation_id[1:])  # Remove 'c' prefix
            if chunk_idx < len(chunks):
                chunk = chunks[chunk_idx]

                # Create a quote that supports the answer
                content = chunk.get('content', '')
                quote = content[:200] + "..." if len(content) > 200 else content

                evidence.append({
                    "citation_id": citation_id,
                    "quote": quote,
                    "why": f"Provides {'code implementation' if citation['type'] == 'code' else 'documentation'} details relevant to the answer"
                })

        return evidence

    def _create_empty_response(self, mode: str) -> Dict[str, Any]:
        """Create response when no chunks are available"""
        return {
            "mode": mode,
            "answer": "I couldn't find any relevant information to answer your question. Please try rephrasing or ensure that relevant documents have been indexed.",
            "evidence": [],
            "citations": []
        }

    def _create_fallback_response(self, query: str, chunks: List[Dict[str, Any]],
                                mode: str) -> Dict[str, Any]:
        """Create fallback response when LLM is not available"""
        # Simple keyword-based answer generation
        answer = f"I found {len(chunks)} relevant pieces of information related to your query: '{query}'."

        if mode == "code":
            answer += " These appear to be code-related results."
        elif mode == "docs":
            answer += " These appear to be documentation results."

        citations = []
        evidence = []

        for i, chunk in enumerate(chunks[:3]):  # Limit to top 3
            metadata = chunk.get('metadata', {})

            citation = {
                "id": f"c{i}",
                "type": chunk.get('chunk_type', 'unknown')
            }

            if citation['type'] == 'code':
                citation.update({
                    "path": metadata.get('path', 'unknown'),
                    "start_line": metadata.get('start_line'),
                    "end_line": metadata.get('end_line')
                })
            else:
                citation.update({
                    "page": metadata.get('page'),
                    "filename": metadata.get('filename', 'unknown')
                })

            citations.append(citation)

            # Create evidence
            content = chunk.get('content', '')
            quote = content[:150] + "..." if len(content) > 150 else content

            evidence.append({
                "citation_id": f"c{i}",
                "quote": quote,
                "why": "Contains relevant information for the query"
            })

        return {
            "mode": mode,
            "answer": answer,
            "evidence": evidence,
            "citations": citations
        }
