/**
 * API Client for GDD RAG Backend
 * 
 * This client defines the API endpoints that will be implemented
 * by the Python backend service. For now, these are placeholder
 * implementations that return mock data or throw errors.
 * 
 * TODO: Replace with actual API calls when backend is ready
 */

import type {
  Workspace,
  Document,
  GDDSummary,
  GameSpec,
  CoverageReport,
  ChatMessage,
  UploadGDDRequest,
  UploadGDDResponse,
  UploadCodeRequest,
  UploadCodeResponse,
  CreateWorkspaceRequest,
  CreateWorkspaceResponse,
  GetGDDSummaryResponse,
  GetCoverageReportResponse,
  ChatRequest,
  ChatResponse,
  APIError,
} from "./types"

// Import mock client for development
import {
  mockDocumentAPI,
  mockGDDAPI,
  mockChatAPI,
  mockCoverageAPI,
} from "./mock-client"

// Base URL for backend API.
// Updated to point to Flask backend on port 8000
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000"

// Use mock client only when explicitly enabled via env var.
// We intentionally ignore localStorage here to avoid accidentally
// forcing mock mode when a real backend is available.
const USE_MOCK = process.env.NEXT_PUBLIC_USE_MOCK === "true"

async function fetchAPI<T>(
  endpoint: string,
  options?: RequestInit
): Promise<T> {
  // Use mock if enabled
  if (USE_MOCK) {
    throw new Error("Mock mode: Use mock client methods directly")
  }

  const url = `${API_BASE_URL}${endpoint}`
  console.log(`[fetchAPI] Calling: ${url}`, options)
  
  try {
    const response = await fetch(url, {
      ...options,
      headers: {
        "Content-Type": "application/json",
        ...options?.headers,
      },
    })

    if (!response.ok) {
      const error: APIError = await response.json().catch(() => ({
        error: "Unknown error",
        message: `HTTP ${response.status}: ${response.statusText}`,
      }))
      console.error(`[fetchAPI] Error response from ${url}:`, error)
      throw error
    }

    return response.json()
  } catch (error: any) {
    console.error(`[fetchAPI] Fetch failed for ${url}:`, error)
    if (error.message?.includes("Failed to fetch") || error.message?.includes("NetworkError")) {
      throw new Error(`Cannot connect to backend at ${API_BASE_URL}. Make sure the backend is running on port 8000.`)
    }
    throw error
  }
}

async function uploadFile<T>(
  endpoint: string,
  file: File | Blob,
  additionalData?: Record<string, string>
): Promise<T> {
  // Use mock if enabled
  if (USE_MOCK) {
    throw new Error("Mock mode: Use mock client methods directly")
  }

  const formData = new FormData()
  formData.append("file", file)
  
  if (additionalData) {
    Object.entries(additionalData).forEach(([key, value]) => {
      formData.append(key, value)
    })
  }

  const url = `${API_BASE_URL}${endpoint}`
  const response = await fetch(url, {
    method: "POST",
    body: formData,
  })

  if (!response.ok) {
    const error: APIError = await response.json().catch(() => ({
      error: "Unknown error",
      message: `HTTP ${response.status}: ${response.statusText}`,
    }))
    throw error
  }

  return response.json()
}

// Helper to check if backend is available
async function checkBackendAvailable(): Promise<boolean> {
  try {
    const response = await fetch(`${API_BASE_URL}/health`, { 
      method: "GET",
      signal: AbortSignal.timeout(2000), // 2 second timeout
    })
    return response.ok
  } catch {
    return false
  }
}

