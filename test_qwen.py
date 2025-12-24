#!/usr/bin/env python3
"""
Test Qwen LLM API connection
"""

import os
import sys

# Add src to path
sys.path.insert(0, 'src')

def test_qwen_api():
    """Test Qwen API connection"""

    print("🔍 Testing Qwen LLM API Connection")
    print("=" * 50)

    # Check environment variables
    dashscope_key = os.getenv("DASHSCOPE_API_KEY")
    qwen_key = os.getenv("QWEN_API_KEY")
    openai_key = os.getenv("OPENAI_API_KEY")

    print("📋 Environment Variables:")
    print(f"  DASHSCOPE_API_KEY: {'✅ Set' if dashscope_key else '❌ Not set'} {'(length: ' + str(len(dashscope_key)) + ')' if dashscope_key else ''}")
    print(f"  QWEN_API_KEY: {'✅ Set' if qwen_key else '❌ Not set'} {'(length: ' + str(len(qwen_key)) + ')' if qwen_key else ''}")
    print(f"  OPENAI_API_KEY: {'✅ Set' if openai_key else '❌ Not set'} {'(length: ' + str(len(openai_key)) + ')' if openai_key else ''}")

    # If no API keys, try to load .env manually
    if not dashscope_key and not qwen_key and not openai_key:
        print("\n📄 Trying to load .env file manually...")
        try:
            with open('.env', 'r') as f:
                for line in f:
                    line = line.strip()
                    if line and not line.startswith('#'):
                        if '=' in line:
                            key, value = line.split('=', 1)
                            os.environ[key] = value
                            print(f"  Loaded {key} from .env")

            # Check again after loading
            dashscope_key = os.getenv("DASHSCOPE_API_KEY")
            qwen_key = os.getenv("QWEN_API_KEY")
            openai_key = os.getenv("OPENAI_API_KEY")

            print("\n📋 After loading .env:")
            print(f"  DASHSCOPE_API_KEY: {'✅ Set' if dashscope_key else '❌ Not set'}")
            print(f"  QWEN_API_KEY: {'✅ Set' if qwen_key else '❌ Not set'}")
            print(f"  OPENAI_API_KEY: {'✅ Set' if openai_key else '❌ Not set'}")

        except Exception as e:
            print(f"❌ Could not load .env file: {e}")

    # Try to import and initialize provider
    try:
        from gdd_rag_backbone.llm_providers.qwen_provider import QwenProvider
        print("\n📚 LLM Provider Import: ✅ Success")

        provider = QwenProvider()
        print("🔧 Qwen Provider Initialization: ✅ Success")

        # Test LLM call
        print("\n🤖 Testing LLM Call...")
        test_prompt = "Hello, can you respond with just 'Qwen is working!'?"
        response = provider.llm(test_prompt)
        print(f"📝 LLM Response: {response}")

        if "Qwen" in response and len(response.strip()) > 0:
            print("✅ LLM Test: PASSED - Qwen API is working!")
        else:
            print("⚠️  LLM Test: Response received but unexpected content")

    except Exception as e:
        print(f"❌ LLM Provider Error: {e}")

        # Check if required packages are installed
        try:
            import openai
            print("📦 OpenAI package: ✅ Available")
        except ImportError:
            print("📦 OpenAI package: ❌ Not installed (pip install openai)")

        try:
            import dashscope
            print("📦 DashScope package: ✅ Available")
        except ImportError:
            print("📦 DashScope package: ❌ Not installed (pip install dashscope)")

if __name__ == "__main__":
    test_qwen_api()
