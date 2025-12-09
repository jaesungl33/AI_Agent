# Troubleshooting: File Upload Issues

## Problem: Can't Upload Files

If you're seeing errors when trying to upload files, it's likely because the backend API isn't running.

## ✅ Solution: The Frontend Now Auto-Detects Backend

The frontend has been updated to **automatically detect** if the backend is available and fall back to mock mode if it's not.

### What This Means:

1. **If backend is running** → Uses real API
2. **If backend is not running** → Uses mock API (for UI testing)

### Try Uploading Now:

1. Go to http://localhost:3000/upload
2. Select a file
3. Click "Upload & Index"
4. You should see a success message (using mock API)

### Check Console:

Open browser DevTools (F12) and check the console. You should see:
- `📝 Using mock API for GDD upload` - if backend is not available
- Or actual API calls if backend is running

---

## 🔧 Manual Mock Mode

If you want to force mock mode:

1. Open browser console (F12)
2. Run: `localStorage.setItem("useMockAPI", "true")`
3. Refresh the page

To disable: `localStorage.removeItem("useMockAPI")`

---

## 🚀 Starting the Real Backend

To use the real backend API, you need to start a Python FastAPI server. 

### Option 1: Use Existing Streamlit UI

The Streamlit UI is already set up:

```bash
cd /Users/madeinheaven/Documents/GitHub/AI_Agent
streamlit run ui/app.py
```

This runs on port 8501 (Streamlit), not 8000 (REST API).

### Option 2: Create FastAPI Backend (Recommended)

We need to create a FastAPI wrapper. See `backend-api/README.md` for instructions.

---

## 📝 Current Status

- ✅ Frontend runs on http://localhost:3000
- ✅ File uploads work with mock API
- ⚠️ Real backend API needs to be created/started
- ✅ UI is fully functional for testing

---

## 🐛 Common Issues

### "Network Error" or "Failed to fetch"
- Backend is not running
- Frontend will auto-use mock mode
- Check console for mock API messages

### "CORS Error"
- Backend needs CORS headers
- Or use mock mode for now

### Files Upload But Nothing Happens
- Check browser console for errors
- Mock mode simulates upload but doesn't actually process files
- Need real backend for actual processing

---

## 💡 Next Steps

1. **For UI Testing**: Use mock mode (automatic)
2. **For Real Processing**: Create FastAPI backend wrapper
3. **For Quick Testing**: Use existing Streamlit UI at port 8501




