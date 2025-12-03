# Demo Guide: GDD RAG Assistant

## Quick Overview
**What it does**: Upload Game Design Documents (GDDs) → AI indexes them → Ask questions and get answers from your design docs using RAG (Retrieval-Augmented Generation).

**Why it matters**: Instead of manually searching through dozens of PDFs, you can ask natural language questions and get instant answers backed by your actual design documents.

---

## Demo Flow (5-7 minutes)

### 1. **Show the Problem** (30 seconds)
- "I have 50+ GDD PDFs covering different game systems (combat, UI, progression, etc.)"
- "Finding specific information means opening PDFs and searching manually"
- "This is slow and error-prone"

### 2. **Show the Solution** (1 minute)
- Open `http://localhost:3000/documents`
- Point out: "All my GDDs are already indexed here"
- Show the list: "I can see all 50+ documents, their status, and chunk counts"

### 3. **Upload a New Document** (1 minute)
- Go to `http://localhost:3000/upload`
- Drag & drop a GDD PDF
- Enter a simple doc ID (e.g., `demo_gdd`)
- Click "Upload & Index"
- Show the status changing: "uploaded" → "indexing" → "indexed"
- Explain: "The system parses, chunks, embeds, and indexes this document"

### 4. **Chat Demo** (3-4 minutes) - **THE KEY PART**
- Go to `http://localhost:3000/chat`
- Ask progressively more specific questions:

  **Question 1** (broad):
  > "What is the core gameplay loop of this game?"
  
  - Show the answer appears
  - Point out: "This answer is generated from the actual GDD content, not generic knowledge"

  **Question 2** (specific system):
  > "How does the tank upgrade system work in-match?"
  
  - Show it finds relevant chunks from the combat/progression modules
  - Point out the "X sources" badge if visible

  **Question 3** (cross-document):
  > "What are the different game modes and how do they differ?"
  
  - Show it pulls from multiple GDDs (deathmatch, outpost breaker, etc.)
  - Explain: "It's searching across all indexed documents simultaneously"

  **Question 4** (technical detail):
  > "What is the shooting logic for tanks?"
  
  - Show it retrieves specific technical details from the combat module
  - Emphasize: "This is pulling exact information from the design docs"

### 5. **Technical Highlights** (1 minute)
- Open `http://localhost:8000/docs` (FastAPI Swagger UI)
- Show the API endpoints:
  - `/documents/gdd` - Upload & index
  - `/chat` - RAG-powered Q&A
  - `/documents` - List all indexed docs
- Explain: "This is a production-ready REST API that can be integrated anywhere"

### 6. **Wrap Up** (30 seconds)
- "This system can scale to hundreds of documents"
- "It's already deployed on Render (backend) and ready for Vercel (frontend)"
- "The RAG pipeline uses Qwen LLM and vector embeddings for accurate retrieval"

---

## Before the Demo: Checklist

### ✅ Technical Setup
- [ ] Backend running: `python3 -m uvicorn backend_api.main:app --reload --port 8000`
- [ ] Frontend running: `cd frontend && npm run dev`
- [ ] Verify `http://localhost:8000/health` returns `{"status":"ok"}`
- [ ] Verify `http://localhost:3000` loads without errors
- [ ] Check `/documents` page shows your indexed GDDs

### ✅ Demo Data
- [ ] Have at least 3-5 GDDs already indexed (you have 50+, so this is done)
- [ ] Know which doc_ids exist (check `/documents` page)
- [ ] Have one small GDD PDF ready to upload during demo (optional, but impressive)

### ✅ Browser Prep
- [ ] Open `http://localhost:3000` in a clean browser window
- [ ] Clear browser console errors (F12 → Console, check for red)
- [ ] Test one chat question beforehand to ensure it works
- [ ] Have `http://localhost:8000/docs` ready in another tab

### ✅ Talking Points Ready
- [ ] Know what "RAG" means (Retrieval-Augmented Generation)
- [ ] Be ready to explain: "It searches your documents first, then generates answers"
- [ ] Have examples of questions that work well vs. ones that don't

---

## Demo Tips

### Do's ✅
- **Start with the problem**: Why manual search is painful
- **Show the UI first**: Clean, modern interface impresses
- **Ask real questions**: Use actual game design questions you care about
- **Point out speed**: "This would take 10 minutes of PDF searching, but here it's instant"
- **Show the API docs**: Demonstrates it's a real backend, not just a UI

### Don'ts ❌
- Don't ask questions that require code (unless you've indexed code too)
- Don't upload a huge document during the demo (do it beforehand)
- Don't get stuck on errors—have a backup plan (refresh, restart backend)
- Don't explain every technical detail—focus on value

---

## Backup Plan (If Something Breaks)

1. **Chat not working?**
   - Check backend logs: `tail -f` the uvicorn output
   - Verify `http://localhost:8000/health` works
   - Hard refresh browser (Cmd+Shift+R)

2. **No documents showing?**
   - Check `rag_storage/kv_store_doc_status.json` exists
   - Verify backend can read it (check logs)
   - Show the API directly: `curl http://localhost:8000/documents`

3. **Slow responses?**
   - Explain: "This is running locally; production would be faster"
   - Or: "The first question warms up the model; subsequent ones are faster"

---

## Key Value Propositions to Emphasize

1. **Time Savings**: "Instead of 10 minutes searching PDFs, get answers in seconds"
2. **Accuracy**: "Answers come directly from your design docs, not generic knowledge"
3. **Scalability**: "Works with 5 docs or 500 docs"
4. **Production-Ready**: "REST API + modern frontend, ready to deploy"
5. **Extensible**: "Can add code coverage, requirement tracking, etc."

---

## Post-Demo Questions (Be Ready)

**Q: "How does it know which documents to search?"**  
A: "It uses semantic similarity—embeds your question and finds the most relevant chunks across all indexed documents."

**Q: "What if the answer is wrong?"**  
A: "The system shows source chunks, so you can verify. It only uses your indexed content, not external knowledge."

**Q: "Can it handle code too?"**  
A: "Yes—we've already indexed the Unity codebase in batches. The coverage evaluation feature compares GDD requirements to actual code implementation."

**Q: "How do you deploy this?"**  
A: "Backend is on Render, frontend can go on Vercel. The RAG storage can be on cloud storage (S3, etc.) for production."

---

## Quick Start Commands

```bash
# Terminal 1: Backend
cd /Users/madeinheaven/Documents/GitHub/AI_Agent
python3 -m uvicorn backend_api.main:app --reload --port 8000

# Terminal 2: Frontend
cd /Users/madeinheaven/Documents/GitHub/AI_Agent/frontend
npm run dev

# Then open: http://localhost:3000
```

---

Good luck with your demo! 🚀

