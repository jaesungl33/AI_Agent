#!/bin/bash
# Index a codebase for code QA
# Based on code_qa implementation

if [ $# -eq 0 ]; then
    echo "Usage: $0 <absolute_path_to_codebase>"
    echo "Example: $0 /Users/username/projects/my-app"
    exit 1
fi

CODEBASE_PATH="$1"

# Check if path exists
if [ ! -d "$CODEBASE_PATH" ]; then
    echo "Error: Directory $CODEBASE_PATH does not exist"
    exit 1
fi

# Convert to absolute path
ABSOLUTE_PATH=$(cd "$CODEBASE_PATH" && pwd)

echo "Indexing codebase: $ABSOLUTE_PATH"
echo "This may take a while for large codebases..."
echo ""

# Make API call to index the codebase
curl -X POST "http://localhost:8000/codeqa/index" \
     -H "Content-Type: application/json" \
     -d "{\"codebase_path\": \"$ABSOLUTE_PATH\", \"force_reindex\": false}" \
     | jq .


