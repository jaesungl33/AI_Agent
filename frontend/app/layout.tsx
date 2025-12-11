import type { Metadata } from "next"
import { Inter } from "next/font/google"
import "./globals.css"
import { WorkspaceProvider } from "@/lib/contexts/workspace-context"

const inter = Inter({ subsets: ["latin"] })

export const metadata: Metadata = {
  title: "GDD RAG Assistant",
  description: "AI-powered Game Design Document analysis and code coverage tool",
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <WorkspaceProvider>{children}</WorkspaceProvider>
      </body>
    </html>
  )
}




