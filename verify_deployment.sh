#!/bin/bash
# Quick script to verify your deployment is working

echo "🔍 Checking Backend (Render)..."
BACKEND_URL="https://ai-agent-l90z.onrender.com"

# Health check
HEALTH=$(curl -s "$BACKEND_URL/health" 2>/dev/null)
if echo "$HEALTH" | grep -q "status.*ok"; then
    echo "✅ Backend health check: OK"
else
    echo "❌ Backend health check: FAILED"
    echo "   Response: $HEALTH"
fi

# Documents check
DOCS=$(curl -s "$BACKEND_URL/documents" 2>/dev/null)
DOC_COUNT=$(echo "$DOCS" | grep -o '"id"' | wc -l | tr -d ' ')
if [ "$DOC_COUNT" -gt 0 ]; then
    echo "✅ Backend documents: $DOC_COUNT docs found"
else
    echo "⚠️  Backend documents: No docs found (might be empty or error)"
fi

# Chat test
echo ""
echo "🔍 Testing Chat Endpoint..."
CHAT_RESPONSE=$(curl -s -X POST "$BACKEND_URL/chat" \
    -H "Content-Type: application/json" \
    -d '{"workspaceId":"default","message":"test","useAllDocs":true,"topK":2}' 2>/dev/null)

if echo "$CHAT_RESPONSE" | grep -q "content"; then
    echo "✅ Chat endpoint: Working"
    echo "   Response preview: $(echo "$CHAT_RESPONSE" | head -c 100)..."
else
    echo "❌ Chat endpoint: FAILED"
    echo "   Response: $CHAT_RESPONSE"
fi

echo ""
echo "📋 Summary:"
echo "   Backend URL: $BACKEND_URL"
echo "   Set NEXT_PUBLIC_API_URL in Vercel to: $BACKEND_URL"
echo ""
echo "💡 Next steps:"
echo "   1. Deploy frontend to Vercel"
echo "   2. Set NEXT_PUBLIC_API_URL=$BACKEND_URL in Vercel environment variables"
echo "   3. Test the full flow at your Vercel URL"

