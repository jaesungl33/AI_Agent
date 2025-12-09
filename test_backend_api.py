#!/usr/bin/env python3
"""
Quick HTTP test to verify the backend API endpoint is working.
Tests the /coverage/evaluate endpoint directly.
"""

import requests
import json
import sys
from pathlib import Path

BACKEND_URL = "http://127.0.0.1:8000"

def test_backend_health():
    """Test if backend is running."""
    print("=" * 60)
    print("BACKEND API QUICK TEST")
    print("=" * 60)
    
    print("\n[1/3] Testing backend health...")
    try:
        response = requests.get(f"{BACKEND_URL}/health", timeout=5)
        if response.status_code == 200:
            print(f"✅ Backend is running: {response.json()}")
            return True
        else:
            print(f"❌ Backend returned status {response.status_code}")
            return False
    except requests.exceptions.ConnectionError:
        print(f"❌ Cannot connect to backend at {BACKEND_URL}")
        print("   Make sure the backend is running:")
        print("   python3 -m uvicorn backend_api.main:app --reload --port 8000")
        return False
    except Exception as e:
        print(f"❌ Error: {e}")
        return False

def test_list_documents():
    """Test document listing."""
    print("\n[2/3] Testing document listing...")
    try:
        response = requests.get(f"{BACKEND_URL}/documents", timeout=10)
        if response.status_code == 200:
            docs = response.json()
            gdd_docs = [d for d in docs if d.get("type") == "gdd"]
            code_docs = [d for d in docs if d.get("type") == "code"]
            print(f"✅ Found {len(gdd_docs)} GDD(s) and {len(code_docs)} code batch(es)")
            
            if not gdd_docs or not code_docs:
                print("⚠️  Warning: Need at least 1 GDD and 1 code batch for evaluation")
                return None, None
            
            return gdd_docs[0]["id"], code_docs[0]["id"]
        else:
            print(f"❌ Failed to list documents: {response.status_code}")
            return None, None
    except Exception as e:
        print(f"❌ Error: {e}")
        return None, None

def test_coverage_evaluate(gdd_id, code_id):
    """Test coverage evaluation with minimal request."""
    print("\n[3/3] Testing coverage evaluation endpoint...")
    print(f"   GDD: {gdd_id}")
    print(f"   Code: {code_id}")
    print("   This will take 2-3 minutes (evaluating 5 requirements)...")
    
    payload = {
        "docId": gdd_id,
        "codeIndexId": code_id,
        "topK": 5  # Small top_k for speed
    }
    
    try:
        # Use a longer timeout for the evaluation
        response = requests.post(
            f"{BACKEND_URL}/coverage/evaluate",
            json=payload,
            timeout=600  # 10 minutes timeout
        )
        
        if response.status_code == 200:
            data = response.json()
            report = data.get("report", {})
            summary = report.get("summary", {})
            
            print("\n" + "=" * 60)
            print("✅ EVALUATION SUCCESSFUL!")
            print("=" * 60)
            print(f"\nTotal Items: {summary.get('totalItems', 0)}")
            print(f"Implemented: {summary.get('implemented', 0)}")
            print(f"Partially Implemented: {summary.get('partiallyImplemented', 0)}")
            print(f"Not Implemented: {summary.get('notImplemented', 0)}")
            print(f"Errors: {summary.get('errors', 0)}")
            
            if data.get("warnings"):
                print(f"\n⚠️  Warnings: {len(data['warnings'])}")
                for warning in data["warnings"][:3]:
                    print(f"   - {warning}")
            
            return True
        else:
            print(f"❌ Evaluation failed: {response.status_code}")
            try:
                error_data = response.json()
                print(f"   Error: {error_data.get('detail', error_data.get('message', 'Unknown error'))}")
            except:
                print(f"   Response: {response.text[:200]}")
            return False
            
    except requests.exceptions.Timeout:
        print("❌ Request timed out (exceeded 10 minutes)")
        print("   The evaluation may still be running on the server.")
        return False
    except requests.exceptions.ConnectionError:
        print(f"❌ Connection lost during evaluation")
        print("   The backend may have crashed or become unresponsive.")
        return False
    except Exception as e:
        print(f"❌ Error: {e}")
        return False

def main():
    """Run all tests."""
    # Test 1: Health check
    if not test_backend_health():
        print("\n❌ Backend health check failed. Cannot continue.")
        sys.exit(1)
    
    # Test 2: List documents
    gdd_id, code_id = test_list_documents()
    if not gdd_id or not code_id:
        print("\n❌ Cannot find required documents. Cannot test evaluation.")
        sys.exit(1)
    
    # Test 3: Coverage evaluation
    success = test_coverage_evaluate(gdd_id, code_id)
    
    if success:
        print("\n✅ All tests passed! Backend API is working correctly.")
        sys.exit(0)
    else:
        print("\n❌ Coverage evaluation test failed.")
        sys.exit(1)

if __name__ == "__main__":
    main()