// Workspace API - Adapted for document-centric backend
export const workspaceAPI = {
  create: async (data: CreateWorkspaceRequest): Promise<Workspace> => {
    if (USE_MOCK) {
      return {
        id: `workspace_${Date.now()}`,
        name: data.name,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      }
    }
    // For now, create a mock workspace since the new backend is document-centric
    return {
      id: `workspace_${Date.now()}`,
      name: data.name,
      description: data.description,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    }
  },

  list: async (): Promise<Workspace[]> => {
    if (USE_MOCK) {
      return [{
        id: "tank_war",
        name: "Tank War GDD & Code",
        description: "Tank War game design documents and codebase",
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        stats: {
          documents: 69,
          gdds: 44,
          codeFiles: 25
        }
      }]
    }

    // New backend doesn't have workspaces, return a default workspace
    return [{
      id: "tank_war",
      name: "Tank War GDD & Code",
      description: "Tank War game design documents and codebase",
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      stats: {
        documents: 69,
        gdds: 44,
        codeFiles: 25
      }
    }]
  },

  get: async (id: string): Promise<Workspace> => {
    if (USE_MOCK) {
      return {
        id,
        name: "Tank War GDD & Code",
        description: "Tank War game design documents and codebase",
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        stats: {
          documents: 69,
          gdds: 44,
          codeFiles: 25
        }
      }
    }

    // Return default workspace for the new backend
    return {
      id,
      name: "Tank War GDD & Code",
      description: "Tank War game design documents and codebase",
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      stats: {
        documents: 69,
        gdds: 44,
        codeFiles: 25
      }
    }
  },

  update: async (id: string, data: { name?: string; description?: string }): Promise<Workspace> => {
    if (USE_MOCK) {
      return {
        id,
        name: data.name || "Tank War GDD & Code",
        description: data.description,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      }
    }

    // Mock update since backend doesn't support workspace updates yet
    return {
      id,
      name: data.name || "Tank War GDD & Code",
      description: data.description,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    }
  },

  delete: async (id: string): Promise<void> => {
    if (USE_MOCK) return
    // Mock delete - not implemented in new backend yet
    return
  },

  setDefault: async (id: string): Promise<void> => {
    if (USE_MOCK) return
    // Mock set default - not implemented in new backend yet
    return
  },

  getDefault: async (): Promise<{ default_workspace: string | null; name?: string }> => {
    if (USE_MOCK) {
      return { default_workspace: "tank_war", name: "Tank War GDD & Code" }
    }

    // Return default workspace
    return { default_workspace: "tank_war", name: "Tank War GDD & Code" }
  },
}

