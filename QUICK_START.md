# Quick Start Guide

Get up and running in 5 minutes!

## 1. Install

```bash
# Install dependencies
pip install -r requirements.txt

# Set up API key
cp env.template .env
# Edit .env and add: QWEN_API_KEY=your_key_here
```

## 2. Launch

```bash
streamlit run ui/app.py
```

Open `http://localhost:8501` in your browser (default Streamlit port).

## 3. Upload a GDD

1. Go to **Tab 1: GDD & Indexing**
2. Upload a PDF/DOCX file
3. Enter a document ID (e.g., `my_gdd`)
4. Click **"Upload & Index"**
5. Wait 2-5 minutes (extraction happens automatically!)

## 4. Ask Questions

1. Go to **Tab 2: GDD Explorer & Analysis**
2. Type a question (e.g., "What tanks are in the game?")
3. Click **"Send"**

## 5. Check Code Coverage (Optional)

1. First, index your codebase:
   ```bash
   python index_tank_online_codebase.py --source ./your_code --doc-id codebase
   ```

2. In the app, go to **Tab 4: Code Coverage**
3. Select your GDD document
4. Enter code index: `codebase`
5. Click **"Run Coverage Evaluation"**

---

**For detailed instructions, see [USER_GUIDE.md](USER_GUIDE.md)**

