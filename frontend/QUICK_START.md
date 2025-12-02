# Quick Start: Deploy to Vercel

## Option 1: GitHub Integration (Easiest) ⭐

### 1. Push to GitHub
```bash
cd /Users/madeinheaven/Documents/GitHub/AI_Agent
git add frontend/
git commit -m "Add Next.js frontend"
git push origin main
```

### 2. Deploy on Vercel
1. Go to [vercel.com/new](https://vercel.com/new)
2. Click **"Import Git Repository"**
3. Select your `AI_Agent` repository
4. **Important**: Set **Root Directory** to `frontend`
   - Click "Edit" → Select `frontend` folder
5. Add Environment Variable:
   - Name: `NEXT_PUBLIC_API_URL`
   - Value: `http://localhost:8000/api` (or your production API URL)
6. Click **"Deploy"**

✅ Done! Your app will be live in ~2 minutes.

---

## Option 2: Vercel CLI

### 1. Install Vercel CLI
```bash
npm install -g vercel
```

### 2. Login
```bash
vercel login
```

### 3. Deploy
```bash
cd frontend
vercel
```

Follow the prompts, then:
```bash
# Set environment variable
vercel env add NEXT_PUBLIC_API_URL

# Deploy to production
vercel --prod
```

---

## Option 3: Use the Helper Script

```bash
cd frontend
./deploy.sh
```

---

## After Deployment

1. **Get your URL**: Vercel will give you a URL like `https://your-project.vercel.app`
2. **Set Environment Variables**: 
   - Go to Vercel Dashboard → Your Project → Settings → Environment Variables
   - Add `NEXT_PUBLIC_API_URL` with your backend API URL
3. **Redeploy**: Changes to env vars require a redeploy

---

## Need Help?

- Full guide: See `DEPLOY.md`
- Vercel Docs: [vercel.com/docs](https://vercel.com/docs)
- Support: [vercel.com/support](https://vercel.com/support)


