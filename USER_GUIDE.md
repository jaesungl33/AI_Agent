# User Guide: Getting Started with GDD RAG Backbone

## 🎯 What is This Tool?

**GDD RAG Backbone** is an AI-powered system that helps you:
- **Process Game Design Documents (GDDs)**: Upload PDFs/DOCX files and automatically extract structured information
- **Ask Questions**: Use natural language to query your documents
- **Extract Requirements**: Automatically identify objects, systems, logic rules, and requirements
- **Check Code Coverage**: Compare your GDD requirements against your actual codebase to see what's implemented

## 📋 Table of Contents

1. [Installation](#installation)
2. [Quick Start](#quick-start)
3. [Using the Streamlit Web Interface](#using-the-streamlit-web-interface)
4. [Indexing Your Codebase](#indexing-your-codebase)
5. [Complete Workflow Example](#complete-workflow-example)
6. [Troubleshooting](#troubleshooting)

---

## 🔧 Installation

### Step 1: Prerequisites

- **Python 3.10 or higher** (check with `python --version`)
- **pip** package manager
- **API Key** for Qwen/DashScope (get one from [Alibaba Cloud DashScope](https://dashscope.aliyun.com/))

### Step 2: Clone/Navigate to Project

```bash
cd /path/to/AI_Agent
```

### Step 3: Install Dependencies

```bash
pip install -r requirements.txt
```

This installs:
- RAG framework for document processing
- Streamlit web UI framework
- LLM provider SDKs
- Data processing tools

### Step 4: Configure API Key

**Option A: Using .env file (Recommended)**

1. Copy the template file:
   ```bash
   cp env.template .env
   ```

2. Edit `.env` and add your API key:
   ```bash
   QWEN_API_KEY=your_actual_api_key_here
   ```

**Option B: Set environment variable**

```bash
# On Linux/Mac
export QWEN_API_KEY="your-api-key-here"

# On Windows (PowerShell)
$env:QWEN_API_KEY="your-api-key-here"
```

### Step 5: Verify Installation

```bash
python -c "import streamlit; import dashscope; print('✅ All dependencies installed!')"
```

---

## 🚀 Quick Start

### Launch the Web Interface

```bash
streamlit run ui/app.py
```

You should see output like:
```
You can now view your Streamlit app in your browser.
Local URL: http://localhost:8501
```

**Open your browser** to the local URL (usually `http://localhost:8501`)

---

## 💻 Using the Streamlit Web Interface

The interface has **4 main sections** accessible from the sidebar that guide you through the workflow:

### 1. GDD & Indexing

**Purpose**: Upload and index your Game Design Documents

#### Uploading a New Document

1. **Select "Upload new document"** from the radio buttons
2. **Click "Choose a file"** and select your PDF/DOCX document
3. **Enter a Document ID** (e.g., `tank_war_gdd`, `my_game_design`)
   - Use lowercase, underscores, no spaces
   - This ID will be used to reference the document later
4. **Click "Index Document"**
   - This may take 2-5 minutes depending on document size
   - The system will:
     - Parse the document
     - Break it into chunks
     - Generate embeddings
     - Index it for querying

#### Re-indexing an Existing Document

1. **Select "Select indexed document"** from the radio buttons
2. **Choose a document** from the dropdown
3. **Click "Re-index Document"** to reprocess it
4. Useful if you updated the document or want to refresh the index

---

### 2. GDD Explorer & Analysis

**Purpose**: Explore your documents and ask questions

#### High-level Analysis

1. **Select a document** from the sidebar dropdown (or leave empty to analyze all)
2. **Scroll down to the Analysis section**
3. **Click "Generate Analysis"**
   - Generates a comprehensive summary of the game design
   - Shows genre, core loop, major systems, etc.

#### Ask Questions

1. **Select a document** from the sidebar (or query across all documents)
2. **Type your question** in the text box
   - Examples:
     - "What tanks are in the game?"
     - "Explain the monetization system"
     - "What are the game modes?"
3. **Adjust settings** (optional):
   - Number of chunks to retrieve
   - Query mode
4. **Click "Ask Question"** to get an answer
5. **View retrieved chunks** below the answer to see source information

---

### 3. Requirements & To-Do

**Purpose**: View extracted requirements and generate developer tasks

#### View Extracted Data

1. **Select a document** from the sidebar dropdown
2. **Click "Extract All Requirements"** to run structured extraction
   - Extracts objects, systems, logic rules, and requirements
   - Results are saved in `checklists/<doc_id>_game_spec.json`
3. **View the extracted data**:
   - **Objects**: Game objects (tanks, items, etc.)
   - **Systems**: Game systems (combat, progression, etc.)
   - **Logic Rules**: Game rules and interactions
   - **Requirements**: Functional requirements

#### Generate Developer To-Do List

1. **Ensure a document is selected** and has extracted requirements
2. **Scroll to the To-Do section**
3. **Click "Generate To-Do List"**
   - Creates a task list based on the extracted requirements
   - Useful for breaking down work
   - Saved to `checklists/<doc_id>_todo.json`

---

### 4. Code Coverage

**Purpose**: Check which requirements are implemented in your codebase

#### Prerequisites

Before using this section, you need to **index your codebase** first (see [Indexing Your Codebase](#indexing-your-codebase))

#### Running Coverage Check

1. **Select a Document** from the sidebar (the GDD you want to check)
2. **Enter Code Index ID** (e.g., `tank_online_codebase`)
   - This is the ID you used when indexing your codebase
3. **Adjust Settings** (optional):
   - **Top K**: Number of code chunks to retrieve per query (default: 8)
4. **Click "Evaluate Coverage"**
   - This may take several minutes for large Game Specs
   - Processes all requirements from the selected document
5. **Review Results**:
   - See summary of implemented vs not implemented requirements
   - Click on individual requirements to see detailed evidence
   - View retrieved code chunks that match each requirement

#### Understanding Results

- **Status Types**:
  - `implemented`: Found relevant code
  - `not_implemented`: No matching code found
  - `error`: Evaluation failed (check logs)
- **Evidence**: Shows file paths and reasons for classification
- **Chunk References**: Actual code snippets that match

---

## 📦 Indexing Your Codebase

To check code coverage, you first need to index your codebase.

### Option 1: Using the Command Line Script

For Unity/C# projects:

```bash
python index_tank_online_codebase.py \
    --source ./tank_online_1-dev \
    --doc-id tank_online_codebase \
    --batch-size 50
```

**Parameters**:
- `--source`: Path to your codebase directory
- `--doc-id`: ID to use for the indexed codebase (use this in Coverage tab)
- `--batch-size`: Number of files per batch (default: 50)

The script will:
1. Scan for code files (.cs, .shader, .json, etc.)
2. Create text snapshots (in batches to avoid memory issues)
3. Index each batch into the RAG system

**Example**: If you have 200 files and batch-size=50, you'll get 4 indexed documents:
- `tank_online_codebase_batch001`
- `tank_online_codebase_batch002`
- `tank_online_codebase_batch003`
- `tank_online_codebase_batch004`

**In the Coverage tab**, you can use:
- Single batch: `tank_online_codebase_batch001`
- Or query all batches by selecting multiple (if supported) or indexing them separately

### Option 2: Using the Streamlit UI (Future)

The UI may support codebase indexing in the future. For now, use the command-line script.

---

## 📖 Complete Workflow Example

Here's a typical workflow from start to finish:

### Step 1: Index Your GDD

1. Launch the app: `streamlit run ui/app.py`
2. Go to **Section 1: GDD & Indexing** (in sidebar)
3. Select "Upload new document"
4. Upload your GDD PDF/DOCX
5. Enter document ID: `my_game_gdd`
6. Click "Index Document"
7. Wait 2-5 minutes for indexing to complete

### Step 2: Explore the GDD

1. Go to **Section 2: GDD Explorer & Analysis** (in sidebar)
2. Select `my_game_gdd` from the dropdown
3. Click "Generate Analysis" to get a summary
4. Ask questions like:
   - "What are the main game systems?"
   - "Explain the combat mechanics"
   - "What tanks are available?"

### Step 3: View Extracted Requirements

1. Go to **Section 3: Requirements & To-Do** (in sidebar)
2. Select `my_game_gdd` from the dropdown
3. Click "Extract All Requirements"
4. View the extracted:
   - Objects
   - Systems
   - Logic Rules
   - Requirements
5. Generate a to-do list for developers

### Step 4: Index Your Codebase

```bash
python index_tank_online_codebase.py \
    --source ./my_game_code \
    --doc-id my_game_codebase
```

Wait for indexing to complete (may take 10-30 minutes for large codebases).

### Step 5: Check Code Coverage

1. Go to **Section 4: Code Coverage** (in sidebar)
2. Select document: `my_game_gdd`
3. Enter code index: `my_game_codebase`
4. Click "Evaluate Coverage"
5. Review which requirements are implemented vs missing
6. Click individual requirements to see evidence

---

## 🔍 File Locations

After using the tool, you'll find:

- **Indexed Documents**: Stored in `docs/` directory
- **RAG Storage**: All embeddings and chunks in `rag_storage/`
- **Game Specs**: Saved in `checklists/<doc_id>_game_spec.json`
- **To-Do Lists**: Saved in `checklists/<doc_id>_todo.json`
- **Coverage Reports**: Saved in `reports/coverage_checks/<doc_id>_<code_index>_coverage.json`
- **Code Snapshots**: Codebase snapshots in `docs/` (`.txt` files)

---

## ❓ Troubleshooting

### App Won't Start

**Problem**: `ModuleNotFoundError` or import errors

**Solution**:
```bash
pip install -r requirements.txt --upgrade
```

### API Key Errors

**Problem**: "API key not found" or authentication errors

**Solution**:
1. Check that your `.env` file exists and contains `QWEN_API_KEY=...`
2. Or set environment variable: `export QWEN_API_KEY="your-key"`
3. Restart the app

### Indexing Takes Too Long

**Problem**: Document indexing takes 10+ minutes

**Solutions**:
- This is normal for large documents (100+ pages)
- Check your internet connection (embeddings require API calls)
- Reduce document size if possible

### Code Coverage Very Slow

**Problem**: Coverage evaluation takes hours

**Solutions**:
1. **Reduce Max Concurrent Items** to 3 or lower
2. **Reduce Chunks per Query** to 4-6
3. The recent optimization should make it 7-8x faster - make sure you have the latest version

### No Documents Found

**Problem**: Dropdown shows "No documents available"

**Solution**:
1. Make sure you've indexed at least one document in Tab 1
2. Click the "🔄 Refresh List" button
3. Check that `rag_storage/` directory exists and has files

### Coverage Shows "Error" Status

**Problem**: Many items show "error" status in coverage results

**Solutions**:
1. Check that your code index ID is correct
2. Verify the codebase was indexed successfully
3. Check the browser console or terminal for error messages
4. Try reducing "Max Concurrent Items" to avoid rate limits

### Can't Find Codebase Index

**Problem**: Coverage tab says code index not found

**Solution**:
1. Make sure you ran the indexing script first
2. Check the exact doc-id you used (case-sensitive)
3. The code index ID should match what you used in `--doc-id` parameter

---

## 💡 Tips & Best Practices

1. **Use Descriptive Document IDs**: 
   - Good: `tank_war_v2_gdd`, `combat_system_spec`
   - Bad: `doc1`, `test`, `gdd`

2. **Batch Large Codebases**: 
   - For 500+ files, use batch-size=50 to avoid memory issues
   - Each batch becomes a separate index you can query

3. **Start Small**: 
   - Test with a small GDD first to understand the workflow
   - Then scale up to larger documents

4. **Check Coverage Regularly**: 
   - Re-run coverage checks as you implement features
   - The system caches results for faster re-runs

5. **Use Questions Effectively**:
   - Be specific: "What is the damage calculation formula?" vs "Explain damage"
   - Ask follow-up questions based on previous answers

6. **Monitor API Usage**: 
   - Each indexing and coverage check uses API calls
   - Check your DashScope dashboard to monitor costs

---

## 🆘 Getting Help

- **Check Logs**: Look at terminal output for detailed error messages
- **Review File Structure**: Ensure all directories exist (`docs/`, `rag_storage/`, etc.)
- **Test API Key**: Try a simple Python script to verify your API key works
- **Check Network**: Ensure you can reach DashScope API endpoints

---

## 📚 Next Steps

- Explore the extracted Game Specs in `checklists/`
- Export coverage reports for documentation
- Use the to-do lists to track development progress
- Query your documents to answer specific design questions

Happy analyzing! 🎮✨

