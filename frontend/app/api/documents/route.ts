import { NextResponse } from "next/server"

// Use 127.0.0.1 instead of localhost for more reliable server-side connections
const getBackendUrl = () => {
  const envUrl = process.env.NEXT_PUBLIC_API_URL
  if (envUrl) {
    return envUrl.replace(/localhost/g, "127.0.0.1")
  }
  return "http://127.0.0.1:8888"
}

const BACKEND_URL = getBackendUrl()

export async function GET(request: Request) {
  try {
    const { searchParams } = new URL(request.url)
    const workspaceId = searchParams.get("workspaceId")
    
    const url = workspaceId 
      ? `${BACKEND_URL}/documents?workspaceId=${encodeURIComponent(workspaceId)}`
      : `${BACKEND_URL}/documents`
    
    // Allow more time for larger workspaces; frontend shows errors if this still times out.
    const res = await fetch(url, {
      method: "GET",
      headers: { "Content-Type": "application/json" },
      signal: AbortSignal.timeout(20000), // 20s to avoid proxy timeout on cold start
    })

    const text = await res.text()

    if (!res.ok) {
      let json: any = null
      try {
        json = JSON.parse(text)
      } catch {
        // ignore parse errors
      }
      const message = json?.detail || json?.message || text || res.statusText
      return NextResponse.json(
        { error: "Documents fetch failed", message },
        { status: res.status },
      )
    }

    // Successful JSON payload
    try {
      const data = JSON.parse(text)
      return NextResponse.json(data, { status: 200 })
    } catch {
      return NextResponse.json(
        { error: "Invalid JSON from backend", raw: text },
        { status: 502 },
      )
    }
  } catch (error: any) {
    let message = error?.message || "Unknown error"
    if (message.includes("Failed to fetch") || message.includes("ECONNREFUSED")) {
      message = `Cannot connect to backend at ${BACKEND_URL}. Make sure it is running.`
    }
    return NextResponse.json(
      { error: "Documents proxy error", message },
      { status: 503 },
    )
  }
}

