#!/usr/bin/env python3
"""
Terminal test script for the new @codebase chat functionality.

Tests:
1. Normal mode: "Explain the system" (should not touch codebase)
2. Codebase mode: "@codebase where is inventory implemented?" (should retrieve code)
"""

import asyncio
import json
import sys
from pathlib import Path

# Add project root to path for imports
PROJECT_ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(PROJECT_ROOT))
sys.path.insert(0, str(PROJECT_ROOT / "src"))

import httpx
from gdd_rag_backbone.workspace import WorkspaceManager


async def test_chat_modes():
    """Test both normal and codebase chat modes."""

    # Get default workspace
    manager = WorkspaceManager()
    workspace_id = manager.get_default_workspace()

    print(f"Testing chat with workspace: {workspace_id}")
    print("=" * 60)

    # Test cases
    test_cases = [
        {
            "name": "Normal mode",
            "message": "Explain the system",
            "expected_mode": "normal"
        },
        {
            "name": "Codebase mode",
            "message": "@codebase where is inventory implemented?",
            "expected_mode": "codebase"
        }
    ]

    async with httpx.AsyncClient() as client:
        for test_case in test_cases:
            print(f"\n🧪 Testing: {test_case['name']}")
            print(f"Message: {test_case['message']}")
            print("-" * 40)

            # Send chat request
            try:
                response = await client.post(
                    "http://localhost:8000/chat",
                    json={
                        "workspaceId": workspace_id,
                        "message": test_case["message"],
                        "topK": 5
                    },
                    timeout=30.0
                )

                if response.status_code == 200:
                    result = response.json()
                    message = result.get("message", {})
                    content = message.get("content", "")
                    context = message.get("context", {})
                    sources = context.get("sources", [])

                    print(f"✅ Success - Mode: {test_case['expected_mode']}")
                    print(f"📊 Retrieved chunks: {len(sources)}")

                    # Show preview of answer
                    preview = content[:150] + "..." if len(content) > 150 else content
                    print(f"💬 Answer preview: {preview}")

                    # Show sources if any
                    if sources:
                        print("📁 Sources:")
                        for i, source in enumerate(sources[:3]):  # Show first 3
                            file_path = source.get("file_path", "unknown")
                            symbol = source.get("symbol", "unknown")
                            score = source.get("score", 0)
                            print(f"  {i+1}. {file_path} ({symbol}) - score: {score:.3f}")

                else:
                    print(f"❌ HTTP {response.status_code}: {response.text}")

            except Exception as e:
                print(f"❌ Error: {e}")

    print("\n" + "=" * 60)
    print("✅ Test completed!")


if __name__ == "__main__":
    print("Testing @codebase chat functionality...")
    print("Make sure the backend is running on http://localhost:8000")
    print()

    try:
        asyncio.run(test_chat_modes())
    except KeyboardInterrupt:
        print("\n🛑 Test interrupted")
    except Exception as e:
        print(f"\n💥 Test failed: {e}")
        sys.exit(1)



