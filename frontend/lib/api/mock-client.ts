/**
 * Mock API Client for Development
 * 
 * Use this when the backend API is not available.
 * Provides mock responses for testing the UI.
 */

import type {
  UploadGDDResponse,
  UploadCodeResponse,
  Document,
  GDDSummary,
  GameSpec,
  CoverageReport,
  ChatMessage,
  ChatResponse,
} from "./types"

// Mock delay to simulate API calls
const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms))

export const mockDocumentAPI = {
  uploadGDD: async (data: { file: File; docId?: string }): Promise<UploadGDDResponse> => {
    await delay(1500) // Simulate upload time
    
    const docId = data.docId || `gdd_${Date.now()}`
    
    return {
      docId,
      status: "indexing",
      message: `File "${data.file.name}" uploaded successfully. Indexing in progress...`,
    }
  },

  uploadCode: async (data: { file: File | Blob; indexId?: string }): Promise<UploadCodeResponse> => {
    await delay(2000) // Simulate longer upload for code
    
    const indexId = data.indexId || `code_${Date.now()}`
    
    return {
      indexId,
      status: "indexing",
      message: `Code file uploaded successfully. Indexing in progress...`,
      batchCount: 5,
    }
  },

  list: async (): Promise<Document[]> => {
    await delay(500)
    return [
      {
        id: "sample_gdd",
        name: "Sample GDD",
        type: "gdd",
        filePath: "docs/sample_gdd.pdf",
        status: "indexed",
        indexedAt: new Date().toISOString(),
        chunksCount: 150,
      },
    ]
  },

  get: async (id: string): Promise<Document> => {
    await delay(300)
    return {
      id,
      name: `Document ${id}`,
      type: "gdd",
      filePath: `docs/${id}.pdf`,
      status: "indexed",
      indexedAt: new Date().toISOString(),
      chunksCount: 100,
    }
  },

  getStatus: async (id: string): Promise<Document> => {
    await delay(300)
    return {
      id,
      name: `Document ${id}`,
      type: "gdd",
      filePath: `docs/${id}.pdf`,
      status: "indexed",
      indexedAt: new Date().toISOString(),
      chunksCount: 100,
    }
  },
}

export const mockGDDAPI = {
  getSummary: async (docId: string) => {
    await delay(800)
    return {
      summary: {
        docId,
        genre: "Action Strategy",
        coreLoop: "Players engage in tank battles, upgrade their tanks, and compete in multiplayer matches",
        majorSystems: ["Combat System", "Tank Progression", "Matchmaking", "Economy"],
        playerInteractions: ["PvP Battles", "Team Play", "Tank Customization"],
        keyObjects: ["Tank", "Map", "Skill", "Artifact"],
        mapsAndModes: ["Deathmatch", "Outpost Breaker", "Base Capture"],
        specialMechanics: ["Elemental Classes", "Auto-Focus System", "Skill Control"],
        extractedAt: new Date().toISOString(),
      },
    }
  },

  getSpec: async (docId: string): Promise<GameSpec> => {
    await delay(1000)
    return {
      docId,
      objects: [
        { id: "tank_1", name: "Light Tank", category: "Vehicle", description: "Fast and agile" },
        { id: "tank_2", name: "Heavy Tank", category: "Vehicle", description: "Slow but powerful" },
      ],
      systems: [
        { id: "combat", name: "Combat System", description: "Handles shooting and damage" },
      ],
      logicRules: [],
      requirements: [],
      extractedAt: new Date().toISOString(),
    }
  },

  analyze: async (docId: string): Promise<GDDSummary> => {
    await delay(1200)
    return {
      docId,
      genre: "Action Strategy",
      coreLoop: "Players engage in tank battles",
      majorSystems: ["Combat", "Progression"],
      playerInteractions: ["PvP"],
      keyObjects: ["Tank"],
      mapsAndModes: ["Deathmatch"],
      specialMechanics: [],
      extractedAt: new Date().toISOString(),
    }
  },
}

export const mockChatAPI = {
  send: async (data: { message: string; workspaceId: string }): Promise<ChatResponse> => {
    await delay(1500)
    
    return {
      message: {
        id: Date.now().toString(),
        role: "assistant",
        content: `This is a mock response to: "${data.message}".\n\nIn production, this would query your RAG backend and return relevant information from your indexed documents.`,
        timestamp: new Date().toISOString(),
        context: {
          docIds: [],
          chunks: [],
        },
      },
    }
  },

  getHistory: async (workspaceId: string): Promise<ChatMessage[]> => {
    await delay(300)
    return []
  },

  clearHistory: async (workspaceId: string): Promise<void> => {
    await delay(200)
  },
}

export const mockCoverageAPI = {
  evaluate: async (docId: string, codeIndexId: string): Promise<CoverageReport> => {
    await delay(2000)
    const results = [
      {
        itemId: "req_1",
        itemType: "requirement" as const,
        itemName: "Core combat loop implemented",
        status: "implemented" as const,
        evidence: [
          {
            file: "Assets/Scripts/Combat/WeaponSystem.cs",
            reason: "Implements basic shooting and damage handling for tanks.",
          },
        ],
        retrievedChunks: [],
      },
      {
        itemId: "req_2",
        itemType: "requirement" as const,
        itemName: "Outpost capture overtime mechanic",
        status: "partially_implemented" as const,
        evidence: [
          {
            file: "Assets/Scripts/GameModes/OutpostCapture.cs",
            reason: "Has basic capture logic but missing overtime handling when timer is low.",
          },
        ],
        retrievedChunks: [],
      },
      {
        itemId: "req_3",
        itemType: "requirement" as const,
        itemName: "Spectator free-cam after death",
        status: "not_implemented" as const,
        evidence: [
          {
            file: undefined,
            reason: "No references to free camera or spectator controls were found in the codebase.",
          },
        ],
        retrievedChunks: [],
      },
      {
        itemId: "req_4",
        itemType: "requirement" as const,
        itemName: "Matchmaking ladder protection",
        status: "error" as const,
        evidence: [
          {
            file: undefined,
            reason: "The evaluation model could not determine coverage due to inconsistent spec data.",
          },
        ],
        retrievedChunks: [],
      },
    ]

    return {
      docId,
      codeIndexId,
      generatedAt: new Date().toISOString(),
      summary: {
        totalItems: results.length,
        implemented: results.filter(r => r.status === "implemented").length,
        partiallyImplemented: results.filter(r => r.status === "partially_implemented").length,
        notImplemented: results.filter(r => r.status === "not_implemented").length,
        errors: results.filter(r => r.status === "error").length,
      },
      results,
    }
  },

  getReport: async (docId: string, codeIndexId: string) => {
    await delay(500)
    return {
      report: {
        docId,
        codeIndexId,
        generatedAt: new Date().toISOString(),
        summary: {
          totalItems: 10,
          implemented: 7,
          notImplemented: 2,
          errors: 1,
        },
        results: [],
      },
    }
  },
}


