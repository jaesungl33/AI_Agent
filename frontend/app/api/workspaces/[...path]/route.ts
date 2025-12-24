import { NextRequest, NextResponse } from "next/server"

const getBackendUrl = () => {
  const envUrl = process.env.NEXT_PUBLIC_API_URL
  if (envUrl) {
    return envUrl.replace(/localhost/g, "127.0.0.1")
  }
  return "http://127.0.0.1:8000"
}

const BACKEND_URL = getBackendUrl()

export async function GET(
  req: NextRequest,
  { params }: { params: { path: string[] } },
) {
  const subpath = params.path?.join("/") || ""
  // Handle default workspace locally to avoid 404 noise
  if (subpath === "default") {
    return NextResponse.json({ default_workspace: "tank_war" })
  }
  const targetUrl = `${BACKEND_URL}/workspaces/${subpath}`
  const res = await fetch(targetUrl, { method: "GET" })
  const text = await res.text()
  if (!res.ok) {
    let json: any = null
    try {
      json = JSON.parse(text)
    } catch {
      // ignore
    }
    const message = json?.detail || json?.message || text || res.statusText
    return NextResponse.json(
      { error: "Workspace request failed", status: res.status, message },
      { status: res.status },
    )
  }
  try {
    const data = JSON.parse(text)
    return NextResponse.json(data, { status: 200 })
  } catch {
    return NextResponse.json({ raw: text }, { status: 200 })
  }
}

export async function POST(
  req: NextRequest,
  { params }: { params: { path: string[] } },
) {
  const body = await req.text()
  const subpath = params.path?.join("/") || ""
  const targetUrl = `${BACKEND_URL}/workspaces/${subpath}`
  const res = await fetch(targetUrl, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body,
  })
  const text = await res.text()
  if (!res.ok) {
    let json: any = null
    try {
      json = JSON.parse(text)
    } catch {
      // ignore
    }
    const message = json?.detail || json?.message || text || res.statusText
    return NextResponse.json(
      { error: "Workspace request failed", status: res.status, message },
      { status: res.status },
    )
  }
  try {
    const data = JSON.parse(text)
    return NextResponse.json(data, { status: 200 })
  } catch {
    return NextResponse.json({ raw: text }, { status: 200 })
  }
}

