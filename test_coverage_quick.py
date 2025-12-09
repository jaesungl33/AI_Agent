#!/usr/bin/env python3
"""
Quick test script to verify coverage evaluation is working.
Tests with minimal requirements for fast execution.
"""

import asyncio
import sys
from pathlib import Path

# Add project root to path
PROJECT_ROOT = Path(__file__).resolve().parent
if str(PROJECT_ROOT) not in sys.path:
    sys.path.insert(0, str(PROJECT_ROOT))

from gdd_rag_backbone.gdd.schemas import GddRequirement
from gdd_rag_backbone.gdd.requirement_matching import (
    evaluate_requirement,
    build_symbol_index,
)
from gdd_rag_backbone.llm_providers import QwenProvider
from gdd_rag_backbone.rag_backend.chunk_qa import load_doc_status

async def test_coverage_quick():
    """Test coverage evaluation with a single simple requirement."""
    print("=" * 60)
    print("QUICK COVERAGE EVALUATION TEST")
    print("=" * 60)
    
    # Step 1: Check available documents
    print("\n[1/4] Loading document status...")
    status = load_doc_status()
    
    if not status:
        print("❌ ERROR: No documents found. Please index some documents first.")
        return False
    
    # Find a GDD and code index
    gdd_ids = [doc_id for doc_id, meta in status.items() 
               if not doc_id.startswith("code_") and not doc_id.startswith("tank_online_codebase")]
    code_ids = [doc_id for doc_id, meta in status.items() 
                if doc_id.startswith("code_") or doc_id.startswith("tank_online_codebase")]
    
    if not gdd_ids:
        print("❌ ERROR: No GDD documents found.")
        print(f"Available documents: {list(status.keys())[:5]}")
        return False
    
    if not code_ids:
        print("❌ ERROR: No code documents found.")
        print(f"Available documents: {list(status.keys())[:5]}")
        return False
    
    test_gdd = gdd_ids[0]
    test_code = code_ids[0]
    
    print(f"✅ Found {len(gdd_ids)} GDD(s) and {len(code_ids)} code batch(es)")
    print(f"   Using GDD: {test_gdd}")
    print(f"   Using Code: {test_code}")
    
    # Step 2: Create a simple test requirement
    print("\n[2/4] Creating test requirement...")
    test_requirement = GddRequirement(
        id="test_requirement_001",
        title="Test Requirement: Simple Function Check",
        description="This is a test requirement to verify the coverage evaluation system is working. It checks if basic code evaluation functions are accessible.",
        category="test",
        priority="low",
    )
    print(f"✅ Created test requirement: {test_requirement.title}")
    
    # Step 3: Build symbol index (fast)
    print("\n[3/4] Building symbol index...")
    try:
        symbol_index = build_symbol_index(test_code)
        symbol_count = len(symbol_index)
        print(f"✅ Symbol index built: {symbol_count} symbols found")
        if symbol_count > 0:
            sample_symbols = list(symbol_index.keys())[:3]
            print(f"   Sample symbols: {', '.join(sample_symbols)}")
    except Exception as e:
        print(f"⚠️  Warning: Could not build symbol index: {e}")
        symbol_index = {}
    
    # Step 4: Run evaluation
    print("\n[4/4] Running coverage evaluation...")
    print("   This may take 30-60 seconds (LLM call)...")
    
    try:
        provider = QwenProvider()
        result = await evaluate_requirement(
            test_requirement,
            test_code,
            provider=provider,
            top_k=5,  # Small top_k for speed
            symbol_index=symbol_index,
        )
        
        print("\n" + "=" * 60)
        print("✅ EVALUATION COMPLETE!")
        print("=" * 60)
        print(f"\nRequirement ID: {result.get('requirement_id')}")
        print(f"Status: {result.get('status', 'unknown')}")
        print(f"Coverage Type: {result.get('coverage_type', 'unknown')}")
        
        if result.get('coverage_type') == 'fast':
            print(f"Used Symbol: {result.get('used_symbol', 'N/A')}")
            matches = result.get('matches', [])
            if matches:
                print(f"Matches: {len(matches)} location(s) found")
        elif result.get('coverage_type') == 'semantic':
            best_match = result.get('best_match', {})
            if best_match:
                classification = best_match.get('classification', 'N/A')
                reason = best_match.get('reason', 'N/A')
                print(f"Best Match: {classification}")
                print(f"Reason: {reason}")
        
        reason = result.get('reason', '')
        if reason:
            print(f"Reason: {reason}")
        
        print("\n✅ Backend connection and evaluation function are working!")
        return True
        
    except Exception as e:
        print(f"\n❌ ERROR during evaluation: {e}")
        import traceback
        traceback.print_exc()
        return False


if __name__ == "__main__":
    print("\n🚀 Starting quick coverage test...\n")
    success = asyncio.run(test_coverage_quick())
    
    if success:
        print("\n✅ Test passed! The coverage evaluation system is working.")
        sys.exit(0)
    else:
        print("\n❌ Test failed. Check the errors above.")
        sys.exit(1)

