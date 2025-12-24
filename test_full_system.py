#!/usr/bin/env python3
"""
Test the complete Tank War system with OpenAI
"""

import os
import sys
import time
import requests
import subprocess
import signal

def test_openai_backend():
    """Test backend startup with OpenAI"""
    print("🚀 Testing Tank War Backend with OpenAI")
    print("=" * 50)

    # Check if OpenAI key is available
    openai_key = os.getenv("OPENAI_API_KEY")
    if not openai_key:
        print("❌ OPENAI_API_KEY not found in environment")
        print("Set it with: export OPENAI_API_KEY=your-key-here")
        return False

    print(f"✅ OpenAI API key found (length: {len(openai_key)})")

    # Start backend in background
    print("\n🔧 Starting backend...")
    try:
        backend_process = subprocess.Popen([
            sys.executable, "run_backend.py"
        ], stdout=subprocess.PIPE, stderr=subprocess.PIPE)

        # Wait for backend to start
        time.sleep(3)

        # Check if backend is running
        try:
            response = requests.get("http://localhost:8000/health", timeout=5)
            if response.status_code == 200:
                print("✅ Backend started successfully on port 8000")
            else:
                print(f"❌ Backend health check failed: {response.status_code}")
                return False
        except Exception as e:
            print(f"❌ Backend not responding: {e}")
            return False

        # Test chat endpoint
        print("\n💬 Testing chat endpoint...")
        test_payload = {
            "message": "@docs what is the diamond use for",
            "workspaceId": "tank_war"
        }

        try:
            response = requests.post("http://localhost:8000/api/chat",
                                   json=test_payload, timeout=30)

            if response.status_code == 200:
                result = response.json()
                message = result.get("message", {})
                content = message.get("content", "")

                print("✅ Chat request successful!")
                print(f"Response: {content[:200]}...")

                if len(content) > 10 and "diamond" in content.lower():
                    print("✅ Diamond query working - OpenAI LLM responding!")
                    return True
                else:
                    print("⚠️  Chat responded but content seems incomplete")
                    return False
            else:
                print(f"❌ Chat request failed: {response.status_code}")
                print(f"Response: {response.text}")
                return False

        except Exception as e:
            print(f"❌ Chat test failed: {e}")
            return False

    finally:
        # Clean up backend process
        try:
            backend_process.terminate()
            backend_process.wait(timeout=5)
            print("\n🧹 Backend process terminated")
        except:
            try:
                backend_process.kill()
            except:
                pass

def main():
    """Main test function"""
    print("🎯 Tank War Full System Test with OpenAI")
    print("=" * 50)

    success = test_openai_backend()

    if success:
        print("\n🎉 SUCCESS! Your Tank War system is working with OpenAI!")
        print("\n🚀 To run the full system:")
        print("1. Backend: python3 run_backend.py")
        print("2. Frontend: cd frontend && npm run dev")
        print("3. Open http://localhost:3000")
        print("4. Ask: '@docs what is the diamond use for'")
    else:
        print("\n❌ System test failed. Check your OpenAI API key and try again.")

if __name__ == "__main__":
    main()
