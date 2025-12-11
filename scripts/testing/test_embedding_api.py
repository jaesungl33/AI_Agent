#!/usr/bin/env python3
"""
Test script to diagnose embedding API issues.
"""
import os
from dotenv import load_dotenv

load_dotenv('.env')

api_key = os.getenv("DASHSCOPE_API_KEY")

print("\n" + "=" * 80)
print("🔍 Testing DashScope Embedding API")
print("=" * 80)
print()

try:
    from dashscope import embeddings
    import dashscope
    
    dashscope.api_key = api_key
    dashscope.region = 'intl'
    
    print(f"API Key: {api_key[:15]}...{api_key[-4:] if len(api_key) > 19 else ''}")
    print(f"Region: intl")
    print()
    
    # Test different embedding models
    models_to_test = [
        'text-embedding-v2',
        'text-embedding-v1',
        'text-embedding-async-v1',
        'text-embedding-async-v2',
    ]
    
    for model in models_to_test:
        print(f"Testing model: {model}")
        try:
            response = embeddings.TextEmbedding.call(
                model=model,
                input=['test embedding'],
            )
            print(f"  Status: {response.status_code}")
            if response.status_code == 200:
                print(f"  ✅ SUCCESS!")
                if hasattr(response, 'output') and hasattr(response.output, 'embeddings'):
                    emb = response.output.embeddings[0]
                    print(f"  Embedding dimensions: {len(emb['embedding'])}")
                break
            else:
                print(f"  ❌ FAILED: {getattr(response, 'message', 'Unknown error')}")
        except Exception as e:
            print(f"  ❌ ERROR: {e}")
        print()
    
    # Test with OpenAI-compatible endpoint
    print("Testing OpenAI-compatible endpoint...")
    try:
        from openai import OpenAI
        client = OpenAI(
            api_key=api_key,
            base_url='https://dashscope-intl.aliyuncs.com/compatible-mode/v1',
        )
        
        models = ['text-embedding-v2', 'text-embedding-v1']
        for model in models:
            try:
                result = client.embeddings.create(
                    model=model,
                    input=['test'],
                )
                print(f"  ✅ {model}: SUCCESS (dimensions: {len(result.data[0].embedding)})")
                break
            except Exception as e:
                print(f"  ❌ {model}: {e}")
    except ImportError:
        print("  openai package not installed")
    except Exception as e:
        print(f"  ❌ ERROR: {e}")
    
except Exception as e:
    print(f"❌ ERROR: {e}")
    import traceback
    traceback.print_exc()

print("\n" + "=" * 80)

