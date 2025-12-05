/* eslint-disable react-hooks/exhaustive-deps */
"use client"

import React, { useEffect, useState } from "react"
import { documentAPI, workspaceAPI, coverageAPI } from "@/lib/api/client"
import type { Document } from "@/lib/api/types"

type Workspace = {
  id: string
  name: string
  createdAt?: string
  updatedAt?: string
  status?: string
}

export default function WorkspacePage() {
  const [workspaces, setWorkspaces] = useState<Workspace[]>([])
  const [selectedWs, setSelectedWs] = useState<string>("")
  const [wsName, setWsName] = useState("")
  const [showList, setShowList] = useState(false)
  const [docs, setDocs] = useState<Document[]>([])
  const [gddIds, setGddIds] = useState<string[]>([])
  const [codeIds, setCodeIds] = useState<string[]>([])
  const [isLoadingDocs, setIsLoadingDocs] = useState(false)
  const [isCreating, setIsCreating] = useState(false)
  const [statusMessage, setStatusMessage] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    loadWorkspaces()
  }, [])

  useEffect(() => {
    if (selectedWs) {
      loadDocuments()
    }
  }, [selectedWs])

  const loadWorkspaces = async () => {
    try {
      setError(null)
      const list = await workspaceAPI.list()
      setWorkspaces(list)
      if (list.length > 0 && !selectedWs) {
        setSelectedWs(list[0].id)
      }
    } catch (e: any) {
      setError(e?.message || "Failed to load workspaces")
    }
  }

  const createWorkspace = async () => {
    if (!wsName.trim()) return
    try {
      setIsCreating(true)
      setError(null)
      const res = await workspaceAPI.create({ name: wsName.trim() })
      setWorkspaces((prev) => [...prev, res.workspace])
      setSelectedWs(res.workspace.id)
      setWsName("")
    } catch (e: any) {
      setError(e?.message || "Failed to create workspace")
    } finally {
      setIsCreating(false)
    }
  }

  const loadDocuments = async () => {
    try {
      setIsLoadingDocs(true)
      setError(null)
      const list = await documentAPI.list()
      setDocs(list)
      const gdds = list.filter((d) => d.type === "gdd" && d.status === "indexed")
      const codes = list.filter((d) => d.type === "code" && d.status === "indexed")
      setGddIds(gdds.map((d) => d.id))
      setCodeIds(codes.map((d) => d.id))
    } catch (e: any) {
      setError(e?.message || "Failed to load documents")
    } finally {
      setIsLoadingDocs(false)
    }
  }

  const runCoverage = async () => {
    if (!gddIds.length || !codeIds.length) {
      setError("Select at least one GDD and one code batch")
      return
    }
    setStatusMessage("Running coverage...")
    setError(null)
    try {
      const docId = gddIds.length === 1 ? gddIds[0] : gddIds
      const codeId = codeIds.length === 1 ? codeIds[0] : codeIds
      const report = await coverageAPI.evaluate(docId, codeId, 8)
      setStatusMessage(`Coverage complete: ${report.summary.totalItems} items`)
    } catch (e: any) {
      setError(e?.message || "Coverage failed")
      setStatusMessage(null)
    }
  }

  const toggleId = (ids: string[], id: string, setter: (v: string[]) => void) => {
    if (ids.includes(id)) setter(ids.filter((x) => x !== id))
    else setter([...ids, id])
  }

  return (
    <div style={{ padding: 16, display: "grid", gap: 16 }}>
      <h1>Workspace & Uploads</h1>

      {error && <div style={{ color: "red" }}>{error}</div>}
      {statusMessage && <div style={{ color: "green" }}>{statusMessage}</div>}

      <section style={{ border: "1px solid #ddd", padding: 12, borderRadius: 8 }}>
        <h3>Workspaces</h3>
        <div style={{ display: "flex", gap: 8, alignItems: "center" }}>
          <select
            value={selectedWs}
            onChange={(e) => setSelectedWs(e.target.value)}
            style={{ padding: 8 }}
          >
            <option value="" disabled>
              Select workspace
            </option>
            {workspaces.map((ws) => (
              <option key={ws.id} value={ws.id}>
                {ws.name || ws.id}
              </option>
            ))}
          </select>
          <input
            placeholder="New workspace name"
            value={wsName}
            onChange={(e) => setWsName(e.target.value)}
            style={{ padding: 8, minWidth: 180 }}
          />
          <button onClick={createWorkspace} disabled={isCreating} style={{ padding: "8px 12px" }}>
            {isCreating ? "Creating..." : "Create"}
          </button>
        </div>
        <div style={{ marginTop: 8 }}>
          <button onClick={() => setShowList(!showList)} style={{ padding: "6px 10px" }}>
            {showList ? "Hide workspace list" : "Show workspace list"}
          </button>
          {showList && (
            <ul style={{ marginTop: 8, paddingLeft: 16 }}>
              {workspaces.length === 0 && <li>No workspaces yet</li>}
              {workspaces.map((ws) => (
                <li key={ws.id}>
                  {ws.name || ws.id} {ws.id === selectedWs ? " (selected)" : ""}
                </li>
              ))}
            </ul>
          )}
        </div>
      </section>

      <section style={{ border: "1px solid #ddd", padding: 12, borderRadius: 8 }}>
        <h3>Documents</h3>
        <button onClick={loadDocuments} disabled={isLoadingDocs} style={{ padding: "8px 12px" }}>
          {isLoadingDocs ? "Loading..." : "Refresh documents"}
        </button>
        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12, marginTop: 12 }}>
          <div>
            <h4>GDDs</h4>
            {docs
              .filter((d) => d.type === "gdd" && d.status === "indexed")
              .map((d) => (
                <label key={d.id} style={{ display: "block" }}>
                  <input
                    type="checkbox"
                    checked={gddIds.includes(d.id)}
                    onChange={() => toggleId(gddIds, d.id, setGddIds)}
                  />{" "}
                  {d.id} ({d.chunksCount} chunks)
                </label>
              ))}
          </div>
          <div>
            <h4>Code Batches</h4>
            {docs
              .filter((d) => d.type === "code" && d.status === "indexed")
              .map((d) => (
                <label key={d.id} style={{ display: "block" }}>
                  <input
                    type="checkbox"
                    checked={codeIds.includes(d.id)}
                    onChange={() => toggleId(codeIds, d.id, setCodeIds)}
                  />{" "}
                  {d.id} ({d.chunksCount} chunks)
                </label>
              ))}
          </div>
        </div>
      </section>

      <section style={{ border: "1px solid #ddd", padding: 12, borderRadius: 8 }}>
        <h3>Coverage</h3>
        <p style={{ fontSize: 12, color: "#555" }}>
          Select at least one GDD and one code batch. Uses current backend evaluation (fast symbol + semantic).
        </p>
        <button onClick={runCoverage} style={{ padding: "10px 16px" }}>
          Run Coverage
        </button>
      </section>
    </div>
  )
}

