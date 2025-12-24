import { NextRequest, NextResponse } from 'next/server'

const BACKEND_URL = process.env.NEXT_PUBLIC_API_URL?.replace('/api', '') || 'http://localhost:8000'

// Proxy all codeqa requests to the backend
export async function POST(request: NextRequest) {
  try {
    const body = await request.json()

    // Transform the payload to match backend expectations
    const backendPayload = {
      message: body.message,
      use_codebase: body.use_codebase || true
    }

    console.log('[CodeQA API] Forwarding to backend:', backendPayload)

    // Forward the request to the backend
    const backendResponse = await fetch(`${BACKEND_URL}/codeqa/chat`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(backendPayload),
    })

    if (!backendResponse.ok) {
      const errorText = await backendResponse.text()
      console.error('[CodeQA API] Backend error:', errorText)
      return NextResponse.json(
        { error: 'Backend request failed', details: errorText },
        { status: backendResponse.status }
      )
    }

    const data = await backendResponse.json()
    return NextResponse.json(data)
  } catch (error) {
    console.error('[CodeQA API] Proxy error:', error)
    return NextResponse.json(
      { error: 'Failed to proxy request to backend' },
      { status: 500 }
    )
  }
}


