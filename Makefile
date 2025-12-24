.PHONY: help setup dev test format lint clean reindex install-deps check-deps

# Default target
help:
	@echo "Available commands:"
	@echo "  setup       - Initial project setup"
	@echo "  dev         - Start development server"
	@echo "  test        - Run tests"
	@echo "  format      - Format code with black and isort"
	@echo "  lint        - Run linting checks"
	@echo "  clean       - Clean up temporary files"
	@echo "  reindex     - Reindex all documents"
	@echo "  install-deps- Install all dependencies"
	@echo "  check-deps  - Check for missing dependencies"

# Setup project
setup: install-deps
	@echo "Setting up AI Agent RAG project..."
	@if [ ! -f .env ]; then \
		echo "Creating .env file from template..."; \
		cp env.example .env; \
		echo "⚠️  Please edit .env with your actual API keys"; \
	fi
	@echo "✅ Setup complete"

# Install dependencies
install-deps:
	@echo "Installing Python dependencies..."
	pip install -r requirements.txt
	@echo "Installing optional development dependencies..."
	pip install -e ".[dev,tree-sitter]"
	@echo "✅ Dependencies installed"

# Check for missing dependencies
check-deps:
	@echo "Checking for required dependencies..."
	@python -c "import fastapi, uvicorn, supabase, sentence_transformers, openai; print('✅ Core dependencies OK')" || echo "❌ Missing core dependencies"
	@python -c "import PyPDF2; print('✅ PDF processing OK')" || echo "❌ PDF processing not available"
	@python -c "import tree_sitter, tree_sitter_python; print('✅ Code parsing OK')" || echo "⚠️  Code parsing not available (optional)"

# Start development server
dev:
	@echo "Starting development server..."
	uvicorn backend.main:app --reload --host 0.0.0.0 --port 8000

# Run tests
test:
	@echo "Running tests..."
	pytest tests/ -v --tb=short

# Format code
format:
	@echo "Formatting code with black and isort..."
	black backend/
	isort backend/
	@echo "✅ Code formatted"

# Run linting
lint:
	@echo "Running linting checks..."
	flake8 backend/ --max-line-length=88 --extend-ignore=E203,W503
	mypy backend/
	@echo "✅ Linting complete"

# Clean up
clean:
	@echo "Cleaning up temporary files..."
	find . -type d -name __pycache__ -exec rm -rf {} +
	find . -type d -name "*.pyc" -delete
	find . -type d -name ".pytest_cache" -exec rm -rf {} +
	find . -type f -name "*.pyc" -delete
	find . -type f -name "*.pyo" -delete
	find . -type f -name "*.pyd" -delete
	@echo "✅ Cleanup complete"

# Reindex documents
reindex:
	@echo "Reindexing all documents..."
	python -c "
	import asyncio
	from backend.database import Database
	from backend.indexing import Indexer
	from backend.storage import Storage

	async def reindex_all():
		db = Database()
		storage = Storage()
		indexer = Indexer(db, storage)

		# Get all ready documents
		documents = await db.get_all_documents()
		for doc in documents:
			if doc['status'] == 'ready':
				print(f'Reindexing {doc[\"filename\"]}...')
				# Download and reindex
				file_content = await storage.download_file(doc['storage_path'])
				if doc['doc_type'] == 'docs':
					await indexer.index_pdf(doc, file_content)
				else:
					await indexer.index_zip(doc, file_content)
				print(f'✅ Reindexed {doc[\"filename\"]}')

	asyncio.run(reindex_all())
	"
	@echo "✅ Reindexing complete"

# Database setup
db-setup:
	@echo "Setting up database schema..."
	@echo "⚠️  Please run the SQL in supabase_schema.sql in your Supabase SQL editor"
	@echo "Then create the 'uploads' storage bucket"

# Create test data
test-data:
	@echo "Creating test data..."
	python -c "
	import asyncio
	from backend.database import Database

	async def create_test_data():
		db = Database()

		# Create test workspace
		await db.create_document(
			document_id='test-docs',
			doc_type='docs',
			filename='test.pdf',
			storage_path='test/path.pdf',
			sha256='test'
		)

		await db.create_document(
			document_id='test-code',
			doc_type='code',
			filename='test.zip',
			storage_path='test/path.zip',
			sha256='test'
		)

		print('✅ Test data created')

	asyncio.run(create_test_data())
	"

# Docker build
docker-build:
	@echo "Building Docker image..."
	docker build -t ai-agent-rag backend/

# Docker run
docker-run:
	@echo "Running Docker container..."
	docker run -p 8000:8000 --env-file .env ai-agent-rag

# Full deployment check
check-deploy:
	@echo "Checking deployment readiness..."
	@python -c "import backend.main; print('✅ Main module imports OK')"
	@python -c "import backend.database, backend.storage, backend.indexing, backend.retrieval, backend.generation; print('✅ All modules import OK')"
	@if [ -f .env ]; then echo "✅ Environment file exists"; else echo "❌ .env file missing"; fi
	@echo "✅ Deployment check complete"

# Show status
status:
	@echo "Project Status:"
	@echo "=================="
	@python --version
	@pip --version
	@python -c "import backend.main; print('✅ Backend module OK')" 2>/dev/null || echo "❌ Backend module broken"
	@if [ -f .env ]; then echo "✅ Environment configured"; else echo "❌ Environment not configured"; fi
	@echo "=================="