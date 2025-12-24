#!/usr/bin/env python3
"""
Indexing pipelines for PDFs and code repositories
"""

import os
import zipfile
import io
import hashlib
from typing import Dict, Any, List
import tempfile
import shutil

# PDF processing
try:
    import PyPDF2
    PDF_AVAILABLE = True
except ImportError:
    PDF_AVAILABLE = False

# Code parsing
try:
    import tree_sitter
    import tree_sitter_python
    TREE_SITTER_AVAILABLE = True
except ImportError:
    TREE_SITTER_AVAILABLE = False

# Embeddings
try:
    from sentence_transformers import SentenceTransformer
    EMBEDDINGS_AVAILABLE = True
except ImportError:
    EMBEDDINGS_AVAILABLE = False

from .database import Database
from .storage import Storage

class Indexer:
    """Handles indexing of documents and code"""

    def __init__(self, db: Database, storage: Storage):
        self.db = db
        self.storage = storage

        # Initialize embedding model
        if EMBEDDINGS_AVAILABLE:
            self.embedding_model = SentenceTransformer('all-MiniLM-L6-v2')  # 384 dimensions
        else:
            self.embedding_model = None

    async def index_pdf(self, document: Dict[str, Any], file_content: bytes):
        """Index a PDF document"""
        if not PDF_AVAILABLE:
            raise Exception("PyPDF2 not available for PDF processing")

        try:
            # Extract text per page
            pdf_reader = PyPDF2.PdfReader(io.BytesIO(file_content))
            pages_text = []

            for page_num in range(len(pdf_reader.pages)):
                page = pdf_reader.pages[page_num]
                text = page.extract_text()
                pages_text.append({
                    'page': page_num + 1,
                    'content': text.strip()
                })

            # Mark document as indexing
            await self.db.update_document_status(document['id'], 'indexing')

            # Process pages into chunks
            chunks = self._chunk_pdf_pages(pages_text)

            # Generate embeddings and store chunks
            for chunk in chunks:
                if self.embedding_model:
                    embedding = self.embedding_model.encode(chunk['content']).tolist()
                else:
                    embedding = [0.1] * 384  # Placeholder

                metadata = {
                    'page': chunk['page'],
                    'filename': document['filename']
                }

                await self.db.create_chunk(
                    document_id=document['id'],
                    chunk_type='docs',
                    content=chunk['content'],
                    embedding=embedding,
                    metadata=metadata
                )

            # Mark document as ready
            await self.db.update_document_status(document['id'], 'ready')

        except Exception as e:
            await self.db.update_document_status(document['id'], 'failed', str(e))
            raise

    async def index_zip(self, document: Dict[str, Any], file_content: bytes):
        """Index a code repository ZIP file"""
        try:
            # Mark document as indexing
            await self.db.update_document_status(document['id'], 'indexing')

            # Extract ZIP to temporary directory
            with tempfile.TemporaryDirectory() as temp_dir:
                with zipfile.ZipFile(io.BytesIO(file_content), 'r') as zip_ref:
                    zip_ref.extractall(temp_dir)

                # Process files
                await self._process_code_files(document['id'], temp_dir)

            # Mark document as ready
            await self.db.update_document_status(document['id'], 'ready')

        except Exception as e:
            await self.db.update_document_status(document['id'], 'failed', str(e))
            raise

    async def _process_code_files(self, document_id: str, repo_path: str):
        """Process code files in repository"""
        # File extension whitelist
        allowed_extensions = {
            '.py', '.js', '.jsx', '.ts', '.tsx', '.java', '.go', '.rs',
            '.cpp', '.c', '.h', '.cs', '.md', '.yml', '.yaml', '.json', '.toml'
        }

        # Directories to ignore
        ignore_dirs = {
            '.git', 'node_modules', 'dist', 'build', '__pycache__',
            '.venv', 'venv', 'env', '.next', '.nuxt'
        }

        for root, dirs, files in os.walk(repo_path):
            # Remove ignored directories
            dirs[:] = [d for d in dirs if d not in ignore_dirs]

            for file in files:
                file_path = os.path.join(root, file)
                rel_path = os.path.relpath(file_path, repo_path)

                # Check extension
                _, ext = os.path.splitext(file)
                if ext.lower() not in allowed_extensions:
                    continue

                try:
                    # Read file content
                    with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
                        content = f.read()

                    # Compute hash
                    sha256 = hashlib.sha256(content.encode('utf-8')).hexdigest()

                    # Check if file already exists
                    existing_file = await self.db.get_file_by_path(document_id, rel_path)
                    if existing_file and existing_file['sha256'] == sha256:
                        continue  # Skip unchanged files

                    # Create/update file record
                    file_id = await self.db.create_file(
                        document_id=document_id,
                        path=rel_path,
                        sha256=sha256,
                        language=self._detect_language(rel_path),
                        size_bytes=len(content.encode('utf-8'))
                    )

                    # Process file content
                    await self._process_file_content(document_id, file_id, rel_path, content)

                except Exception as e:
                    print(f"Error processing file {rel_path}: {e}")
                    continue

    async def _process_file_content(self, document_id: str, file_id: str,
                                  file_path: str, content: str):
        """Process individual file content"""
        language = self._detect_language(file_path)

        # Extract symbols for Python files
        if language == 'python' and TREE_SITTER_AVAILABLE:
            symbols = self._extract_python_symbols(content)
            for symbol in symbols:
                await self.db.create_symbol(
                    document_id=document_id,
                    file_id=file_id,
                    path=file_path,
                    language=language,
                    **symbol
                )

        # Chunk the file
        chunks = self._chunk_code_file(content, language)

        # Generate embeddings and store chunks
        for chunk in chunks:
            if self.embedding_model:
                embedding = self.embedding_model.encode(chunk['content']).tolist()
            else:
                embedding = [0.1] * 384  # Placeholder

            metadata = {
                'path': file_path,
                'start_line': chunk['start_line'],
                'end_line': chunk['end_line'],
                'language': language
            }

            if 'symbol' in chunk:
                metadata['symbol'] = chunk['symbol']

            await self.db.create_chunk(
                document_id=document_id,
                chunk_type='code',
                content=chunk['content'],
                embedding=embedding,
                metadata=metadata,
                file_id=file_id
            )

    def _chunk_pdf_pages(self, pages: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """Chunk PDF pages into smaller pieces"""
        chunks = []
        target_chunk_size = 2400  # ~600-900 tokens

        current_chunk = ""
        current_page = pages[0]['page'] if pages else 1

        for page in pages:
            page_content = page['content']
            page_words = page_content.split()

            if len(current_chunk) + len(page_content) > target_chunk_size and current_chunk:
                # Save current chunk
                chunks.append({
                    'content': current_chunk.strip(),
                    'page': current_page
                })
                current_chunk = ""
                current_page = page['page']

            current_chunk += " " + page_content if current_chunk else page_content

        # Add final chunk
        if current_chunk:
            chunks.append({
                'content': current_chunk.strip(),
                'page': current_page
            })

        return chunks

    def _detect_language(self, file_path: str) -> str:
        """Detect programming language from file extension"""
        ext_map = {
            '.py': 'python',
            '.js': 'javascript',
            '.jsx': 'javascript',
            '.ts': 'typescript',
            '.tsx': 'typescript',
            '.java': 'java',
            '.go': 'go',
            '.rs': 'rust',
            '.cpp': 'cpp',
            '.c': 'c',
            '.h': 'c',
            '.cs': 'csharp',
            '.md': 'markdown',
            '.yml': 'yaml',
            '.yaml': 'yaml',
            '.json': 'json',
            '.toml': 'toml'
        }

        _, ext = os.path.splitext(file_path)
        return ext_map.get(ext.lower(), 'unknown')

    def _extract_python_symbols(self, content: str) -> List[Dict[str, Any]]:
        """Extract functions and classes from Python code"""
        symbols = []

        try:
            # Simple regex-based extraction (could be improved with AST)
            lines = content.split('\n')
            for i, line in enumerate(lines):
                line = line.strip()

                # Function definitions
                if line.startswith('def '):
                    func_name = line.split('(')[0].replace('def ', '').strip()
                    symbols.append({
                        'symbol_type': 'function',
                        'symbol_name': func_name,
                        'start_line': i + 1,
                        'end_line': self._find_function_end(lines, i),
                        'signature': line
                    })

                # Class definitions
                elif line.startswith('class '):
                    class_name = line.split('(')[0].split(':')[0].replace('class ', '').strip()
                    symbols.append({
                        'symbol_type': 'class',
                        'symbol_name': class_name,
                        'start_line': i + 1,
                        'end_line': self._find_class_end(lines, i),
                        'signature': line
                    })

        except Exception as e:
            print(f"Error extracting symbols: {e}")

        return symbols

    def _find_function_end(self, lines: List[str], start_idx: int) -> int:
        """Find the end line of a function"""
        indent_level = len(lines[start_idx]) - len(lines[start_idx].lstrip())
        for i in range(start_idx + 1, len(lines)):
            line = lines[i]
            if line.strip() and not line.startswith(' ') * (indent_level + 1) and not line.startswith('\t') * (indent_level + 1):
                return i
        return len(lines)

    def _find_class_end(self, lines: List[str], start_idx: int) -> int:
        """Find the end line of a class"""
        indent_level = len(lines[start_idx]) - len(lines[start_idx].lstrip())
        for i in range(start_idx + 1, len(lines)):
            line = lines[i]
            if line.strip() and len(line) - len(line.lstrip()) <= indent_level:
                return i
        return len(lines)

    def _chunk_code_file(self, content: str, language: str) -> List[Dict[str, Any]]:
        """Chunk code file into smaller pieces"""
        lines = content.split('\n')
        chunks = []

        # Use line-based chunking with overlap
        chunk_size = 160
        overlap = 40

        i = 0
        while i < len(lines):
            end_idx = min(i + chunk_size, len(lines))

            chunk_lines = lines[i:end_idx]
            chunk_content = '\n'.join(chunk_lines)

            chunks.append({
                'content': chunk_content,
                'start_line': i + 1,
                'end_line': end_idx
            })

            i += chunk_size - overlap

        return chunks
