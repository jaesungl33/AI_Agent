# How to Start Your Frontend

## 🚀 Local Development (Recommended for Testing)

Run the Next.js development server locally:

```bash
cd frontend
npm run dev
```

Then open: **http://localhost:3000**

This gives you:
- ✅ Hot reload (changes update instantly)
- ✅ Fast refresh
- ✅ Error messages in browser
- ✅ Best for development

---

## 🌐 Deploy to Vercel (Production)

### Option 1: GitHub Integration (Easiest) ⭐

1. **Push your code to GitHub:**
   ```bash
   cd /Users/madeinheaven/Documents/GitHub/AI_Agent
   git add frontend/
   git commit -m "Add Next.js frontend"
   git push origin main
   ```

2. **Deploy on Vercel:**
   - Go to [vercel.com/new](https://vercel.com/new)
   - Click **"Import Git Repository"**
   - Select your `AI_Agent` repository
   - **Set Root Directory to `frontend`** (important!)
   - Add environment variable: `NEXT_PUBLIC_API_URL`
   - Click **"Deploy"**

✅ Your app will be live in ~2 minutes!

---

### Option 2: Vercel CLI

1. **Install Vercel CLI:**
   ```bash
   npm install -g vercel
   ```

2. **Login:**
   ```bash
   vercel login
   ```

3. **Deploy from frontend folder:**
   ```bash
   cd frontend
   vercel
   ```

4. **Set environment variable:**
   ```bash
   vercel env add NEXT_PUBLIC_API_URL
   # Enter: http://localhost:8000/api (or your API URL)
   ```

5. **Deploy to production:**
   ```bash
   vercel --prod
   ```

---

### Option 3: Use Helper Script

```bash
cd frontend
./deploy.sh
```

---

## 📝 Quick Commands

```bash
# Local development
cd frontend && npm run dev

# Build for production
cd frontend && npm run build

# Run production build locally
cd frontend && npm start

# Deploy to Vercel (if CLI installed)
cd frontend && vercel --prod
```

---

## 🔧 Environment Variables

For local development, create `frontend/.env.local`:
```
NEXT_PUBLIC_API_URL=http://localhost:8000/api
```

For Vercel, add in dashboard:
- Settings → Environment Variables
- Add `NEXT_PUBLIC_API_URL` with your backend URL

---

## ❓ Which Should I Use?

- **Local Development** (`npm run dev`): Use while coding/testing
- **Vercel Deployment**: Use when you want to share your app online


