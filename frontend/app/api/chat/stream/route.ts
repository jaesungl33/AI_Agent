import { NextRequest } from "next/server"

const getBackendUrl = () => {
  const envUrl = process.env.NEXT_PUBLIC_API_URL
  if (envUrl) {
    return envUrl.replace(/localhost/g, "127.0.0.1")
  }
  return "http://127.0.0.1:8000"
}

const BACKEND_URL = getBackendUrl()

export async function POST(req: NextRequest) {
  const body = await req.json()
  const targetUrl = `${BACKEND_URL}/chat/stream`

  // Stream the backend response directly
  const response = await fetch(targetUrl, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  })

  return new Response(response.body, {
    status: response.status,
    headers: {
      "Content-Type": "text/event-stream",
    },
  })
}

