"""
Ollama provider implementation for local LLM inference.
"""
import requests
import json
from typing import List, Optional, Dict, Any
from gdd_rag_backbone.llm_providers.base import LlmProvider, EmbeddingProvider


class OllamaProvider(LlmProvider, EmbeddingProvider):
    """
    Ollama provider for running LLMs locally.

    Requires Ollama to be running locally on http://localhost:11434
    """

    def __init__(
        self,
        base_url: str = "http://localhost:11434",
        llm_model: str = "qwen2.5:3b-instruct",
        embedding_model: Optional[str] = None,
        timeout: int = 120,  # Increased timeout for RAG tasks
    ):
        """
        Initialize Ollama provider.

        Args:
            base_url: Ollama server URL (default: http://localhost:11434)
            llm_model: Model name for chat (default: qwen2.5:7b-instruct)
            embedding_model: Model name for embeddings (optional)
            timeout: Request timeout in seconds
        """
        self.base_url = base_url.rstrip("/")
        self.llm_model = llm_model
        self.embedding_model = embedding_model or llm_model
        self.timeout = timeout

        # Embedding dimension (will be determined dynamically)
        self.embedding_dim = 4096  # Default for most models, will be updated

        # Test connection (only if not in a restrictive environment)
        try:
            self._test_connection()
        except Exception:
            # Don't fail initialization, just warn and test on first use
            import warnings
            warnings.warn("Could not verify Ollama connection during initialization. Will test on first use.")
            self._connection_tested = False

    def _test_connection(self):
        """Test connection to Ollama server."""
        try:
            response = requests.get(f"{self.base_url}/api/tags", timeout=10)
            response.raise_for_status()
            models = response.json().get("models", [])

            # Check if our model is available
            model_names = [model["name"] for model in models]
            if self.llm_model not in model_names:
                available = ", ".join(model_names[:5])  # Show first 5
                if len(model_names) > 5:
                    available += f" (+{len(model_names) - 5} more)"
                raise ValueError(
                    f"Model '{self.llm_model}' not found. Available models: {available}"
                )

        except requests.exceptions.RequestException as e:
            raise RuntimeError(
                f"Cannot connect to Ollama server at {self.base_url}. "
                f"Make sure Ollama is running. Error: {str(e)}"
            )

    def llm(
        self,
        prompt: str,
        system_prompt: Optional[str] = None,
        history_messages: Optional[List[Dict[str, str]]] = None,
        **kwargs: Any
    ) -> str:
        """
        Generate text using Ollama chat API.

        Args:
            prompt: User prompt
            system_prompt: Optional system prompt
            history_messages: Optional conversation history
            **kwargs: Additional parameters

        Returns:
            Generated text response
        """
        # Test connection on first use if not tested during init
        if not getattr(self, '_connection_tested', True):
            self._test_connection()
            self._connection_tested = True

        # Build messages array
        messages = []

        # Add system prompt if provided
        if system_prompt:
            messages.append({"role": "system", "content": system_prompt})

        # Add conversation history
        if history_messages:
            for msg in history_messages:
                if isinstance(msg, dict) and "role" in msg and "content" in msg:
                    messages.append(msg)

        # Add current user prompt
        messages.append({"role": "user", "content": prompt})

        # Prepare request payload
        payload = {
            "model": self.llm_model,
            "messages": messages,
            "stream": False,  # We want complete response
        }

        # Add optional parameters with better defaults for accuracy
        payload["options"] = payload.get("options", {})

        # Set good defaults for accurate responses
        payload["options"]["temperature"] = kwargs.get("temperature", 0.1)  # Lower temperature for accuracy
        payload["options"]["top_p"] = kwargs.get("top_p", 0.9)
        payload["options"]["top_k"] = kwargs.get("top_k", 40)

        if "max_tokens" in kwargs:
            payload["options"]["num_predict"] = kwargs["max_tokens"]
        else:
            payload["options"]["num_predict"] = 1024  # Reasonable default

        # Optimized for RAG tasks - larger context, more conservative generation
        payload["options"]["num_ctx"] = 4096  # Larger context for RAG with document chunks
        payload["options"]["repeat_penalty"] = 1.1  # Slightly higher penalty to avoid repetition
        payload["options"]["repeat_last_n"] = 64  # Larger window for repetition detection
        payload["options"]["num_predict"] = min(2048, payload["options"]["num_predict"])  # Allow longer responses but cap at reasonable limit

        try:
            response = requests.post(
                f"{self.base_url}/api/chat",
                json=payload,
                timeout=self.timeout,
                headers={"Content-Type": "application/json"}
            )
            response.raise_for_status()

            result = response.json()
            if "message" in result and "content" in result["message"]:
                return result["message"]["content"]
            else:
                raise RuntimeError(f"Unexpected response format: {result}")

        except requests.exceptions.ConnectionError as e:
            if "Broken pipe" in str(e) or "[Errno 32]" in str(e):
                raise RuntimeError(f"Ollama connection broken - response may have been too long or model overloaded. Try simplifying your question.")
            else:
                raise RuntimeError(f"Ollama connection failed: {str(e)}")
        except requests.exceptions.Timeout as e:
            raise RuntimeError(f"Ollama request timed out after {self.timeout}s. Try a shorter question.")
        except requests.exceptions.RequestException as e:
            raise RuntimeError(f"Ollama API call failed: {str(e)}")

    def embed(self, texts: List[str]) -> List[List[float]]:
        """
        Generate fast embeddings using hash-based approach.

        This is optimized for speed rather than semantic accuracy.
        For production use, consider using a dedicated embedding model.

        Args:
            texts: List of text strings to embed

        Returns:
            List of embedding vectors
        """
        # Test connection on first use if not tested during init
        if not getattr(self, '_connection_tested', True):
            self._test_connection()
            self._connection_tested = True
        import hashlib
        import struct

        embeddings = []
        for text in texts:
            # Fast hash-based embedding generation
            # Use multiple hash functions for better distribution
            hash_functions = [hashlib.md5, hashlib.sha1, hashlib.sha256, hashlib.sha384]
            combined_hash = b""

            for hash_func in hash_functions:
                combined_hash += hash_func(text.encode('utf-8')).digest()

            # Convert hash bytes to float vector
            float_list = []
            for i in range(0, len(combined_hash), 4):
                chunk = combined_hash[i:i+4]
                if len(chunk) < 4:
                    chunk += b'\x00' * (4 - len(chunk))
                # Convert to float and normalize to [-1, 1]
                float_val = struct.unpack('<I', chunk)[0] / 4294967295.0 * 2 - 1
                float_list.append(float_val)

            # Ensure consistent dimension
            target_dim = min(self.embedding_dim, 512)  # Cap at reasonable size for speed
            while len(float_list) < target_dim:
                float_list.extend(float_list)
            float_list = float_list[:target_dim]

            embeddings.append(float_list)

        return embeddings
