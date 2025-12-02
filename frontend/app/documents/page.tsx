"use client"

import { LayoutWithSidebar } from "../layout-with-sidebar"
import { DocumentList } from "@/components/documents/document-list"
import { Button } from "@/components/ui/button"
import { Plus } from "lucide-react"
import { useState, useEffect } from "react"
import { documentAPI } from "@/lib/api/client"
import type { Document } from "@/lib/api/types"
import Link from "next/link"

export default function DocumentsPage() {
  const [documents, setDocuments] = useState<Document[]>([])
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    loadDocuments()
  }, [])

  const loadDocuments = async () => {
    try {
      setIsLoading(true)
      const docs = await documentAPI.list()
      setDocuments(docs)
    } catch (error) {
      console.error("Failed to load documents:", error)
    } finally {
      setIsLoading(false)
    }
  }

  const handleDelete = async (docId: string) => {
    // TODO: Implement delete API call
    console.log("Delete document:", docId)
    await loadDocuments()
  }

  return (
    <LayoutWithSidebar>
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold">Documents</h1>
            <p className="text-muted-foreground mt-2">
              Manage your GDDs and code indexes
            </p>
          </div>
          <Link href="/upload">
            <Button>
              <Plus className="mr-2 h-4 w-4" />
              Upload Document
            </Button>
          </Link>
        </div>

        <DocumentList
          documents={documents}
          onDelete={handleDelete}
          isLoading={isLoading}
        />
      </div>
    </LayoutWithSidebar>
  )
}

