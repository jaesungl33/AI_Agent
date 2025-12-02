"use client"

import { LayoutWithSidebar } from "../layout-with-sidebar"
import { WorkspaceSetup } from "@/components/workspace-setup"
import { useState } from "react"
import { useRouter } from "next/navigation"

export default function UploadPage() {
  const [workspaceId] = useState("default")
  const router = useRouter()

  const handleGDDUploaded = (docId: string) => {
    // Redirect to documents page after upload
    router.push("/documents")
  }

  const handleCodeUploaded = (indexId: string) => {
    router.push("/documents")
  }

  return (
    <LayoutWithSidebar>
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold">Upload Documents</h1>
          <p className="text-muted-foreground mt-2">
            Upload your Game Design Documents and codebase for analysis
          </p>
        </div>
        <WorkspaceSetup
          workspaceId={workspaceId}
          onGDDUploaded={handleGDDUploaded}
          onCodeUploaded={handleCodeUploaded}
        />
      </div>
    </LayoutWithSidebar>
  )
}

