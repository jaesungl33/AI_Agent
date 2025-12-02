# GDD RAG Frontend

Modern Next.js frontend for the GDD RAG Assistant - an AI-powered tool for analyzing Game Design Documents and comparing them against code implementations.

## Features

- 🎮 **Drag & Drop Upload**: Upload GDDs (PDF/DOCX) and game code (ZIP)
- 📊 **GDD Summary**: Automatically extracted game design summary
- 💬 **AI Chat**: Interactive chat interface to discuss your game design
- ✅ **Code Coverage**: Compare requirements vs implementation status

## Tech Stack

- **Next.js 14+** (App Router)
- **TypeScript**
- **Tailwind CSS**
- **shadcn/ui** components
- **Lucide-react** icons

## Getting Started

### Prerequisites

- Node.js 18+ and npm/yarn/pnpm

### Installation

```bash
cd frontend
npm install
```

### Development

```bash
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) in your browser.

### Build for Production

```bash
npm run build
npm start
```

## Project Structure

```
frontend/
├── app/                    # Next.js App Router
│   ├── layout.tsx         # Root layout
│   ├── page.tsx           # Main page
│   └── globals.css        # Global styles
├── components/            # React components
│   ├── ui/               # shadcn/ui components
│   ├── workspace-setup.tsx
│   ├── gdd-summary.tsx
│   ├── chat-interface.tsx
│   └── code-coverage.tsx
└── lib/                  # Utilities & API
    ├── api/              # API client & types
    └── utils.ts          # Helper functions
```

## API Integration

The frontend defines API endpoints in `lib/api/client.ts` and types in `lib/api/types.ts`. These are designed to work with the Python backend service.

Set the API URL via environment variable:
```bash
NEXT_PUBLIC_API_URL=http://localhost:8000/api
```

## Deployment

This app is ready for Vercel deployment. Simply connect your GitHub repository to Vercel and deploy.

## License

MIT

