#!/bin/bash
# Start the backend server

cd "$(dirname "$0")/backend"

# Add src to PYTHONPATH
export PYTHONPATH="${PWD}/../src:${PWD}/..:${PYTHONPATH}"

echo "🚀 Starting backend server..."
echo "📍 Backend will run on http://localhost:8000"
echo "📖 API docs: http://localhost:8000/docs"
echo ""

uvicorn main:app --reload --host 0.0.0.0 --port 8000

