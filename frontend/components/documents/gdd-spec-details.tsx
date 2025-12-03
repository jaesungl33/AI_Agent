"use client"

import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import type { GameSpec } from "@/lib/api/types"

interface GddSpecDetailsProps {
  spec: GameSpec
}

export function GddSpecDetails({ spec }: GddSpecDetailsProps) {
  if (!spec) return null

  return (
    <div className="space-y-4">

      <Card>
        <CardHeader>
          <CardTitle>Requirements</CardTitle>
          <CardDescription>Detailed requirements extracted from the GDD</CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          {spec.requirements.length === 0 && (
            <p className="text-sm text-muted-foreground">No requirements extracted yet.</p>
          )}
          {spec.requirements.slice(0, 25).map((req) => (
            <div key={req.id} className="p-3 rounded-lg border">
              <div className="flex items-center justify-between gap-2">
                <p className="font-medium">{req.title || req.id}</p>
                {req.priority && (
                  <Badge variant="outline" className="uppercase">
                    {req.priority}
                  </Badge>
                )}
              </div>
              {req.description && (
                <p className="text-sm text-muted-foreground mt-1">{req.description}</p>
              )}
              {req.acceptanceCriteria && (
                <p className="text-sm text-primary mt-1">
                  Acceptance: {req.acceptanceCriteria}
                </p>
              )}
            </div>
          ))}
          {spec.requirements.length > 25 && (
            <p className="text-xs text-muted-foreground">
              Showing first 25 of {spec.requirements.length} requirements.
            </p>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Systems</CardTitle>
          <CardDescription>Gameplay systems defined in the GDD</CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          {spec.systems.length === 0 && (
            <p className="text-sm text-muted-foreground">No systems extracted.</p>
          )}
          {spec.systems.map((system) => (
            <div key={system.id} className="p-3 rounded-lg border">
              <p className="font-medium">{system.name}</p>
              {system.description && (
                <p className="text-sm text-muted-foreground mt-1">{system.description}</p>
              )}
              {system.mechanics && (
                <p className="text-xs text-primary mt-1">Mechanics: {system.mechanics}</p>
              )}
            </div>
          ))}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Objects</CardTitle>
          <CardDescription>Core objects, tanks, and props defined in the GDD</CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          {spec.objects.length === 0 && (
            <p className="text-sm text-muted-foreground">No objects extracted.</p>
          )}
          {spec.objects.map((object) => (
            <div key={object.id} className="p-3 rounded-lg border">
              <div className="flex items-center gap-2">
                <p className="font-medium">{object.name}</p>
                {object.category && (
                  <Badge variant="secondary" className="uppercase">
                    {object.category}
                  </Badge>
                )}
              </div>
              {object.description && (
                <p className="text-sm text-muted-foreground mt-1">{object.description}</p>
              )}
              {object.specialRules && (
                <p className="text-xs text-primary mt-1">
                  Special: {object.specialRules}
                </p>
              )}
            </div>
          ))}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Logic Rules & Interactions</CardTitle>
          <CardDescription>Conditional rules or interactions discovered in the GDD</CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          {spec.logicRules.length === 0 && (
            <p className="text-sm text-muted-foreground">No logic rules extracted.</p>
          )}
          {spec.logicRules.map((rule) => (
            <div key={rule.id} className="p-3 rounded-lg border">
              <p className="font-medium">{rule.summary}</p>
              {rule.description && (
                <p className="text-sm text-muted-foreground mt-1">{rule.description}</p>
              )}
              {(rule.trigger || rule.effect) && (
                <div className="text-xs text-primary mt-2 space-y-1">
                  {rule.trigger && <p>Trigger: {rule.trigger}</p>}
                  {rule.effect && <p>Effect: {rule.effect}</p>}
                </div>
              )}
            </div>
          ))}
        </CardContent>
      </Card>
    </div>
  )
}

