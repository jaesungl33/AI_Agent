"use client"

import React, { useState } from "react"
import { useWorkspace } from "@/lib/contexts/workspace-context"
import { Button } from "@/components/ui/button"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Input } from "@/components/ui/input"
import { Badge } from "@/components/ui/badge"
import { Plus, Loader2 } from "lucide-react"

export function WorkspaceSelector() {
  const {
    currentWorkspace,
    workspaces,
    isLoading,
    setCurrentWorkspace,
    createWorkspace,
  } = useWorkspace()

  const [isCreating, setIsCreating] = useState(false)
  const [newWorkspaceName, setNewWorkspaceName] = useState("")
  const [showCreateForm, setShowCreateForm] = useState(false)

  const handleCreate = async () => {
    if (!newWorkspaceName.trim()) return
    try {
      setIsCreating(true)
      const workspace = await createWorkspace(newWorkspaceName.trim())
      setNewWorkspaceName("")
      setShowCreateForm(false)
    } catch (error: any) {
      console.error("Failed to create workspace:", error)
      alert(error.message || "Failed to create workspace")
    } finally {
      setIsCreating(false)
    }
  }

  if (isLoading) {
    return (
      <div className="flex items-center gap-2 text-sm text-muted-foreground">
        <Loader2 className="h-4 w-4 animate-spin" />
        Loading workspaces...
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-2 max-w-[260px] w-full">
      <span className="text-sm font-medium">Workspace</span>
      <Select
        value={currentWorkspace?.id || ""}
        onValueChange={(val) => {
          const ws = workspaces.find((w) => w.id === val)
          if (ws) setCurrentWorkspace(ws)
        }}
        disabled={workspaces.length === 0}
      >
        <SelectTrigger className="h-9 text-sm w-full">
          <SelectValue placeholder="Select workspace" />
        </SelectTrigger>
        <SelectContent>
          {workspaces.length === 0 ? (
            <SelectItem value="__none" disabled>
              No workspaces
            </SelectItem>
          ) : (
            workspaces.map((ws) => (
              <SelectItem key={ws.id} value={ws.id}>
                {ws.name || ws.id}
              </SelectItem>
            ))
          )}
        </SelectContent>
      </Select>

      {showCreateForm ? (
        <div className="flex items-center gap-2">
          <Input
            type="text"
            placeholder="New workspace"
            value={newWorkspaceName}
            onChange={(e) => setNewWorkspaceName(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter") handleCreate()
              if (e.key === "Escape") {
                setShowCreateForm(false)
                setNewWorkspaceName("")
              }
            }}
            className="h-9 w-full text-sm"
            autoFocus
          />
          <Button
            size="sm"
            onClick={handleCreate}
            disabled={isCreating || !newWorkspaceName.trim()}
          >
            {isCreating ? <Loader2 className="h-3 w-3 animate-spin" /> : "Create"}
          </Button>
          <Button
            size="sm"
            variant="outline"
            onClick={() => {
              setShowCreateForm(false)
              setNewWorkspaceName("")
            }}
          >
            Cancel
          </Button>
        </div>
      ) : (
        <div className="flex items-center gap-2">
          <Button
            size="sm"
            variant="secondary"
            className="h-9"
            onClick={() => setShowCreateForm(true)}
          >
            <Plus className="h-3 w-3 mr-1" />
            New
          </Button>
          {currentWorkspace && (
            <Badge variant="secondary" className="text-xs">
              {currentWorkspace.stats?.documents || 0} docs
            </Badge>
          )}
        </div>
      )}
    </div>
  )
}

