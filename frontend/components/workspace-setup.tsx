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
import type { Document } from "@/lib/api/types"

interface WorkspaceSetupProps {
  workspaceId?: string
  onGDDUploaded?: (docId: string) => void
  onCodeUploaded?: (indexId: string) => void
}

export function WorkspaceSetup({
  workspaceId,
  onGDDUploaded,
  onCodeUploaded,
}: WorkspaceSetupProps) {
  const [gddFile, setGddFile] = useState<File | null>(null)
  const [codeFile, setCodeFile] = useState<File | null>(null)
  const [gddDocId, setGddDocId] = useState<string>("")
  const [codeIndexId, setCodeIndexId] = useState<string>("")
  const [isUploadingGDD, setIsUploadingGDD] = useState(false)
  const [isUploadingCode, setIsUploadingCode] = useState(false)
  const [gddStatus, setGddStatus] = useState<Document["status"] | null>(null)
  const [codeStatus, setCodeStatus] = useState<Document["status"] | null>(null)

  const handleGDDUpload = async () => {
    if (!gddFile) return

    setIsUploadingGDD(true)
    try {
      const response = await documentAPI.uploadGDD({
        file: gddFile,
        docId: gddDocId || undefined,
      })
      setGddStatus(response.status)
      onGDDUploaded?.(response.docId)
    } catch (error) {
      console.error("Failed to upload GDD:", error)
      setGddStatus("error")
    } finally {
      setIsUploadingGDD(false)
    }
  }

  const handleCodeUpload = async () => {
    if (!codeFile) return

    setIsUploadingCode(true)
    try {
      const response = await documentAPI.uploadCode({
        file: codeFile,
        indexId: codeIndexId || undefined,
      })
      setCodeStatus(response.status)
      onCodeUploaded?.(response.indexId)
    } catch (error) {
      console.error("Failed to upload code:", error)
      setCodeStatus("error")
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
            Upload your GDD (PDF, DOCX, or text file)
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="gdd-doc-id">Document ID (optional)</Label>
            <Input
              id="gdd-doc-id"
              placeholder="e.g., my_game_design"
              value={gddDocId}
              onChange={(e) => setGddDocId(e.target.value)}
              disabled={isUploadingGDD || !!gddFile}
            />
          </div>
          <FileUpload
            accept={{
              "application/pdf": [".pdf"],
              "application/vnd.openxmlformats-officedocument.wordprocessingml.document": [".docx"],
              "text/plain": [".txt"],
            }}
            label="Upload GDD"
            description="PDF, DOCX, or TXT files up to 100MB"
            selectedFile={gddFile}
            isUploading={isUploadingGDD}
            onFileSelect={setGddFile}
            onRemove={() => {
              setGddFile(null)
              setGddStatus(null)
            }}
          />
          {gddFile && (
            <div className="flex items-center justify-between">
              {getStatusBadge(gddStatus)}
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
            Upload your game code as a ZIP file or project folder
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="code-index-id">Index ID (optional)</Label>
            <Input
              id="code-index-id"
              placeholder="e.g., game_codebase"
              value={codeIndexId}
              onChange={(e) => setCodeIndexId(e.target.value)}
              disabled={isUploadingCode || !!codeFile}
            />
          </div>
          <FileUpload
            accept={{
              "application/zip": [".zip"],
              "application/x-zip-compressed": [".zip"],
            }}
            maxSize={500 * 1024 * 1024} // 500MB for code
            label="Upload Code"
            description="ZIP file containing your game code (up to 500MB)"
            selectedFile={codeFile}
            isUploading={isUploadingCode}
            onFileSelect={setCodeFile}
            onRemove={() => {
              setCodeFile(null)
              setCodeStatus(null)
            }}
          />
          {codeFile && (
            <div className="flex items-center justify-between">
              {getStatusBadge(codeStatus)}
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
          )}
        </CardContent>
      </Card>
    </div>
  )
}

