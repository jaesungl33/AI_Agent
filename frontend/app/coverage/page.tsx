"use client"

import { LayoutWithSidebar } from "../layout-with-sidebar"
import { useState, useEffect } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import { documentAPI } from "@/lib/api/client"
import { useWorkspace } from "@/lib/contexts/workspace-context"
import type { Document } from "@/lib/api/types"
import { Loader2, FileText, Code, CheckCircle2, AlertCircle, Play, ArrowRight, Download, Eye } from "lucide-react"
import { Badge } from "@/components/ui/badge"

type WorkflowStep =
  | "idle"
  | "extracting_gdd"
  | "extracting_code"
  | "comparing"
  | "complete"
  | "error"

interface SummaryData {
  file: string
  extractedAt: string
  content: string
  items: string[]
}

interface ComparisonResult {
  implemented: string[]
  missing: string[]
  partial: string[]
  summary: {
    totalGddItems: number
    totalCodeItems: number
    implementedCount: number
    missingCount: number
    partialCount: number
  }
}

export default function CoveragePage() {
  const { currentWorkspace } = useWorkspace()
  const workspaceId = currentWorkspace?.id ?? "tank_war"
  const [documents, setDocuments] = useState<Document[]>([])
  const [isLoadingDocs, setIsLoadingDocs] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  // Workflow state
  const [workflowStep, setWorkflowStep] = useState<WorkflowStep>("idle")
  const [gddSummary, setGddSummary] = useState<SummaryData | null>(null)
  const [codeSummary, setCodeSummary] = useState<SummaryData | null>(null)
  const [comparisonResult, setComparisonResult] = useState<ComparisonResult | null>(null)
  const [isGddExtracting, setIsGddExtracting] = useState(false)
  const [isCodeExtracting, setIsCodeExtracting] = useState(false)
  const [isComparing, setIsComparing] = useState(false)
  const [error, setError] = useState<string | null>(null)

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
    } catch (error: any) {
      console.error("[CoveragePage] Failed to load documents:", error)
      setLoadError(error?.message || "Failed to load documents. Check if backend is running at http://localhost:8000")
    } finally {
      setIsLoadingDocs(false)
    }
  }

  const extractGddSummary = async () => {
    try {
      setIsGddExtracting(true)
      setError(null)
      setWorkflowStep("extracting_gdd")

      const response = await fetch(`/api/export/gdd?workspaceId=${workspaceId}`)
      if (!response.ok) {
        const text = await response.text()
        throw new Error(text || "Failed to extract GDD summary")
      }

      const data = await response.json()
      const summaryData: SummaryData = {
        file: data.file,
        extractedAt: new Date().toISOString(),
        content: data.content || "GDD summary extracted successfully",
        items: data.items || []
      }

      setGddSummary(summaryData)
      setWorkflowStep("idle")
    } catch (error: any) {
      console.error("Failed to extract GDD summary:", error)
      setError(error?.message || "Failed to extract GDD summary")
      setWorkflowStep("error")
    } finally {
      setIsGddExtracting(false)
    }
  }

  const extractCodeSummary = async () => {
    try {
      setIsCodeExtracting(true)
      setError(null)
      setWorkflowStep("extracting_code")

      const response = await fetch(`/api/export/code?workspaceId=${workspaceId}`)
      if (!response.ok) {
        const text = await response.text()
        throw new Error(text || "Failed to extract code summary")
      }

      const data = await response.json()
      const summaryData: SummaryData = {
        file: data.file,
        extractedAt: new Date().toISOString(),
        content: data.content || "Code summary extracted successfully",
        items: data.items || []
      }

      setCodeSummary(summaryData)
      setWorkflowStep("idle")
    } catch (error: any) {
      console.error("Failed to extract code summary:", error)
      setError(error?.message || "Failed to extract code summary")
      setWorkflowStep("error")
    } finally {
      setIsCodeExtracting(false)
    }
  }

  const compareSummaries = async () => {
    if (!gddSummary || !codeSummary) {
      setError("Both GDD and code summaries are required for comparison")
      return
    }

    try {
      setIsComparing(true)
      setError(null)
      setWorkflowStep("comparing")

      const response = await fetch(`/api/export/compare?workspaceId=${workspaceId}`, { method: "POST" })
      if (!response.ok) {
        const text = await response.text()
        throw new Error(text || "Failed to compare summaries")
      }

      const data = await response.json()
      const result: ComparisonResult = {
        implemented: data.implemented || [],
        missing: data.missing || [],
        partial: data.partial || [],
        summary: {
          totalGddItems: gddSummary.items.length,
          totalCodeItems: codeSummary.items.length,
          implementedCount: data.implemented?.length || 0,
          missingCount: data.missing?.length || 0,
          partialCount: data.partial?.length || 0
        }
      }

      setComparisonResult(result)
      setWorkflowStep("complete")
    } catch (error: any) {
      console.error("Failed to compare summaries:", error)
      setError(error?.message || "Failed to compare summaries")
      setWorkflowStep("error")
    } finally {
      setIsComparing(false)
    }
  }

  const isIndexed = (status?: string) => status === "indexed" || status === "processed"
  const gddDocs = documents.filter(d => d.type === "gdd" && isIndexed(d.status))
  const codeDocs = documents.filter(d => d.type === "code" && isIndexed(d.status))

  return (
    <LayoutWithSidebar>
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold">Coverage Analysis</h1>
          <p className="text-muted-foreground mt-2">
            Extract summaries from GDDs and code, then compare to see what&apos;s implemented
          </p>
        </div>

        {/* Document Status Overview */}
        <Card>
          <CardHeader>
            <CardTitle>Document Status</CardTitle>
            <CardDescription>
              Overview of available GDD and code documents
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <Label className="flex items-center gap-2">
                    <FileText className="h-4 w-4" />
                    GDD Documents
                  </Label>
                  <Badge variant="secondary">{gddDocs.length}</Badge>
                </div>
                {gddDocs.length === 0 ? (
                  <p className="text-sm text-muted-foreground">No indexed GDDs found</p>
                ) : (
                  <div className="space-y-1">
                    {gddDocs.slice(0, 3).map((doc) => (
                      <p key={doc.id} className="text-xs text-muted-foreground truncate">
                        {doc.name}
                      </p>
                    ))}
                    {gddDocs.length > 3 && (
                      <p className="text-xs text-muted-foreground">
                        +{gddDocs.length - 3} more
                      </p>
                    )}
                  </div>
                )}
              </div>

              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <Label className="flex items-center gap-2">
                    <Code className="h-4 w-4" />
                    Code Files
                  </Label>
                  <Badge variant="secondary">{codeDocs.length}</Badge>
                </div>
                {codeDocs.length === 0 ? (
                  <p className="text-sm text-muted-foreground">No indexed code files found</p>
                ) : (
                  <div className="space-y-1">
                    {codeDocs.slice(0, 3).map((doc) => (
                      <p key={doc.id} className="text-xs text-muted-foreground truncate">
                        {doc.name}
                      </p>
                    ))}
                    {codeDocs.length > 3 && (
                      <p className="text-xs text-muted-foreground">
                        +{codeDocs.length - 3} more
                      </p>
                    )}
                  </div>
                )}
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Main Workflow */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Step 1: Extract GDD Summary */}
          <Card>
            <CardHeader>
              <div className="flex items-center gap-3">
                <div className={`flex items-center justify-center w-8 h-8 rounded-full border-2 ${
                  gddSummary
                    ? "bg-green-500 border-green-500 text-white"
                    : isGddExtracting
                    ? "bg-blue-500 border-blue-500 text-white"
                    : "bg-muted border-border"
                }`}>
                  {gddSummary ? (
                    <CheckCircle2 className="h-5 w-5" />
                  ) : isGddExtracting ? (
                    <Loader2 className="h-5 w-5 animate-spin" />
                  ) : (
                    <span className="text-sm font-bold">1</span>
                  )}
                </div>
                <div className="flex-1">
                  <CardTitle className="text-lg">Extract GDD Summary</CardTitle>
                  <CardDescription>
                    Get requirements, functions & summaries from all GDD docs
                  </CardDescription>
                </div>
              </div>
            </CardHeader>
            <CardContent className="space-y-4">
              {gddSummary && (
                <div className="p-3 rounded-lg bg-green-50 dark:bg-green-950 border border-green-200 dark:border-green-800">
                  <div className="flex items-center justify-between text-sm">
                    <span className="font-medium">Items extracted:</span>
                    <Badge>{gddSummary.items.length}</Badge>
                  </div>
                  <div className="text-xs text-muted-foreground mt-1">
                    {new Date(gddSummary.extractedAt).toLocaleString()}
                  </div>
                </div>
              )}

              <Button
                onClick={extractGddSummary}
                disabled={isGddExtracting || gddDocs.length === 0}
                className="w-full"
                variant={gddSummary ? "outline" : "default"}
              >
                {isGddExtracting ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Extracting...
                  </>
                ) : gddSummary ? (
                  <>
                    <Download className="mr-2 h-4 w-4" />
                    Re-extract
                  </>
                ) : (
                  <>
                    <FileText className="mr-2 h-4 w-4" />
                    Extract GDD Summary
                  </>
                )}
              </Button>

              {gddSummary && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => window.open(gddSummary.file, '_blank')}
                  className="w-full"
                >
                  <Eye className="mr-2 h-4 w-4" />
                  View Summary
                </Button>
              )}
            </CardContent>
          </Card>

          {/* Step 2: Extract Code Summary */}
          <Card>
            <CardHeader>
              <div className="flex items-center gap-3">
                <div className={`flex items-center justify-center w-8 h-8 rounded-full border-2 ${
                  codeSummary
                    ? "bg-green-500 border-green-500 text-white"
                    : isCodeExtracting
                    ? "bg-blue-500 border-blue-500 text-white"
                    : "bg-muted border-border"
                }`}>
                  {codeSummary ? (
                    <CheckCircle2 className="h-5 w-5" />
                  ) : isCodeExtracting ? (
                    <Loader2 className="h-5 w-5 animate-spin" />
                  ) : (
                    <span className="text-sm font-bold">2</span>
                  )}
                </div>
                <div className="flex-1">
                  <CardTitle className="text-lg">Extract Code Summary</CardTitle>
                  <CardDescription>
                    Get main functions & summaries from all code files
                  </CardDescription>
                </div>
              </div>
            </CardHeader>
            <CardContent className="space-y-4">
              {codeSummary && (
                <div className="p-3 rounded-lg bg-green-50 dark:bg-green-950 border border-green-200 dark:border-green-800">
                  <div className="flex items-center justify-between text-sm">
                    <span className="font-medium">Functions extracted:</span>
                    <Badge>{codeSummary.items.length}</Badge>
                  </div>
                  <div className="text-xs text-muted-foreground mt-1">
                    {new Date(codeSummary.extractedAt).toLocaleString()}
                  </div>
                </div>
              )}

              <Button
                onClick={extractCodeSummary}
                disabled={isCodeExtracting || codeDocs.length === 0}
                className="w-full"
                variant={codeSummary ? "outline" : "default"}
              >
                {isCodeExtracting ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Extracting...
                  </>
                ) : codeSummary ? (
                  <>
                    <Download className="mr-2 h-4 w-4" />
                    Re-extract
                  </>
                ) : (
                  <>
                    <Code className="mr-2 h-4 w-4" />
                    Extract Code Summary
                  </>
                )}
              </Button>

              {codeSummary && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => window.open(codeSummary.file, '_blank')}
                  className="w-full"
                >
                  <Eye className="mr-2 h-4 w-4" />
                  View Summary
                </Button>
              )}
            </CardContent>
          </Card>

          {/* Step 3: Compare & Results */}
          <Card>
            <CardHeader>
              <div className="flex items-center gap-3">
                <div className={`flex items-center justify-center w-8 h-8 rounded-full border-2 ${
                  comparisonResult
                    ? "bg-green-500 border-green-500 text-white"
                    : isComparing
                    ? "bg-blue-500 border-blue-500 text-white"
                    : "bg-muted border-border"
                }`}>
                  {comparisonResult ? (
                    <CheckCircle2 className="h-5 w-5" />
                  ) : isComparing ? (
                    <Loader2 className="h-5 w-5 animate-spin" />
                  ) : (
                    <span className="text-sm font-bold">3</span>
                  )}
                </div>
                <div className="flex-1">
                  <CardTitle className="text-lg">Compare & Analyze</CardTitle>
                  <CardDescription>
                    See what&apos;s implemented vs missing from GDDs
                  </CardDescription>
                </div>
              </div>
            </CardHeader>
            <CardContent className="space-y-4">
              {comparisonResult && (
                <div className="space-y-3">
                  <div className="grid grid-cols-3 gap-2">
                    <div className="text-center p-2 rounded-lg bg-green-50 dark:bg-green-950">
                      <div className="text-lg font-bold text-green-600">
                        {comparisonResult.summary.implementedCount}
                      </div>
                      <div className="text-xs text-muted-foreground">Implemented</div>
                    </div>
                    <div className="text-center p-2 rounded-lg bg-red-50 dark:bg-red-950">
                      <div className="text-lg font-bold text-red-600">
                        {comparisonResult.summary.missingCount}
                      </div>
                      <div className="text-xs text-muted-foreground">Missing</div>
                    </div>
                    <div className="text-center p-2 rounded-lg bg-yellow-50 dark:bg-yellow-950">
                      <div className="text-lg font-bold text-yellow-600">
                        {comparisonResult.summary.partialCount}
                      </div>
                      <div className="text-xs text-muted-foreground">Partial</div>
                    </div>
                  </div>


                  <div className="space-y-2">
                    <h4 className="font-medium text-sm">Implemented Functions:</h4>
                    <div className="max-h-32 overflow-y-auto space-y-1">
                      {comparisonResult.implemented.slice(0, 5).map((item, idx) => (
                        <div key={idx} className="text-xs p-2 rounded bg-green-50 dark:bg-green-950 text-green-700 dark:text-green-300">
                          {item}
                        </div>
                      ))}
                      {comparisonResult.implemented.length > 5 && (
                        <div className="text-xs text-muted-foreground">
                          +{comparisonResult.implemented.length - 5} more...
                        </div>
                      )}
                    </div>
                  </div>

                  {comparisonResult.missing.length > 0 && (
                    <div className="space-y-2">
                      <h4 className="font-medium text-sm">Missing Functions:</h4>
                      <div className="max-h-32 overflow-y-auto space-y-1">
                        {comparisonResult.missing.slice(0, 5).map((item, idx) => (
                          <div key={idx} className="text-xs p-2 rounded bg-red-50 dark:bg-red-950 text-red-700 dark:text-red-300">
                            {item}
                          </div>
                        ))}
                        {comparisonResult.missing.length > 5 && (
                          <div className="text-xs text-muted-foreground">
                            +{comparisonResult.missing.length - 5} more...
                          </div>
                        )}
                      </div>
                    </div>
                  )}
                </div>
              )}

              <Button
                onClick={compareSummaries}
                disabled={isComparing || !gddSummary || !codeSummary}
                className="w-full"
                variant={comparisonResult ? "outline" : "default"}
              >
                {isComparing ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Comparing...
                  </>
                ) : comparisonResult ? (
                  <>
                    <ArrowRight className="mr-2 h-4 w-4" />
                    Re-compare
                  </>
                ) : (
                  <>
                    <Play className="mr-2 h-4 w-4" />
                    Compare Summaries
                  </>
                )}
              </Button>
            </CardContent>
          </Card>
        </div>

        {/* Error Display */}
        {error && (
          <Card className="border-destructive">
            <CardContent className="pt-6">
              <div className="flex items-center gap-2 text-destructive">
                <AlertCircle className="h-5 w-5" />
                <span className="font-medium">Error</span>
              </div>
              <p className="text-sm text-muted-foreground mt-2">{error}</p>
            </CardContent>
          </Card>
        )}

        {/* Loading States */}
        {(isLoadingDocs || isGddExtracting || isCodeExtracting || isComparing) && (
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center gap-2">
                <Loader2 className="h-5 w-5 animate-spin" />
                <span className="text-sm">
                  {isLoadingDocs ? "Loading documents..." :
                   isGddExtracting ? "Extracting GDD summary..." :
                   isCodeExtracting ? "Extracting code summary..." :
                   "Comparing summaries..."}
                </span>
              </div>
            </CardContent>
          </Card>
        )}
      </div>
    </LayoutWithSidebar>
  )
}
