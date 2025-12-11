# Testing Guide - Post Reorganization

## ✅ Import Verification

### Backend Imports

All imports have been tested and verified to work:

```bash
cd backend
python3 test_imports.py
```

Expected output:
```
✅ gdd_rag_backbone.config
✅ gdd_rag_backbone.llm_providers
✅ gdd_rag_backbone.rag_backend
✅ gdd_rag_backbone.gdd
✅ gdd_rag_backbone.workspace
✅ All imports successful!
```

### Frontend Imports

Frontend imports remain unchanged (TypeScript/Next.js):
- All imports use relative paths or `@/` aliases
- No changes needed

## 🚀 Starting the Backend

### Option 1: Command Line
```bash
cd backend
uvicorn main:app --reload
```

### Option 2: VS Code Debugger
1. Open VS Code
2. Go to Run and Debug (F5)
3. Select "Python: Backend API"
4. Press F5 to start

### Option 3: Using the Script
```bash
cd backend
python3 -m uvicorn main:app --reload
```

**Expected:** Server starts on `http://localhost:8000`

**Test:** Open `http://localhost:8000/health` in browser

## 🎨 Starting the Frontend

### Option 1: Command Line
```bash
cd frontend
npm run dev
```

### Option 2: Using npm scripts
```bash
cd frontend
npm install  # If not already done
npm run dev
```

**Expected:** Server starts on `http://localhost:3000`

## 🔍 Verifying Everything Works

### 1. Backend Health Check
```bash
curl http://localhost:8000/health
```

Expected response:
```json
{"status":"ok","time":"2024-12-10T..."}
```

### 2. Backend API Endpoints
```bash
# List documents
curl http://localhost:8000/documents

# List workspaces
curl http://localhost:8000/workspaces
```

### 3. Frontend Connection
1. Start backend (port 8000)
2. Start frontend (port 3000)
3. Open `http://localhost:3000`
4. Check browser console for errors

## 🛠️ IDE Setup

### VS Code

1. **Python Path Configuration**
   - Already configured in `.vscode/settings.json`
   - `src/` is added to `python.analysis.extraPaths`

2. **Debugging**
   - Launch configurations in `.vscode/launch.json`
   - Use F5 to start backend with debugging

3. **Source Root**
   - VS Code will automatically recognize `src/` as source root
   - IntelliSense should work for all imports

### PyCharm

1. **Mark Directory as Source Root**
   - Right-click `src/` folder
   - Select "Mark Directory as" → "Sources Root"

2. **Python Interpreter**
   - File → Settings → Project → Python Interpreter
   - Ensure correct interpreter is selected

### Other IDEs

Add `src/` to your Python path:
- **Sublime Text**: Add to `Python.sublime-settings`
- **Vim/Neovim**: Configure `PYTHONPATH` in your config
- **Emacs**: Add to `python-shell-extra-pythonpaths`

## 📝 Common Issues

### Import Errors

**Problem:** `ModuleNotFoundError: No module named 'gdd_rag_backbone'`

**Solution:**
```python
import sys
from pathlib import Path
PROJECT_ROOT = Path(__file__).parent.parent
SRC_DIR = PROJECT_ROOT / "src"
sys.path.insert(0, str(SRC_DIR))
```

### Path Issues

**Problem:** Paths pointing to wrong locations

**Solution:** Check `src/gdd_rag_backbone/config.py`:
- `DEFAULT_WORKING_DIR` should be `data/rag_storage/`
- `DEFAULT_DOCS_DIR` should be `data/gdd_documents/`

### Frontend Can't Connect to Backend

**Problem:** CORS or connection errors

**Solution:**
1. Ensure backend is running on port 8000
2. Check `frontend/.env` or `frontend/.env.local`:
   ```
   NEXT_PUBLIC_API_URL=http://localhost:8000
   ```

## ✅ Verification Checklist

- [ ] Backend imports work (`python3 backend/test_imports.py`)
- [ ] Backend starts without errors
- [ ] Backend health endpoint responds
- [ ] Frontend starts without errors
- [ ] Frontend can connect to backend
- [ ] IDE recognizes `src/` as source root
- [ ] IntelliSense/autocomplete works for imports

## 🎯 Quick Test Commands

```bash
# Test imports
cd backend && python3 test_imports.py

# Start backend
cd backend && uvicorn main:app --reload

# Start frontend (in another terminal)
cd frontend && npm run dev

# Test backend
curl http://localhost:8000/health
```

