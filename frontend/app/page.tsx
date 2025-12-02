"use client"

import { LayoutWithSidebar } from "./layout-with-sidebar"
import { DocumentList } from "@/components/documents/document-list"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Plus, FileText, MessageSquare, Code, Upload } from "lucide-react"
import Link from "next/link"
import { useState, useEffect } from "react"
import { documentAPI } from "@/lib/api/client"
import type { Document } from "@/lib/api/types"

export default function Home() {
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

  const quickActions = [
    {
      title: "Upload Document",
      description: "Add a new GDD or codebase",
      icon: Upload,
      href: "/upload",
      color: "text-blue-500",
    },
    {
      title: "Start Chat",
      description: "Ask questions about your documents",
      icon: MessageSquare,
      href: "/chat",
      color: "text-green-500",
    },
    {
      title: "View Documents",
      description: "Manage your indexed documents",
      icon: FileText,
      href: "/documents",
      color: "text-purple-500",
    },
  ]

  return (
    <LayoutWithSidebar>
      <div className="space-y-8">
        <div>
          <h1 className="text-4xl font-bold tracking-tight">Workspace</h1>
          <p className="text-muted-foreground mt-2">
            Welcome to your GDD RAG Assistant workspace
          </p>
        </div>

        {/* Quick Actions */}
        <div className="grid gap-4 md:grid-cols-3">
          {quickActions.map((action) => {
            const Icon = action.icon
            return (
              <Link key={action.href} href={action.href}>
                <Card className="cursor-pointer hover:shadow-md transition-shadow">
                  <CardHeader>
                    <div className="flex items-center gap-3">
                      <Icon className={`h-6 w-6 ${action.color}`} />
                      <CardTitle className="text-lg">{action.title}</CardTitle>
                    </div>
                    <CardDescription>{action.description}</CardDescription>
                  </CardHeader>
                </Card>
              </Link>
            )
          })}
        </div>

        {/* Recent Documents */}
        <div>
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-2xl font-semibold">Recent Documents</h2>
            <Link href="/documents">
              <Button variant="outline">View All</Button>
            </Link>
          </div>
          <DocumentList
            documents={documents.slice(0, 6)}
            isLoading={isLoading}
          />
        </div>
      </div>
    </LayoutWithSidebar>
  )
}

