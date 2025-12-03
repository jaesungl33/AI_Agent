# Production Deployment Guide

## Architecture Overview

```
┌─────────────────┐         ┌──────────────────┐
│   Vercel        │         │   Render         │
│   (Frontend)    │ ──────> │   (Backend API)  │
│   Next.js       │         │   FastAPI        │
└─────────────────┘         └──────────────────┘
                                      │
                                      ▼
                            ┌──────────────────┐
                            │   RAG Storage    │
                            │   (on Render)    │
                            └──────────────────┘
```

---

## Part 1: Backend Deployment (Render.com)

### Current Status
✅ You already have: `https://ai-agent-l90z.onrender.com`

### What You Need to Do

1. **Verify Backend is Working**
   ```bash
   curl https://ai-agent-l90z.onrender.com/health
   ```
   Should return: `{"status":"ok",...}`

2. **Set Environment Variables in Render**
   - Go to Render Dashboard → Your Service → Environment
   - Add these variables:
     ```
     DASHSCOPE_API_KEY=your_qwen_api_key_here
     QWEN_API_KEY=your_qwen_api_key_here  (if different)
     REGION=intl  (or cn, depending on your Qwen region)
     ```

3. **Ensure RAG Storage Persists**
   - Render's free tier has **ephemeral disk** (data can be lost on restart)
   - **Solution**: Use Render's persistent disk or external storage
   
   **Option A: Render Persistent Disk** (Recommended for small-medium projects)
   - In Render Dashboard → Your Service → Settings
   - Enable "Persistent Disk" (if available on your plan)
   - Mount it at `/app/rag_storage` or similar
   
   **Option B: Cloud Storage** (Recommended for production)
   - Use AWS S3, Google Cloud Storage, or similar
   - Modify `backend_api/main.py` to read/write RAG files from cloud storage
   - More complex but scales better

4. **Update Build/Start Commands** (if needed)
   - Build Command: `pip install -r backend_api/requirements.txt`
   - Start Command: `uvicorn backend_api.main:app --host 0.0.0.0 --port 10000`

---

## Part 2: Frontend Deployment (Vercel)

### Step 1: Push Code to GitHub
```bash
cd /Users/madeinheaven/Documents/GitHub/AI_Agent
git add .
git commit -m "Prepare for production deployment"
git push origin main
```

### Step 2: Connect to Vercel

