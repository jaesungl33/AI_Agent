#!/usr/bin/env python3
"""
Test script using OpenAI-compatible API endpoint for Qwen models.

This uses the OpenAI client library with DashScope's compatible-mode endpoint.
"""
import sys
import os
from pathlib import Path
from dotenv import load_dotenv

sys.path.insert(0, str(Path(__file__).parent.parent.parent))

load_dotenv('.env')

try:
    from openai import OpenAI
except ImportError:
    print("❌ ERROR: openai package not installed.")
    print("Install it with: pip install openai")
    sys.exit(1)

def test_qwen_openai_compatible():
    """Test Qwen models using OpenAI-compatible API."""
    print("\n" + "=" * 80)
    print("🔍 Testing Qwen Models with OpenAI-Compatible API")
    print("=" * 80)
    print()
    
    # Get configuration
    api_key = os.getenv("DASHSCOPE_API_KEY", "").strip()
    region = os.getenv("REGION", "intl").strip().lower()
    
    if not api_key:
        print("❌ ERROR: DASHSCOPE_API_KEY not found in environment variables!")
        print("Set it in your .env file: DASHSCOPE_API_KEY=sk-your-key")
        sys.exit(1)
    
    # Set base URL based on region
    if region == "intl":
        base_url = "https://dashscope-intl.aliyuncs.com/compatible-mode/v1"
    elif region == "cn":
        base_url = "https://dashscope.aliyuncs.com/compatible-mode/v1"
    else:
        base_url = "https://dashscope-intl.aliyuncs.com/compatible-mode/v1"  # Default to INTL
        print(f"⚠️  Unknown region '{region}', defaulting to INTL")
    
    print(f"📋 Configuration:")
    print(f"   API Key: {api_key[:15]}...{api_key[-4:]}")
    print(f"   Region: {region}")
    print(f"   Base URL: {base_url}")
    print()
    
    # Initialize OpenAI client
    client = OpenAI(
        api_key=api_key,
        base_url=base_url,
    )
    
    # Test models
    models_to_test = [
        "qwen-max",
        "qwen-plus",
        "qwen-turbo",
    ]
    
    successful_models = []
    failed_models = []
    
    print("🧪 Testing models...")
    print("-" * 80)
    
    for model in models_to_test:
        print(f"\n[{len(successful_models) + len(failed_models) + 1}/{len(models_to_test)}] Testing: {model}")
        
        try:
            response = client.chat.completions.create(
                model=model,
                messages=[{"role": "user", "content": "Say 'Hello' if you can read this."}],
                max_tokens=20,
            )
            
            content = response.choices[0].message.content
            print(f"   ✅ SUCCESS!")
            print(f"   Response: {content[:100]}...")
            successful_models.append(model)
            
        except Exception as e:
            error_msg = str(e)
            print(f"   ❌ FAILED: {error_msg[:100]}")
            failed_models.append((model, error_msg))
    
    # Summary
    print("\n" + "=" * 80)
    print("📊 RESULTS SUMMARY")
    print("=" * 80)
    
    if successful_models:
        print(f"\n✅ SUCCESSFUL MODELS ({len(successful_models)}):")
        for model in successful_models:
            print(f"   ✓ {model}")
        
        print(f"\n💡 RECOMMENDED MODEL:")
        print(f"   → {successful_models[0]}")
        print(f"\n   Update your config.py or .env to use:")
        print(f"   DEFAULT_LLM_MODEL={successful_models[0]}")
    else:
        print("\n❌ NO MODELS WORKED")
        print("   All models failed. Check:")
        print("   1. API key is correct and active")
        print("   2. Region matches your account (intl vs cn)")
        print("   3. Account has access to Qwen models")
    
    if failed_models:
        print(f"\n❌ FAILED MODELS ({len(failed_models)}):")
        for model, error in failed_models:
            print(f"   ✗ {model}: {error[:80]}")
    
    print("\n" + "=" * 80)
    print()

if __name__ == "__main__":
    test_qwen_openai_compatible()

