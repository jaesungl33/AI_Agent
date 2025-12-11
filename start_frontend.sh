#!/bin/bash
# Start the frontend development server

cd "$(dirname "$0")/frontend"

echo "🎨 Starting frontend development server..."
echo "📍 Frontend will run on http://localhost:3000"
echo ""

npm run dev

