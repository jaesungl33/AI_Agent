#!/bin/bash
# Start the backend server

cd "$(dirname "$0")"

echo "🚀 Starting backend server..."
echo "   Port: 8000"
echo "   Host: 0.0.0.0 (accessible from all interfaces)"
echo ""

# Kill any existing backend on port 8000
lsof -ti:8000 | xargs kill -9 2>/dev/null
sleep 1

# Start backend
python3 -m uvicorn backend_api.main:app --reload --port 8000 --host 0.0.0.0

