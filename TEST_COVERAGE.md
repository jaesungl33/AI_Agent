# Quick Coverage Evaluation Tests

This document describes how to quickly test if the coverage evaluation system is working.

## ✅ Test 1: Direct Function Test (FASTEST - ~30 seconds)

This tests the evaluation function directly, bypassing the HTTP layer:

```bash
python3 test_coverage_quick.py
```

**What it does:**
- Tests the core evaluation function directly
- Uses 1 simple test requirement
- Takes ~30-60 seconds
- Verifies backend logic is working

**Expected output:**
```
✅ EVALUATION COMPLETE!
Status: not_implemented (or implemented/partially_implemented)
Coverage Type: semantic (or fast)
✅ Backend connection and evaluation function are working!
```

---

## ✅ Test 2: HTTP API Test (Full Stack - ~2-3 minutes)

This tests the full HTTP API endpoint:

```bash
python3 test_backend_api.py
```

**What it does:**
- Tests `/health` endpoint
- Tests `/documents` endpoint  
- Tests `/coverage/evaluate` endpoint with real data
- Takes ~2-3 minutes (evaluates 5 requirements)

**Expected output:**
```
✅ Backend is running
✅ Found X GDD(s) and Y code batch(es)
✅ EVALUATION SUCCESSFUL!
Total Items: 5
Implemented: X
Partially Implemented: Y
Not Implemented: Z
```

---

## ✅ Test 3: Frontend UI Test (Full End-to-End)

1. Make sure both servers are running:
   ```bash
   # Terminal 1: Backend
   python3 -m uvicorn backend_api.main:app --reload --port 8000
   
   # Terminal 2: Frontend
   cd frontend && npm run dev
   ```

2. Open browser: http://localhost:3000/coverage

3. Select:
   - **1 GDD** (any GDD document)
   - **1 Code Batch** (any code batch)

4. Click "Evaluate Coverage"

5. Watch for:
   - Progress indicator showing "Evaluating requirement X/5"
   - Status updates in console
   - Results appearing after 2-3 minutes

---

## 🔍 Troubleshooting

### Backend not responding to HTTP requests

If `test_backend_api.py` fails but `test_coverage_quick.py` works:

1. **Backend might be stuck**: Restart it
   ```bash
   # Kill existing backend
   pkill -f "uvicorn.*main:app"
   
   # Restart
   python3 -m uvicorn backend_api.main:app --reload --port 8000
   ```

2. **Check backend logs**: Look for errors or hanging LLM calls

3. **Test health endpoint directly**:
   ```bash
   curl http://127.0.0.1:8000/health
   ```

### Connection errors in frontend

If frontend shows "Cannot connect to backend":

1. **Verify backend is running**:
   ```bash
   curl http://127.0.0.1:8000/health
   ```

2. **Check environment variables**:
   ```bash
   cd frontend
   cat .env.local
   # Should show: NEXT_PUBLIC_API_URL=http://localhost:8000
   ```

3. **Restart frontend** (clears Next.js cache):
   ```bash
   cd frontend
   rm -rf .next
   npm run dev
   ```

### Evaluation takes too long

- **Normal**: 2-3 minutes for 5 requirements
- **If stuck**: Check backend logs for hanging LLM calls
- **Solution**: The system now has 25s timeout per LLM call and early exit optimization

---

## 📊 What Each Test Verifies

| Test | Backend Function | HTTP API | Frontend | Time |
|------|------------------|----------|----------|------|
| `test_coverage_quick.py` | ✅ | ❌ | ❌ | ~30s |
| `test_backend_api.py` | ✅ | ✅ | ❌ | ~2-3min |
| Frontend UI | ✅ | ✅ | ✅ | ~2-3min |

**Recommendation**: Start with `test_coverage_quick.py` to verify core functionality, then test the full stack.

