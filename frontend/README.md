# Frontend - Next.js Application

Modern Next.js frontend deployed on Vercel for the GDD RAG Backbone project.

## Development

```bash
npm install
npm run dev
```

Open [http://localhost:3000](http://localhost:3000)

## Deployment

Connected to Vercel via GitHub. Auto-deploys on push to `main`.

**Manual deploy:**
```bash
vercel --prod
```

**Environment Variables (Vercel Dashboard):**
- `NEXT_PUBLIC_API_URL` - Backend API URL
