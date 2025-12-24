# AI Agent - Chat-First RAG System

A production-ready **Retrieval-Augmented Generation (RAG)** system for analyzing codebases and documentation. Built with **FastAPI + Supabase + Render** following modern AI engineering practices.

## 🎯 Core Features

- **🔍 Hybrid Search**: Vector similarity + lexical matching for accurate retrieval
- **📚 Multi-Modal**: Supports code repositories (ZIP) and documentation (PDF)
- **💬 Intelligent Chat**: Context-aware Q&A with grounded citations
- **🔧 Code Extraction**: Extract functions, classes, and methods by name
- **📄 Document Extraction**: Extract sections and phrases from PDFs
- **🎛️ Admin Tools**: Background job processing for indexing
- **☁️ Cloud-Ready**: Deployed on Render with Supabase vector storage

## 🏗️ System Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Next.js UI    │    │   FastAPI       │    │   Supabase      │
│   (Vercel)      │◄──►│   Backend       │◄──►│   Postgres      │
│                 │    │   (Render)      │    │   + pgvector     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                              │                        │
                              ▼                        ▼
                       ┌─────────────────┐    ┌─────────────────┐
                       │   Supabase      │    │   OpenAI/Qwen   │
                       │   Storage       │    │   LLM APIs      │
                       │   (uploads)     │    │                 │
                       └─────────────────┘    └─────────────────┘
```

### Data Flow

1. **Upload** → Store raw files in Supabase Storage
2. **Index** → Background jobs process PDFs/code into chunks with embeddings
3. **Retrieve** → Hybrid search (vector + lexical) with optional reranking
4. **Generate** → Grounded answers with citations using retrieved context

## 🚀 Quick Start

### Prerequisites
- Python 3.10+
- Node.js 18+
- Supabase account
- OpenAI API key (or DashScope for Qwen)

### Local Development

```bash
# Clone and setup
git clone <repository-url>
cd ai-agent-rag
make setup

# Configure environment
cp env.example .env
# Edit .env with your API keys

# Start development
make dev

# The backend will be available at http://localhost:8000
# The frontend will be available at http://localhost:3000
```

## 📚 API Reference

### Ingestion Endpoints

```bash
# Upload PDF document
curl -X POST "http://localhost:8000/ingest/docs" \
  -F "file=@document.pdf"

# Upload code repository
curl -X POST "http://localhost:8000/ingest/code" \
  -F "file=@codebase.zip"
```

### Chat Endpoint

```bash
# Ask about code
curl -X POST "http://localhost:8000/chat" \
  -H "Content-Type: application/json" \
  -d '{
    "message": "@codebase Where is authentication validated?",
    "document_scope": {
      "code_document_id": "uuid"
    }
  }'

# Ask about docs
curl -X POST "http://localhost:8000/chat" \
  -H "Content-Type: application/json" \
  -d '{
    "message": "What does the GDD say about progression?",
    "document_scope": {
      "docs_document_id": "uuid"
    }
  }'
```

### Extraction Endpoints

```bash
# Extract function
curl -X POST "http://localhost:8000/extract/code" \
  -H "Content-Type: application/json" \
  -d '{
    "document_id": "uuid",
    "symbol_name": "authenticate_user",
    "symbol_type": "function"
  }'

# Extract from docs
curl -X POST "http://localhost:8000/extract/docs" \
  -H "Content-Type: application/json" \
  -d '{
    "document_id": "uuid",
    "query": "watermark embedding"
  }'
```

### Admin Endpoints

```bash
# Run indexing job
curl -X POST "http://localhost:8000/admin/run-job"

# Check jobs status
curl "http://localhost:8000/jobs"
```

## ⚙️ Configuration

### Environment Variables

```bash
# Supabase (required)
SUPABASE_URL=your_project_url
SUPABASE_SERVICE_ROLE_KEY=your_service_role_key
SUPABASE_DB_URL=postgresql://...

# LLM (required - choose one)
OPENAI_API_KEY=sk-...
# OR
QWEN_API_KEY=sk-...
QWEN_BASE_URL=https://dashscope.aliyuncs.com/compatible-mode/v1