// Document API
export const documentAPI = {
  uploadGDD: async (data: UploadGDDRequest & { workspaceId?: string }): Promise<UploadGDDResponse> => {
    // Check if backend is available, fallback to mock
    const backendAvailable = await checkBackendAvailable()

    if (USE_MOCK || !backendAvailable) {
      console.log("📝 Using mock API for GDD upload")
      return mockDocumentAPI.uploadGDD(data)
    }

    const formData = new FormData()
    formData.append("file", data.file)

    const url = `${API_BASE_URL}/ingest/docs`
    const response = await fetch(url, {
      method: "POST",
      body: formData,
    })

    if (!response.ok) {
      const error: APIError = await response.json().catch(() => ({
        error: "Unknown error",
        message: `HTTP ${response.status}: ${response.statusText}`,
      }))
      throw error
    }

    const result = await response.json()
    // Transform new backend response to expected format
    return {
      docId: result.document_id,
      status: "uploaded", // Will be updated by indexing job
      message: `Document uploaded successfully. Job ID: ${result.job_id}`
    }
  },

  uploadCode: async (data: UploadCodeRequest & { workspaceId?: string }): Promise<UploadCodeResponse> => {
    // Check if backend is available, fallback to mock
    const backendAvailable = await checkBackendAvailable()

    if (USE_MOCK || !backendAvailable) {
      console.log("📝 Using mock API for code upload")
      return mockDocumentAPI.uploadCode(data)
    }

    const formData = new FormData()
    formData.append("file", data.file)

    const url = `${API_BASE_URL}/ingest/code`
    const response = await fetch(url, {
      method: "POST",
      body: formData,
    })

    if (!response.ok) {
      const error: APIError = await response.json().catch(() => ({
        error: "Unknown error",
        message: `HTTP ${response.status}: ${response.statusText}`,
      }))
      throw error
    }

    const result = await response.json()
    // Transform new backend response to expected format
    return {
      indexId: result.document_id,
      status: "uploaded", // Will be updated by indexing job
      message: `Code uploaded successfully. Job ID: ${result.job_id}`
    }
  },

  list: async (workspaceId?: string): Promise<Document[]> => {
    // The new backend is document-centric, so we'll return mock data for now
    // In a real implementation, this would query the documents table
    if (USE_MOCK) {
      return mockDocumentAPI.list()
    }

    // For now, return a list of known documents from the Tank War workspace
    const documents: Document[] = [
      {
        id: "tank_war_docs",
        name: "Tank War GDD Documents",
        type: "gdd",
        filePath: "tank_war_gdd.zip",
        status: "indexed",
        indexedAt: new Date().toISOString(),
        chunksCount: 69
      },
      {
        id: "tank_war_code",
        name: "Tank War Codebase",
        type: "code",
        filePath: "tank_war_code.zip",
        status: "indexed",
        indexedAt: new Date().toISOString(),
        chunksCount: 25
      }
    ]

    return documents
  },

  get: async (id: string): Promise<Document> => {
    const backendAvailable = await checkBackendAvailable()
    
    if (USE_MOCK || !backendAvailable) {
      return mockDocumentAPI.get(id)
    }
    return fetchAPI(`/documents/${id}`)
  },

  getStatus: async (id: string): Promise<Document> => {
    const backendAvailable = await checkBackendAvailable()
    
    if (USE_MOCK || !backendAvailable) {
      return mockDocumentAPI.getStatus(id)
    }
    return fetchAPI(`/documents/${id}/status`)
  },

  uploadGDDBatch: async (files: File[]): Promise<any> => {
    const backendAvailable = await checkBackendAvailable()
    if (USE_MOCK || !backendAvailable) {
      throw new Error("Batch upload requires backend; mock mode not supported.")
    }
    const formData = new FormData()
    files.forEach((file) => formData.append("files", file))
    const url = `${API_BASE_URL}/documents/gdd/batch`
    const res = await fetch(url, { method: "POST", body: formData })
    if (!res.ok) {
      const text = await res.text()
      throw new Error(text || `Upload failed (HTTP ${res.status})`)
    }
    return res.json()
  },

  uploadCodeBatch: async (files: File[], options?: { rebuildBehaviorIndex?: boolean; workspaceId?: string }): Promise<any> => {
    const backendAvailable = await checkBackendAvailable()
    if (USE_MOCK || !backendAvailable) {
      throw new Error("Batch upload requires backend; mock mode not supported.")
    }
    const formData = new FormData()
    files.forEach((file) => formData.append("files", file))
    if (options?.rebuildBehaviorIndex) {
      formData.append("rebuildBehaviorIndex", "true")
    }
    if (options?.workspaceId) {
      formData.append("workspaceId", options.workspaceId)
    }
    const url = `${API_BASE_URL}/documents/code/batch`
    const res = await fetch(url, { method: "POST", body: formData })
    if (!res.ok) {
      const text = await res.text()
      throw new Error(text || `Upload failed (HTTP ${res.status})`)
    }
    return res.json()
  },

  uploadGDDArchive: async (file: File, options?: { workspaceId?: string }): Promise<any> => {
    const backendAvailable = await checkBackendAvailable()
    if (USE_MOCK || !backendAvailable) {
      throw new Error("Archive upload requires backend; mock mode not supported.")
    }
    const formData = new FormData()
    formData.append("file", file)
    if (options?.workspaceId) {
      formData.append("workspaceId", options.workspaceId)
    }
    const url = `${API_BASE_URL}/documents/gdd/archive`
    const res = await fetch(url, { method: "POST", body: formData })
    if (!res.ok) {
      const text = await res.text()
      throw new Error(text || `Upload failed (HTTP ${res.status})`)
    }
    return res.json()
  },

  uploadCodeArchive: async (file: File, opts?: { rebuildBehaviorIndex?: boolean; workspaceId?: string }): Promise<any> => {
    const backendAvailable = await checkBackendAvailable()
    if (USE_MOCK || !backendAvailable) {
      throw new Error("Archive upload requires backend; mock mode not supported.")
    }
    const formData = new FormData()
    formData.append("file", file)
    if (opts?.rebuildBehaviorIndex) {
      formData.append("rebuildBehaviorIndex", "true")
    }
    if (opts?.workspaceId) {
      formData.append("workspaceId", opts.workspaceId)
    }
    const url = `${API_BASE_URL}/documents/code/archive`
    const res = await fetch(url, { method: "POST", body: formData })
    if (!res.ok) {
      const text = await res.text()
      throw new Error(text || `Upload failed (HTTP ${res.status})`)
    }
    return res.json()
  },
}

