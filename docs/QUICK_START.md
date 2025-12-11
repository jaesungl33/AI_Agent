# Quick Start Guide

## ✅ Verification Complete

All imports and paths have been verified to work after reorganization!

## 🚀 Starting the Application

### Option 1: Use Startup Scripts (Easiest)

**Backend:**
```bash
./start_backend.sh
```

**Frontend (in another terminal):**
```bash
./start_frontend.sh
```

### Option 2: Manual Start

**Backend:**
```bash
cd backend
uvicorn main:app --reload
```

**Frontend:**
```bash
cd frontend
npm run dev
```

## ✅ Test Results

### Backend Imports
```
✅ gdd_rag_backbone.config
✅ gdd_rag_backbone.llm_providers
✅ gdd_rag_backbone.rag_backend
✅ gdd_rag_backbone.gdd
✅ gdd_rag_backbone.workspace
✅ All imports successful!
```

### Paths Verified
- `DEFAULT_DOCS_DIR`: `data/gdd_documents/` ✅
- `DEFAULT_WORKING_DIR`: `data/rag_storage/` ✅
- Workspace storage: `data/workspaces/` ✅

## 🛠️ IDE Setup

### VS Code
- ✅ `.vscode/settings.json` configured
- ✅ `src/` added to Python analysis paths
- ✅ Launch configurations ready

**To use:**
1. Open VS Code
2. Press F5 to start backend with debugging
3. Or use Run → Start Debugging

### Other IDEs
See `TESTING_GUIDE.md` for PyCharm and other IDE setup instructions.

## 📍 Access Points

Once started:
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:8000
- **API Docs**: http://localhost:8000/docs
- **Health Check**: http://localhost:8000/health

## 🔍 Quick Verification

```bash
# Test backend health
curl http://localhost:8000/health

# Test imports
cd backend && python3 test_imports.py

# Check frontend
cd frontend && npm run build
```

## 📚 Documentation

- **Testing Guide**: `TESTING_GUIDE.md`
- **Storage Locations**: `docs/STORAGE_LOCATIONS.md`
- **Project Structure**: `docs/STRUCTURE.md`

## ✨ Everything is Ready!

The reorganization is complete and verified. You can now:
1. Start developing
2. Deploy to production
3. Add new features

All imports work, paths are correct, and the structure is clean! 🎉

