"use client"

import { LayoutWithSidebar } from "../layout-with-sidebar"
import { WorkspaceSetup } from "@/components/workspace-setup"
import { useRouter } from "next/navigation"
import { useWorkspace } from "@/lib/contexts/workspace-context"

export default function UploadPage() {
  const { currentWorkspace } = useWorkspace()
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
        {currentWorkspace ? (
          <WorkspaceSetup
            workspaceId={currentWorkspace.id}
            onGDDUploaded={handleGDDUploaded}
            onCodeUploaded={handleCodeUploaded}
          />
        ) : (
          <div className="text-center py-12 text-muted-foreground">
            <p>Please select or create a workspace first</p>
          </div>
        )}
      </div>
    </LayoutWithSidebar>
  )
}

