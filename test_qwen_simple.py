#!/usr/bin/env python3
"""
Simple Qwen API test - manual key entry
"""

import os
import sys

# Add src to path
sys.path.insert(0, 'src')

def test_with_key(api_key, provider_type="qwen"):
    """Test API with provided key"""

    print(f"🔍 Testing {provider_type.upper()} API Connection")
    print("=" * 50)

    # Set the API key
    if provider_type == "qwen":
        os.environ["DASHSCOPE_API_KEY"] = api_key
        print(f"✅ Set DASHSCOPE_API_KEY (length: {len(api_key)})")
    else:
        os.environ["OPENAI_API_KEY"] = api_key
        print(f"✅ Set OPENAI_API_KEY (length: {len(api_key)})")

    # Test the provider
    try:
        if provider_type == "qwen":
            from gdd_rag_backbone.llm_providers.qwen_provider import QwenProvider
            provider = QwenProvider()
        else:
            from gdd_rag_backbone.llm_providers.openai_provider import OpenAIProvider
            provider = OpenAIProvider()

        print("🔧 Provider Initialization: ✅ Success")

        # Test LLM call
        print("\n🤖 Testing LLM Call...")
        test_prompt = "Hello! Please respond with exactly: 'API test successful!'"
        response = provider.llm(test_prompt)

        print(f"📝 Response: {response[:100]}...")

        if "successful" in response.lower():
            print("✅ API Test: PASSED!")
            return True
        else:
            print("⚠️  Response received but unexpected content")
            return False

    except Exception as e:
        print(f"❌ API Error: {e}")
        return False

if __name__ == "__main__":
    # Check if API key is provided as argument or environment variable
    if len(sys.argv) >= 2:
        # API key provided as argument
        api_key = sys.argv[1]
        provider_type = sys.argv[2] if len(sys.argv) > 2 else "qwen"
    else:
        # Check environment variables
        if os.getenv("OPENAI_API_KEY"):
            api_key = os.getenv("OPENAI_API_KEY")
            provider_type = "openai"
            print(f"📋 Using OPENAI_API_KEY from environment")
        elif os.getenv("DASHSCOPE_API_KEY"):
            api_key = os.getenv("DASHSCOPE_API_KEY")
            provider_type = "qwen"
            print(f"📋 Using DASHSCOPE_API_KEY from environment")
        else:
            print("❌ No API key found!")
            print("\nUsage options:")
            print("1. As arguments: python3 test_qwen_simple.py YOUR_API_KEY [qwen|openai]")
            print("2. As environment: OPENAI_API_KEY=sk-... python3 test_qwen_simple.py")
            print("3. As environment: DASHSCOPE_API_KEY=sk-... python3 test_qwen_simple.py")
            print("\nExamples:")
            print("  python3 test_qwen_simple.py sk-your-openai-key openai")
            print("  OPENAI_API_KEY=sk-your-key python3 test_qwen_simple.py")
            sys.exit(1)

    success = test_with_key(api_key, provider_type)

    if success:
        print(f"\n🎉 {provider_type.upper()} API is working! Ready to use in Tank War backend.")
    else:
        print(f"\n❌ {provider_type.upper()} API test failed. Check your API key.")
