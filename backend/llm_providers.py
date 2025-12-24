"""
LLM provider wrapper for backend services.

This module provides a unified interface to get LLM providers,
wrapping the gdd_rag_backbone.llm_providers module.
"""

import os
import logging

logger = logging.getLogger(__name__)


def get_llm_provider():
    """
    Get the appropriate LLM provider based on available API keys.
    
    Priority order:
    1. Qwen/DashScope (if DASHSCOPE_API_KEY or QWEN_API_KEY is set)
    2. OpenAI (if OPENAI_API_KEY is set)
    3. Ollama (fallback, local)
    
    Returns:
        An LLM provider instance (QwenProvider, OpenAIProvider, or OllamaProvider)
        
    Raises:
        RuntimeError: If no LLM providers are available
    """
    # Try Qwen first
    if os.environ.get("DASHSCOPE_API_KEY") or os.environ.get("QWEN_API_KEY"):
        try:
            from gdd_rag_backbone.llm_providers import QwenProvider
            provider = QwenProvider()
            logger.info("✅ Using QwenProvider")
            return provider
        except Exception as e:
            logger.warning(f"Qwen not available: {e}")

    # Try OpenAI
    if os.environ.get("OPENAI_API_KEY"):
        try:
            from gdd_rag_backbone.llm_providers import OpenAIProvider
            provider = OpenAIProvider()
            logger.info("✅ Using OpenAIProvider")
            return provider
        except Exception as e:
            logger.warning(f"OpenAI not available: {e}")

    # Try Ollama as last resort
    try:
        from gdd_rag_backbone.llm_providers import OllamaProvider
        provider = OllamaProvider()
        logger.info("✅ Using OllamaProvider")
        return provider
    except Exception as e:
        logger.error(f"No LLM providers available: {e}")
        raise RuntimeError(
            "No LLM providers available. Please configure API keys:\n"
            "- DASHSCOPE_API_KEY or QWEN_API_KEY for Qwen\n"
            "- OPENAI_API_KEY for OpenAI\n"
            "- Or ensure Ollama is running locally"
        )


