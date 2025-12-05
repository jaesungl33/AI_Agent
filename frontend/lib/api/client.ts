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
// Default points to local FastAPI server without '/api' suffix,
// because our FastAPI routes are mounted at the root (e.g. /health, /documents/gdd).
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

// Workspace API
export const workspaceAPI = {
  create: async (data: CreateWorkspaceRequest): Promise<CreateWorkspaceResponse> => {
    if (USE_MOCK) {
      return {
        workspace: {
          id: `workspace_${Date.now()}`,
          name: data.name,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
          status: "ready",
        },
      }
    }
    return fetchAPI("/workspaces", {
      method: "POST",
      body: JSON.stringify(data),
    })
  },

  list: async (): Promise<Workspace[]> => {
    if (USE_MOCK) {
      return []
    }
    return fetchAPI("/workspaces")
  },

  get: async (id: string): Promise<Workspace> => {
    if (USE_MOCK) {
      return {
        id,
        name: "Mock Workspace",
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        status: "ready",
      }
    }
    return fetchAPI(`/workspaces/${id}`)
  },

  delete: async (id: string): Promise<void> => {
    if (USE_MOCK) return
    return fetchAPI(`/workspaces/${id}`, { method: "DELETE" })
  },
}

// Document API
export const documentAPI = {
  uploadGDD: async (data: UploadGDDRequest): Promise<UploadGDDResponse> => {
    // Check if backend is available, fallback to mock
    const backendAvailable = await checkBackendAvailable()
    
    if (USE_MOCK || !backendAvailable) {
      console.log("📝 Using mock API for GDD upload")
      return mockDocumentAPI.uploadGDD(data)
    }

    const formData = new FormData()
    formData.append("file", data.file)
    if (data.docId) {
      formData.append("docId", data.docId)
    }

    const url = `${API_BASE_URL}/documents/gdd`
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
  },

  uploadCode: async (data: UploadCodeRequest): Promise<UploadCodeResponse> => {
    // Check if backend is available, fallback to mock
    const backendAvailable = await checkBackendAvailable()
    
    if (USE_MOCK || !backendAvailable) {
      console.log("📝 Using mock API for code upload")
      return mockDocumentAPI.uploadCode(data)
    }

    return uploadFile("/documents/code", data.file, data.indexId ? { indexId: data.indexId } : undefined)
  },

  list: async (): Promise<Document[]> => {
    // Always try real backend first - don't fallback to mock
    if (USE_MOCK) {
      return mockDocumentAPI.list()
    }
    try {
      // Use Next.js proxy to avoid direct :8000 fetch issues
      const res = await fetch("/api/documents", {
        method: "GET",
        headers: { "Content-Type": "application/json" },
      })
      if (!res.ok) {
        const msg = await res.text().catch(() => res.statusText)
        throw new Error(msg || "Failed to fetch documents")
      }
      return (await res.json()) as Document[]
    } catch (error) {
      console.error("[DocumentAPI] Failed to fetch documents:", error)
      throw error // Don't fallback to mock, show the error
    }
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
    const response = await fetchAPI<GameSpec>(`/gdd/${docId}/spec`)
    return response
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
    docId: string | string[],
    codeIndexId: string | string[],
    topK?: number
  ): Promise<CoverageReport> => {
    // Always use real backend - never fallback to mock for coverage
    if (USE_MOCK) {
      console.warn("[CoverageAPI] Mock mode enabled, but coverage evaluation requires real backend")
    }
    
    // Call Next.js API route, which proxies to the Python backend.
    // This avoids CORS issues between :3000 and :8000.
    const payload = { docId, codeIndexId, topK }

    console.log("[CoverageAPI] Starting evaluation request:", payload)

    try {
      // Use AbortController for timeout (10 minutes for long evaluations)
      const controller = new AbortController()
      const timeoutId = setTimeout(() => controller.abort(), 10 * 60 * 1000) // 10 minutes

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

// Chat API
export const chatAPI = {
  send: async (data: ChatRequest): Promise<ChatResponse> => {
    // For chat we always prefer the real backend when configured.
    // If NEXT_PUBLIC_API_URL is set correctly and the backend is running,
    // this will hit FastAPI's /chat endpoint.
    // Never use mock for chat - always try the real backend first.
    try {
      return await fetchAPI("/chat", {
        method: "POST",
        body: JSON.stringify(data),
      })
    } catch (error) {
      // If backend fails, throw the error instead of falling back to mock
      // This ensures users know the backend isn't working
      console.error("Chat API error:", error)
      throw error
    }
  },

  sendStream: async (
    data: ChatRequest,
    onToken: (token: string) => void,
    onContext?: (context: { docIds: string[]; chunks: any[] }) => void,
    onDone?: (timestamp: string) => void,
    onError?: (error: string) => void
  ): Promise<void> => {
    const url = `${API_BASE_URL}/chat/stream`
    const response = await fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
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
}
