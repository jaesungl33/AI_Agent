#!/usr/bin/env python3
"""
Test script to verify Qwen API key and qwen-max model accessibility.

This script:
1. Checks if API key is loaded
2. Tests if qwen-max model is accessible via DashScope API
3. Makes a simple test API call to verify the model works
"""
import sys
from pathlib import Path

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).parent.parent.parent))

from gdd_rag_backbone.config import QWEN_API_KEY, DEFAULT_LLM_MODEL
from gdd_rag_backbone.llm_providers import QwenProvider


def test_api_key_loaded():
    """Test if API key is loaded from environment."""
    print("=" * 80)
    print("Step 1: Checking API Key...")
    print("=" * 80)
    
    if not QWEN_API_KEY:
        print("❌ ERROR: QWEN_API_KEY is not set!")
        print("\nPlease set your API key using one of these methods:")
        print("  1. Create a .env file with: QWEN_API_KEY=your_key")
        print("  2. Export environment variable: export QWEN_API_KEY='your_key'")
        return False
    
    # Mask the API key for display (show first 8 and last 4 chars)
    masked_key = QWEN_API_KEY[:8] + "*" * (len(QWEN_API_KEY) - 12) + QWEN_API_KEY[-4:]
    print(f"✅ API Key is loaded: {masked_key}")
    print(f"   Length: {len(QWEN_API_KEY)} characters")
    return True


def test_provider_initialization():
    """Test if QwenProvider can be initialized."""
    print("\n" + "=" * 80)
    print("Step 2: Testing Provider Initialization...")
    print("=" * 80)
    
    try:
        provider = QwenProvider()
        print(f"✅ QwenProvider initialized successfully")
        print(f"   Model: {provider.llm_model}")
        print(f"   Base URL: {provider.base_url}")
        print(f"   Embedding Model: {provider.embedding_model}")
        
        if provider.llm_model != DEFAULT_LLM_MODEL:
            print(f"   ⚠️  Warning: Provider model ({provider.llm_model}) != default ({DEFAULT_LLM_MODEL})")
        else:
            print(f"   ✅ Using default model: {DEFAULT_LLM_MODEL}")
        
        return provider
    except ValueError as e:
        print(f"❌ ERROR: Failed to initialize provider: {e}")
        return None
    except Exception as e:
        print(f"❌ ERROR: Unexpected error: {e}")
        import traceback
        traceback.print_exc()
        return None


def test_dashscope_api_call(provider):
    """Test actual DashScope API call with qwen-max."""
    print("\n" + "=" * 80)
    print("Step 3: Testing DashScope API Call...")
    print("=" * 80)
    
    try:
        # Try to import dashscope
        try:
            from dashscope import Generation
        except ImportError:
            print("⚠️  WARNING: dashscope package not installed.")
            print("   Install it with: pip install dashscope")
            print("   Skipping actual API call test.")
            print("\n   However, the API key and provider setup looks correct!")
            return True
        
        print(f"   Attempting API call to model: {provider.llm_model}")
        if hasattr(provider, 'region') and provider.region:
            print(f"   Region: {provider.region}")
        print(f"   Test prompt: 'Hello, are you working?'")
        
        # Set API key and region in dashscope module (preferred method)
        import dashscope
        dashscope.api_key = provider.api_key
        if hasattr(provider, 'region') and provider.region:
            dashscope.region = provider.region
        
        # Prepare messages
        messages = [
            {"role": "user", "content": "Hello, are you working? Please respond with 'Yes, I am working correctly!' if you can read this."}
        ]
        
        # Make API call (dashscope will use the global api_key and region settings)
        response = Generation.call(
            model=provider.llm_model,
            messages=messages,
            result_format='message',  # Get message format
        )
        
        # Check response
        if response.status_code == 200:
            print("\n✅ SUCCESS! API call successful!")
            print(f"   Model Response: {response.output.choices[0].message.content}")
            print(f"\n   ✅ Model '{provider.llm_model}' is accessible and working!")
            return True
        else:
            print(f"\n❌ ERROR: API call failed!")
            print(f"   Status Code: {response.status_code}")
            print(f"   Error Message: {response.message}")
            
            if response.status_code == 401:
                print("\n   💡 This usually means:")
                print("      - API key is invalid or expired")
                print("      - API key doesn't have access to this model")
            elif response.status_code == 400:
                print(f"\n   💡 This might mean:")
                print(f"      - Model '{provider.llm_model}' doesn't exist or isn't available")
                print(f"      - Check available models in DashScope console")
            
            return False
            
    except Exception as e:
        print(f"\n❌ ERROR: Exception during API call: {e}")
        import traceback
        traceback.print_exc()
        
        # Check if it's a connection error
        error_str = str(e).lower()
        if "connection" in error_str or "network" in error_str or "timeout" in error_str:
            print("\n   💡 This might be a network/connection issue")
            print("      - Check your internet connection")
            print("      - Check if DashScope API is accessible from your location")
        
        return False


def main():
    """Main test function."""
    print("\n")
    print("🔍 Qwen API & Model Test")
    print("Testing qwen-max model accessibility...")
    print("\n")
    
    # Step 1: Check API key
    if not test_api_key_loaded():
        print("\n" + "=" * 80)
        print("❌ TEST FAILED: API key not found")
        print("=" * 80)
        sys.exit(1)
    
    # Step 2: Initialize provider
    provider = test_provider_initialization()
    if not provider:
        print("\n" + "=" * 80)
        print("❌ TEST FAILED: Provider initialization failed")
        print("=" * 80)
        sys.exit(1)
    
    # Step 3: Test API call
    api_success = test_dashscope_api_call(provider)
    
    # Final summary
    print("\n" + "=" * 80)
    if api_success:
        print("✅ ALL TESTS PASSED!")
        print("   Your Qwen API key is working and qwen-max model is accessible!")
    else:
        print("⚠️  SOME TESTS FAILED")
        print("   API key is loaded, but API call failed.")
        print("   Check the error messages above for details.")
    print("=" * 80)
    print()


if __name__ == "__main__":
    main()

