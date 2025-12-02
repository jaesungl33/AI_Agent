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

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"

// Use mock client in development if backend is not available
const USE_MOCK = process.env.NEXT_PUBLIC_USE_MOCK === "true" || 
                 (typeof window !== "undefined" && localStorage.getItem("useMockAPI") === "true")

async function fetchAPI<T>(
  endpoint: string,
  options?: RequestInit
): Promise<T> {
  // Use mock if enabled
  if (USE_MOCK) {
    throw new Error("Mock mode: Use mock client methods directly")
  }

  const url = `${API_BASE_URL}${endpoint}`
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
    throw error
  }

  return response.json()
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
    const backendAvailable = await checkBackendAvailable()
    
    if (USE_MOCK || !backendAvailable) {
      return mockDocumentAPI.list()
    }
    return fetchAPI("/documents")
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
    return fetchAPI(`/gdd/${docId}/spec`)
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
    docId: string,
    codeIndexId: string,
    topK?: number
  ): Promise<CoverageReport> => {
    const backendAvailable = await checkBackendAvailable()
    
    if (USE_MOCK || !backendAvailable) {
      return mockCoverageAPI.evaluate(docId, codeIndexId)
    }
    return fetchAPI(`/coverage/evaluate`, {
      method: "POST",
      body: JSON.stringify({ docId, codeIndexId, topK }),
    })
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
    const backendAvailable = await checkBackendAvailable()
    
    if (USE_MOCK || !backendAvailable) {
      return mockChatAPI.send({
        message: data.message,
        workspaceId: data.workspaceId,
      })
    }
    return fetchAPI("/chat", {
      method: "POST",
      body: JSON.stringify(data),
    })
  },

  getHistory: async (workspaceId: string): Promise<ChatMessage[]> => {
    const backendAvailable = await checkBackendAvailable()
    
    if (USE_MOCK || !backendAvailable) {
      return mockChatAPI.getHistory(workspaceId)
    }
    return fetchAPI(`/chat/${workspaceId}/history`)
  },

  clearHistory: async (workspaceId: string): Promise<void> => {
    const backendAvailable = await checkBackendAvailable()
    
    if (USE_MOCK || !backendAvailable) {
      return mockChatAPI.clearHistory(workspaceId)
    }
    return fetchAPI(`/chat/${workspaceId}/history`, { method: "DELETE" })
  },
}
