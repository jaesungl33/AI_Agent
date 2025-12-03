# OpenAI SDK Usage in GDD RAG Backbone

## Overview

**Important:** This program uses the **OpenAI Python SDK library** (`openai` package), but it does **NOT** call OpenAI's services. Instead, it uses the OpenAI SDK as a client library to call **Qwen/DashScope's API** through their OpenAI-compatible endpoint.

## Why Use OpenAI SDK?

Qwen/DashScope provides an OpenAI-compatible API endpoint, which means:
- You can use the same OpenAI client library code
- The API interface matches OpenAI's format
- No need to learn a different SDK for Qwen models
- Easier to switch between providers if needed

## Where It's Used

### 1. LLM Text Generation (`qwen_provider.py` - `llm()` method)

**Location:** `gdd_rag_backbone/llm_providers/qwen_provider.py` (lines 143-184)

**What it does:**
```python
from openai import OpenAI

client = OpenAI(
    api_key=self.api_key,        # Your DashScope API key
    base_url=self.base_url,      # Points to DashScope, NOT OpenAI!
    # base_url = "https://dashscope-intl.aliyuncs.com/compatible-mode/v1"
)

response = client.chat.completions.create(
    model=self.llm_model,  # e.g., "qwen-max" (Qwen model, not OpenAI)
    messages=messages,
    **filtered_kwargs
)
```

**Function:**
- Makes LLM API calls for text generation
- Used when extracting structured data from GDDs
- Used for question-answering in the RAG system
- Used for generating analysis summaries

**Note:** The `base_url` points to DashScope's compatible endpoint (`dashscope.aliyuncs.com`), NOT OpenAI's endpoint (`api.openai.com`).

### 2. Embedding Generation (`qwen_provider.py` - `embed()` method)

**Location:** `gdd_rag_backbone/llm_providers/qwen_provider.py` (lines 208-231)

**What it does:**
```python
from openai import OpenAI

client = OpenAI(
    api_key=self.api_key,        # Your DashScope API key
    base_url=self.base_url,      # Points to DashScope, NOT OpenAI!
)

response = client.embeddings.create(
    model=self.embedding_model,  # e.g., "text-embedding-v3" (Qwen model)
    input=texts,
)
```

**Function:**
- Generates vector embeddings for document chunks
- Used during document indexing to create searchable vectors
- Used for semantic search when querying documents
- Critical for RAG (Retrieval-Augmented Generation) functionality

## Fallback Mechanism

The code has a smart fallback system:

1. **First tries:** OpenAI SDK with DashScope's compatible endpoint (preferred)
2. **Falls back to:** Native DashScope SDK if OpenAI package not available

This ensures the code works even if the `openai` package isn't installed, though using the OpenAI SDK is recommended for better compatibility.

## Configuration

The base URL is configured in `gdd_rag_backbone/config.py`:

```python
# Default points to DashScope, NOT OpenAI
DEFAULT_QWEN_BASE_URL = "https://dashscope-intl.aliyuncs.com/compatible-mode/v1"
```

**This means:**
- ✅ Uses Qwen/DashScope API (Alibaba's service)
- ❌ Does NOT use OpenAI's API
- ✅ Uses OpenAI SDK as a client library (for compatibility)

## Summary

| Component | What It Is | What It Does |
|-----------|-----------|--------------|
| `openai` package | Python SDK library | Client library for making API calls |
| Base URL | `dashscope.aliyuncs.com` | Points to Qwen/DashScope, NOT OpenAI |
| API Key | `QWEN_API_KEY` | Your DashScope API key, NOT OpenAI key |
| Models | `qwen-max`, `text-embedding-v3` | Qwen models, NOT OpenAI models |

## Why This Matters

- **No OpenAI account needed:** You only need a DashScope/Qwen API key
- **No OpenAI costs:** All API calls go to DashScope, not OpenAI
- **Same interface:** Uses OpenAI SDK for code compatibility
- **Easy to understand:** Same patterns as OpenAI, but different backend

## Testing Scripts

Several test scripts use the OpenAI-compatible endpoint:
- `scripts/test_qwen_openai_compatible.py` - Tests Qwen models via OpenAI SDK
- `scripts/test_embedding_api.py` - Tests embedding API via OpenAI SDK
- `scripts/list_embedding_models.py` - Lists available models via OpenAI SDK

All of these use the OpenAI SDK to call DashScope's API, not OpenAI's API.

















