"use client"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { CheckCircle2, XCircle, AlertCircle, Loader2, Search } from "lucide-react"
import { coverageAPI } from "@/lib/api/client"
import type { CoverageReport, CoverageResult } from "@/lib/api/types"

interface CodeCoverageProps {
  docId: string
  codeIndexId?: string
}

export function CodeCoverage({ docId, codeIndexId: initialCodeIndexId }: CodeCoverageProps) {
  const [codeIndexId, setCodeIndexId] = useState(initialCodeIndexId || "")
  const [topK, setTopK] = useState(8)
  const [report, setReport] = useState<CoverageReport | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [selectedItem, setSelectedItem] = useState<CoverageResult | null>(null)

  const handleEvaluate = async () => {
    if (!codeIndexId.trim()) return

    setIsLoading(true)
    try {
      const report = await coverageAPI.evaluate(docId, codeIndexId, topK)
      setReport(report)
    } catch (error) {
      console.error("Coverage evaluation error:", error)
    } finally {
      setIsLoading(false)
    }
  }

  const getStatusIcon = (status: CoverageResult["status"]) => {
    switch (status) {
      case "implemented":
        return <CheckCircle2 className="h-4 w-4 text-green-500" />
      case "not_implemented":
        return <XCircle className="h-4 w-4 text-red-500" />
      case "error":
        return <AlertCircle className="h-4 w-4 text-yellow-500" />
    }
  }

  const getStatusBadge = (status: CoverageResult["status"]) => {
    switch (status) {
      case "implemented":
        return <Badge variant="success">Implemented</Badge>
      case "not_implemented":
        return <Badge variant="destructive">Not Implemented</Badge>
      case "error":
        return <Badge variant="warning">Error</Badge>
    }
  }

  return (
    <div className="space-y-6">
      {/* Evaluation Controls */}
      <Card>
        <CardHeader>
          <CardTitle>Code Coverage Evaluation</CardTitle>
          <CardDescription>
            Compare GDD requirements against your codebase
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="code-index">Code Index ID</Label>
            <Input
              id="code-index"
              placeholder="e.g., tank_online_codebase_batch001"
              value={codeIndexId}
              onChange={(e) => setCodeIndexId(e.target.value)}
              disabled={isLoading}
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="top-k">Chunks per Query</Label>
            <Input
              id="top-k"
              type="number"
              min={4}
              max={12}
              value={topK}
              onChange={(e) => setTopK(parseInt(e.target.value) || 8)}
              disabled={isLoading}
            />
          </div>
          <Button onClick={handleEvaluate} disabled={isLoading || !codeIndexId.trim()}>
            {isLoading ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Evaluating...
              </>
            ) : (
              <>
                <Search className="mr-2 h-4 w-4" />
                Run Coverage Evaluation
              </>
            )}
          </Button>
        </CardContent>
      </Card>

      {/* Summary */}
      {report && (
        <Card>
          <CardHeader>
            <CardTitle>Coverage Summary</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-4 gap-4">
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
                <p className="text-2xl font-bold text-red-600">
                  {report.summary.notImplemented}
                </p>
                <p className="text-sm text-muted-foreground">Not Implemented</p>
              </div>
              <div className="text-center">
                <p className="text-2xl font-bold text-yellow-600">
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
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              {report.results.map((result) => (
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

