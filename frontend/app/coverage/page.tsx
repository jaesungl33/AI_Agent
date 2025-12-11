"use client"

import { LayoutWithSidebar } from "../layout-with-sidebar"
import { CodeCoverage } from "@/components/code-coverage"
import { useState, useEffect } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import { 
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { documentAPI, gddAPI } from "@/lib/api/client"
import { useWorkspace } from "@/lib/contexts/workspace-context"
import type { Document, GameSpec } from "@/lib/api/types"
import { GddSpecDetails } from "@/components/documents/gdd-spec-details"
import { Loader2, FileText, Code } from "lucide-react"

export default function CoveragePage() {
  const { currentWorkspace } = useWorkspace()
  const workspaceId = currentWorkspace?.id ?? "tank_war"
  const [documents, setDocuments] = useState<Document[]>([])
  const [fastMode, setFastMode] = useState(true)
  const [selectedGddIds, setSelectedGddIds] = useState<string[]>([])  // legacy multi (auto all)
  const [selectedCodeIds, setSelectedCodeIds] = useState<string[]>([]) // legacy multi (auto all)
  const [selectedSingleGdd, setSelectedSingleGdd] = useState<string | null>(null)
  const [selectedSingleCode, setSelectedSingleCode] = useState<string | null>(null)
  const [spec, setSpec] = useState<GameSpec | null>(null)
  const [isLoadingSpec, setIsLoadingSpec] = useState(false)
  const [isLoadingDocs, setIsLoadingDocs] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [exportLoading, setExportLoading] = useState<"gdd" | "code" | "compare" | null>(null)

  useEffect(() => {
    loadDocuments()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const loadDocuments = async () => {
    try {
      setIsLoadingDocs(true)
      setLoadError(null)
      console.log("[CoveragePage] Loading documents from backend...")
      const docs = await documentAPI.list(workspaceId)
      console.log("[CoveragePage] Loaded documents:", docs.length)
      setDocuments(docs)
      
      // Auto-select ALL GDDs and ALL code batches by default
      const gddDocs = docs.filter(d => d.type === "gdd" && d.status === "indexed")
      const codeDocs = docs.filter(d => d.type === "code" && d.status === "indexed")
      
      console.log("[CoveragePage] Found GDDs:", gddDocs.length, "Code batches:", codeDocs.length)
      
      // Select all GDDs and all code batches for whole-codebase comparison
      if (gddDocs.length > 0 && selectedGddIds.length === 0) {
        setSelectedGddIds(gddDocs.map(d => d.id))
      }
      if (codeDocs.length > 0 && selectedCodeIds.length === 0) {
        setSelectedCodeIds(codeDocs.map(d => d.id))
      }
      if (!selectedSingleGdd && gddDocs.length > 0) {
        setSelectedSingleGdd(gddDocs[0].id)
      }
      if (!selectedSingleCode && codeDocs.length > 0) {
        setSelectedSingleCode(codeDocs[0].id)
      }
    } catch (error: any) {
      console.error("[CoveragePage] Failed to load documents:", error)
      setLoadError(error?.message || "Failed to load documents. Check if backend is running at http://localhost:8000")
    } finally {
      setIsLoadingDocs(false)
    }
  }

  const handleExtractSpec = async () => {
    const evalGddIds = fastMode
      ? (selectedSingleGdd ? [selectedSingleGdd] : [])
      : selectedGddIds
    if (evalGddIds.length === 0) return
    
    try {
      setIsLoadingSpec(true)
      // Extract from first GDD for display (we'll merge all in backend)
      const specData = await gddAPI.getSpec(evalGddIds[0])
      setSpec(specData)
    } catch (error) {
      console.error("Failed to extract spec:", error)
      alert("Failed to extract requirements. Make sure the GDDs are indexed.")
    } finally {
      setIsLoadingSpec(false)
    }
  }

  const isIndexed = (status?: string) => status === "indexed" || status === "processed"
  const gddDocs = documents.filter(d => d.type === "gdd" && isIndexed(d.status))
  const codeDocs = documents.filter(d => d.type === "code" && isIndexed(d.status))

  const evalGddIds = fastMode
    ? (selectedSingleGdd ? [selectedSingleGdd] : [])
    : selectedGddIds
  const evalCodeIds = fastMode
    ? (selectedSingleCode ? [selectedSingleCode] : [])
    : selectedCodeIds

  return (
    <LayoutWithSidebar>
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold">Code Coverage</h1>
          <p className="text-muted-foreground mt-2">
            Behavior-based coverage: extract GDD behaviors, match to code behaviors, LLM-verify top candidates.
          </p>
        </div>

        {/* Document Selection */}
        <Card>
          <CardHeader>
            <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
              <div>
                <CardTitle>Document Selection</CardTitle>
                <CardDescription>
                  Fast mode runs a single GDD vs a single code batch to avoid timeouts.
                </CardDescription>
              </div>
              <div className="flex items-center gap-2">
                <Label className="text-sm">Fast mode</Label>
                <Button
                  size="sm"
                  variant={fastMode ? "default" : "outline"}
                  onClick={() => setFastMode(!fastMode)}
                >
                  {fastMode ? "On" : "Off"}
                </Button>
              </div>
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            {isLoadingDocs && (
              <div className="flex items-center gap-2 text-sm text-muted-foreground">
                <Loader2 className="h-4 w-4 animate-spin" />
                Loading documents from backend...
              </div>
            )}
            {loadError && (
              <div className="p-3 rounded-lg bg-destructive/10 border border-destructive/20">
                <p className="text-sm text-destructive font-medium">Error loading documents:</p>
                <p className="text-sm text-destructive/80 mt-1">{loadError}</p>
                <Button 
                  size="sm" 
                  variant="outline" 
                  onClick={loadDocuments}
                  className="mt-2"
                >
                  Retry
                </Button>
              </div>
            )}
            {fastMode ? (
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>
                    <FileText className="inline h-4 w-4 mr-2" />
                    Select one GDD
                  </Label>
                  <Select
                    onValueChange={(val) => setSelectedSingleGdd(val)}
                    value={selectedSingleGdd ?? undefined}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Choose a GDD" />
                    </SelectTrigger>
                    <SelectContent>
                      {gddDocs.map((doc) => (
                        <SelectItem key={doc.id} value={doc.id}>
                          {doc.name} ({doc.chunksCount || 0} chunks)
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label>
                    <Code className="inline h-4 w-4 mr-2" />
                    Select one Code batch
                  </Label>
                  <Select
                    onValueChange={(val) => setSelectedSingleCode(val)}
                    value={selectedSingleCode ?? undefined}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Choose code batch" />
                    </SelectTrigger>
                    <SelectContent>
                      {codeDocs.map((doc) => (
                        <SelectItem key={doc.id} value={doc.id}>
                          {doc.name} ({doc.chunksCount || 0} chunks)
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>
            ) : (
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>
                    <FileText className="inline h-4 w-4 mr-2" />
                    Game Design Documents ({selectedGddIds.length} selected)
                  </Label>
                  <div className="p-3 rounded-lg bg-muted border border-border">
                    {selectedGddIds.length > 0 ? (
                      <div className="space-y-1">
                        {gddDocs
                          .filter(d => selectedGddIds.includes(d.id))
                          .map((doc) => (
                            <div key={doc.id} className="text-sm">
                              ✓ {doc.name} ({doc.chunksCount || 0} chunks)
                            </div>
                          ))}
                      </div>
                    ) : (
                      <p className="text-sm text-muted-foreground">No GDDs available</p>
                    )}
                  </div>
                </div>

                <div className="space-y-2">
                  <Label>
                    <Code className="inline h-4 w-4 mr-2" />
                    Code Batches ({selectedCodeIds.length} selected)
                  </Label>
                  <div className="p-3 rounded-lg bg-muted border border-border">
                    {selectedCodeIds.length > 0 ? (
                      <div className="space-y-1 max-h-32 overflow-y-auto">
                        {codeDocs
                          .filter(d => selectedCodeIds.includes(d.id))
                          .map((doc) => (
                            <div key={doc.id} className="text-sm">
                              ✓ {doc.name} ({doc.chunksCount || 0} chunks)
                            </div>
                          ))}
                      </div>
                    ) : (
                      <p className="text-sm text-muted-foreground">No code batches available</p>
                    )}
                  </div>
                </div>
              </div>
            )}

            <div className="p-3 rounded-lg bg-blue-50 dark:bg-blue-950 border border-blue-200 dark:border-blue-800">
              <p className="text-sm text-blue-900 dark:text-blue-100">
                <strong>Whole-Codebase Comparison:</strong> Requirements from all {selectedGddIds.length} GDD(s) 
                will be compared against all {selectedCodeIds.length} code batch(es) for complete coverage.
              </p>
            </div>

            <Button
              onClick={handleExtractSpec}
              disabled={selectedGddIds.length === 0 || isLoadingSpec}
            >
              {isLoadingSpec ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Extracting Requirements from All GDDs...
                </>
              ) : (
                <>
                  <FileText className="mr-2 h-4 w-4" />
                  Extract Requirements from All GDDs
                </>
              )}
            </Button>
          </CardContent>
        </Card>

        {/* Exports */}
        <Card>
          <CardHeader>
            <CardTitle>Exports</CardTitle>
            <CardDescription>
              Generate markdown exports for all GDD requirements and code functions, then compare.
            </CardDescription>
          </CardHeader>
          <CardContent className="flex flex-wrap gap-3">
            <Button
              variant="outline"
              disabled={!!exportLoading}
              onClick={async () => {
                try {
                  setExportLoading("gdd")
                  const res = await fetch(`/api/export/gdd?workspaceId=${workspaceId}`)
                  if (!res.ok) throw new Error(await res.text())
                  const data = await res.json()
                  alert(`GDD export done: ${data.file}`)
                } catch (e: any) {
                  alert(`GDD export failed: ${e?.message || "Error"}`)
                } finally {
                  setExportLoading(null)
                }
              }}
            >
              {exportLoading === "gdd" ? <Loader2 className="h-4 w-4 animate-spin mr-2" /> : null}
              Export GDD Requirements
            </Button>
            <Button
              variant="outline"
              disabled={!!exportLoading}
              onClick={async () => {
                try {
                  setExportLoading("code")
                  const res = await fetch(`/api/export/code?workspaceId=${workspaceId}`)
                  if (!res.ok) throw new Error(await res.text())
                  const data = await res.json()
                  alert(`Code export done: ${data.file}`)
                } catch (e: any) {
                  alert(`Code export failed: ${e?.message || "Error"}`)
                } finally {
                  setExportLoading(null)
                }
              }}
            >
              {exportLoading === "code" ? <Loader2 className="h-4 w-4 animate-spin mr-2" /> : null}
              Export Code Functions
            </Button>
            <Button
              variant="default"
              disabled={!!exportLoading}
              onClick={async () => {
                try {
                  setExportLoading("compare")
                  const res = await fetch(`/api/export/compare?workspaceId=${workspaceId}`, { method: "POST" })
                  if (!res.ok) throw new Error(await res.text())
                  const data = await res.json()
                  alert(`Comparison ready: ${data.file}`)
                } catch (e: any) {
                  alert(`Compare failed: ${e?.message || "Error"}`)
                } finally {
                  setExportLoading(null)
                }
              }}
            >
              {exportLoading === "compare" ? <Loader2 className="h-4 w-4 animate-spin mr-2" /> : null}
              Compare Exports
            </Button>
          </CardContent>
        </Card>

        {/* Spec Summary & Details */}
        {spec && (
          <div className="space-y-4">
        <Card>
          <CardHeader>
            <CardTitle>Extracted Game Specification</CardTitle>
            <CardDescription>
              Requirements, systems, objects, and logic rules found in the GDD
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-4 gap-4">
              <div className="text-center p-4 rounded-lg bg-muted">
                <p className="text-2xl font-bold">{spec.requirements?.length ?? 0}</p>
                <p className="text-sm text-muted-foreground">Requirements</p>
              </div>
              <div className="text-center p-4 rounded-lg bg-muted">
                <p className="text-2xl font-bold">{spec.systems?.length ?? 0}</p>
                <p className="text-sm text-muted-foreground">Systems</p>
              </div>
              <div className="text-center p-4 rounded-lg bg-muted">
                <p className="text-2xl font-bold">{spec.objects?.length ?? 0}</p>
                <p className="text-sm text-muted-foreground">Objects</p>
              </div>
              <div className="text-center p-4 rounded-lg bg-muted">
                <p className="text-2xl font-bold">{spec.logicRules?.length ?? 0}</p>
                <p className="text-sm text-muted-foreground">Logic Rules</p>
              </div>
            </div>
          </CardContent>
        </Card>

            <GddSpecDetails spec={spec} />
          </div>
        )}

        {/* Coverage Evaluation */}
        {evalGddIds.length > 0 && evalCodeIds.length > 0 && (
          <CodeCoverage 
            docId={evalGddIds.length === 1 ? evalGddIds[0] : evalGddIds} 
            codeIndexId={evalCodeIds.length === 1 ? evalCodeIds[0] : evalCodeIds}
            workspaceId={workspaceId}
          />
        )}

        {(evalGddIds.length === 0 || evalCodeIds.length === 0) && (
          <Card>
            <CardContent className="py-8 text-center text-muted-foreground">
              Please ensure you have at least one GDD and one code batch indexed.
            </CardContent>
          </Card>
        )}
      </div>
    </LayoutWithSidebar>
  )
}

