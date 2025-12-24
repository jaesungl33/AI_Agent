"""
OpenAI provider using HTTP requests (no dependency on openai SDK).
Uses gpt-4o-mini for chat and text-embedding-3-small for embeddings.
"""
from __future__ import annotations

import os
import requests
from typing import List, Dict, Any, Optional


class OpenAIProvider:
    def __init__(
        self,
        *,
        api_key: Optional[str] = None,
        chat_model: str = "gpt-4o-mini",
        embedding_model: str = "text-embedding-3-small",
    ):
        self.api_key = api_key or os.environ.get("OPENAI_API_KEY")
        if not self.api_key:
            raise ValueError("OPENAI_API_KEY is required for OpenAIProvider")
        self.chat_model = chat_model
        self.embedding_model = embedding_model
        self.embedding_dim = 1536  # text-embedding-3-small dimension

    # LLM chat
    def llm(
        self,
        prompt: str,
        system_prompt: Optional[str] = None,
        history_messages: Optional[List[Dict[str, str]]] = None,
        temperature: float = 0.1,
        **kwargs: Any,
    ) -> str:
        headers = {
            "Authorization": f"Bearer {self.api_key}",
            "Content-Type": "application/json",
        }
        messages = []
        if system_prompt:
            messages.append({"role": "system", "content": system_prompt})
        if history_messages:
            messages.extend(history_messages)
        messages.append({"role": "user", "content": prompt})

        resp = requests.post(
            "https://api.openai.com/v1/chat/completions",
            headers=headers,
            json={
                "model": self.chat_model,
                "messages": messages,
                "temperature": temperature,
            },
            timeout=30,
        )
        resp.raise_for_status()
        data = resp.json()
        return data["choices"][0]["message"]["content"]

    # Embeddings
    def embed(self, texts: List[str]) -> List[List[float]]:
        headers = {
            "Authorization": f"Bearer {self.api_key}",
            "Content-Type": "application/json",
        }
        resp = requests.post(
            "https://api.openai.com/v1/embeddings",
            headers=headers,
            json={
                "model": self.embedding_model,
                "input": texts,
            },
            timeout=30,
        )
        resp.raise_for_status()
        data = resp.json()
        return [item["embedding"] for item in data["data"]]


__all__ = ["OpenAIProvider"]

