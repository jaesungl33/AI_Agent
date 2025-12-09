import { NextResponse } from "next/server"

// Use 127.0.0.1 instead of localhost for more reliable server-side connections
const getBackendUrl = () => {
  const envUrl = process.env.NEXT_PUBLIC_API_URL
  if (envUrl) {
    return envUrl.replace(/localhost/g, "127.0.0.1")
  }
  return "http://127.0.0.1:8000"
}

const BACKEND_URL = getBackendUrl()

export async function GET() {
  try {
    const res = await fetch(`${BACKEND_URL}/documents`, {
      method: "GET",
      headers: { "Content-Type": "application/json" },
      signal: AbortSignal.timeout(5000), // short timeout for healthiness
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

