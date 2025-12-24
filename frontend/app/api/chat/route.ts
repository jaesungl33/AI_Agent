import { NextRequest, NextResponse } from "next/server"

const getBackendUrl = () => {
  const envUrl = process.env.NEXT_PUBLIC_API_URL
  if (envUrl) {
    return envUrl.replace(/localhost/g, "127.0.0.1")
  }
  return "http://127.0.0.1:8000"
}

const BACKEND_URL = getBackendUrl()

export async function POST(req: NextRequest) {
  try {
    const body = await req.json()
    const targetUrl = `${BACKEND_URL}/chat`

    const res = await fetch(targetUrl, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    })

    const text = await res.text()

    if (!res.ok) {
      let json: any = null
      try {
        json = JSON.parse(text)
      } catch {
        // ignore
      }
      const errorMessage = json?.detail || json?.message || text || res.statusText
      return NextResponse.json(
        { error: "Chat failed", status: res.status, message: errorMessage },
        { status: res.status },
      )
    }

    // Return raw JSON payload
    try {
      const data = JSON.parse(text)
      return NextResponse.json(data, { status: 200 })
    } catch {
      return NextResponse.json({ raw: text }, { status: 200 })
    }
  } catch (err: any) {
    return NextResponse.json(
      { error: "Chat proxy error", message: err?.message || "Unknown error" },
      { status: 500 },
    )
  }
}

