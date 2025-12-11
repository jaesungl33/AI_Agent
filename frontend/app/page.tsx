"use client"

import { LayoutWithSidebar } from "./layout-with-sidebar"
import { DocumentList } from "@/components/documents/document-list"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Plus, FileText, MessageSquare, Code, Upload, FolderOpen, BarChart3 } from "lucide-react"
import Link from "next/link"
import { useState, useEffect } from "react"
import { documentAPI } from "@/lib/api/client"
import { useWorkspace } from "@/lib/contexts/workspace-context"
import type { Document } from "@/lib/api/types"

export default function Home() {
  const { currentWorkspace } = useWorkspace()
  const [documents, setDocuments] = useState<Document[]>([])
  const [isLoading, setIsLoading] = useState(true)

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

  const gddDocs = documents.filter(d => d.type === "gdd")
  const codeDocs = documents.filter(d => d.type === "code")
  const indexedDocs = documents.filter(d => d.status === "indexed")

  const quickActions = [
    {
      title: "Upload Documents",
      description: "Add GDDs or codebase files",
      icon: Upload,
      href: "/upload",
      color: "text-blue-500",
      bgColor: "bg-blue-50 dark:bg-blue-950",
    },
    {
      title: "View Documents",
      description: `${documents.length} total documents`,
      icon: FileText,
      href: "/documents",
      color: "text-purple-500",
      bgColor: "bg-purple-50 dark:bg-purple-950",
    },
    {
      title: "Code Coverage",
      description: "Evaluate GDD vs code",
      icon: Code,
      href: "/coverage",
      color: "text-orange-500",
      bgColor: "bg-orange-50 dark:bg-orange-950",
    },
    {
      title: "Chat Assistant",
      description: "Ask questions about docs",
      icon: MessageSquare,
      href: "/chat",
      color: "text-green-500",
      bgColor: "bg-green-50 dark:bg-green-950",
    },
  ]

  return (
    <LayoutWithSidebar>
      <div className="space-y-6">
        {/* Workspace Header */}
        <div className="flex items-start justify-between">
          <div>
            <div className="flex items-center gap-3 mb-2">
              <FolderOpen className="h-8 w-8 text-primary" />
              <h1 className="text-4xl font-bold tracking-tight">
                {currentWorkspace?.name || "Workspace"}
              </h1>
            </div>
            <p className="text-muted-foreground mt-1">
              {currentWorkspace?.description || "Manage your GDDs and codebase"}
            </p>
          </div>
          <Link href="/upload">
            <Button>
              <Plus className="mr-2 h-4 w-4" />
              Upload
            </Button>
          </Link>
        </div>

        {/* Stats Cards */}
        <div className="grid gap-4 md:grid-cols-4">
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-sm font-medium text-muted-foreground">Total Documents</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold">{documents.length}</div>
              <p className="text-xs text-muted-foreground mt-1">
                {indexedDocs.length} indexed
              </p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-sm font-medium text-muted-foreground">GDD Documents</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold">{gddDocs.length}</div>
              <p className="text-xs text-muted-foreground mt-1">
                {gddDocs.filter(d => d.status === "indexed").length} ready
              </p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-sm font-medium text-muted-foreground">Code Files</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold">{codeDocs.length}</div>
              <p className="text-xs text-muted-foreground mt-1">
                {codeDocs.filter(d => d.status === "indexed").length} indexed
              </p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-sm font-medium text-muted-foreground">Status</CardTitle>
            </CardHeader>
            <CardContent>
              <Badge variant={indexedDocs.length > 0 ? "default" : "secondary"} className="text-lg px-3 py-1">
                {indexedDocs.length > 0 ? "Ready" : "Empty"}
              </Badge>
            </CardContent>
          </Card>
        </div>

        {/* Quick Actions */}
        <div>
          <h2 className="text-2xl font-semibold mb-4">Quick Actions</h2>
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
            {quickActions.map((action) => {
              const Icon = action.icon
              return (
                <Link key={action.href} href={action.href}>
                  <Card className={`cursor-pointer hover:shadow-lg transition-all ${action.bgColor} border-2 hover:border-primary/50`}>
                    <CardHeader>
                      <div className="flex items-center gap-3 mb-2">
                        <div className={`p-2 rounded-lg ${action.bgColor}`}>
                          <Icon className={`h-5 w-5 ${action.color}`} />
                        </div>
                        <CardTitle className="text-base">{action.title}</CardTitle>
                      </div>
                      <CardDescription className="text-sm">{action.description}</CardDescription>
                    </CardHeader>
                  </Card>
                </Link>
              )
            })}
          </div>
        </div>

        {/* Documents Overview */}
        <div className="grid gap-6 md:grid-cols-2">
          {/* GDD Documents */}
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle className="flex items-center gap-2">
                  <FileText className="h-5 w-5 text-purple-500" />
                  GDD Documents
                </CardTitle>
                <Badge variant="secondary">{gddDocs.length}</Badge>
              </div>
              <CardDescription>
                Game Design Documents and specifications
              </CardDescription>
            </CardHeader>
            <CardContent>
              {isLoading ? (
                <div className="text-center py-8 text-muted-foreground">Loading...</div>
              ) : gddDocs.length === 0 ? (
                <div className="text-center py-8 text-muted-foreground">
                  <FileText className="h-12 w-12 mx-auto mb-2 opacity-50" />
                  <p>No GDD documents yet</p>
                  <Link href="/upload">
                    <Button variant="outline" size="sm" className="mt-4">
                      Upload GDD
                    </Button>
                  </Link>
                </div>
              ) : (
                <DocumentList
                  documents={gddDocs.slice(0, 5)}
                  isLoading={false}
                />
              )}
              {gddDocs.length > 5 && (
                <Link href="/documents?type=gdd">
                  <Button variant="ghost" className="w-full mt-4">
                    View All {gddDocs.length} GDDs
                  </Button>
                </Link>
              )}
            </CardContent>
          </Card>

          {/* Code Documents */}
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle className="flex items-center gap-2">
                  <Code className="h-5 w-5 text-orange-500" />
                  Code Files
                </CardTitle>
                <Badge variant="secondary">{codeDocs.length}</Badge>
              </div>
              <CardDescription>
                Indexed codebase files and batches
              </CardDescription>
            </CardHeader>
            <CardContent>
              {isLoading ? (
                <div className="text-center py-8 text-muted-foreground">Loading...</div>
              ) : codeDocs.length === 0 ? (
                <div className="text-center py-8 text-muted-foreground">
                  <Code className="h-12 w-12 mx-auto mb-2 opacity-50" />
                  <p>No code files yet</p>
                  <Link href="/upload">
                    <Button variant="outline" size="sm" className="mt-4">
                      Upload Code
                    </Button>
                  </Link>
                </div>
              ) : (
                <DocumentList
                  documents={codeDocs.slice(0, 5)}
                  isLoading={false}
                />
              )}
              {codeDocs.length > 5 && (
                <Link href="/documents?type=code">
                  <Button variant="ghost" className="w-full mt-4">
                    View All {codeDocs.length} Code Files
                  </Button>
                </Link>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </LayoutWithSidebar>
  )
}

