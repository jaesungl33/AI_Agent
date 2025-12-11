# GDD RAG Backbone

AI-powered framework for processing Game Design Documents (GDDs) with RAG-based analysis, requirement extraction, and code coverage checking.

## 📁 Project Structure

```
AI_Agent/
├── src/                          # Core library
│   └── gdd_rag_backbone/
│       ├── gdd/                  # GDD processing modules
│       ├── llm_providers/        # LLM provider implementations
│       ├── rag_backend/          # RAG indexing and querying
│       └── workspace/            # Workspace management
│
├── backend/                      # FastAPI backend server
│   ├── main.py
│   └── requirements.txt
│
├── frontend/                     # Next.js frontend
│   ├── app/                     # Next.js app router
│   ├── components/              # React components
│   └── lib/                     # Utilities and API client
│
├── scripts/                      # Utility scripts
│   ├── migration/               # Data migration scripts
│   ├── indexing/                # Indexing utilities
│   ├── testing/                 # Test scripts
│   └── utilities/               # Helper scripts
│
├── data/                         # Runtime data
│   ├── rag_storage/            # RAG indices and vectors
│   ├── output/                  # Processing output
│   ├── reports/                 # Coverage reports
│   ├── workspaces/              # Workspace data
│   └── gdd_documents/           # GDD source files
│
├── docs/                         # Documentation
│   ├── architecture/            # Architecture docs
│   ├── guides/                  # User guides
│   └── api/                     # API documentation
│
└── tests/                        # Test files and test codebase
```

## 🚀 Quick Start

### Installation

```bash
# Clone the repository
git clone https://github.com/yourusername/AI_Agent.git
cd AI_Agent

# Install Python dependencies
pip install -r requirements.txt
pip install -r backend/requirements.txt

# Install frontend dependencies
cd frontend && npm install && cd ..
```

### Environment Setup

Create a `.env` file in the project root:

```bash
# Qwen/DashScope API
DASHSCOPE_API_KEY=your_api_key_here
REGION=intl  # or "cn"

# Optional: Override defaults
DEFAULT_LLM_MODEL=qwen-max
DEFAULT_EMBEDDING_MODEL=text-embedding-v3
```

### Local Development

**Backend API:**
```bash
cd backend
uvicorn main:app --reload
# Runs on http://localhost:8000
```

**Frontend (Next.js):**
```bash
cd frontend
npm run dev
# Runs on http://localhost:3000
```

## ✨ Features

- 📄 **Document Processing**: Index and query PDF/DOCX game design documents
- 🤖 **AI-Powered Analysis**: Ask natural language questions about your GDDs
- 📊 **Structured Extraction**: Automatically extract objects, systems, requirements
- ✅ **Code Coverage**: Behavior-based pipeline that matches GDD behaviors to code behaviors
- 🏢 **Workspace Management**: Isolated workspaces for different projects
- 🚀 **Multiple Interfaces**: Web UI (Vercel), REST API (Render)

## 📚 Documentation

- [Architecture Documentation](docs/architecture/)
- [User Guides](docs/guides/)
- [API Documentation](docs/api/)

## 🧪 Testing

```bash
# Run tests
python -m pytest tests/

# Run specific test
python -m pytest tests/test_extraction.py
```

## 📝 License

[Your License Here]

## 🤝 Contributing

[Contributing Guidelines]
