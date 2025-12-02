# Deploying to Vercel

This guide will help you deploy your Next.js frontend to Vercel.

## Method 1: GitHub Integration (Recommended)

This is the easiest and most automated way to deploy.

### Step 1: Push to GitHub

1. Make sure your code is committed and pushed to a GitHub repository:
   ```bash
   git add .
   git commit -m "Add Next.js frontend"
   git push origin main
   ```

### Step 2: Connect to Vercel

1. Go to [vercel.com](https://vercel.com) and sign in (or create an account)
2. Click **"Add New Project"**
3. Import your GitHub repository
4. Vercel will auto-detect Next.js

### Step 3: Configure Project Settings

**Root Directory:**
- Set Root Directory to: `frontend`
- Click "Edit" next to Root Directory
- Select `frontend` folder

**Build Settings:**
- Framework Preset: Next.js (auto-detected)
- Build Command: `npm run build` (default)
- Output Directory: `.next` (default)
- Install Command: `npm install` (default)

**Environment Variables:**
- Add `NEXT_PUBLIC_API_URL` with your backend API URL
  - For production: `https://your-backend-api.com/api`
  - For development: `http://localhost:8000/api` (if testing locally)

### Step 4: Deploy

1. Click **"Deploy"**
2. Wait for the build to complete (usually 2-3 minutes)
3. Your app will be live at `https://your-project.vercel.app`

## Method 2: Vercel CLI

### Step 1: Install Vercel CLI

```bash
npm install -g vercel
```

### Step 2: Login

```bash
vercel login
```

### Step 3: Deploy

Navigate to the frontend directory and deploy:

```bash
cd frontend
vercel
```

Follow the prompts:
- Set up and deploy? **Yes**
- Which scope? (Select your account)
- Link to existing project? **No** (first time) or **Yes** (if updating)
- Project name? (Press Enter for default)
- Directory? `./` (current directory)
- Override settings? **No**

### Step 4: Set Environment Variables

```bash
vercel env add NEXT_PUBLIC_API_URL
```

Enter your API URL when prompted.

### Step 5: Production Deploy

```bash
vercel --prod
```

## Post-Deployment

### Custom Domain (Optional)

1. Go to your project settings on Vercel
2. Click **"Domains"**
3. Add your custom domain
4. Follow DNS configuration instructions

### Environment Variables

You can update environment variables anytime:
- Via Vercel Dashboard: Project → Settings → Environment Variables
- Via CLI: `vercel env add VARIABLE_NAME`

### Automatic Deployments

With GitHub integration:
- **Production**: Every push to `main` branch
- **Preview**: Every push to other branches (creates preview URLs)

## Troubleshooting

### Build Fails

1. Check build logs in Vercel dashboard
2. Ensure all dependencies are in `package.json`
3. Verify Node.js version (Vercel uses Node 18+ by default)

### API Connection Issues

1. Ensure `NEXT_PUBLIC_API_URL` is set correctly
2. Check CORS settings on your backend API
3. Verify the API is accessible from the internet

### 404 Errors

1. Ensure you're deploying from the `frontend` directory
2. Check that `vercel.json` is in the frontend folder
3. Verify Next.js routing is set up correctly

## Useful Commands

```bash
# Deploy to preview
vercel

# Deploy to production
vercel --prod

# View deployment logs
vercel logs

# List all deployments
vercel ls

# Remove deployment
vercel remove
```


