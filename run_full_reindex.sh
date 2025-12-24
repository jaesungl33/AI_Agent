#!/bin/bash
# Complete reindexing script for GDD documents and code

set -e  # Exit on any error

echo "🚀 STARTING COMPLETE REINDEXING PROCESS"
echo "========================================"

# Check if API key is set
if [[ -z "$DASHSCOPE_API_KEY" && -z "$QWEN_API_KEY" ]]; then
    echo "❌ ERROR: No API key found!"
    echo "   Set DASHSCOPE_API_KEY or QWEN_API_KEY environment variable"
    echo "   Example: export DASHSCOPE_API_KEY='your_key_here'"
    exit 1
fi

echo "✅ API key found"

# Check if documents exist
if [ ! -d "data/gdd_documents" ]; then
    echo "❌ ERROR: data/gdd_documents directory not found!"
    exit 1
fi

echo "✅ GDD documents directory found"

# Count documents
PDF_COUNT=$(find data/gdd_documents -name "*.pdf" | wc -l)
TXT_COUNT=$(find data/gdd_documents -name "*.txt" | wc -l)
CSV_COUNT=$(find data/gdd_documents -name "*.csv" | wc -l)
TOTAL_DOCS=$((PDF_COUNT + TXT_COUNT + CSV_COUNT))

echo "📄 Found $TOTAL_DOCS documents to index ($PDF_COUNT PDFs, $TXT_COUNT text, $CSV_COUNT CSV)"

echo ""
echo "🔄 PHASE 1: Indexing GDD Documents"
echo "-----------------------------------"
python3 scripts/testing/process_all_docs.py --force --parser docling

echo ""
echo "💻 PHASE 2: Indexing Code Files"
echo "-------------------------------"
python3 scripts/migration/index_codebase.py --workspace tank_war --force

echo ""
echo "🔄 PHASE 3: Reindexing Vectors"
echo "------------------------------"
python3 backend/scripts/reindex_vectors.py --workspace tank_war

echo ""
echo "========================================"
echo "🎉 REINDEXING COMPLETE!"
echo "========================================"
echo ""
echo "📊 Summary:"
echo "   - GDD Documents indexed: $TOTAL_DOCS"
echo "   - Code files indexed: Check tank_war workspace"
echo "   - Vectors reindexed: ✓"
echo ""
echo "🔄 Next steps:"
echo "   - Restart your backend: python3 backend/simple.py"
echo "   - Refresh your frontend at http://localhost:3000"
echo "   - Your tank_war workspace should now have fresh indexed data!"
