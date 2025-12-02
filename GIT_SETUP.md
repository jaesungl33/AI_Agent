# Git Setup for Large Files

## ✅ What Was Done

1. **Created `.gitignore`** - Excludes large files and directories:
   - `tank_online_1-dev/` (1.9GB Unity project)
   - `rag_storage/*.json` and `*.graphml` (large RAG data files)
   - `node_modules/` (frontend dependencies)
   - `output/` (generated files)
   - `__pycache__/` (Python cache)
   - Large code snapshot files

2. **Removed large files from git tracking**:
   - All `rag_storage/*.json` files (removed from git, kept locally)
   - All `tank_online_1-dev/` files (removed from git, kept locally)

## 📝 Next Steps

### Commit the changes:

```bash
# Stage the .gitignore and removal of large files
git add .gitignore rag_storage/.gitkeep

# Commit the changes
git commit -m "Add .gitignore and remove large files from tracking"

# Push to GitHub
git push origin main
```

## ⚠️ Important Notes

### Files Still Local (Not Deleted)
- All files are **still on your computer**
- Only removed from **git tracking**
- You can still use them locally

### What Gets Committed Now:
- ✅ Frontend code (`frontend/`)
- ✅ Python backend code (`gdd_rag_backbone/`)
- ✅ Configuration files
- ✅ Documentation
- ❌ Large Unity project (excluded)
- ❌ RAG storage JSON files (excluded)
- ❌ Node modules (excluded)

### If You Need to Share Large Files:

**Option 1: Git LFS (Large File Storage)**
```bash
# Install Git LFS
brew install git-lfs  # macOS
# or: apt-get install git-lfs  # Linux

# Initialize in your repo
git lfs install

# Track large files
git lfs track "*.so"
git lfs track "*.bundle"
git lfs track "rag_storage/*.json"

# Commit .gitattributes
git add .gitattributes
```

**Option 2: External Storage**
- Use Google Drive, Dropbox, or AWS S3
- Share links in README
- Don't commit to git

**Option 3: Separate Repository**
- Create a private repo for `tank_online_1-dev`
- Link to it in your main README

## 🔍 Verify Before Pushing

```bash
# Check what will be committed
git status

# Check repository size
du -sh .git

# Verify large files are ignored
git check-ignore -v tank_online_1-dev/
git check-ignore -v rag_storage/*.json
```

## 🚀 Ready to Deploy

Once you've committed and pushed, you can:
1. Deploy frontend to Vercel (see `frontend/QUICK_START.md`)
2. The frontend will be small and deploy quickly
3. Backend can be deployed separately (Python service)


