#!/usr/bin/env python3
"""
Small-scale test to verify behavior-based comparison works.

This script:
1. Uploads a single .cs file (AbilityBase.cs or WeaponManager.cs)
2. Extracts requirements from a GDD
3. Runs coverage evaluation
4. Shows if it correctly detects implementation
"""

import asyncio
import json
import sys
from pathlib import Path

# Add src to path
sys.path.insert(0, str(Path(__file__).parent.parent.parent / "src"))

from gdd_rag_backbone.gdd.extraction import extract_all_requirements
from gdd_rag_backbone.gdd.requirement_matching import (
    evaluate_requirement_behavior,
)
from gdd_rag_backbone.gdd.behavior_indexing import index_code_behaviors
from gdd_rag_backbone.llm_providers import QwenProvider, make_llm_model_func, make_embedding_func
from gdd_rag_backbone.gdd.schemas import GddRequirement

async def test_small_scale():
    """Test behavior-based comparison with a single file and GDD."""
    
    print("=" * 80)
    print("SMALL-SCALE BEHAVIOR-BASED COMPARISON TEST")
    print("=" * 80)
    print()
    
    # Configuration
    workspace_id = "tank_war"
    
    # Test file: AbilityBase.cs (handles tank abilities)
    test_code_file = "AbilityBase"
    
    # Pick a GDD that might relate to abilities/weapons
    # You can change this to any GDD ID
    test_gdd_id = None  # Will be set from available GDDs
    
    print(f"📁 Workspace: {workspace_id}")
    print(f"💻 Test Code File: {test_code_file}")
    print()
    
    # Initialize provider
    print("🔧 Initializing LLM provider...")
    provider = QwenProvider()
    llm_func = make_llm_model_func(provider)
    embedding_func = make_embedding_func(provider)
    print("✅ Provider initialized")
    print()
    
    # Step 1: Check if code file is indexed
    print("=" * 80)
    print("STEP 1: Check Code File Status")
    print("=" * 80)
    
    from gdd_rag_backbone.rag_backend.chunk_qa import load_doc_chunks, load_doc_status
    
    status = load_doc_status(workspace_id=workspace_id)
    
    if test_code_file not in status:
        print(f"❌ Code file '{test_code_file}' not found in workspace")
        print(f"Available code files: {[k for k in status.keys() if k.startswith('code_') or 'Ability' in k or 'Weapon' in k][:10]}")
        return
    
    code_meta = status[test_code_file]
    print(f"✅ Found code file: {test_code_file}")
    print(f"   Status: {code_meta.get('status', 'unknown')}")
    print(f"   Chunks: {len(code_meta.get('chunks_list', []))}")
    print()
    
    # Step 2: Find a relevant GDD
    print("=" * 80)
    print("STEP 2: Find Relevant GDD")
    print("=" * 80)
    
    gdd_ids = [k for k in status.keys() if k not in [test_code_file] and status[k].get('status') in ['indexed', 'processed']]
    
    if not gdd_ids:
        print("❌ No GDDs found in workspace")
        return
    
    # Try to find a GDD that might relate to abilities/weapons
    # Look for keywords in GDD names - prioritize combat/skill related
    relevant_keywords = ['skill', 'ability', 'combat', 'shooting', 'weapon']
    test_gdd_id = None
    
    for gdd_id in gdd_ids:
        gdd_name = status[gdd_id].get('file_name', gdd_id).lower()
        if any(keyword in gdd_name for keyword in relevant_keywords):
            test_gdd_id = gdd_id
            print(f"   Found relevant GDD: {gdd_id} (matches: {[k for k in relevant_keywords if k in gdd_name]})")
            break
    
    # If no match, try broader keywords
    if not test_gdd_id:
        broader_keywords = ['tank', 'gameplay', 'system']
        for gdd_id in gdd_ids:
            gdd_name = status[gdd_id].get('file_name', gdd_id).lower()
            if any(keyword in gdd_name for keyword in broader_keywords):
                test_gdd_id = gdd_id
                break
    
    # If still no match, use first GDD
    if not test_gdd_id:
        test_gdd_id = gdd_ids[0]
        print(f"   Using first available GDD: {test_gdd_id}")
    
    print(f"✅ Selected GDD: {test_gdd_id}")
    print(f"   File: {status[test_gdd_id].get('file_name', 'unknown')}")
    print()
    
    # Step 3: Extract requirements from GDD
    print("=" * 80)
    print("STEP 3: Extract Requirements from GDD")
    print("=" * 80)
    
    try:
        spec_data = await extract_all_requirements(
            test_gdd_id,
            llm_func=llm_func,
            workspace_id=workspace_id
        )
        requirements_data = spec_data.get("requirements", [])
        print(f"✅ Extracted {len(requirements_data)} requirements")
        
        if not requirements_data:
            print("⚠️  No requirements found in GDD. This GDD might not contain extractable requirements.")
            print("   Try a different GDD or check if the GDD has been properly indexed.")
            return
        
        # Show first few requirements
        print("\n📋 Sample Requirements:")
        for i, req in enumerate(requirements_data[:3], 1):
            print(f"   {i}. {req.get('id', 'unknown')}: {req.get('title', req.get('summary', 'no title'))[:60]}")
        
        # Convert to GddRequirement objects
        requirements = []
        for req_dict in requirements_data[:5]:  # Limit to 5 for test
            try:
                req = GddRequirement(
                    id=req_dict.get("id", ""),
                    title=req_dict.get("title", req_dict.get("summary", "")),
                    description=req_dict.get("description", req_dict.get("details", "")),
                    category=req_dict.get("category"),
                    priority=req_dict.get("priority"),
                )
                requirements.append(req)
            except Exception as e:
                print(f"⚠️  Skipping invalid requirement: {e}")
                continue
        
        print(f"\n✅ Using {len(requirements)} requirements for test")
        print()
        
    except Exception as e:
        print(f"❌ Failed to extract requirements: {e}")
        import traceback
        traceback.print_exc()
        return
    
    # Step 4: Index code behaviors
    print("=" * 80)
    print("STEP 4: Index Code Behaviors")
    print("=" * 80)
    
    try:
        print(f"📦 Extracting behaviors from: {test_code_file}")
        code_behaviors = await index_code_behaviors(
            test_code_file,
            provider=provider,
            llm_func=llm_func,
            workspace_id=workspace_id,
            max_methods=20,  # Limit for test
        )
        print(f"✅ Extracted {len(code_behaviors)} code behaviors")
        
        if code_behaviors:
            print("\n💻 Sample Code Behaviors:")
            for i, behavior in enumerate(code_behaviors[:3], 1):
                print(f"   {i}. {behavior.symbol}")
                print(f"      Description: {behavior.description[:60]}")
                print(f"      Triggers: {behavior.trigger_patterns[:2]}")
                print()
        
    except Exception as e:
        print(f"❌ Failed to index code behaviors: {e}")
        import traceback
        traceback.print_exc()
        return
    
    # Step 5: Evaluate requirements
    print("=" * 80)
    print("STEP 5: Evaluate Requirements Against Code")
    print("=" * 80)
    
    results = []
    for i, requirement in enumerate(requirements, 1):
        print(f"\n🔍 Evaluating requirement {i}/{len(requirements)}: {requirement.id}")
        print(f"   Title: {requirement.title[:60]}")
        
        try:
            result = await evaluate_requirement_behavior(
                requirement,
                test_code_file,
                code_behaviors=code_behaviors,
                provider=provider,
                llm_func=llm_func,
                embedding_func=embedding_func,
                workspace_id=workspace_id,
                top_k=5,
            )
            
            status = result.get("status", "unknown")
            reason = result.get("reason", result.get("llm_reason", ""))
            best_match = result.get("best_match")
            
            print(f"   ✅ Status: {status}")
            if best_match:
                print(f"   🎯 Best Match: {best_match}")
            if reason:
                print(f"   📝 Reason: {reason[:100]}")
            
            results.append({
                "requirement_id": requirement.id,
                "requirement_title": requirement.title,
                "status": status,
                "best_match": best_match,
                "reason": reason,
                "result": result,
            })
            
        except Exception as e:
            print(f"   ❌ Error: {e}")
            results.append({
                "requirement_id": requirement.id,
                "requirement_title": requirement.title,
                "status": "error",
                "error": str(e),
            })
    
    # Step 6: Summary
    print()
    print("=" * 80)
    print("STEP 6: Test Summary")
    print("=" * 80)
    
    implemented = sum(1 for r in results if r.get("status") == "implemented")
    partially = sum(1 for r in results if r.get("status") == "partially_implemented")
    not_implemented = sum(1 for r in results if r.get("status") == "not_implemented")
    errors = sum(1 for r in results if r.get("status") == "error")
    
    print(f"\n📊 Results:")
    print(f"   ✅ Implemented: {implemented}")
    print(f"   ⚠️  Partially Implemented: {partially}")
    print(f"   ❌ Not Implemented: {not_implemented}")
    print(f"   🚫 Errors: {errors}")
    print()
    
    print("📋 Detailed Results:")
    for result in results:
        status_icon = {
            "implemented": "✅",
            "partially_implemented": "⚠️",
            "not_implemented": "❌",
            "error": "🚫",
        }.get(result.get("status"), "❓")
        
        print(f"\n{status_icon} {result['requirement_id']}: {result['requirement_title'][:50]}")
        print(f"   Status: {result.get('status')}")
        if result.get("best_match"):
            print(f"   Best Match: {result['best_match']}")
        if result.get("reason"):
            print(f"   Reason: {result['reason'][:80]}")
    
    print()
    print("=" * 80)
    print("✅ TEST COMPLETE")
    print("=" * 80)
    print()
    print("💡 Interpretation:")
    print("   - If you see 'implemented' or 'partially_implemented', the method detected")
    print("     that the code matches the GDD requirement!")
    print("   - If you see 'not_implemented', the method correctly identified that")
    print("     the requirement is not found in this specific code file.")
    print("   - This is expected if the requirement relates to a different part of")
    print("     the codebase (e.g., UI requirements won't match AbilityBase.cs)")
    print()

if __name__ == "__main__":
    asyncio.run(test_small_scale())