1. Go to [vercel.com](https://vercel.com) and sign in
2. Click **"Add New Project"**
3. Import your GitHub repository
4. Configure:
   - **Framework Preset**: Next.js
   - **Root Directory**: `frontend`
   - **Build Command**: `npm run build` (auto-detected)
   - **Output Directory**: `.next` (auto-detected)

### Step 3: Set Environment Variables

In Vercel Dashboard → Your Project → Settings → Environment Variables:

Add:
```
NEXT_PUBLIC_API_URL=https://ai-agent-l90z.onrender.com
```

**Important**: 
- Use your actual Render backend URL
- The `NEXT_PUBLIC_` prefix makes it available in the browser
- Set for **Production**, **Preview**, and **Development** environments

### Step 4: Deploy

- Click **"Deploy"**
- Vercel will build and deploy automatically
- You'll get a URL like: `https://your-project.vercel.app`

---

## Part 3: Storage Considerations

### Problem
Your RAG storage (`rag_storage/` folder) contains:
- `kv_store_doc_status.json` - All your indexed documents
- `kv_store_text_chunks.json` - Document chunks
- `vdb_*.json` - Vector embeddings
- `graph_chunk_entity_relation.graphml` - Knowledge graph

**On Render's free tier, this data is ephemeral** (lost on restart).

### Solutions

#### Option A: Keep Data on Render (Quick Fix)
1. **Upload your existing `rag_storage/` folder to Render**
   - Use Render's file upload or SSH access
   - Place it in the service's persistent directory (if available)

2. **Or: Re-index on first deploy**
   - Upload GDDs through the web UI after deployment
   - They'll be indexed on Render's disk (temporary but works)

#### Option B: Use Cloud Storage (Production-Ready)

**AWS S3 Example**:
1. Create an S3 bucket
2. Upload your `rag_storage/` folder to S3
3. Modify `gdd_rag_backbone/config.py` to read from S3:
   ```python
   import boto3
   s3 = boto3.client('s3')
   # Read/write RAG files from S3 instead of local disk
   ```
4. Set AWS credentials in Render environment variables

**Google Cloud Storage** or **Azure Blob Storage** work similarly.

#### Option C: Database (Most Scalable)
- Store chunks/embeddings in a vector database (Pinecone, Weaviate, Qdrant)
- Store metadata in PostgreSQL
- More setup but handles large scale

---

## Part 4: Testing Production

### 1. Test Backend
```bash
# Health check
curl https://ai-agent-l90z.onrender.com/health

# List documents
curl https://ai-agent-l90z.onrender.com/documents

# Test chat
curl -X POST https://ai-agent-l90z.onrender.com/chat \
  -H "Content-Type: application/json" \
  -d '{"workspaceId":"default","message":"test","useAllDocs":true,"topK":4}'
```

### 2. Test Frontend
1. Open your Vercel URL: `https://your-project.vercel.app`
2. Go to `/documents` - should show your indexed docs
3. Go to `/chat` - send a test message
4. Check browser console (F12) for errors

### 3. Test End-to-End
1. Upload a new GDD via `/upload`
2. Wait for indexing to complete
3. Ask a question about it in `/chat`
4. Verify answer is correct

---

## Part 5: Monitoring & Maintenance

### Monitor Backend Logs
- Render Dashboard → Your Service → Logs
- Watch for errors, timeouts, memory issues

### Monitor Frontend
- Vercel Dashboard → Your Project → Analytics
- Check for build errors, deployment status

### Common Issues

**Backend returns 500 errors?**
- Check Render logs for Python errors
- Verify environment variables are set
- Check if RAG storage files are accessible

**Frontend shows "no docs available"?**
- Verify `NEXT_PUBLIC_API_URL` points to correct Render URL
- Check backend `/documents` endpoint returns data
- Check browser console for CORS errors

**Chat is slow?**
- Render free tier has cold starts (first request after inactivity)
- Consider upgrading to paid tier for always-on service
- Or use a faster LLM model (qwen-turbo instead of qwen-max)

---

## Part 6: Quick Start Commands

### Deploy Backend (Render)
```bash
# Already done - you have https://ai-agent-l90z.onrender.com
# Just verify it's working and set environment variables
```

### Deploy Frontend (Vercel)
```bash
cd /Users/madeinheaven/Documents/GitHub/AI_Agent
git add frontend/
git commit -m "Deploy frontend to Vercel"
git push origin main

# Then connect repo to Vercel via web UI
```

### Update Environment Variables
- **Render**: Dashboard → Service → Environment → Add `DASHSCOPE_API_KEY`
- **Vercel**: Dashboard → Project → Settings → Environment Variables → Add `NEXT_PUBLIC_API_URL`

---

## Recommended Production Setup

### For Serious Use:
1. **Backend**: Render paid tier (always-on, no cold starts)
2. **Frontend**: Vercel (free tier is fine)
3. **Storage**: AWS S3 or similar for RAG data persistence
4. **Database**: Optional - PostgreSQL for metadata, vector DB for embeddings
5. **Monitoring**: Render logs + Vercel analytics

### For Demo/Testing:
1. **Backend**: Render free tier (works, but has cold starts)
2. **Frontend**: Vercel free tier
3. **Storage**: Render's ephemeral disk (re-index after restarts)

---

## Next Steps

1. ✅ Verify Render backend is working
2. ✅ Set `DASHSCOPE_API_KEY` in Render
3. ✅ Deploy frontend to Vercel
4. ✅ Set `NEXT_PUBLIC_API_URL` in Vercel
5. ✅ Test end-to-end
6. ⚠️ Decide on storage strategy (ephemeral vs. cloud)

---

**Your system will be live and accessible from anywhere!** 🌐

