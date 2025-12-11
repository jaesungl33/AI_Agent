# GDD RAG Backbone

AI-powered framework for processing Game Design Documents (GDDs) with RAG-based analysis, requirement extraction, and code coverage checking.

## Features

- 📄 **Document Processing**: Index and query PDF/DOCX game design documents
- 🤖 **AI-Powered Analysis**: Ask natural language questions about your GDDs
- 📊 **Structured Extraction**: Automatically extract objects, systems, requirements
- ✅ **Code Coverage**: Behavior-based pipeline that matches GDD behaviors to code behaviors (fast + LLM-verified)
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

### Behavior-Based Coverage (new pipeline)
- GDD → Behavior requirements (triggers, effects, entities)
- Code → Behavior summaries per method (cached behavior index)
- Match behaviors via embeddings, then LLM verifies only the top candidates
- Outputs `implemented`, `partially_implemented`, or `not_implemented` plus gaps (missing triggers/effects) and top matches

### Behavior-Based Coverage (new pipeline)
- GDD → Behavior requirements (`triggers`, `effects`, `entities`)
- Code → Behavior summaries per method (cached index)
- Match behaviors via embeddings, then LLM verifies only the top candidates
- Outputs `implemented`, `partially_implemented`, or `not_implemented` plus gap analysis


### Via Web UI

1. **Start backend**: `cd backend_api && uvicorn main:app --reload`
2. **Start frontend**: `cd frontend && npm run dev`
3. **Access**: Open `http://localhost:3000` in your browser
4. Upload documents, ask questions, check code coverage

### Via Python API

**Index a Document:**
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

**Query Documents:**
```python
from gdd_rag_backbone.rag_backend.chunk_qa import ask_across_docs

result = ask_across_docs(
    doc_ids=["my_gdd"],
    question="What are the main game systems?",
    provider=provider,
)
print(result["answer"])
```

**Index Codebase:**
```bash
python scripts/index_tank_online_codebase.py \
    --source ./your_code_directory \
    --doc-id your_codebase_id
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

## Known Limitations

### Document Indexing

- **PDF files**: ✅ Fully supported and working
- **DOCX files**: ✅ Supported
- **TXT/CSV files**: ⚠️ May fail with "EOF marker not found" error due to raganything parser limitations
  - **Workaround**: For code files (.txt), upload through the web UI which may handle them differently
  - **Note**: GDD PDFs are successfully indexed (43/71 in current setup), which is sufficient for coverage evaluation
  - **MindMap (.mm) files**: Not currently supported

The behavior-based coverage evaluation works with successfully indexed documents. Code files can be processed through the behavior indexing system even if raganything indexing fails.

## Requirements

- Python 3.10+
- Node.js 18+
- API key for Qwen/DashScope

## License

[Your License]
