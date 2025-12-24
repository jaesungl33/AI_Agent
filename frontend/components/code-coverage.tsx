"use client"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { CheckCircle2, XCircle, AlertCircle, Loader2, Search, Play } from "lucide-react"
import { coverageAPI } from "@/lib/api/client"
import type { CoverageReport, CoverageResult } from "@/lib/api/types"

interface CodeCoverageProps {
  docId: string | string[]  // Support single or multiple GDDs
  codeIndexId?: string | string[]  // Support single or multiple code batches
  workspaceId?: string  // Workspace ID for scoped operations
}

export function CodeCoverage({ docId, codeIndexId: initialCodeIndexId, workspaceId }: CodeCoverageProps) {
  // Ensure workspaceId is provided
  const finalWorkspaceId = workspaceId || "tank_war"

  // Normalize to arrays for API calls
  const docIds = Array.isArray(docId) ? docId : [docId]
  const codeIds = initialCodeIndexId
    ? (Array.isArray(initialCodeIndexId) ? initialCodeIndexId : [initialCodeIndexId])
    : []
  const [topK, setTopK] = useState(8)
  const [report, setReport] = useState<CoverageReport | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [selectedItem, setSelectedItem] = useState<CoverageResult | null>(null)
  const [statusFilter, setStatusFilter] = useState<"all" | "implemented" | "partially_implemented" | "not_implemented" | "error">("all")
  const [fullEvaluation, setFullEvaluation] = useState(false)  // Toggle for full evaluation (no limit)

  const [error, setError] = useState<string | null>(null)
  const [statusMessage, setStatusMessage] = useState<string>("")
  const [startTime, setStartTime] = useState<number | null>(null)

  const handleEvaluate = async () => {
    if (codeIds.length === 0) {
      setError("Please select at least one code batch to evaluate")
      return
    }

    console.log("[Coverage] Starting evaluation", {
      docIds,
      codeIds,
      topK,
    })
    setIsLoading(true)
    setError(null)
    setReport(null)
    const start = Date.now()
    setStartTime(start)
    setStatusMessage("Initializing evaluation...")
    
    // Update status messages periodically while loading
    let statusInterval: NodeJS.Timeout | null = null
    const updateStatus = () => {
      const elapsed = Math.floor((Date.now() - start) / 1000)
      setStatusMessage(`Evaluating requirements... (${elapsed}s elapsed)`)
    }
    statusInterval = setInterval(updateStatus, 2000)
    
    try {
      // Pass arrays to API (or single strings if only one)
      // The new API requires single document IDs
      const gddDocId = docIds.length >= 1 ? docIds[0] : ""
      const codeBatchId = codeIds.length >= 1 ? codeIds[0] : ""

      if (!gddDocId || !codeBatchId) {
        throw new Error("Please select at least one GDD and one code batch")
      }

      setStatusMessage("Running coverage evaluation...")
      const mode = fullEvaluation ? "full" : "fast"
      const maxRequirements = fullEvaluation ? undefined : 5

      const report = await coverageAPI.evaluate(
        finalWorkspaceId,
        gddDocId,
        codeBatchId,
        mode,
        topK,
        maxRequirements
      )
      
      if (statusInterval) clearInterval(statusInterval)
      
      const elapsed = Math.floor((Date.now() - start) / 1000)
      console.log(`[Coverage] Evaluation finished in ${elapsed}s, report:`, report)
      setReport(report)
      setError(null)
      setStatusMessage(`✅ Evaluation complete! (took ${elapsed}s)`)
      
      // Clear status message after 3 seconds
      setTimeout(() => setStatusMessage(""), 3000)
    } catch (error: any) {
      if (statusInterval) clearInterval(statusInterval)
      console.error("[Coverage] Evaluation error:", error)
      const errorMessage = error?.message || "Failed to evaluate coverage. Please check the console for details."
      setError(errorMessage)
      setStatusMessage("❌ Evaluation failed")
      // Show error to user
      alert(`Coverage Evaluation Error:\n\n${errorMessage}`)
    } finally {
      setIsLoading(false)
      if (statusInterval) clearInterval(statusInterval)
    }
  }

  const getStatusIcon = (status: CoverageResult["status"]) => {
    switch (status) {
      case "implemented":
        return <CheckCircle2 className="h-4 w-4 text-green-500" />
      case "partially_implemented":
        return <AlertCircle className="h-4 w-4 text-yellow-500" />
      case "not_implemented":
        return <XCircle className="h-4 w-4 text-red-500" />
      case "error":
        return <AlertCircle className="h-4 w-4 text-yellow-500" />
    }
  }

  const getStatusBadge = (status: CoverageResult["status"]) => {
    switch (status) {
      case "implemented":
        return <Badge className="bg-green-500">Implemented</Badge>
      case "partially_implemented":
        return <Badge className="bg-yellow-500">Partially Implemented</Badge>
      case "not_implemented":
        return <Badge variant="destructive">Not Implemented</Badge>
      case "error":
        return <Badge variant="outline" className="border-yellow-500 text-yellow-500">Error</Badge>
    }
  }

  const filteredResults: CoverageResult[] =
    report?.results.filter((r) => {
      if (statusFilter === "all") return true
      return r.status === statusFilter
    }) ?? []

  return (
    <div className="space-y-6">
      {/* Evaluation Controls */}
      <Card>
        <CardHeader>
          <CardTitle>Behavior-Based Coverage Evaluation</CardTitle>
          <CardDescription>
            Fast symbol check → behavior-to-behavior match → LLM verification on top candidates.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="rounded-lg border bg-muted/40 p-3 text-sm text-muted-foreground">
            <div className="font-medium text-foreground mb-2">📋 Evaluation Process</div>
            <ol className="list-decimal list-inside space-y-1">
              <li><strong>Extract Requirements:</strong> Converts GDD text to structured behavior requirements (triggers, effects, entities)</li>
              <li><strong>Index Code Behaviors:</strong> Extracts methods from code files and converts them to behavior descriptions (cached after first run)</li>
              <li><strong>Match Behaviors:</strong> Uses embeddings to find similar behaviors (fast semantic search)</li>
              <li><strong>LLM Verification:</strong> Only verifies top 3-5 matches per requirement (efficient, accurate)</li>
            </ol>
            <div className="mt-3 p-2 rounded bg-blue-50 dark:bg-blue-950 border border-blue-200 dark:border-blue-800">
              <p className="text-xs text-blue-900 dark:text-blue-100">
                <strong>💡 Tip:</strong> First run may take longer as it builds the behavior index. Subsequent runs use the cached index and are much faster!
              </p>
            </div>
          </div>
          <div className="space-y-2">
            <Label>Code Batches</Label>
            <div className="p-3 rounded-lg bg-muted border border-border">
              {codeIds.length > 0 ? (
                <div className="text-sm space-y-1">
                  {codeIds.map((id) => (
                    <div key={id}>✓ {id}</div>
                  ))}
                  <p className="text-xs text-muted-foreground mt-2">
                    Searching across {codeIds.length} code batch(es)
                  </p>
                </div>
              ) : (
                <p className="text-sm text-muted-foreground">No code batches selected</p>
              )}
            </div>
          </div>
          <div className="space-y-2">
            <Label htmlFor="top-k">Top behavior matches (LLM will check these)</Label>
            <Input
              id="top-k"
              type="number"
              min={4}
              max={12}
              value={topK}
              onChange={(e) => setTopK(parseInt(e.target.value) || 8)}
              disabled={isLoading}
            />
            <p className="text-xs text-muted-foreground">
              Higher = broader recall, lower = faster LLM pass. Recommended: 5–8.
            </p>
          </div>
          
          <div className="p-3 rounded-lg border bg-muted/40 space-y-2">
            <div className="flex items-center justify-between">
              <div>
                <div className="font-medium text-sm">Full Evaluation Mode</div>
                <div className="text-xs text-muted-foreground mt-0.5">
                  {fullEvaluation 
                    ? "⚡ Processing ALL requirements with parallel processing (faster, but may take longer)"
                    : "⚡ Fast mode: Limit to 5 requirements for quick testing"}
                </div>
              </div>
              <Button
                type="button"
                variant={fullEvaluation ? "default" : "outline"}
                size="sm"
                onClick={() => setFullEvaluation(!fullEvaluation)}
                disabled={isLoading}
              >
                {fullEvaluation ? "Full" : "Fast"}
              </Button>
            </div>
          </div>
          <Button 
            onClick={handleEvaluate} 
            disabled={isLoading || codeIds.length === 0}
            size="lg"
            className="w-full sm:w-auto"
          >
            {isLoading ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Evaluating... ({statusMessage || "Please wait"})
              </>
            ) : (
              <>
                <Play className="mr-2 h-4 w-4" />
                Run Coverage Evaluation
              </>
            )}
          </Button>
          
          {codeIds.length > 0 && !isLoading && (
            <div className="text-xs text-muted-foreground">
              Will evaluate {docIds.length} GDD(s) against {codeIds.length} code batch(es)
            </div>
          )}
          
          {/* Status Message */}
          {statusMessage && (
            <div className={`mt-4 p-3 rounded-lg border ${
              statusMessage.includes("✅") 
                ? "bg-green-50 border-green-200 text-green-800" 
                : statusMessage.includes("❌")
                ? "bg-red-50 border-red-200 text-red-800"
                : "bg-blue-50 border-blue-200 text-blue-800"
            }`}>
              <div className="flex items-center gap-2">
                {isLoading && <Loader2 className="h-4 w-4 animate-spin" />}
                <p className="text-sm font-medium">{statusMessage}</p>
              </div>
              {isLoading && (
                <p className="text-xs mt-2 opacity-75">
                  💡 Check browser console (F12) and backend logs for detailed progress
                </p>
              )}
            </div>
          )}
          
          {error && (
            <div className="mt-4 p-3 rounded-lg bg-destructive/10 border border-destructive/20">
              <p className="text-sm text-destructive font-medium">Error:</p>
              <p className="text-sm text-destructive/80 mt-1">{error}</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Summary */}
      {report && (
        <Card>
          <CardHeader>
            <CardTitle>Coverage Summary</CardTitle>
            <CardDescription>
              {Array.isArray(docId) ? `${docIds.length} GDDs` : docIds[0]} → {codeIds.length} code batch(es) | Behavior-based
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-5 gap-4">
              <div className="text-center">
                <p className="text-2xl font-bold">{report.summary.totalItems}</p>
                <p className="text-sm text-muted-foreground">Total Items</p>
              </div>
              <div className="text-center">
                <p className="text-2xl font-bold text-green-600">
                  {report.summary.implemented}
                </p>
                <p className="text-sm text-muted-foreground">Implemented</p>
              </div>
              <div className="text-center">
                <p className="text-2xl font-bold text-yellow-600">
                  {report.summary.partiallyImplemented || 0}
                </p>
                <p className="text-sm text-muted-foreground">Partially Implemented</p>
              </div>
              <div className="text-center">
                <p className="text-2xl font-bold text-red-600">
                  {report.summary.notImplemented}
                </p>
                <p className="text-sm text-muted-foreground">Not Implemented</p>
              </div>
              <div className="text-center">
                <p className="text-2xl font-bold text-orange-600">
                  {report.summary.errors}
                </p>
                <p className="text-sm text-muted-foreground">Errors</p>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Results List */}
      {report && (
        <Card>
          <CardHeader>
            <CardTitle>Requirements & Implementation Status</CardTitle>
            <CardDescription>
              Behavior-based results. Filter to quickly see what’s missing or needs attention.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="mb-4 flex flex-wrap gap-2">
              <Button
                size="sm"
                variant={statusFilter === "all" ? "default" : "outline"}
                onClick={() => setStatusFilter("all")}
              >
                All ({report.summary.totalItems})
              </Button>
              <Button
                size="sm"
                variant={statusFilter === "implemented" ? "default" : "outline"}
                onClick={() => setStatusFilter("implemented")}
              >
                Implemented ({report.summary.implemented})
              </Button>
              <Button
                size="sm"
                variant={statusFilter === "partially_implemented" ? "default" : "outline"}
                onClick={() => setStatusFilter("partially_implemented")}
              >
                Partial ({report.summary.partiallyImplemented || 0})
              </Button>
              <Button
                size="sm"
                variant={statusFilter === "not_implemented" ? "default" : "outline"}
                onClick={() => setStatusFilter("not_implemented")}
              >
                Not Implemented ({report.summary.notImplemented})
              </Button>
            </div>

            <div className="space-y-2">
              {filteredResults.map((result) => (
                <div
                  key={result.itemId}
                  className={`p-4 rounded-lg border cursor-pointer transition-colors ${
                    selectedItem?.itemId === result.itemId
                      ? "border-primary bg-primary/5"
                      : "border-border hover:bg-muted/50"
                  }`}
                  onClick={() => setSelectedItem(result)}
                >
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-1">
                        {getStatusIcon(result.status)}
                        <span className="font-medium">{result.itemName}</span>
                        <Badge variant="outline" className="text-xs">
                          {result.itemType}
                        </Badge>
                        {getStatusBadge(result.status)}
                      </div>
                      {result.evidence.length > 0 && (
                        <p className="text-sm text-muted-foreground mt-2">
                          {result.evidence.length} evidence point(s) found
                        </p>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Item Details */}
      {selectedItem && (
        <Card>
          <CardHeader>
            <CardTitle>Implementation Details</CardTitle>
            <CardDescription>{selectedItem.itemName}</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <p className="text-sm font-medium mb-2">Status</p>
              {getStatusBadge(selectedItem.status)}
            </div>

            {(selectedItem.topMatches?.length || 0) > 0 && (
              <div>
                <p className="text-sm font-medium mb-2">Top Behavior Matches</p>
                <div className="space-y-2">
                  {selectedItem.topMatches!.slice(0, 5).map((match, idx) => (
                    <div key={idx} className="p-3 rounded-lg bg-muted">
                      <div className="flex items-center justify-between">
                        <p className="text-sm font-mono text-foreground">{match.symbol}</p>
                        <Badge variant="outline" className="text-xs">
                          Sim: {match.similarity.toFixed(3)}
                        </Badge>
                      </div>
                      {match.description && (
                        <p className="text-xs text-muted-foreground mt-1">{match.description}</p>
                      )}
                    </div>
                  ))}
                </div>
              </div>
            )}

            {((selectedItem.missingTriggers?.length || 0) > 0 || (selectedItem.missingEffects?.length || 0) > 0) && (
              <div>
                <p className="text-sm font-medium mb-2">Gaps</p>
                <div className="space-y-2">
                  {(selectedItem.missingTriggers || []).length > 0 && (
                    <div className="p-3 rounded-lg bg-muted">
                      <p className="text-xs font-semibold text-foreground">Missing triggers</p>
                      <p className="text-xs text-muted-foreground mt-1">
                        {selectedItem.missingTriggers!.join(", ")}
                      </p>
                    </div>
                  )}
                  {(selectedItem.missingEffects || []).length > 0 && (
                    <div className="p-3 rounded-lg bg-muted">
                      <p className="text-xs font-semibold text-foreground">Missing effects</p>
                      <p className="text-xs text-muted-foreground mt-1">
                        {selectedItem.missingEffects!.join(", ")}
                      </p>
                    </div>
                  )}
                </div>
              </div>
            )}

            {selectedItem.evidence.length > 0 && (
              <div>
                <p className="text-sm font-medium mb-2">Evidence</p>
                <div className="space-y-2">
                  {selectedItem.evidence.map((ev, idx) => (
                    <div key={idx} className="p-3 rounded-lg bg-muted">
                      {ev.file && (
                        <p className="text-sm font-medium">{ev.file}</p>
                      )}
                      <p className="text-sm text-muted-foreground mt-1">
                        {ev.reason}
                      </p>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {selectedItem.retrievedChunks.length > 0 && (
              <div>
                <p className="text-sm font-medium mb-2">
                  Retrieved Code Chunks ({selectedItem.retrievedChunks.length})
                </p>
                <div className="space-y-2 max-h-96 overflow-y-auto">
                  {selectedItem.retrievedChunks.slice(0, 5).map((chunk, idx) => (
                    <div key={idx} className="p-3 rounded-lg bg-muted">
                      <div className="flex items-center justify-between mb-2">
                        <p className="text-xs font-mono text-muted-foreground">
                          {chunk.chunkId}
                        </p>
                        <Badge variant="outline" className="text-xs">
                          Score: {chunk.score.toFixed(3)}
                        </Badge>
                      </div>
                      <pre className="text-xs overflow-x-auto">
                        {chunk.content.substring(0, 500)}
                        {chunk.content.length > 500 && "..."}
                      </pre>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  )
}


