# GDD RAG Backbone

A clean, extensible Python framework for ingesting, indexing, and extracting structured data from Game Design Documents (GDDs) using RAG-Anything.

## Overview

The GDD RAG Backbone provides:

- **Document Ingestion & Indexing**: Parse, chunk, embed, and index GDDs (PDFs, DOCX, etc.) using RAG-Anything
- **RAG Query Engine**: Ask natural language questions about indexed documents
- **GDD Compiler Layer**: Extract structured JSON checklists (objects, tanks, maps, etc.) from GDDs
- **Provider-Agnostic Design**: Works with any LLM provider (Qwen, Gemini/Vertex AI, OpenAI, etc.) via pluggable interfaces

## Project Structure

```
gdd_rag_backbone/
├── __init__.py
├── config.py                    # Global configuration
├── llm_providers/               # LLM provider abstractions and implementations
│   ├── __init__.py
│   ├── base.py                  # Abstract interfaces (Protocols)
│   ├── qwen_provider.py         # Qwen/DashScope implementation
│   └── vertex_provider.py       # Vertex AI/Gemini implementation
├── rag_backend/                 # RAG-Anything integration
│   ├── __init__.py
│   ├── rag_config.py            # RAGAnything instance factory
│   ├── indexing.py              # Document indexing logic
│   └── query_engine.py          # Query and debug functions
├── gdd/                         # Game Design Document logic
│   ├── __init__.py
│   ├── schemas.py               # Pydantic models (GddObject, TankSpec, MapSpec)
│   └── extraction.py            # Structured data extraction functions
├── scripts/                     # Manual entry points
│   └── index_and_test.py        # Index document and run test queries
└── tests/                       # Test suite
    ├── test_schemas.py
    ├── test_parsing.py
    ├── test_retrieval.py
    └── test_extraction.py
```

## Installation

### Prerequisites

- Python 3.10+
- `raganything` package (already installed)

### Setup

1. **Clone or navigate to the project**:
   ```bash
   cd /Users/madeinheaven/Documents/GitHub/AI_Agent
   ```

2. **Set up API keys** - You have **three options**:

   **Option 1: Use a .env file (Recommended)**
   
   Create a `.env` file in the project root:
   ```bash
   # Copy the template
   cp env.template .env
   
   # Edit .env and add your API key
   # QWEN_API_KEY=your_actual_api_key_here
   ```
   
   The config will automatically load `.env` if `python-dotenv` is installed:
   ```bash
   pip install python-dotenv  # Optional but recommended
   ```

   **Option 2: Set environment variables directly**
   ```bash
   # For Qwen/DashScope (required for Qwen provider)
   export QWEN_API_KEY="your-api-key"
   
   # For Vertex AI (optional)
   export VERTEX_PROJECT_ID="your-project-id"
   export VERTEX_LOCATION="us-central1"
   
   # For OpenAI (optional, if adding support)
   export OPENAI_API_KEY="your-api-key"
   ```

   **Option 3: Pass API key directly to provider**
   ```python
   from gdd_rag_backbone.llm_providers import QwenProvider
   
   provider = QwenProvider(api_key="your-api-key-here")
   ```

3. **Install dependencies** (if needed):
   ```bash
   pip install raganything pydantic pytest
   ```

## Quick Start

### Basic Usage

1. **Index a document**:
   ```python
   import asyncio
   from gdd_rag_backbone.rag_backend import index_document
   from gdd_rag_backbone.llm_providers import QwenProvider, make_llm_model_func, make_embedding_func
   
   async def main():
       # Initialize provider
       provider = QwenProvider()
       llm_func = make_llm_model_func(provider)
       embedding_func = make_embedding_func(provider)
       
       # Index document
       await index_document(
           doc_path="docs/sample_gdd.pdf",
           doc_id="my_gdd",
           llm_func=llm_func,
           embedding_func=embedding_func,
       )
   
   asyncio.run(main())
   ```

2. **Ask questions**:
   ```python
   from gdd_rag_backbone.rag_backend import ask_question
   
   async def main():
       answer = await ask_question(
           doc_id="my_gdd",
           query="What tanks are mentioned in the document?",
       )
       print(answer)
   
   asyncio.run(main())
   ```

3. **Extract structured data**:
   ```python
   from gdd_rag_backbone.gdd import extract_tanks, extract_maps
   
   async def main():
       # Extract tanks
       tanks = await extract_tanks("my_gdd")
       for tank in tanks:
           print(f"{tank.name}: {tank.class_name}, HP: {tank.hp}")
       
       # Extract maps
       maps = await extract_maps("my_gdd")
       for map_spec in maps:
           print(f"{map_spec.name}: {map_spec.mode}")
   
   asyncio.run(main())
   ```

### Using the Script

Run the test script to index and query a document:

```bash
python gdd_rag_backbone/scripts/index_and_test.py [doc_path] [doc_id]
```

Or with environment variables:

```bash
export GDD_DOC_PATH="docs/sample_gdd.pdf"
export GDD_DOC_ID="my_gdd"
python gdd_rag_backbone/scripts/index_and_test.py
```

## Core Components

### 1. Configuration (`config.py`)