# Optional
DEBUG=false
PORT=8000
```

### Supabase Setup

1. **Create Project**: Go to [Supabase](https://supabase.com) and create a new project
2. **Run Schema**: Execute `supabase_schema.sql` in the SQL editor
3. **Create Bucket**: Create a storage bucket named `uploads`
4. **Enable Extensions**: Enable `vector` and `pgcrypto` extensions

## 🚢 Deployment

### Render + Vercel (Recommended)

```bash
# Backend (Render)
# 1. Connect GitHub repo to Render
# 2. Use render.yaml configuration
# 3. Set environment variables in Render dashboard

# Frontend (Vercel)
# 1. Connect GitHub repo to Vercel
# 2. Set NEXT_PUBLIC_API_URL to your Render backend URL
# 3. Deploy
```

### Docker Deployment

```bash
# Build and run
make docker-build
make docker-run

# Or manually
docker build -t ai-agent-rag backend/
docker run -p 8000:8000 --env-file .env ai-agent-rag
```

## 🧪 Testing & Development

```bash
# Run all tests
make test

# Format code
make format

# Lint code
make lint

# Check deployment readiness
make check-deploy

# Reindex all documents
make reindex
```

## 📊 Database Schema

### Core Tables

- **`documents`**: Uploaded files (PDF/code) with metadata
- **`files`**: Individual files within code repositories
- **`chunks`**: Text chunks with embeddings for retrieval
- **`symbols`**: Function/class definitions for extraction
- **`jobs`**: Background processing queue

### Key Features

- **pgvector Integration**: HNSW indexing for fast similarity search
- **Hybrid Retrieval**: Combines vector and lexical search
- **Deduplication**: Prevents duplicate chunks across files
- **Incremental Updates**: Skip reprocessing unchanged files

## 🔧 Advanced Configuration

### Custom Embedding Models

The system uses `all-MiniLM-L6-v2` (384 dimensions) by default. To use a different model:

```python
# In backend/indexing.py
self.embedding_model = SentenceTransformer('your-model-name')
# Update EMBEDDING_DIMENSIONS in schema
```

### Reranking Configuration

Reranking improves result quality but increases latency:

```python
# Enable in retrieval.py
if self.llm_client and len(candidates) > 10:
    reranked_candidates = await self._rerank_candidates(query, candidates)
```

### Chunking Strategies

**Documents (PDF)**:
- Size: 600-900 tokens
- Overlap: 10-20%
- Metadata: page number, heading

**Code**:
- Python: AST-based symbol extraction + line windows
- Other languages: 160-line windows with 40-line overlap
- Metadata: file path, line numbers, language

## 🤝 Contributing

1. **Fork** the repository
2. **Create** a feature branch
3. **Make** your changes
4. **Test** thoroughly with `make test`
5. **Format** code with `make format`
6. **Submit** a pull request

## 📄 License

MIT License - see LICENSE file for details.

## 🙏 Acknowledgments

- **Supabase** for vector database and storage
- **OpenAI** for LLM capabilities
- **Sentence Transformers** for embeddings
- **FastAPI** for the web framework
- **Render** for hosting

---

**Built with ❤️ for developers who need AI-powered code and documentation analysis**
cd frontend && npm install && cd ..
```

#### Configuration

Create `.env` file (see `env.example`):
```bash
# Required
SUPABASE_URL=your_supabase_project_url
SUPABASE_ANON_KEY=your_supabase_anon_key
OPENAI_API_KEY=your_openai_api_key

# Optional
DASHSCOPE_API_KEY=your_dashscope_api_key
SUPABASE_SERVICE_ROLE_KEY=your_service_role_key
```

#### Launch Services

```bash
# Quick start all services
make dev

# Or use the startup script
./start_all.sh
```

**Access:**
- Frontend: http://localhost:3000
- Backend API: http://localhost:8000

### Chat Examples

**General Questions:**
```
What are the game modes?
→ Deathmatch, Outpost Breaker, Gold in Match with detailed descriptions

How does progression work?
→ Fusion Artifact system, enhancement tables, upgrade mechanics

What about the UI design?
→ Main screens, garage, HUD, accessibility features

Tell me about weapons
→ Shooting mechanics, projectile physics, weapon balancing
```

**Code Search:**
```
@codebase How does player movement work?
→ Searches codebase and returns relevant code snippets with explanations

@codebase Find the matchmaking logic
→ Vector search through C# files for matchmaking implementation
```

### Option 2: Production Deployment

#### Backend (Render)

