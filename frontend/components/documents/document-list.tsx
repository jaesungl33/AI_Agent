"use client"

import { useState } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { FileText, Code, Trash2, Eye, Loader2 } from "lucide-react"
import type { Document } from "@/lib/api/types"

interface DocumentListProps {
  documents: Document[]
  onSelect?: (doc: Document) => void
  onDelete?: (docId: string) => void
  isLoading?: boolean
}

export function DocumentList({
  documents,
  onSelect,
  onDelete,
  isLoading = false,
}: DocumentListProps) {
  const [deletingId, setDeletingId] = useState<string | null>(null)

  const handleDelete = async (docId: string, e: React.MouseEvent) => {
    e.stopPropagation()
    if (!onDelete) return
    
    setDeletingId(docId)
    try {
      await onDelete(docId)
    } finally {
      setDeletingId(null)
    }
  }

  const getStatusBadge = (status: Document["status"]) => {
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

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    )
  }

  if (documents.length === 0) {
    return (
      <Card>
        <CardContent className="p-12">
          <div className="text-center">
            <FileText className="mx-auto h-12 w-12 text-muted-foreground" />
            <h3 className="mt-4 text-lg font-semibold">No documents</h3>
            <p className="mt-2 text-sm text-muted-foreground">
              Upload your first GDD or codebase to get started
            </p>
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
      {documents.map((doc) => (
        <Card
          key={doc.id}
          className="cursor-pointer hover:shadow-md transition-shadow"
          onClick={() => onSelect?.(doc)}
        >
          <CardHeader>
            <div className="flex items-start justify-between">
              <div className="flex items-center gap-2">
                {doc.type === "gdd" ? (
                  <FileText className="h-5 w-5 text-primary" />
                ) : (
                  <Code className="h-5 w-5 text-primary" />
                )}
                <CardTitle className="text-base">{doc.name}</CardTitle>
              </div>
              {onDelete && (
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-8 w-8"
                  onClick={(e) => handleDelete(doc.id, e)}
                  disabled={deletingId === doc.id}
                >
                  {deletingId === doc.id ? (
                    <Loader2 className="h-4 w-4 animate-spin" />
                  ) : (
                    <Trash2 className="h-4 w-4" />
                  )}
                </Button>
              )}
            </div>
            <CardDescription>
              {doc.type === "gdd" ? "Game Design Document" : "Code Index"}
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              {getStatusBadge(doc.status)}
              {doc.chunksCount && (
                <p className="text-xs text-muted-foreground">
                  {doc.chunksCount.toLocaleString()} chunks
                </p>
              )}
              {doc.indexedAt && (
                <p className="text-xs text-muted-foreground">
                  Indexed {new Date(doc.indexedAt).toLocaleDateString()}
                </p>
              )}
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  )
}

