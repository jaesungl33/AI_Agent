#!/bin/bash

# Quick Vercel Deployment Script
# This script helps you deploy to Vercel

echo "🚀 Vercel Deployment Helper"
echo ""

# Check if Vercel CLI is installed
if ! command -v vercel &> /dev/null; then
    echo "📦 Installing Vercel CLI..."
    npm install -g vercel
    echo ""
fi

# Check if logged in
if ! vercel whoami &> /dev/null; then
    echo "🔐 Please login to Vercel:"
    vercel login
    echo ""
fi

echo "📁 Deploying from frontend directory..."
echo ""

# Ask for deployment type
read -p "Deploy to production? (y/n): " -n 1 -r
echo ""

if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "🚀 Deploying to production..."
    vercel --prod
else
    echo "🧪 Deploying to preview..."
    vercel
fi

echo ""
echo "✅ Deployment complete!"
echo ""
echo "💡 Next steps:"
echo "   1. Set environment variables in Vercel dashboard"
echo "   2. Add NEXT_PUBLIC_API_URL with your backend URL"
echo "   3. Visit your deployment URL"

