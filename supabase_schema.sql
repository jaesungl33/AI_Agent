-- Supabase RAG Database Schema
-- Based on chat-first technical specification

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS vector;
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- Documents table: One uploaded artifact (PDF or repo zip)
CREATE TABLE documents (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  doc_type TEXT NOT NULL CHECK (doc_type IN ('docs','code')),
  filename TEXT NOT NULL,
  storage_path TEXT NOT NULL,
  sha256 TEXT NOT NULL,
  status TEXT NOT NULL DEFAULT 'uploaded' CHECK (status IN ('uploaded','indexing','ready','failed')),
  error TEXT,
  created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Files table: Tracks code files within a zip for incremental updates
CREATE TABLE files (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  document_id UUID NOT NULL REFERENCES documents(id) ON DELETE CASCADE,
  path TEXT NOT NULL,  -- repo-relative path
  sha256 TEXT NOT NULL,
  language TEXT,
  size_bytes INTEGER,
  created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Create unique index to prevent duplicate files per document
CREATE UNIQUE INDEX idx_files_document_path ON files(document_id, path);

-- Chunks table: The retrieval unit
CREATE TABLE chunks (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  document_id UUID NOT NULL REFERENCES documents(id) ON DELETE CASCADE,
  file_id UUID REFERENCES files(id) ON DELETE SET NULL,
  chunk_type TEXT NOT NULL CHECK (chunk_type IN ('docs','code')),
  content TEXT NOT NULL,
  embedding VECTOR(384),  -- Adjust dimension based on embedding model
  metadata JSONB NOT NULL DEFAULT '{}',
  created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Indexes for chunks table
CREATE INDEX idx_chunks_document ON chunks(document_id);
CREATE INDEX idx_chunks_embedding ON chunks USING hnsw (embedding vector_cosine_ops);
CREATE INDEX idx_chunks_type ON chunks(chunk_type);

-- Symbols table: Precise symbol table for function/class extraction
CREATE TABLE symbols (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  document_id UUID NOT NULL REFERENCES documents(id) ON DELETE CASCADE,
  file_id UUID NOT NULL REFERENCES files(id) ON DELETE CASCADE,
  path TEXT NOT NULL,
  language TEXT NOT NULL,
  symbol_type TEXT NOT NULL CHECK (symbol_type IN ('function','class','method')),
  symbol_name TEXT NOT NULL,
  start_line INTEGER NOT NULL,
  end_line INTEGER NOT NULL,
  signature TEXT,
  created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Indexes for symbols table
CREATE INDEX idx_symbols_document_name ON symbols(document_id, symbol_name);
CREATE INDEX idx_symbols_document_path ON symbols(document_id, path);

-- Jobs table: Queue system for indexing jobs
CREATE TABLE jobs (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  job_type TEXT NOT NULL CHECK (job_type IN ('index_docs','index_code')),
  document_id UUID NOT NULL REFERENCES documents(id) ON DELETE CASCADE,
  status TEXT NOT NULL DEFAULT 'queued' CHECK (status IN ('queued','running','done','failed')),
  progress JSONB DEFAULT '{}',
  error TEXT,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Index for jobs
CREATE INDEX idx_jobs_status ON jobs(status);
CREATE INDEX idx_jobs_type ON jobs(job_type);

-- Function to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Trigger for jobs table
CREATE TRIGGER update_jobs_updated_at BEFORE UPDATE ON jobs
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Row Level Security (RLS) policies
ALTER TABLE documents ENABLE ROW LEVEL SECURITY;
ALTER TABLE files ENABLE ROW LEVEL SECURITY;
ALTER TABLE chunks ENABLE ROW LEVEL SECURITY;
ALTER TABLE symbols ENABLE ROW LEVEL SECURITY;
ALTER TABLE jobs ENABLE ROW LEVEL SECURITY;

-- Basic RLS policies (adjust based on your auth requirements)
-- For now, allow all operations (you should implement proper auth)
CREATE POLICY "Allow all operations on documents" ON documents FOR ALL USING (true);
CREATE POLICY "Allow all operations on files" ON files FOR ALL USING (true);
CREATE POLICY "Allow all operations on chunks" ON chunks FOR ALL USING (true);
CREATE POLICY "Allow all operations on symbols" ON symbols FOR ALL USING (true);
CREATE POLICY "Allow all operations on jobs" ON jobs FOR ALL USING (true);

-- Storage bucket setup (run this in Supabase SQL editor)
-- This creates the uploads bucket for file storage
-- INSERT INTO storage.buckets (id, name, public) VALUES ('uploads', 'uploads', false);
