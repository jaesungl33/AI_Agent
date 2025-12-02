"use client"

import { useCallback, useState } from "react"
import { useDropzone } from "react-dropzone"
import { Upload, File, X, Loader2 } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { cn } from "@/lib/utils"

interface FileUploadProps {
  accept?: Record<string, string[]>
  maxSize?: number
  onFileSelect: (file: File) => void
  onRemove?: () => void
  selectedFile?: File | null
  isUploading?: boolean
  label?: string
  description?: string
}

export function FileUpload({
  accept,
  maxSize = 100 * 1024 * 1024, // 100MB default
  onFileSelect,
  onRemove,
  selectedFile,
  isUploading = false,
  label = "Upload file",
  description = "Drag and drop a file here, or click to select",
}: FileUploadProps) {
  const [error, setError] = useState<string | null>(null)

  const onDrop = useCallback(
    (acceptedFiles: File[]) => {
      setError(null)
      if (acceptedFiles.length > 0) {
        const file = acceptedFiles[0]
        if (file.size > maxSize) {
          setError(`File size exceeds ${Math.round(maxSize / 1024 / 1024)}MB limit`)
          return
        }
        onFileSelect(file)
      }
    },
    [maxSize, onFileSelect]
  )

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept,
    maxFiles: 1,
    disabled: isUploading || !!selectedFile,
  })

  const handleRemove = () => {
    setError(null)
    onRemove?.()
  }

  return (
    <div className="space-y-2">
      <Card>
        <CardContent className="p-6">
          {selectedFile ? (
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <File className="h-5 w-5 text-muted-foreground" />
                <div>
                  <p className="text-sm font-medium">{selectedFile.name}</p>
                  <p className="text-xs text-muted-foreground">
                    {(selectedFile.size / 1024 / 1024).toFixed(2)} MB
                  </p>
                </div>
              </div>
              {!isUploading && (
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={handleRemove}
                  className="h-8 w-8"
                >
                  <X className="h-4 w-4" />
                </Button>
              )}
              {isUploading && (
                <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
              )}
            </div>
          ) : (
            <div
              {...getRootProps()}
              className={cn(
                "cursor-pointer rounded-lg border-2 border-dashed p-8 text-center transition-colors",
                isDragActive
                  ? "border-primary bg-primary/5"
                  : "border-muted-foreground/25 hover:border-primary/50",
                isUploading && "pointer-events-none opacity-50"
              )}
            >
              <input {...getInputProps()} />
              <div className="flex flex-col items-center gap-4">
                {isUploading ? (
                  <Loader2 className="h-12 w-12 animate-spin text-muted-foreground" />
                ) : (
                  <Upload className="h-12 w-12 text-muted-foreground" />
                )}
                <div>
                  <p className="text-sm font-medium">{label}</p>
                  <p className="text-xs text-muted-foreground mt-1">
                    {description}
                  </p>
                </div>
                {!isUploading && (
                  <Button variant="outline" size="sm">
                    Select File
                  </Button>
                )}
              </div>
            </div>
          )}
          {error && (
            <p className="mt-2 text-sm text-destructive">{error}</p>
          )}
        </CardContent>
      </Card>
    </div>
  )
}

