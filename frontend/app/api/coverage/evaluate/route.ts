import { NextRequest, NextResponse } from "next/server"

const BACKEND_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000"
const EVALUATION_TIMEOUT = 10 * 60 * 1000 // 10 minutes (increased from 5 to handle more requirements)

export async function POST(req: NextRequest) {
  try {
    const body = await req.json()
    console.log("[API] Proxying coverage evaluation request:", {
      docId: body.docId,
      codeIndexId: body.codeIndexId,
      topK: body.topK,
      backendUrl: BACKEND_URL,
    })

    // Test backend connection first
    try {
      const healthCheck = await fetch(`${BACKEND_URL}/health`, {
        method: "GET",
        signal: AbortSignal.timeout(2000),
      })
      if (!healthCheck.ok) {
        throw new Error(`Backend health check failed: ${healthCheck.status}`)
      }
      console.log("[API] Backend health check passed")
    } catch (healthError: any) {
      console.error("[API] Backend health check failed:", healthError)
      return NextResponse.json(
        {
          error: "Backend connection failed",
          message: `Cannot connect to backend at ${BACKEND_URL}. Make sure the backend is running on port 8000. Error: ${healthError.message}`,
        },
        { status: 503 },
      )
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
      
      const res = await fetch(targetUrl, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(body),
        signal: controller.signal,
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