// GDD API
export const gddAPI = {
  getSummary: async (docId: string): Promise<GetGDDSummaryResponse> => {
    const backendAvailable = await checkBackendAvailable()
    
    if (USE_MOCK || !backendAvailable) {
      return mockGDDAPI.getSummary(docId)
    }
    return fetchAPI(`/gdd/${docId}/summary`)
  },

  getSpec: async (docId: string): Promise<GameSpec> => {
    const backendAvailable = await checkBackendAvailable()
    
    if (USE_MOCK || !backendAvailable) {
      return mockGDDAPI.getSpec(docId)
    }
    const response = await fetchAPI<any>(`/gdd/${docId}/spec`)
    // Backend may return either the spec directly or { spec, savedTo }
    if (response?.spec) {
      return response.spec as GameSpec
    }
    return response as GameSpec
  },

  analyze: async (docId: string): Promise<GDDSummary> => {
    const backendAvailable = await checkBackendAvailable()
    
    if (USE_MOCK || !backendAvailable) {
      return mockGDDAPI.analyze(docId)
    }
    return fetchAPI(`/gdd/${docId}/analyze`, { method: "POST" })
  },
}

// Coverage API
export const coverageAPI = {
  evaluate: async (
    workspaceId: string,
    gddDocId: string,
    codeBatchId: string,
    mode: "fast" | "full",
    topK?: number,
    maxRequirements?: number
  ): Promise<CoverageReport> => {
    // Always use real backend - never fallback to mock for coverage
    if (USE_MOCK) {
      console.warn("[CoverageAPI] Mock mode enabled, but coverage evaluation requires real backend")
    }

    // Strict contract payload
    const payload = {
      workspaceId,
      gddDocId,
      codeBatchId,
      mode,
      topK: topK || 4,
      maxRequirements: mode === "fast" ? (maxRequirements || 5) : undefined
    }

    console.log("[CoverageAPI] Starting evaluation request:", payload)

    try {
      // Use AbortController for timeout (30 minutes for large evaluations)
      const controller = new AbortController()
      const timeoutId = setTimeout(() => controller.abort(), 30 * 60 * 1000) // 30 minutes

      const res = await fetch("/api/coverage/evaluate", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
        signal: controller.signal,
      })

      clearTimeout(timeoutId)

      if (!res.ok) {
        let errorMessage = `HTTP ${res.status}: ${res.statusText}`
        try {
          const errorData = await res.json()
          errorMessage = errorData?.message || errorData?.detail || errorMessage
          console.error("[CoverageAPI] Backend error response:", errorData)
        } catch {
          // If response isn't JSON, use status text
          const text = await res.text().catch(() => "")
          errorMessage = text || errorMessage
        }
        throw new Error(errorMessage)
      }

      const data = await res.json()
      console.log("[CoverageAPI] Evaluation response received:", {
        hasReport: !!data.report,
        warnings: data.warnings?.length || 0,
      })

      if (!data.report) {
        throw new Error("Invalid response: missing report data")
      }

      return data.report as CoverageReport
    } catch (error: any) {
      if (error.name === "AbortError") {
        console.error("[CoverageAPI] Request timeout after 5 minutes")
        throw new Error("Evaluation timed out after 5 minutes. The evaluation may still be running on the server.")
      }
      
      console.error("[CoverageAPI] Error calling /api/coverage/evaluate:", error)
      
      // Provide helpful error messages
      if (error.message) {
        throw error
      }
      
      // Don't fallback to mock - show the real error
      console.error("[CoverageAPI] Evaluation failed, not using mock:", error)
      throw new Error(`Coverage evaluation failed: ${error.message || "Unknown error"}`)
    }
  },

  getReport: async (
    docId: string,
    codeIndexId: string
  ): Promise<GetCoverageReportResponse> => {
    const backendAvailable = await checkBackendAvailable()
    
    if (USE_MOCK || !backendAvailable) {
      return mockCoverageAPI.getReport(docId, codeIndexId)
    }
    return fetchAPI(`/coverage/${docId}/${codeIndexId}`)
  },
}

