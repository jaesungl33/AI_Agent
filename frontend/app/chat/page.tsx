"use client"

import { LayoutWithSidebar } from "../layout-with-sidebar"
import { EnhancedChat } from "@/components/chat/enhanced-chat"
import { useState } from "react"

export default function ChatPage() {
  const [workspaceId] = useState("default")
  const [docIds] = useState<string[]>([])

  return (
    <LayoutWithSidebar>
      <div className="h-[calc(100vh-8rem)]">
        <div className="mb-6">
          <h1 className="text-3xl font-bold">Chat</h1>
          <p className="text-muted-foreground mt-2">
            Ask questions about your game design documents and codebase
          </p>
        </div>
        <EnhancedChat
          workspaceId={workspaceId}
          docIds={docIds}
          useAllDocs={true}
        />
      </div>
    </LayoutWithSidebar>
  )
}

