/**
 * API Types for GDD RAG Backend
 * These types define the contract between the frontend and backend API
 */

// Workspace & Document Types
export interface Workspace {
  id: string
  name: string
  createdAt: string
  updatedAt: string
  gddDocId?: string
  codeIndexId?: string
  status: WorkspaceStatus
}

export type WorkspaceStatus = 
  | "empty"
  | "uploading"
  | "indexing"
  | "ready"
  | "error"

export interface Document {
  id: string
  name: string
  type: "gdd" | "code"
  filePath: string
  status: DocumentStatus
  indexedAt?: string
  chunksCount?: number
}

export type DocumentStatus = 
  | "uploaded"
  | "indexing"
  | "indexed"
  | "error"

// GDD Summary Types
export interface GDDSummary {
  docId: string
  genre?: string
  coreLoop?: string
  majorSystems: string[]
  playerInteractions: string[]
  keyObjects: string[]
  mapsAndModes: string[]
  specialMechanics: string[]
  extractedAt: string
}

export interface GameSpec {
  docId: string
  objects: GameObject[]
  systems: GameSystem[]
  logicRules: LogicRule[]
  requirements: Requirement[]
  extractedAt: string
}

export interface GameObject {
  id: string
  name: string
  category?: string
  description?: string
  specialRules?: string
}

export interface GameSystem {
  id: string
  name: string
  description?: string
  mechanics?: string
  objectives?: string
}

export interface LogicRule {
  id: string
  summary: string
  description?: string
  trigger?: string
  effect?: string
}

export interface Requirement {
  id: string
  title: string
  description?: string
  acceptanceCriteria?: string
  priority?: "high" | "medium" | "low"
}

// Code Coverage Types
export interface CoverageReport {
  docId: string
  codeIndexId: string
  generatedAt: string
  summary: CoverageSummary
  results: CoverageResult[]
}

export interface CoverageSummary {
  totalItems: number
  implemented: number
  partiallyImplemented?: number
  notImplemented: number
  errors: number
}

export interface CoverageResult {
  itemId: string
  itemType: "object" | "system" | "logic_rule" | "requirement"
  itemName: string
  status: "implemented" | "partially_implemented" | "not_implemented" | "error"
  evidence: Evidence[]
  retrievedChunks: CodeChunk[]
}

export interface Evidence {
  file?: string
  reason: string
}

export interface CodeChunk {
  chunkId: string
  content: string
  score: number
  filePath?: string
}

// Chat Types
export interface ChatMessage {
  id: string
  role: "user" | "assistant"
  content: string
  timestamp: string
  context?: ChatContext
}

export interface ChatContext {
  docIds: string[]
  chunks: CodeChunk[]
}

// API Request/Response Types
export interface UploadGDDRequest {
  file: File
  docId?: string
}

export interface UploadGDDResponse {
  docId: string
  status: DocumentStatus
  message: string
}

export interface UploadCodeRequest {
  file: File | Blob // zip file
  indexId?: string
}

export interface UploadCodeResponse {
  indexId: string
  status: DocumentStatus
  message: string
  batchCount?: number
}

export interface CreateWorkspaceRequest {
  name: string
}

export interface CreateWorkspaceResponse {
  workspace: Workspace
}

export interface GetGDDSummaryResponse {
  summary: GDDSummary
  spec?: GameSpec
}

export interface GetCoverageReportResponse {
  report: CoverageReport
}

export interface ChatRequest {
  workspaceId: string
  message: string
  useAllDocs?: boolean
  docIds?: string[]
  topK?: number
}

export interface ChatResponse {
  message: ChatMessage
}

// Error Types
export interface APIError {
  error: string
  message: string
  code?: string
}


