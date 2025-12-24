#!/bin/bash
# Start all services for Code QA testing

echo "🚀 Starting Code QA Environment"
echo "==============================="

# Check if .env exists
if [ ! -f ".env" ]; then
    echo "❌ .env file not found. Please create it with your DASHSCOPE_API_KEY"
    exit 1
fi

# Check if DASHSCOPE_API_KEY is set
if ! grep -q "DASHSCOPE_API_KEY" .env; then
    echo "❌ DASHSCOPE_API_KEY not found in .env file"
    exit 1
fi

echo "✅ Configuration looks good"

# Start Redis in background (if not already running)
echo "🔄 Starting Redis..."
redis-server --daemonize yes 2>/dev/null || echo "Redis already running or not installed"

# Wait a moment
sleep 2

# Start backend in background
echo "🚀 Starting Flask backend..."
cd backend && python3 fresh_backend.py &
BACKEND_PID=$!

# Wait for backend to start
sleep 3

# Start frontend in background (if frontend directory exists)
if [ -d "../frontend" ]; then
    echo "🎨 Starting frontend..."
    cd ../frontend && npm run dev &
    FRONTEND_PID=$!
else
    echo "⚠️  Frontend directory not found, skipping frontend startup"
    FRONTEND_PID=""
fi

echo ""
echo "🎉 All services started!"
echo "📍 Frontend: http://localhost:3000"
echo "📍 Backend API: http://localhost:8000"
echo "🌐 Code QA Interface: http://localhost:8000/codeqa"
echo ""
if [ -n "$FRONTEND_PID" ]; then
    echo "To stop all services, run: kill $BACKEND_PID $FRONTEND_PID"
else
    echo "To stop backend, run: kill $BACKEND_PID"
fi

# Wait for services
wait

