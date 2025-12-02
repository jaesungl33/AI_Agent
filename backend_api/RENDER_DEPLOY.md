## FastAPI Backend Deployment (Render.com)

This backend (`backend_api/main.py`) can be deployed as a web service on Render.

### 1. Push this repo to GitHub

If you haven't already:

```bash
cd /Users/madeinheaven/Documents/GitHub/AI_Agent
git add .
git commit -m "Add FastAPI backend for GDD RAG"
git push origin main
```

### 2. Create a Render account

1. Go to `https://render.com`.
2. Sign up or log in.

### 3. Create a new Web Service

1. Click **"New" → "Web Service"**.
2. Choose **"Build and deploy from a Git repository"**.
3. Select this GitHub repo (`AI_Agent`).

### 4. Configure service

- **Name**: `gdd-rag-backend` (or anything)
- **Region**: choose one near you
- **Branch**: `main`
- **Root Directory**: `.` (repo root)
- **Runtime**: `Python 3`
- **Build Command**:

  ```bash
  pip install -r backend_api/requirements.txt
  ```

- **Start Command**:

  ```bash
  uvicorn backend_api.main:app --host 0.0.0.0 --port 10000
  ```

  Render will map its own external port to this internal port.

### 5. Environment variables

In Render's **Environment** section, add anything you use locally in `.env`:

- `DASHSCOPE_API_KEY`
- `OPENAI_API_KEY` (or equivalent)
- Any other keys your RAG pipeline requires.

You **do not** need `NEXT_PUBLIC_API_URL` here; that is for the frontend.

### 6. Deploy

Click **"Create Web Service"**. Render will:

1. Clone the repo.
2. Run `pip install -r backend_api/requirements.txt`.
3. Start `uvicorn backend_api.main:app`.

Once it's live, Render will show you a URL like:

```text
https://gdd-rag-backend.onrender.com
```

You can test it:

```bash
curl https://gdd-rag-backend.onrender.com/health
```

You should see a JSON response with `"status": "ok"`.

### 7. Connect the frontend (Vercel)

In your Vercel project settings for the frontend:

1. Go to **Settings → Environment Variables**.
2. Add:

   - **Name**: `NEXT_PUBLIC_API_URL`
   - **Value**: `https://gdd-rag-backend.onrender.com`

3. Redeploy your frontend (or trigger a deploy by pushing a commit).

Now your Vercel-hosted Next.js app will talk to the Render-hosted FastAPI backend.


