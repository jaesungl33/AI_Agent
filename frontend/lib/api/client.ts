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

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"

async function fetchAPI<T>(
  endpoint: string,
  options?: RequestInit
): Promise<T> {
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

// Workspace API
export const workspaceAPI = {
  create: async (data: CreateWorkspaceRequest): Promise<CreateWorkspaceResponse> => {
    return fetchAPI("/workspaces", {
      method: "POST",
      body: JSON.stringify(data),
    })
  },

  list: async (): Promise<Workspace[]> => {
    return fetchAPI("/workspaces")
  },

  get: async (id: string): Promise<Workspace> => {
    return fetchAPI(`/workspaces/${id}`)
  },

  delete: async (id: string): Promise<void> => {
    return fetchAPI(`/workspaces/${id}`, { method: "DELETE" })
  },
}

// Document API
export const documentAPI = {
  uploadGDD: async (data: UploadGDDRequest): Promise<UploadGDDResponse> => {
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
    return uploadFile("/documents/code", data.file, data.indexId ? { indexId: data.indexId } : undefined)
  },

  list: async (): Promise<Document[]> => {
    return fetchAPI("/documents")
  },

  get: async (id: string): Promise<Document> => {
    return fetchAPI(`/documents/${id}`)
  },

  getStatus: async (id: string): Promise<Document> => {
    return fetchAPI(`/documents/${id}/status`)
  },
}

// GDD API
export const gddAPI = {
  getSummary: async (docId: string): Promise<GetGDDSummaryResponse> => {
    return fetchAPI(`/gdd/${docId}/summary`)
  },

  getSpec: async (docId: string): Promise<GameSpec> => {
    return fetchAPI(`/gdd/${docId}/spec`)
  },

  analyze: async (docId: string): Promise<GDDSummary> => {
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
    return fetchAPI(`/coverage/evaluate`, {
      method: "POST",
      body: JSON.stringify({ docId, codeIndexId, topK }),
    })
  },

  getReport: async (
    docId: string,
    codeIndexId: string
  ): Promise<GetCoverageReportResponse> => {
    return fetchAPI(`/coverage/${docId}/${codeIndexId}`)
  },
}

// Chat API
export const chatAPI = {
  send: async (data: ChatRequest): Promise<ChatResponse> => {
    return fetchAPI("/chat", {
      method: "POST",
      body: JSON.stringify(data),
    })
  },

  getHistory: async (workspaceId: string): Promise<ChatMessage[]> => {
    return fetchAPI(`/chat/${workspaceId}/history`)
  },

  clearHistory: async (workspaceId: string): Promise<void> => {
    return fetchAPI(`/chat/${workspaceId}/history`, { method: "DELETE" })
  },
}