1. **Create Supabase Project:**
   - Go to [supabase.com](https://supabase.com)
   - Create a new project
   - Note your project URL and API keys

2. **Deploy Backend on Render:**
   ```bash
   # Fork this repository
   # Connect to Render
   # Use render.yaml configuration
   # Set environment variables in Render dashboard
   ```

3. **Set Environment Variables on Render:**
   ```
   SUPABASE_URL=https://your-project.supabase.co
   SUPABASE_ANON_KEY=your-anon-key
   OPENAI_API_KEY=your-openai-key
   PORT=10000
   FLASK_DEBUG=false
   ```

#### Frontend (Vercel)

1. **Deploy Frontend:**
   ```bash
   cd frontend
   npm install
   npm run build
   # Deploy to Vercel
   ```

2. **Set Environment Variable on Vercel:**
   ```
   NEXT_PUBLIC_API_URL=https://your-render-app.onrender.com
   ```

#### Supabase Setup

Create these tables in your Supabase SQL editor:

```sql
-- Code chunks table
CREATE TABLE code_chunks (
  id SERIAL PRIMARY KEY,
  workspace_id TEXT NOT NULL,
  file_path TEXT NOT NULL,
  content TEXT NOT NULL,
  start_line INTEGER,
  end_line INTEGER,
  language TEXT,
  embedding VECTOR(384), -- Adjust dimension based on your embedding model
  metadata JSONB,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- GDD chunks table
CREATE TABLE gdd_chunks (
  id SERIAL PRIMARY KEY,
  workspace_id TEXT NOT NULL,
  doc_id TEXT NOT NULL,
  doc_name TEXT NOT NULL,
  content TEXT NOT NULL,
  page_number INTEGER,
  chunk_index INTEGER,
  embedding VECTOR(384),
  metadata JSONB,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Enable vector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create indexes
CREATE INDEX idx_code_chunks_workspace ON code_chunks(workspace_id);
CREATE INDEX idx_gdd_chunks_workspace ON gdd_chunks(workspace_id);
CREATE INDEX idx_code_chunks_embedding ON code_chunks USING ivfflat (embedding vector_cosine_ops);
CREATE INDEX idx_gdd_chunks_embedding ON gdd_chunks USING ivfflat (embedding vector_cosine_ops);
```

## 🎯 Core Features

- **🤖 Intelligent Chat**: Natural language Q&A about Tank War game design
  - Ask about game modes, weapons, progression, UI, tanks, matchmaking, etc.
  - Get detailed responses with document references
  - Context-aware answers based on 69+ design documents
- **🔍 Code Search**: Use `@codebase` queries to search the actual codebase
  - Vector similarity search across C# scripts and documentation
  - Find relevant code snippets with explanations
  - Integrated with Supabase for scalable vector storage
- **📄 Document Analysis**: Process and query 82+ design documents
- **✅ Code-GDD Coverage**: Automated behavior matching between GDDs and code
- **📊 Structured Extraction**: Extract game objects, systems, and requirements
- **🏢 Workspace Management**: Isolated project workspaces
- **🚀 Modern Web UI**: Responsive Next.js interface

## 📋 Available Endpoints

| Endpoint | Description |
|----------|-------------|
| `GET /workspaces` | List available workspaces |
| `GET /documents` | Get workspace documents |
| `POST /chat` | Chat with GDD knowledge |
| `GET /health` | Service health check |

## 🧪 Testing

```bash
# Run test suite
python -m pytest tests/ -v

# Test specific module
python -m pytest tests/test_extraction.py
```

## 🛠️ Development

### Project Organization

```
AI_Agent/
├── pyproject.toml    # Python project configuration
├── Makefile         # Development tasks
├── .gitignore       # Git ignore rules
├── start_all.sh     # Service startup script
│
├── backend/         # Flask API server
├── frontend/        # Next.js web interface
├── src/            # Core Python library
├── data/           # Documents and indices
├── docs/           # Documentation
├── scripts/        # Utility scripts
└── tests/          # Test suite
```

### Development Commands

```bash
# Setup development environment
make setup

# Install dependencies
make install-dev

# Run all services
make dev

# Run tests
make test

# Format code
make format

# Lint code
make lint

# Clean project
make clean

# Reindex documents
make reindex
```

### Coverage Analysis CLI
```bash
cd backend
python scripts/run_coverage.py --workspace tank_war --gdd "Demo_Map_Design_file" --code "AbilityBase"
```

## 📚 Documentation

- [Architecture Overview](docs/architecture/)
- [User Guides](docs/guides/)
- [API Reference](docs/api/)

## 📝 License

MIT License - see LICENSE file for details.
