#!/usr/bin/env python3
"""
Test script to verify API key format and try alternative authentication methods.
"""
import sys
import os
from pathlib import Path

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).parent.parent.parent))

from gdd_rag_backbone.config import QWEN_API_KEY


def test_api_key_format():
    """Test API key format and try different authentication methods."""
    print("\n" + "=" * 80)
    print("🔍 API Key Format & Authentication Test")
    print("=" * 80)
    print()
    
    if not QWEN_API_KEY:
        print("❌ ERROR: QWEN_API_KEY is not set!")
        sys.exit(1)
    
    # Check key format
    print("📋 API Key Information:")
    print("-" * 80)
    print(f"  Length: {len(QWEN_API_KEY)} characters")
    print(f"  Starts with 'sk-': {QWEN_API_KEY.startswith('sk-')}")
    print(f"  First 10 chars: {QWEN_API_KEY[:10]}...")
    print(f"  Last 4 chars: ...{QWEN_API_KEY[-4:]}")
    print()
    
    try:
        from dashscope import Generation
        
        # Try method 1: Using api_key parameter
        print("🔧 Testing Method 1: api_key parameter...")
        try:
            response = Generation.call(
                model="qwen-turbo",
                messages=[{"role": "user", "content": "Hi"}],
                api_key=QWEN_API_KEY,
                result_format='message',
                max_tokens=10,
            )
            if response.status_code == 200:
                print("  ✅ SUCCESS with api_key parameter!")
                return True
            else:
                print(f"  ❌ FAILED: {response.status_code} - {response.message}")
        except Exception as e:
            print(f"  ❌ ERROR: {e}")
        
        # Try method 2: Using DASHSCOPE_API_KEY environment variable
        print("\n🔧 Testing Method 2: DASHSCOPE_API_KEY environment variable...")
        try:
            # Temporarily set environment variable
            original_env = os.environ.get("DASHSCOPE_API_KEY")
            os.environ["DASHSCOPE_API_KEY"] = QWEN_API_KEY
            
            response = Generation.call(
                model="qwen-turbo",
                messages=[{"role": "user", "content": "Hi"}],
                result_format='message',
                max_tokens=10,
            )
            
            # Restore original
            if original_env:
                os.environ["DASHSCOPE_API_KEY"] = original_env
            elif "DASHSCOPE_API_KEY" in os.environ:
                del os.environ["DASHSCOPE_API_KEY"]
            
            if response.status_code == 200:
                print("  ✅ SUCCESS with DASHSCOPE_API_KEY environment variable!")
                print("\n💡 SOLUTION: Use DASHSCOPE_API_KEY instead of QWEN_API_KEY in .env")
                return True
            else:
                print(f"  ❌ FAILED: {response.status_code} - {response.message}")
        except Exception as e:
            print(f"  ❌ ERROR: {e}")
            # Restore original
            if original_env:
                os.environ["DASHSCOPE_API_KEY"] = original_env
        
        # Try method 3: Check if key needs to be set via dashscope
        print("\n🔧 Testing Method 3: dashscope.api_key setting...")
        try:
            import dashscope
            original_dashscope_key = getattr(dashscope, 'api_key', None)
            dashscope.api_key = QWEN_API_KEY
            
            response = Generation.call(
                model="qwen-turbo",
                messages=[{"role": "user", "content": "Hi"}],
                result_format='message',
                max_tokens=10,
            )
            
            # Restore
            if original_dashscope_key:
                dashscope.api_key = original_dashscope_key
            elif hasattr(dashscope, 'api_key'):
                delattr(dashscope, 'api_key')
            
            if response.status_code == 200:
                print("  ✅ SUCCESS with dashscope.api_key setting!")
                return True
            else:
                print(f"  ❌ FAILED: {response.status_code} - {response.message}")
        except Exception as e:
            print(f"  ❌ ERROR: {e}")
        
        print("\n" + "=" * 80)
        print("❌ ALL METHODS FAILED")
        print("=" * 80)
        print("\n💡 Troubleshooting:")
        print("  1. Verify your API key is correct in DashScope console")
        print("  2. Make sure your API key is active and not expired")
        print("  3. Check if your account has access to Qwen models")
        print("  4. Try using DASHSCOPE_API_KEY instead of QWEN_API_KEY")
        print("  5. Ensure your API key format is correct (usually starts with 'sk-')")
        print()
        
        return False
        
    except ImportError:
        print("❌ ERROR: dashscope package not installed.")
        print("Install it with: pip install dashscope")
        return False


if __name__ == "__main__":
    test_api_key_format()

