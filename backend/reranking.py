"""
ColBERT-based reranking for code search results.
Based on code_qa implementation.
"""

from typing import List, Dict, Any
import torch
from transformers import AutoTokenizer, AutoModel
import numpy as np


class ColBERTReranker:
    """Rerank search results using ColBERT."""

    def __init__(self, model_name: str = "colbert-ir/colbertv2.0"):
        self.model_name = model_name
        self.tokenizer = None
        self.model = None
        self.device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')

    def load_model(self):
        """Load the ColBERT model and tokenizer."""
        if self.model is None:
            print(f"Loading ColBERT model: {self.model_name}")
            self.tokenizer = AutoTokenizer.from_pretrained(self.model_name)
            self.model = AutoModel.from_pretrained(self.model_name)
            self.model.to(self.device)
            self.model.eval()
            print("ColBERT model loaded successfully")

    def rerank(self, query: str, documents: List[Dict[str, Any]], top_k: int = 10) -> List[Dict[str, Any]]:
        """Rerank documents based on query using ColBERT."""
        if not documents:
            return []

        if self.model is None:
            self.load_model()

        # Prepare query and documents
        query_tokens = self._tokenize_query(query)
        doc_tokens = [self._tokenize_doc(doc.get('content', '')) for doc in documents]

        # Compute scores
        scores = []
        with torch.no_grad():
            # Encode query
            query_input = self.tokenizer(query, return_tensors='pt', truncation=True, max_length=512)
            query_input = {k: v.to(self.device) for k, v in query_input.items()}
            query_output = self.model(**query_input)
            query_embeds = query_output.last_hidden_state.mean(dim=1)  # Average pooling

            # Encode documents
            for doc_tokens_batch in self._batch_documents(doc_tokens, batch_size=8):
                doc_inputs = self.tokenizer(doc_tokens_batch, return_tensors='pt', truncation=True,
                                           max_length=512, padding=True)
                doc_inputs = {k: v.to(self.device) for k, v in doc_inputs.items()}
                doc_outputs = self.model(**doc_inputs)
                doc_embeds = doc_outputs.last_hidden_state.mean(dim=1)  # Average pooling

                # Compute similarity scores
                batch_scores = torch.cosine_similarity(query_embeds.unsqueeze(1), doc_embeds.unsqueeze(0), dim=2)
                max_scores = batch_scores.max(dim=1)[0]  # Max similarity per query
                scores.extend(max_scores.cpu().numpy())

        # Add scores to documents and sort
        for i, doc in enumerate(documents):
            doc['rerank_score'] = float(scores[i]) if i < len(scores) else 0.0

        # Sort by rerank score (descending)
        reranked = sorted(documents, key=lambda x: x.get('rerank_score', 0), reverse=True)

        return reranked[:top_k]

    def _tokenize_query(self, query: str) -> str:
        """Tokenize query for ColBERT."""
        # ColBERT works with raw text, but we can do some basic preprocessing
        return query.strip()

    def _tokenize_doc(self, doc: str) -> str:
        """Tokenize document for ColBERT."""
        # Truncate long documents
        if len(doc) > 1000:
            doc = doc[:1000] + "..."
        return doc.strip()

    def _batch_documents(self, documents: List[str], batch_size: int = 8) -> List[List[str]]:
        """Batch documents for efficient processing."""
        return [documents[i:i + batch_size] for i in range(0, len(documents), batch_size)]


# Global reranker instance
colbert_reranker = ColBERTReranker()


