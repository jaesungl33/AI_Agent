# GDD RAG Backbone

AI-powered framework for processing Game Design Documents (GDDs) with RAG-based analysis, requirement extraction, and code coverage checking.

## Features

- 📄 **Document Processing**: Index and query PDF/DOCX game design documents
- 🤖 **AI-Powered Analysis**: Ask natural language questions about your GDDs
- 📊 **Structured Extraction**: Automatically extract objects, systems, requirements
- ✅ **Code Coverage**: Compare GDD requirements against your codebase
- 🚀 **Multiple Interfaces**: Web UI (Vercel), REST API (Render)

## Quick Start

### Installation

```bash
# Clone the repository
git clone https://github.com/yourusername/AI_Agent.git
cd AI_Agent

# Install Python dependencies
pip install -r requirements.txt

# Install frontend dependencies
cd frontend && npm install && cd ..

# Setup environment
cp .env.example .env
# Edit .env with your API keys
```

### Local Development

**Backend API:**
```bash
cd backend_api
uvicorn main:app --reload
# Runs on http://localhost:8000
```

**Frontend (Next.js):**
```bash
cd frontend
npm run dev
# Runs on http://localhost:3000
```

## Deployment

### Frontend (Vercel)

1. Push code to GitHub
2. Connect repository to [Vercel](https://vercel.com)
3. Set root directory to `frontend`
4. Add environment variable: `NEXT_PUBLIC_API_URL`
5. Deploy automatically on push to `main`

### Backend (Render)

1. Push code to GitHub
2. Connect repository to [Render](https://render.com)
3. Render auto-detects `Render.yaml` configuration
4. Add environment variables in Render dashboard
5. Deploy automatically on push to `main`

## Project Structure

```
AI_Agent/
├── backend_api/          # FastAPI REST backend
│   ├── main.py          # API endpoints
│   └── requirements.txt # Backend dependencies
├── frontend/            # Next.js frontend (Vercel)
│   ├── app/            # Next.js app router
│   ├── components/     # React components
│   └── lib/            # Utilities and API client
├── gdd_rag_backbone/   # Core Python library
│   ├── rag_backend/    # RAG indexing and querying
│   ├── gdd/           # GDD extraction logic
│   └── llm_providers/ # LLM provider abstractions
├── docs/               # Document storage
├── rag_storage/        # RAG data (indexed documents)
├── scripts/            # Utility scripts
└── Render.yaml         # Render deployment config
```

## Usage

### Index a Document

```python
from gdd_rag_backbone.rag_backend import index_document
from gdd_rag_backbone.llm_providers import QwenProvider, make_llm_model_func, make_embedding_func

provider = QwenProvider()
await index_document(
    doc_path="docs/game_design.pdf",
    doc_id="my_gdd",
    llm_func=make_llm_model_func(provider),
    embedding_func=make_embedding_func(provider),
)
```

### Query Documents

```python
from gdd_rag_backbone.rag_backend.chunk_qa import ask_across_docs

result = ask_across_docs(
    doc_ids=["my_gdd"],
    question="What are the main game systems?",
    provider=provider,
)
print(result["answer"])
```

## Documentation

- **[USER_GUIDE.md](USER_GUIDE.md)** - Complete user guide
- **[OPENAI_USAGE.md](OPENAI_USAGE.md)** - API usage notes
- **[TEST_COVERAGE.md](TEST_COVERAGE.md)** - Code coverage testing

## Environment Variables

**Local Development:**
```bash
cp .env.example .env
# Edit .env with your API keys
```

**Deployment:**
- **Vercel**: Set `NEXT_PUBLIC_API_URL` in dashboard
- **Render**: Set `QWEN_API_KEY`, `DASHSCOPE_API_KEY` in dashboard

## Requirements

- Python 3.10+
- Node.js 18+
- API key for Qwen/DashScope

## License

[Your License]
