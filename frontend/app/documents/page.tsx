"use client"

import { LayoutWithSidebar } from "../layout-with-sidebar"
import { DocumentList } from "@/components/documents/document-list"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Plus, FileText, Code, Search } from "lucide-react"
import { useState, useEffect } from "react"
import { documentAPI } from "@/lib/api/client"
import { useWorkspace } from "@/lib/contexts/workspace-context"
import type { Document } from "@/lib/api/types"
import Link from "next/link"

export default function DocumentsPage() {
  const { currentWorkspace } = useWorkspace()
  const [typeFilter, setTypeFilter] = useState<"all" | "gdd" | "code">("all")
  const [documents, setDocuments] = useState<Document[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState("")

  useEffect(() => {
    if (currentWorkspace) {
      loadDocuments()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentWorkspace])

  const loadDocuments = async () => {
    try {
      setIsLoading(true)
      const docs = await documentAPI.list(currentWorkspace?.id)
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

  // Filter documents
  const filteredDocs = documents.filter(doc => {
    const matchesType = typeFilter === "all" || doc.type === typeFilter
    const matchesSearch = !searchQuery || 
      doc.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      doc.id.toLowerCase().includes(searchQuery.toLowerCase())
    return matchesType && matchesSearch
  })

  const gddDocs = filteredDocs.filter(d => d.type === "gdd")
  const codeDocs = filteredDocs.filter(d => d.type === "code")
  const allGddDocs = documents.filter(d => d.type === "gdd")
  const allCodeDocs = documents.filter(d => d.type === "code")

  return (
    <LayoutWithSidebar>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold">Documents</h1>
            <p className="text-muted-foreground mt-2">
              {currentWorkspace?.name || "Workspace"} • {documents.length} total documents
            </p>
          </div>
          <Link href="/upload">
            <Button>
              <Plus className="mr-2 h-4 w-4" />
              Upload
            </Button>
          </Link>
        </div>

        {/* Search and Filters */}
        <div className="flex items-center gap-4">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <input
              type="text"
              placeholder="Search documents..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border rounded-md bg-background"
            />
          </div>
        </div>

        {/* Tabs */}
        <Tabs value={typeFilter} onValueChange={(v) => setTypeFilter(v as "all" | "gdd" | "code")} className="w-full">
          <TabsList>
            <TabsTrigger value="all">
              All Documents
              <Badge variant="secondary" className="ml-2">{documents.length}</Badge>
            </TabsTrigger>
            <TabsTrigger value="gdd">
              <FileText className="h-4 w-4 mr-2" />
              GDDs
              <Badge variant="secondary" className="ml-2">{allGddDocs.length}</Badge>
            </TabsTrigger>
            <TabsTrigger value="code">
              <Code className="h-4 w-4 mr-2" />
              Code
              <Badge variant="secondary" className="ml-2">{allCodeDocs.length}</Badge>
            </TabsTrigger>
          </TabsList>

          <TabsContent value="all" className="mt-4">
            <DocumentList
              documents={filteredDocs}
              onDelete={handleDelete}
              isLoading={isLoading}
            />
          </TabsContent>
          <TabsContent value="gdd" className="mt-4">
            <DocumentList
              documents={gddDocs}
              onDelete={handleDelete}
              isLoading={isLoading}
            />
          </TabsContent>
          <TabsContent value="code" className="mt-4">
            <DocumentList
              documents={codeDocs}
              onDelete={handleDelete}
              isLoading={isLoading}
            />
          </TabsContent>
        </Tabs>
      </div>
    </LayoutWithSidebar>
  )
}
