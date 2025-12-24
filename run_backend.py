#!/usr/bin/env python3
"""
Run Tank War backend with API key
"""

import os
import sys

def run_backend(api_key=None, provider="openai"):
    """Run backend with provided API key"""

    # If no API key provided, try to get from environment
    if not api_key:
        if provider == "openai":
            api_key = os.getenv("OPENAI_API_KEY")
        elif provider == "qwen":
            api_key = os.getenv("DASHSCOPE_API_KEY")

    if not api_key:
        print(f"❌ No {provider.upper()} API key found!")
        print(f"Set {provider.upper()}_API_KEY environment variable or pass as argument")
        return

    # Set environment variable
    if provider == "qwen":
        os.environ["DASHSCOPE_API_KEY"] = api_key
        print(f"✅ Set DASHSCOPE_API_KEY for Qwen")
    else:
        os.environ["OPENAI_API_KEY"] = api_key
        print(f"✅ Set OPENAI_API_KEY for OpenAI")

    # Import and run the backend
    print("🚀 Starting Tank War Backend...")
    from backend.fresh_backend import app

    # Run the app
    app.run(host='0.0.0.0', port=8000, debug=False)

if __name__ == "__main__":
    api_key = None
    provider = "openai"  # Default to OpenAI

    if len(sys.argv) >= 2:
        # Check if first argument is a provider type
        if sys.argv[1] in ["openai", "qwen"]:
            provider = sys.argv[1]
            if len(sys.argv) >= 3:
                api_key = sys.argv[2]
        else:
            # First argument is API key
            api_key = sys.argv[1]
            if len(sys.argv) >= 3:
                provider = sys.argv[2]

    print(f"🔧 Starting backend with {provider.upper()} provider")
    run_backend(api_key, provider)
