#!/usr/bin/env python3
"""
Detailed test to diagnose API key issues.
"""
import sys
import os
from pathlib import Path
from dotenv import load_dotenv

sys.path.insert(0, str(Path(__file__).parent.parent.parent))

load_dotenv('.env')

api_key = os.getenv('DASHSCOPE_API_KEY', '').strip()
region = os.getenv('REGION', '').strip()

print("\n" + "=" * 80)
print("Detailed API Key Diagnosis")
print("=" * 80)
print()

print("📋 Configuration:")
print(f"  API Key: {api_key[:15]}...{api_key[-4:] if len(api_key) > 19 else ''} (length: {len(api_key)})")
print(f"  Region: {region}")
print()

try:
    import dashscope
    from dashscope import Generation
    
    print("🔧 Testing different configurations:")
    print("-" * 80)
    
    # Test 1: Without setting region
    print("\n[Test 1] Without region setting...")
    dashscope.api_key = api_key
    if hasattr(dashscope, 'region'):
        delattr(dashscope, 'region')
    r1 = Generation.call(model='qwen-turbo', messages=[{'role': 'user', 'content': 'Hi'}], max_tokens=5)
    print(f"  Status: {r1.status_code} - {getattr(r1, 'message', '')}")
    
    # Test 2: With region='intl'
    print("\n[Test 2] With region='intl'...")
    dashscope.api_key = api_key
    dashscope.region = 'intl'
    r2 = Generation.call(model='qwen-turbo', messages=[{'role': 'user', 'content': 'Hi'}], max_tokens=5)
    print(f"  Status: {r2.status_code} - {getattr(r2, 'message', '')}")
    
    # Test 3: With region=None (unset)
    print("\n[Test 3] With region=None...")
    dashscope.api_key = api_key
    dashscope.region = None
    r3 = Generation.call(model='qwen-turbo', messages=[{'role': 'user', 'content': 'Hi'}], max_tokens=5)
    print(f"  Status: {r3.status_code} - {getattr(r3, 'message', '')}")
    
    # Test 4: With explicit api_key parameter
    print("\n[Test 4] With explicit api_key parameter (no global setting)...")
    if hasattr(dashscope, 'api_key'):
        delattr(dashscope, 'api_key')
    if hasattr(dashscope, 'region'):
        delattr(dashscope, 'region')
    r4 = Generation.call(model='qwen-turbo', messages=[{'role': 'user', 'content': 'Hi'}], api_key=api_key, max_tokens=5)
    print(f"  Status: {r4.status_code} - {getattr(r4, 'message', '')}")
    
    # Test 5: Check if DASHSCOPE_API_KEY env var works
    print("\n[Test 5] With DASHSCOPE_API_KEY environment variable...")
    os.environ['DASHSCOPE_API_KEY'] = api_key
    if 'DASHSCOPE_REGION' in os.environ:
        del os.environ['DASHSCOPE_REGION']
    if hasattr(dashscope, 'api_key'):
        delattr(dashscope, 'api_key')
    if hasattr(dashscope, 'region'):
        delattr(dashscope, 'region')
    r5 = Generation.call(model='qwen-turbo', messages=[{'role': 'user', 'content': 'Hi'}], max_tokens=5)
    print(f"  Status: {r5.status_code} - {getattr(r5, 'message', '')}")
    
    # Summary
    print("\n" + "=" * 80)
    print("📊 Summary:")
    print("=" * 80)
    
    results = [
        ("Without region", r1.status_code),
        ("With region='intl'", r2.status_code),
        ("With region=None", r3.status_code),
        ("Explicit api_key param", r4.status_code),
        ("DASHSCOPE_API_KEY env var", r5.status_code),
    ]
    
    success = [r for _, code in results if code == 200]
    failures = [r for _, code in results if code != 200]
    
    if success:
        print(f"\n✅ Successful configurations: {len(success)}")
        for name, code in results:
            if code == 200:
                print(f"   ✓ {name}")
    else:
        print(f"\n❌ All configurations failed with status 401")
        print("\n💡 Possible issues:")
        print("   1. API key might be invalid or expired")
        print("   2. API key might not have access to qwen-turbo model")
        print("   3. API key might be for a different region (CN vs INTL)")
        print("   4. Account might need to enable API access in console")
        print("   5. API key might need to be regenerated")
        print("\n   Please check your Alibaba Cloud DashScope console:")
        print("   https://dashscope.aliyun.com/")
        print("   - Verify the API key is correct")
        print("   - Check if API access is enabled")
        print("   - Verify account has access to Qwen models")
        print("   - Check if there are any IP whitelist restrictions")
    
    print()
    
except ImportError as e:
    print(f"❌ ERROR: {e}")
    print("Install dashscope: pip install dashscope")
except Exception as e:
    print(f"❌ ERROR: {e}")
    import traceback
    traceback.print_exc()

