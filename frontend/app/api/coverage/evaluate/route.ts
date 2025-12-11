import { NextRequest, NextResponse } from "next/server"

// Use 127.0.0.1 instead of localhost for more reliable server-side connections
// Next.js server-side fetch sometimes has issues with localhost resolution
const getBackendUrl = () => {
  const envUrl = process.env.NEXT_PUBLIC_API_URL
  if (envUrl) {
    // Replace localhost with 127.0.0.1 for server-side reliability
    return envUrl.replace(/localhost/g, "127.0.0.1")
  }
  return "http://127.0.0.1:8000"
}

const BACKEND_URL = getBackendUrl()
const EVALUATION_TIMEOUT = 30 * 60 * 1000 // 30 minutes for large evaluations

export async function POST(req: NextRequest) {
  try {
    const body = await req.json()
    if (!body.workspaceId) {
      body.workspaceId = "tank_war"
    }
    console.log("[API] Proxying coverage evaluation request:", {
      docId: body.docId,
      codeIndexId: body.codeIndexId,
      topK: body.topK,
      backendUrl: BACKEND_URL,
    })

    // Optional health check - don't block if it fails, just try the actual request
    // Sometimes health check can timeout but the main endpoint works fine
    try {
      const healthCheck = await Promise.race([
        fetch(`${BACKEND_URL}/health`, {
          method: "GET",
          signal: AbortSignal.timeout(2000), // Quick 2s check
        }),
        new Promise((_, reject) => setTimeout(() => reject(new Error("Health check timeout")), 2000))
      ]) as Response
      
      if (healthCheck.ok) {
        console.log("[API] Backend health check passed")
      }
    } catch (healthError: any) {
      // Health check failed, but continue anyway - the actual request might work
      console.warn("[API] Health check failed or timed out, proceeding with evaluation request anyway:", healthError.message)
    }

    // Create AbortController for timeout
    const controller = new AbortController()
    const timeoutId = setTimeout(() => {
      controller.abort()
      console.error("[API] Coverage evaluation request timed out after 10 minutes")
    }, EVALUATION_TIMEOUT)

    try {
      const targetUrl = `${BACKEND_URL}/coverage/evaluate`
      console.log("[API] Calling backend:", targetUrl)
      
      // Use fetch with extended timeout and keep-alive
      // Note: Node.js fetch doesn't support keep-alive directly, but we can increase timeout
      const res = await fetch(targetUrl, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(body),
        signal: controller.signal,
        // Add keep-alive hint (though Node.js fetch may not fully support this)
        keepalive: true,
      })

      clearTimeout(timeoutId)

      const text = await res.text()

      if (!res.ok) {
        // Try to parse JSON error payload if possible
        let json: any = null
        try {
          json = JSON.parse(text)
        } catch {
          // ignore, will wrap raw text
        }
        const errorMessage = json?.detail || json?.message || text || res.statusText
        console.error("[API] Backend returned error:", {
          status: res.status,
          message: errorMessage,
        })
        return NextResponse.json(
          {
            error: "Coverage evaluation failed",
            status: res.status,
            message: errorMessage,
          },
          { status: res.status },
        )
      }

      // Successful JSON payload from backend
      let data: any
      try {
        data = JSON.parse(text)
        console.log("[API] Coverage evaluation successful:", {
          hasReport: !!data.report,
          totalItems: data.report?.summary?.totalItems || 0,
          warnings: data.warnings?.length || 0,
        })
      } catch (parseError) {
        console.error("[API] Failed to parse backend response as JSON:", parseError)
        // Backend returned non-JSON, wrap it
        data = { report: null, raw: text, error: "Invalid JSON response from backend" }
      }

      return NextResponse.json(data, { status: 200 })
    } catch (fetchError: any) {
      clearTimeout(timeoutId)
      
      if (fetchError.name === "AbortError") {
        console.error("[API] Request aborted (timeout or cancelled)")
        return NextResponse.json(
          {
            error: "Request timeout",
            message: "Coverage evaluation timed out after 10 minutes. The evaluation may still be running on the server.",
          },
          { status: 504 },
        )
      }
      
      console.error("[API] Fetch error details:", {
        name: fetchError.name,
        message: fetchError.message,
        cause: fetchError.cause,
        stack: fetchError.stack,
      })
      
      // Provide helpful error message
      let errorMessage = fetchError.message || "Unknown fetch error"
      if (errorMessage.includes("fetch failed") || errorMessage.includes("ECONNREFUSED")) {
        errorMessage = `Cannot connect to backend at ${BACKEND_URL}. Make sure the Python backend is running on port 8000.`
      }
      
      throw new Error(errorMessage)
    }
  } catch (error: any) {
    console.error("[API] /api/coverage/evaluate error:", error)
    return NextResponse.json(
      {
        error: "Coverage evaluation proxy error",
        message: error?.message || "Unknown error occurred while proxying request to backend",
        details: process.env.NODE_ENV === "development" ? error?.stack : undefined,
      },
      { status: 500 },
    )
  }
}




