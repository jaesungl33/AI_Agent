"use client"

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { FileText, Gamepad2, Users, Map, Sparkles } from "lucide-react"
import type { GDDSummary } from "@/lib/api/types"

interface GDDSummaryProps {
  summary: GDDSummary | null
  isLoading?: boolean
}

export function GDDSummary({ summary, isLoading }: GDDSummaryProps) {
  if (isLoading) {
    return (
      <Card>
        <CardContent className="p-6">
          <div className="flex items-center justify-center py-12">
            <div className="text-center">
              <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-current border-r-transparent"></div>
              <p className="mt-4 text-sm text-muted-foreground">Loading summary...</p>
            </div>
          </div>
        </CardContent>
      </Card>
    )
  }

  if (!summary) {
    return (
      <Card>
        <CardContent className="p-6">
          <div className="text-center py-12">
            <FileText className="mx-auto h-12 w-12 text-muted-foreground" />
            <p className="mt-4 text-sm text-muted-foreground">
              No GDD summary available. Upload and index a GDD first.
            </p>
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <div className="space-y-6">
      {/* Overview */}
      <Card>
        <CardHeader>
          <CardTitle>Overview</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {summary.genre && (
            <div>
              <p className="text-sm font-medium text-muted-foreground">Genre</p>
              <p className="mt-1">{summary.genre}</p>
            </div>
          )}
          {summary.coreLoop && (
            <div>
              <p className="text-sm font-medium text-muted-foreground">Core Loop</p>
              <p className="mt-1">{summary.coreLoop}</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Major Systems */}
      {summary.majorSystems.length > 0 && (
        <Card>
          <CardHeader>
            <div className="flex items-center gap-2">
              <Gamepad2 className="h-5 w-5" />
              <CardTitle>Major Systems</CardTitle>
            </div>
          </CardHeader>
          <CardContent>
            <div className="flex flex-wrap gap-2">
              {summary.majorSystems.map((system, idx) => (
                <Badge key={idx} variant="secondary">
                  {system}
                </Badge>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Player Interactions */}
      {summary.playerInteractions.length > 0 && (
        <Card>
          <CardHeader>
            <div className="flex items-center gap-2">
              <Users className="h-5 w-5" />
              <CardTitle>Player Interactions</CardTitle>
            </div>
          </CardHeader>
          <CardContent>
            <div className="flex flex-wrap gap-2">
              {summary.playerInteractions.map((interaction, idx) => (
                <Badge key={idx} variant="outline">
                  {interaction}
                </Badge>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Key Objects */}
      {summary.keyObjects.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Key Objects & Entities</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex flex-wrap gap-2">
              {summary.keyObjects.map((obj, idx) => (
                <Badge key={idx} variant="outline">
                  {obj}
                </Badge>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Maps & Modes */}
      {summary.mapsAndModes.length > 0 && (
        <Card>
          <CardHeader>
            <div className="flex items-center gap-2">
              <Map className="h-5 w-5" />
              <CardTitle>Maps & Modes</CardTitle>
            </div>
          </CardHeader>
          <CardContent>
            <div className="flex flex-wrap gap-2">
              {summary.mapsAndModes.map((map, idx) => (
                <Badge key={idx} variant="outline">
                  {map}
                </Badge>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Special Mechanics */}
      {summary.specialMechanics.length > 0 && (
        <Card>
          <CardHeader>
            <div className="flex items-center gap-2">
              <Sparkles className="h-5 w-5" />
              <CardTitle>Special Mechanics</CardTitle>
            </div>
          </CardHeader>
          <CardContent>
            <div className="flex flex-wrap gap-2">
              {summary.specialMechanics.map((mechanic, idx) => (
                <Badge key={idx} variant="outline">
                  {mechanic}
                </Badge>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}


