"use client"

import { useState } from "react"
import { FileUpload } from "@/components/file-upload"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Badge } from "@/components/ui/badge"
import { FileText, Code, Loader2, CheckCircle2 } from "lucide-react"
import { documentAPI } from "@/lib/api/client"
import { useWorkspace } from "@/lib/contexts/workspace-context"
import type { Document } from "@/lib/api/types"

interface WorkspaceSetupProps {
  workspaceId?: string
  onGDDUploaded?: (docId: string) => void
  onCodeUploaded?: (indexId: string) => void
}

export function WorkspaceSetup({
  workspaceId: propWorkspaceId,
  onGDDUploaded,
  onCodeUploaded,
}: WorkspaceSetupProps) {
  const { currentWorkspace } = useWorkspace()
  const workspaceId = propWorkspaceId || currentWorkspace?.id
  const [gddMode, setGddMode] = useState<"files" | "archive">("files")
  const [gddFiles, setGddFiles] = useState<File[]>([])
  const [gddArchiveFile, setGddArchiveFile] = useState<File | null>(null)
  const [codeMode, setCodeMode] = useState<"batch" | "archive">("batch")
  const [codeFiles, setCodeFiles] = useState<File[]>([])
  const [codeArchiveFile, setCodeArchiveFile] = useState<File | null>(null)
  const [gddDocId, setGddDocId] = useState<string>("")
  const [codeIndexId, setCodeIndexId] = useState<string>("")
  const [isUploadingGDD, setIsUploadingGDD] = useState(false)
  const [isUploadingCode, setIsUploadingCode] = useState(false)
  const [gddStatuses, setGddStatuses] = useState<Record<string, Document["status"] | null>>({})
  const [codeStatuses, setCodeStatuses] = useState<Record<string, Document["status"] | null>>({})
  const [rebuildBehaviorIndex, setRebuildBehaviorIndex] = useState(false)

  const handleGDDUpload = async () => {
    if (gddMode === "archive") {
      if (!gddArchiveFile) return
      setIsUploadingGDD(true)
      try {
        const response = await documentAPI.uploadGDDArchive(gddArchiveFile, workspaceId ? { workspaceId } : undefined)
        const statuses: Record<string, Document["status"] | null> = {}
        ;(response.results || []).forEach((r: any) => {
          statuses[r.docId || r.indexId || gddArchiveFile.name] = r.status
          if (r.docId) onGDDUploaded?.(r.docId)
        })
        setGddStatuses(statuses)
      } catch (error) {
        console.error("Failed to upload GDD archive:", error)
        setGddStatuses({ [gddArchiveFile.name]: "error" })
      } finally {
        setIsUploadingGDD(false)
      }
      return
    }

    if (gddFiles.length === 0) return

    setIsUploadingGDD(true)
    try {
      const statuses: Record<string, Document["status"] | null> = {}

      for (let i = 0; i < gddFiles.length; i++) {
        const file = gddFiles[i]
        const docId = gddDocId
          ? gddFiles.length > 1
            ? `${gddDocId}_${i + 1}`
            : gddDocId
          : file.name.replace(/\.[^/.]+$/, "")

        const response = await documentAPI.uploadGDD({
          file,
          docId,
          workspaceId,
        })
        statuses[file.name] = response.status
        onGDDUploaded?.(response.docId)
      }

      setGddStatuses(statuses)
    } catch (error) {
      console.error("Failed to upload GDD:", error)
      const errorStatuses: Record<string, Document["status"] | null> = {}
      gddFiles.forEach((file) => {
        errorStatuses[file.name] = "error"
      })
      setGddStatuses(errorStatuses)
    } finally {
      setIsUploadingGDD(false)
    }
  }

  const handleCodeUpload = async () => {
    if (codeMode === "archive") {
      if (!codeArchiveFile) return
      setIsUploadingCode(true)
      try {
        const response = await documentAPI.uploadCodeArchive(codeArchiveFile, { rebuildBehaviorIndex, workspaceId })
        const statuses: Record<string, Document["status"] | null> = {}
        ;(response.results || []).forEach((r: any) => {
          statuses[r.indexId || codeArchiveFile.name] = r.status
          if (r.indexId) onCodeUploaded?.(r.indexId)
        })
        setCodeStatuses(statuses)
      } catch (error) {
        console.error("Failed to upload code archive:", error)
        setCodeStatuses({ [codeArchiveFile.name]: "error" })
      } finally {
        setIsUploadingCode(false)
      }
      return
    }

    if (codeFiles.length === 0) return

    setIsUploadingCode(true)
    try {
      const statuses: Record<string, Document["status"] | null> = {}

      // If only one file and indexId provided, use single upload to honor the id
      if (codeFiles.length === 1) {
        const response = await documentAPI.uploadCode({
          file: codeFiles[0],
          indexId: codeIndexId || undefined,
          rebuildBehaviorIndex,
          workspaceId,
        })
        statuses[codeFiles[0].name] = response.status
        onCodeUploaded?.(response.indexId)
      } else {
        const response = await documentAPI.uploadCodeBatch(codeFiles, { rebuildBehaviorIndex, workspaceId })
        ;(response.results || []).forEach((r: any) => {
          statuses[r.indexId || "batch"] = r.status
          if (r.indexId) onCodeUploaded?.(r.indexId)
        })
      }

      setCodeStatuses(statuses)
    } catch (error) {
      console.error("Failed to upload code:", error)
      const errorStatuses: Record<string, Document["status"] | null> = {}
      codeFiles.forEach((file) => {
        errorStatuses[file.name] = "error"
      })
      setCodeStatuses(errorStatuses)
    } finally {
      setIsUploadingCode(false)
    }
  }

  const getStatusBadge = (status: Document["status"] | null) => {
    if (!status) return null
    switch (status) {
      case "indexed":
        return <Badge variant="success">Indexed</Badge>
      case "indexing":
        return <Badge variant="warning">Indexing...</Badge>
      case "error":
        return <Badge variant="destructive">Error</Badge>
      default:
        return <Badge variant="outline">{status}</Badge>
    }
  }

  return (
    <div className="space-y-6">
      {/* GDD Upload */}
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            <CardTitle>Game Design Document</CardTitle>
          </div>
          <CardDescription>
            Upload your GDD (PDF, DOCX, or text file). You can upload individual files or a ZIP archive to index all contained docs.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex gap-2">
            <Button variant={gddMode === "files" ? "default" : "outline"} size="sm" onClick={() => { setGddMode("files"); setGddArchiveFile(null); }}>
              Files
            </Button>
            <Button variant={gddMode === "archive" ? "default" : "outline"} size="sm" onClick={() => { setGddMode("archive"); setGddFiles([]); setGddStatuses({}); }}>
              ZIP Archive
            </Button>
          </div>
          <div className="space-y-2">
              <Label htmlFor="gdd-doc-id">Document ID (optional)</Label>
            <Input
              id="gdd-doc-id"
                placeholder="e.g., my_game_design (will auto-number for multiple files)"
              value={gddDocId}
              onChange={(e) => setGddDocId(e.target.value)}
              disabled={isUploadingGDD || gddMode === "archive"}
            />
          </div>
          {gddMode === "files" ? (
            <>
              <FileUpload
                accept={{
                  "application/pdf": [".pdf"],
                  "application/vnd.openxmlformats-officedocument.wordprocessingml.document": [".docx"],
                  "text/plain": [".txt"],
                  "text/csv": [".csv"],
                  "application/xml": [".mm", ".xml"],
                  "application/json": [".json"],
                }}
                label="Upload GDD"
                description="PDF, DOCX, TXT, CSV, MM, or JSON files up to 100MB"
                selectedFiles={gddFiles}
                isUploading={isUploadingGDD}
                allowMultiple
                maxFiles={10}
                onFilesSelect={(files) => {
                  setGddFiles((prev) => [...prev, ...files])
                  setGddStatuses({})
                }}
                onRemoveFile={(file) => {
                  setGddFiles((prev) => prev.filter((f) => f !== file))
                  setGddStatuses((prev) => {
                    const next = { ...prev }
                    delete next[file.name]
                    return next
                  })
                }}
              />
              {gddFiles.length > 0 && (
                <div className="space-y-3">
                  {gddFiles.map((file) => (
                    <div key={file.name} className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium">{file.name}</p>
                        <p className="text-xs text-muted-foreground">
                          {(file.size / 1024 / 1024).toFixed(2)} MB
                        </p>
                      </div>
                      {getStatusBadge(gddStatuses[file.name] || null)}
                    </div>
                  ))}
                  <Button
                    onClick={handleGDDUpload}
                    disabled={isUploadingGDD}
                  >
                    {isUploadingGDD ? (
                      <>
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                        Uploading...
                      </>
                    ) : (
                      "Upload & Index"
                    )}
                  </Button>
                </div>
              )}
            </>
          ) : (
            <>
              <FileUpload
                accept={{
                  "application/zip": [".zip"],
                  "application/x-zip-compressed": [".zip"],
                }}
                label="Upload GDD ZIP"
                description="ZIP containing multiple GDD files (PDF/DOCX/TXT/etc.)"
                selectedFiles={gddArchiveFile ? [gddArchiveFile] : []}
                isUploading={isUploadingGDD}
                onFilesSelect={(files) => {
                  setGddArchiveFile(files[0])
                  setGddStatuses({})
                }}
                onRemoveFile={() => {
                  setGddArchiveFile(null)
                  setGddStatuses({})
                }}
              />
              {gddArchiveFile && (
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-medium">{gddArchiveFile.name}</p>
                    <p className="text-xs text-muted-foreground">
                      {(gddArchiveFile.size / 1024 / 1024).toFixed(2)} MB
                    </p>
                  </div>
                  {getStatusBadge(gddStatuses[gddArchiveFile.name] || null)}
                  <Button onClick={handleGDDUpload} disabled={isUploadingGDD}>
                    {isUploadingGDD ? (
                      <>
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                        Uploading...
                      </>
                    ) : (
                      "Upload & Index"
                    )}
                  </Button>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>

      {/* Code Upload */}
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <Code className="h-5 w-5" />
            <CardTitle>Game Code</CardTitle>
          </div>
          <CardDescription>
            Upload your game code as ZIPs or a ZIP archive. Archives can optionally trigger a behavior-index rebuild.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex gap-2">
            <Button variant={codeMode === "batch" ? "default" : "outline"} size="sm" onClick={() => { setCodeMode("batch"); setCodeArchiveFile(null); }}>
              ZIP as code batch(es)
            </Button>
            <Button variant={codeMode === "archive" ? "default" : "outline"} size="sm" onClick={() => { setCodeMode("archive"); setCodeFiles([]); setCodeStatuses({}); }}>
              ZIP archive (split & index)
            </Button>
          </div>
          <div className="space-y-2">
            <Label htmlFor="code-index-id">Index ID (optional)</Label>
            <Input
              id="code-index-id"
              placeholder="e.g., game_codebase"
              value={codeIndexId}
              onChange={(e) => setCodeIndexId(e.target.value)}
              disabled={isUploadingCode || codeMode === "archive"}
            />
          </div>
          {codeMode === "batch" ? (
            <>
              <FileUpload
                accept={{
                  "application/zip": [".zip"],
                  "application/x-zip-compressed": [".zip"],
                }}
                maxSize={500 * 1024 * 1024} // 500MB for code
                label="Upload Code ZIP(s)"
                description="ZIP file(s) containing your game code (up to 500MB each)"
                selectedFiles={codeFiles}
                isUploading={isUploadingCode}
                allowMultiple
                onFilesSelect={(files) => {
                  setCodeFiles((prev) => [...prev, ...files])
                  setCodeStatuses({})
                }}
                onRemoveFile={(file) => {
                  setCodeFiles((prev) => prev.filter((f) => f !== file))
                  setCodeStatuses((prev) => {
                    const next = { ...prev }
                    delete next[file.name]
                    return next
                  })
                }}
              />
              {codeFiles.length > 0 && (
                <div className="space-y-3">
                  {codeFiles.map((file) => (
                    <div key={file.name} className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium">{file.name}</p>
                        <p className="text-xs text-muted-foreground">
                          {(file.size / 1024 / 1024).toFixed(2)} MB
                        </p>
                      </div>
                      {getStatusBadge(codeStatuses[file.name] || null)}
                    </div>
                  ))}
                  <div className="flex items-center justify-between gap-4">
                    <label className="flex items-center gap-2 text-sm">
                      <input
                        type="checkbox"
                        checked={rebuildBehaviorIndex}
                        onChange={(e) => setRebuildBehaviorIndex(e.target.checked)}
                        disabled={isUploadingCode}
                      />
                      Rebuild behavior index after upload
                    </label>
                    <Button
                      onClick={handleCodeUpload}
                      disabled={isUploadingCode}
                    >
                      {isUploadingCode ? (
                        <>
                          <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                          Uploading...
                        </>
                      ) : (
                        "Upload & Index"
                      )}
                    </Button>
                  </div>
                </div>
              )}
            </>
          ) : (
            <>
              <FileUpload
                accept={{
                  "application/zip": [".zip"],
                  "application/x-zip-compressed": [".zip"],
                }}
                maxSize={1000 * 1024 * 1024} // 1GB for archive
                label="Upload Code ZIP Archive"
                description="ZIP archive to unpack and index allowed code files"
                selectedFiles={codeArchiveFile ? [codeArchiveFile] : []}
                isUploading={isUploadingCode}
                onFilesSelect={(files) => {
                  setCodeArchiveFile(files[0])
                  setCodeStatuses({})
                }}
                onRemoveFile={() => {
                  setCodeArchiveFile(null)
                  setCodeStatuses({})
                }}
              />
              {codeArchiveFile && (
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm font-medium">{codeArchiveFile.name}</p>
                      <p className="text-xs text-muted-foreground">
                        {(codeArchiveFile.size / 1024 / 1024).toFixed(2)} MB
                      </p>
                    </div>
                    {getStatusBadge(codeStatuses[codeArchiveFile.name] || null)}
                  </div>
                  <div className="flex items-center justify-between gap-4">
                    <label className="flex items-center gap-2 text-sm">
                      <input
                        type="checkbox"
                        checked={rebuildBehaviorIndex}
                        onChange={(e) => setRebuildBehaviorIndex(e.target.checked)}
                        disabled={isUploadingCode}
                      />
                      Rebuild behavior index after upload
                    </label>
                    <Button
                      onClick={handleCodeUpload}
                      disabled={isUploadingCode}
                    >
                      {isUploadingCode ? (
                        <>
                          <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                          Uploading...
                        </>
                      ) : (
                        "Upload & Index"
                      )}
                    </Button>
                  </div>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>
    </div>
  )
}

