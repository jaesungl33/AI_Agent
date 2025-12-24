import { NextRequest, NextResponse } from "next/server"

const getBackendUrl = () => {
  const envUrl = process.env.NEXT_PUBLIC_API_URL
  if (envUrl) return envUrl.replace(/localhost/g, "127.0.0.1")
  return "http://127.0.0.1:8000"
}

const BACKEND_URL = getBackendUrl()

export async function POST(req: NextRequest) {
  try {
    const { searchParams } = new URL(req.url)
    const workspaceId = searchParams.get("workspaceId") || "tank_war"
    const url = `${BACKEND_URL}/export/compare?workspaceId=${encodeURIComponent(workspaceId)}`
    const res = await fetch(url, { method: "POST", headers: { "Content-Type": "application/json" } })
    const text = await res.text()
    if (!res.ok) {
      let message = text
      try {
        const j = JSON.parse(text)
        message = j?.detail || j?.message || message
      } catch {}
      return NextResponse.json({ error: "Compare failed", message }, { status: res.status })
    }
    try {
      const data = JSON.parse(text)
      return NextResponse.json(data, { status: 200 })
    } catch {
      return NextResponse.json({ raw: text }, { status: 200 })
    }
  } catch (err: any) {
    return NextResponse.json(
      { error: "Compare proxy error", message: err?.message || "Unknown error" },
      { status: 500 },
    )
  }
}




