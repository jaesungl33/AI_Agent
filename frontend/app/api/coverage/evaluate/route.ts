import { NextRequest, NextResponse } from "next/server"

const BACKEND_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000"

export async function POST(req: NextRequest) {
  try {
    const body = await req.json()

    const res = await fetch(`${BACKEND_URL}/coverage/evaluate`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(body),
    })

    const text = await res.text()

    if (!res.ok) {
      // Try to parse JSON error payload if possible
      let json: any = null
      try {
        json = JSON.parse(text)
      } catch {
        // ignore, will wrap raw text
      }
      return NextResponse.json(
        {
          error: "Coverage evaluation failed",
          status: res.status,
          message: json?.detail || json?.message || text || res.statusText,
        },
        { status: res.status },
      )
    }

    // Successful JSON payload from backend
    let data: any
    try {
      data = JSON.parse(text)
    } catch {
      // Backend returned non-JSON, wrap it
      data = { report: null, raw: text }
    }

    return NextResponse.json(data, { status: 200 })
  } catch (error: any) {
    console.error("[API] /api/coverage/evaluate error", error)
    return NextResponse.json(
      {
        error: "Coverage evaluation proxy error",
        message: error?.message || "Unknown error",
      },
      { status: 500 },
    )
  }
}