// Chat API - Updated for new FastAPI RAG backend
export const chatAPI = {
  send: async (data: ChatRequest): Promise<ChatResponse> => {
    console.log("[ChatAPI] Sending message:", data.message)

    try {
      // Use the new /api/chat endpoint with document scope support
      const payload = {
        message: data.message,
        document_scope: {
          code_document_id: data.workspaceId, // Map workspaceId to code_document_id for now
          docs_document_id: undefined // Will be populated when docs are added
        },
        topK: data.topK || 10
      }

      const res = await fetch(`${API_BASE_URL}/api/chat`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      })

      if (!res.ok) {
        const errorText = await res.text()
        console.error("[ChatAPI] Backend error:", errorText)
        throw new Error(`HTTP ${res.status}: ${res.statusText}`)
      }

      const result = await res.json()
      console.log("[ChatAPI] Received response:", result)

      // Transform new backend response format to expected frontend format
      const chatMessage: ChatMessage = {
        id: result.timestamp || Date.now().toString(),
        role: "assistant",
        content: result.answer || result.content || "No response generated",
        timestamp: result.timestamp || new Date().toISOString(),
        context: {
          sources: result.citations || [],
          chunks: result.evidence ? result.evidence.map((e: any) => ({
            chunkId: e.citation_id,
            content: e.quote,
            score: 0.8, // Default score
            filePath: e.citation_id
          })) : []
        }
      }

      return { message: chatMessage }

    } catch (error: any) {
      console.error("[ChatAPI] Chat failed:", error)
      throw new Error(error.message || "Chat failed")
    }
  },

  sendStream: async (
    data: ChatRequest,
    onToken: (token: string) => void,
    onContext?: (context: { docIds: string[]; chunks: any[] }) => void,
    onDone?: (timestamp: string) => void,
    onError?: (error: string) => void
  ): Promise<void> => {
    const payload = { ...data, workspaceId: data.workspaceId || "tank_war" }
    const response = await fetch("/api/chat/stream", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(payload),
    })

    if (!response.ok) {
      const error = await response.json().catch(() => ({
        error: "Unknown error",
        message: `HTTP ${response.status}: ${response.statusText}`,
      }))
      if (onError) onError(error.message || "Stream failed")
      throw error
    }

    const reader = response.body?.getReader()
    const decoder = new TextDecoder()

    if (!reader) {
      if (onError) onError("No response body")
      return
    }

    let buffer = ""

    while (true) {
      const { done, value } = await reader.read()
      if (done) break

      buffer += decoder.decode(value, { stream: true })
      const lines = buffer.split("\n")
      buffer = lines.pop() || ""

      for (const line of lines) {
        if (line.startsWith("data: ")) {
          try {
            const jsonStr = line.slice(6)
            const event = JSON.parse(jsonStr)

            if (event.type === "token" && event.content) {
              onToken(event.content)
            } else if (event.type === "context" && onContext) {
              onContext(event)
            } else if (event.type === "done" && onDone) {
              onDone(event.timestamp)
            } else if (event.type === "error" && onError) {
              onError(event.content)
            }
          } catch (e) {
            console.error("Failed to parse SSE event:", e)
          }
        }
      }
    }
  },

  getHistory: async (workspaceId: string): Promise<ChatMessage[]> => {
    // Always call real backend; do not fallback to mock
    if (USE_MOCK) {
      console.warn("[chatAPI] Mock mode enabled, but chat history will call real backend")
    }
    return fetchAPI(`/chat/${workspaceId}/history`)
  },

  clearHistory: async (workspaceId: string): Promise<void> => {
    // Always call real backend; do not fallback to mock
    if (USE_MOCK) {
      console.warn("[chatAPI] Mock mode enabled, but clearHistory will call real backend")
    }
    return fetchAPI(`/chat/${workspaceId}/history`, { method: "DELETE" })
  },

  // New extraction APIs for the chat-first RAG system
  extractCode: async (documentId: string, symbolName: string, symbolType: string) => {
    const backendAvailable = await checkBackendAvailable()

    if (USE_MOCK || !backendAvailable) {
      throw new Error("Code extraction requires backend; mock mode not supported.")
    }

    return fetchAPI(`/extract/code`, {
      method: "POST",
      body: JSON.stringify({
        document_id: documentId,
        symbol_name: symbolName,
        symbol_type: symbolType
      })
    })
  },

  extractDocs: async (documentId: string, query: string, mode: string = "phrase") => {
    const backendAvailable = await checkBackendAvailable()

    if (USE_MOCK || !backendAvailable) {
      throw new Error("Document extraction requires backend; mock mode not supported.")
    }

    return fetchAPI(`/extract/docs`, {
      method: "POST",
      body: JSON.stringify({
        document_id: documentId,
        query: query,
        mode: mode
      })
    })
  },
}