Global configuration for paths, parser settings, and LLM provider credentials.

Key settings:
- `DEFAULT_WORKING_DIR`: RAG storage directory (default: `./rag_storage`)
- `DEFAULT_OUTPUT_DIR`: Parsed content output directory (default: `./output`)
- `DEFAULT_PARSER`: Parser choice - "mineru" or "docling" (default: "mineru")
- Environment variables for API keys (see Installation)

### 2. LLM Providers (`llm_providers/`)

**Base Interfaces** (`base.py`):
- `LlmProvider`: Protocol for LLM text generation
- `EmbeddingProvider`: Protocol for text embedding generation
- Helper functions: `make_llm_model_func()`, `make_embedding_func()`

**Implementations**:
- `QwenProvider`: Alibaba DashScope/Qwen implementation
- `VertexProvider`: Google Vertex AI/Gemini implementation

Both implementations include TODO markers where actual API calls need to be filled in.

### 3. RAG Backend (`rag_backend/`)

**RAG Configuration** (`rag_config.py`):
- `get_rag_instance()`: Factory function to create RAGAnything instances

**Indexing** (`indexing.py`):
- `index_document()`: Async function to parse, chunk, embed, and index documents
- Stores RAG instances in a global registry for querying

**Query Engine** (`query_engine.py`):
- `ask_question()`: Query indexed documents with natural language
- `debug_query()`: Debug query retrieval by showing retrieved chunks

### 4. GDD Extraction (`gdd/`)

**Schemas** (`schemas.py`):
- `GddObject`: Dataclass for game objects (BR, HI, TA, etc.)
- `TankSpec`: Pydantic model for tank specifications
- `MapSpec`: Pydantic model for map specifications

**Extraction** (`extraction.py`):
- `extract_tanks()`: Extract tank specifications from GDD
- `extract_maps()`: Extract map specifications from GDD
- `extract_objects()`: Extract game objects (all categories or filtered)
- `extract_breakable_objects()`: Extract BR category objects
- `extract_hiding_objects()`: Extract HI category objects

Each extraction function:
1. Uses RAG to retrieve relevant context
2. Calls LLM with structured prompt to generate JSON
3. Parses JSON into model instances and returns them

## API Reference

### Indexing

```python
async def index_document(
    doc_path: str | Path,
    doc_id: str,
    *,
    llm_func: Optional[Callable] = None,
    embedding_func: Optional[Callable] = None,
    working_dir: Optional[Path | str] = None,
    output_dir: Optional[Path | str] = None,
    parser: Optional[str] = None,
    parse_method: Optional[str] = None,
    **parser_kwargs
) -> None
```

### Querying

```python
async def ask_question(
    doc_id: str,
    query: str,
    *,
    mode: str = "mix",
    debug: bool = False,
    **query_kwargs
) -> str
```

### Extraction

```python
async def extract_tanks(doc_id: str, *, llm_func=None) -> List[TankSpec]
async def extract_maps(doc_id: str, *, llm_func=None) -> List[MapSpec]
async def extract_objects(doc_id: str, category: Optional[str] = None, *, llm_func=None) -> List[GddObject]
async def extract_breakable_objects(doc_id: str, *, llm_func=None) -> List[GddObject]
async def extract_hiding_objects(doc_id: str, *, llm_func=None) -> List[GddObject]
```

## Testing

Run the test suite:

```bash
pytest gdd_rag_backbone/tests/
```

Tests cover:
- Schema creation and validation
- Document parsing and indexing error handling
- Query function signatures and error handling
- Extraction function structure

## Extending the Framework

### Adding a New LLM Provider

1. Implement the `LlmProvider` and/or `EmbeddingProvider` protocols in `llm_providers/`
2. Add concrete implementation (e.g., `openai_provider.py`)
3. Update `llm_providers/__init__.py` to export the new provider

### Adding New Extraction Schemas

1. Define new Pydantic models in `gdd/schemas.py`
2. Create extraction function in `gdd/extraction.py` following the pattern:
   - Query RAG for relevant context
   - Call LLM with structured prompt
   - Parse JSON into model instances

### Customizing RAG Configuration

Modify `rag_backend/rag_config.py` to change default parser settings, working directories, etc.

## Success Criteria

The framework is considered successful if:

1. ✅ The user can run `python scripts/index_and_test.py` and see:
   - The document indexed
   - At least one RAG-driven answer printed

2. ✅ The user can import `from gdd.extraction import extract_tanks` and call `await extract_tanks("some_doc_id")` to get a list of `TankSpec` objects, even if the underlying LLM call is currently a stub or mock.

## TODO Items

The following items are marked with `# TODO:` comments and need to be filled in by the user:

1. **LLM Provider Implementations**:
   - `llm_providers/qwen_provider.py`: DashScope API calls
   - `llm_providers/vertex_provider.py`: Vertex AI API calls

2. **Optional Enhancements**:
   - Add more LLM providers (OpenAI, Anthropic, etc.)
   - Add more extraction schemas (weapons, abilities, etc.)
   - Add batch processing support
   - Add progress tracking for large documents

## License

[Add your license here]

## Contributing

[Add contribution guidelines here]
