#!/usr/bin/env python3
"""
Test the Tank War backend with Qwen LLM
"""

import os
import requests
import time
import sys

def test_backend():
    """Test the backend with diamond query"""

    print("🚀 Testing Tank War Backend with Qwen LLM")
    print("=" * 50)

    # Check if backend is running
    try:
        response = requests.get("http://localhost:8000/health", timeout=5)
        if response.status_code == 200:
            print("✅ Backend is running on port 8000")
        else:
            print(f"❌ Backend health check failed: {response.status_code}")
            return
    except Exception as e:
        print(f"❌ Backend not accessible: {e}")
        print("💡 Start the backend first: python3 backend/fresh_backend.py")
        return

    # Test the diamond query
    print("\n💎 Testing diamond query...")
    payload = {
        "message": "@docs what is the diamond use for",
        "workspaceId": "tank_war"
    }

    try:
        response = requests.post("http://localhost:8000/api/chat",
                               json=payload,
                               timeout=30)

        if response.status_code == 200:
            result = response.json()
            message = result.get("message", {})

            print("✅ Query successful!")
            print(f"Response: {message.get('content', 'No content')[:200]}...")

            if "diamond" in message.get('content', '').lower():
                print("✅ Diamond query working - LLM is responding!")
            else:
                print("⚠️  Response received but may not be from LLM")

        else:
            print(f"❌ Query failed: {response.status_code}")
            print(f"Response: {response.text}")

    except Exception as e:
        print(f"❌ Query error: {e}")

def start_backend():
    """Helper to start backend if needed"""
    print("🔧 To start the backend manually:")
    print("1. Open a new terminal window")
    print("2. Run: cd /Users/madeinheaven/Documents/GitHub/AI_Agent")
    print("3. Run: python3 backend/fresh_backend.py")
    print("4. Wait for 'INFO:__main__:Starting CodeQA server on port 8000'")
    print("5. Come back and run this test")

if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "--start":
        start_backend()
    else:
        test_backend()
