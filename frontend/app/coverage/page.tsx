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
import type { Document, GameSpec } from "@/lib/api/types"
import { GddSpecDetails } from "@/components/documents/gdd-spec-details"
import { Loader2, FileText, Code } from "lucide-react"

export default function CoveragePage() {
  const [documents, setDocuments] = useState<Document[]>([])
  const [selectedGddIds, setSelectedGddIds] = useState<string[]>([])  // All GDDs by default
  const [selectedCodeIds, setSelectedCodeIds] = useState<string[]>([])  // All code batches by default
  const [spec, setSpec] = useState<GameSpec | null>(null)
  const [isLoadingSpec, setIsLoadingSpec] = useState(false)
  const [isLoadingDocs, setIsLoadingDocs] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  useEffect(() => {
    loadDocuments()
  }, [])

  const loadDocuments = async () => {
    try {
      setIsLoadingDocs(true)
      setLoadError(null)
      console.log("[CoveragePage] Loading documents from backend...")
      const docs = await documentAPI.list()
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
    } catch (error: any) {
      console.error("[CoveragePage] Failed to load documents:", error)
      setLoadError(error?.message || "Failed to load documents. Check if backend is running at http://localhost:8000")
    } finally {
      setIsLoadingDocs(false)
    }
  }

  const handleExtractSpec = async () => {
    if (selectedGddIds.length === 0) return
    
    try {
      setIsLoadingSpec(true)
      // Extract from first GDD for display (we'll merge all in backend)
      const specData = await gddAPI.getSpec(selectedGddIds[0])
      setSpec(specData)
    } catch (error) {
      console.error("Failed to extract spec:", error)
      alert("Failed to extract requirements. Make sure the GDDs are indexed.")
    } finally {
      setIsLoadingSpec(false)
    }
  }

  const gddDocs = documents.filter(d => d.type === "gdd" && d.status === "indexed")
  const codeDocs = documents.filter(d => d.type === "code" && d.status === "indexed")

  return (
    <LayoutWithSidebar>
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold">Code Coverage</h1>
          <p className="text-muted-foreground mt-2">
            Extract requirements from ALL GDDs and compare with your ENTIRE codebase
          </p>
        </div>

        {/* Document Selection */}
        <Card>
          <CardHeader>
            <CardTitle>Document Selection</CardTitle>
            <CardDescription>
              Comparing ALL GDDs against ENTIRE codebase (all batches). This gives you complete coverage.
            </CardDescription>
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
                    <p className="text-2xl font-bold">{spec.requirements.length}</p>
                    <p className="text-sm text-muted-foreground">Requirements</p>
                  </div>
                  <div className="text-center p-4 rounded-lg bg-muted">
                    <p className="text-2xl font-bold">{spec.systems.length}</p>
                    <p className="text-sm text-muted-foreground">Systems</p>
                  </div>
                  <div className="text-center p-4 rounded-lg bg-muted">
                    <p className="text-2xl font-bold">{spec.objects.length}</p>
                    <p className="text-sm text-muted-foreground">Objects</p>
                  </div>
                  <div className="text-center p-4 rounded-lg bg-muted">
                    <p className="text-2xl font-bold">{spec.logicRules.length}</p>
                    <p className="text-sm text-muted-foreground">Logic Rules</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <GddSpecDetails spec={spec} />
          </div>
        )}

        {/* Coverage Evaluation */}
        {selectedGddIds.length > 0 && selectedCodeIds.length > 0 && (
          <CodeCoverage 
            docId={selectedGddIds.length === 1 ? selectedGddIds[0] : selectedGddIds} 
            codeIndexId={selectedCodeIds.length === 1 ? selectedCodeIds[0] : selectedCodeIds} 
          />
        )}

        {(selectedGddIds.length === 0 || selectedCodeIds.length === 0) && (
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

