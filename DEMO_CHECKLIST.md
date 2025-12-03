# Pre-Demo Checklist

Run through this **15 minutes before your demo** to ensure everything works.

## Quick Health Check

```bash
# 1. Check backend is running
curl http://localhost:8000/health
# Should return: {"status":"ok",...}

# 2. Check documents endpoint
curl http://localhost:8000/documents | head -c 200
# Should return JSON array with your docs

# 3. Test chat endpoint
curl -X POST http://localhost:8000/chat \
  -H "Content-Type: application/json" \
  -d '{"workspaceId":"default","message":"What is the core gameplay loop?","useAllDocs":true,"topK":4}'
# Should return JSON with an answer
```

## Browser Checks

1. **Open** `http://localhost:3000`
2. **Check Documents page** (`/documents`)
   - Should show list of your indexed GDDs
   - If empty, check backend logs
3. **Check Chat page** (`/chat`)
   - Send a test question: "What is the core gameplay loop?"
   - Should get a response within 10-15 seconds
   - If it hangs or errors, check browser console (F12)

## Demo Questions (Pre-Test These)

Test these questions work before the demo:

1. ✅ "What is the core gameplay loop of this game?"
2. ✅ "How does the tank upgrade system work?"
3. ✅ "What are the different game modes?"
4. ✅ "What is the shooting logic for tanks?"

If any fail, note which ones and have backup questions ready.

## If Something's Broken

### Backend not responding?
```bash
cd /Users/madeinheaven/Documents/GitHub/AI_Agent
pkill -f 'uvicorn backend_api.main:app'
python3 -m uvicorn backend_api.main:app --reload --port 8000
```

### Frontend not loading?
```bash
cd /Users/madeinheaven/Documents/GitHub/AI_Agent/frontend
pkill -f 'next dev'
npm run dev
```

### Chat returns errors?
- Check browser console (F12) for red errors
- Check backend terminal for Python errors
- Verify `rag_storage/kv_store_doc_status.json` exists and has entries

## Final Pre-Demo Steps

- [ ] Both servers running (backend + frontend)
- [ ] `/documents` page shows your GDDs
- [ ] `/chat` responds to a test question
- [ ] Browser console has no red errors
- [ ] Have `http://localhost:8000/docs` ready in another tab
- [ ] Know which questions work well (tested above)

## During Demo

- Keep backend terminal visible (shows it's working)
- Have browser DevTools closed (looks cleaner)
- If something breaks, stay calm: "Let me refresh that" and continue

---

**You're ready!** 🎯

