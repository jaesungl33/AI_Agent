#!/usr/bin/env python3
"""
Test evaluation function with related GDD and code.

This test:
1. Finds a GDD related to abilities/combat
2. Uses AbilityBase.cs or WeaponManager.cs as code
3. Runs the evaluation
4. Shows if it correctly detects matches
"""

import asyncio
import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent.parent / "src"))

from gdd_rag_backbone.gdd.extraction import extract_all_requirements
from gdd_rag_backbone.gdd.requirement_matching import (
    evaluate_requirement_behavior,
    evaluate_all_requirements_behavior,
)
from gdd_rag_backbone.gdd.behavior_indexing import index_code_behaviors
from gdd_rag_backbone.gdd.schemas import GddRequirement
from gdd_rag_backbone.llm_providers import QwenProvider, make_llm_model_func, make_embedding_func
from gdd_rag_backbone.rag_backend.chunk_qa import load_doc_status
from gdd_rag_backbone.workspace.storage import WorkspaceStorage

async def test_related_evaluation():
    """Test evaluation with related GDD and code."""
    
    print("=" * 80)
    print("TEST: Evaluation with Related GDD and Code")
    print("=" * 80)
    print()
    
    workspace_id = "tank_war"
    
    # Initialize
    provider = QwenProvider()
    llm_func = make_llm_model_func(provider)
    embedding_func = make_embedding_func(provider)
    
    print(f"📁 Workspace: {workspace_id}")
    print()
    
    # Step 1: Find related GDD and code
    print("=" * 80)
    print("STEP 1: Finding Related GDD and Code Files")
    print("=" * 80)
    
    status = load_doc_status(workspace_id=workspace_id)
    
    # Find combat/ability related GDDs
    combat_gdds = [
        k for k in status.keys() 
        if k not in ['AbilityBase', 'WeaponManager'] and 
        status[k].get('status') in ['indexed', 'processed'] and
        any(kw in k.lower() for kw in ['skill', 'ability', 'combat', 'shooting', 'weapon', 'mobile'])
    ]
    
    print(f"📄 Found {len(combat_gdds)} combat-related GDDs")
    
    # Pick best GDD - prioritize skill/ability documents
    test_gdd_id = None
    priority_keywords = ['skill', 'ability']  # Most relevant for AbilityBase
    
    for gdd_id in combat_gdds:
        gdd_lower = gdd_id.lower()
        if any(kw in gdd_lower for kw in priority_keywords):
            test_gdd_id = gdd_id
            print(f"   Found priority GDD: {gdd_id}")
            break
    
    # If no skill/ability GDD, try shooting/combat
    if not test_gdd_id:
        secondary_keywords = ['shooting', 'combat']
        for gdd_id in combat_gdds:
            gdd_lower = gdd_id.lower()
            if any(kw in gdd_lower for kw in secondary_keywords):
                test_gdd_id = gdd_id
                break
    
    if not test_gdd_id and combat_gdds:
        test_gdd_id = combat_gdds[0]
    
    if not test_gdd_id:
        print("❌ No combat-related GDDs found")
        return
    
    print(f"✅ Selected GDD: {test_gdd_id}")
    print(f"   File: {status[test_gdd_id].get('file_name', 'unknown')}")
    print()
    
    # Find related code files
    code_files = ['AbilityBase', 'AbilityManager', 'WeaponManager', 'Weapon']
    test_code_id = None
    
    for code_id in code_files:
        if code_id in status:
            test_code_id = code_id
            break
    
    if not test_code_id:
        # Try to find any ability/weapon related code
        for k in status.keys():
            if any(kw in k.lower() for kw in ['ability', 'weapon']) and status[k].get('status') in ['indexed', 'processed']:
                test_code_id = k
                break
    
    if not test_code_id:
        print("❌ No related code files found")
        return
    
    print(f"✅ Selected Code: {test_code_id}")
    print(f"   Status: {status[test_code_id].get('status', 'unknown')}")
    print(f"   Chunks: {len(status[test_code_id].get('chunks_list', []))}")
    print()
    
    # Step 2: Extract requirements from GDD
    print("=" * 80)
    print("STEP 2: Extract Requirements from GDD")
    print("=" * 80)
    
    try:
        print(f"📄 Extracting requirements from: {test_gdd_id}")
        spec_data = await extract_all_requirements(
            test_gdd_id,
            llm_func=llm_func,
            workspace_id=workspace_id
        )
        requirements_data = spec_data.get("requirements", [])
        print(f"✅ Extracted {len(requirements_data)} requirements")
        
        if not requirements_data:
            print("⚠️  No requirements found. Trying a different GDD...")
            # Try another GDD
            if len(combat_gdds) > 1:
                test_gdd_id = combat_gdds[1]
                spec_data = await extract_all_requirements(
                    test_gdd_id,
                    llm_func=llm_func,
                    workspace_id=workspace_id
                )
                requirements_data = spec_data.get("requirements", [])
                print(f"✅ Extracted {len(requirements_data)} requirements from alternative GDD")
        
        if not requirements_data:
            print("❌ No requirements found in any GDD")
            return
        
        # Show sample requirements
        print("\n📋 Sample Requirements:")
        for i, req in enumerate(requirements_data[:5], 1):
            print(f"   {i}. {req.get('id', 'unknown')}: {req.get('title', req.get('summary', 'no title'))[:60]}")
        
        # Convert to GddRequirement objects (limit to 5 for test)
        requirements = []
        for req_dict in requirements_data[:5]:
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
        
        print(f"\n✅ Using {len(requirements)} requirements for evaluation")
        print()
        
    except Exception as e:
        print(f"❌ Failed to extract requirements: {e}")
        import traceback
        traceback.print_exc()
        return
    
    # Step 3: Index code behaviors
    print("=" * 80)
    print("STEP 3: Index Code Behaviors")
    print("=" * 80)
    
    try:
        print(f"📦 Extracting behaviors from: {test_code_id}")
        code_behaviors = await index_code_behaviors(
            test_code_id,
            provider=provider,
            llm_func=llm_func,
            workspace_id=workspace_id,
            max_methods=30,  # Limit for faster test
        )
        print(f"✅ Extracted {len(code_behaviors)} code behaviors")
        
        if code_behaviors:
            print("\n💻 Sample Code Behaviors:")
            for i, behavior in enumerate(code_behaviors[:5], 1):
                print(f"   {i}. {behavior.symbol}")
                print(f"      Description: {behavior.description[:60]}")
                if behavior.trigger_patterns:
                    print(f"      Triggers: {behavior.trigger_patterns[:2]}")
                print()
        
    except Exception as e:
        print(f"❌ Failed to index code behaviors: {e}")
        import traceback
        traceback.print_exc()
        return
    
    # Step 4: Evaluate requirements
    print("=" * 80)
    print("STEP 4: Evaluate Requirements Against Code")
    print("=" * 80)
    print()
    print(f"🔍 Comparing {len(requirements)} requirements from '{test_gdd_id}'")
    print(f"   against {len(code_behaviors)} behaviors from '{test_code_id}'")
    print()
    
    results = []
    for i, requirement in enumerate(requirements, 1):
        print(f"📋 Evaluating {i}/{len(requirements)}: {requirement.id}")
        print(f"   Title: {requirement.title[:70]}")
        print(f"   Description: {requirement.description[:100] if requirement.description else 'N/A'}...")
        
        try:
            result = await evaluate_requirement_behavior(
                requirement,
                test_code_id,
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
            top_matches = result.get("top_matches", [])
            
            # Status icon
            status_icon = {
                "implemented": "✅",
                "partially_implemented": "⚠️",
                "not_implemented": "❌",
                "error": "🚫",
            }.get(status, "❓")
            
            print(f"   {status_icon} Status: {status}")
            
            if best_match:
                print(f"   🎯 Best Match: {best_match}")
            
            if top_matches:
                print(f"   📊 Top Matches:")
                for j, match in enumerate(top_matches[:3], 1):
                    symbol = match.get("symbol", "")
                    similarity = match.get("similarity", 0)
                    desc = match.get("description", "")[:50]
                    print(f"      {j}. {symbol} (similarity: {similarity:.3f})")
                    if desc:
                        print(f"         {desc}...")
            
            if reason:
                print(f"   📝 Reason: {reason[:120]}")
            
            results.append({
                "requirement_id": requirement.id,
                "requirement_title": requirement.title,
                "requirement_desc": requirement.description,
                "status": status,
                "best_match": best_match,
                "top_matches": top_matches,
                "reason": reason,
            })
            
            print()
            
        except Exception as e:
            print(f"   ❌ Error: {e}")
            import traceback
            traceback.print_exc()
            results.append({
                "requirement_id": requirement.id,
                "requirement_title": requirement.title,
                "status": "error",
                "error": str(e),
            })
            print()
    
    # Step 5: Summary
    print("=" * 80)
    print("STEP 5: Evaluation Summary")
    print("=" * 80)
    
    implemented = sum(1 for r in results if r.get("status") == "implemented")
    partially = sum(1 for r in results if r.get("status") == "partially_implemented")
    not_implemented = sum(1 for r in results if r.get("status") == "not_implemented")
    errors = sum(1 for r in results if r.get("status") == "error")
    
    total = len(results)
    
    print(f"\n📊 Results Summary:")
    print(f"   ✅ Implemented: {implemented}/{total} ({implemented/total*100:.1f}%)")
    print(f"   ⚠️  Partially Implemented: {partially}/{total} ({partially/total*100:.1f}%)")
    print(f"   ❌ Not Implemented: {not_implemented}/{total} ({not_implemented/total*100:.1f}%)")
    print(f"   🚫 Errors: {errors}/{total}")
    print()
    
    print("📋 Detailed Results:")
    for result in results:
        status_icon = {
            "implemented": "✅",
            "partially_implemented": "⚠️",
            "not_implemented": "❌",
            "error": "🚫",
        }.get(result.get("status"), "❓")
        
        print(f"\n{status_icon} {result['requirement_id']}: {result['requirement_title'][:60]}")
        print(f"   Status: {result.get('status')}")
        if result.get("best_match"):
            print(f"   Best Match: {result['best_match']}")
        if result.get("top_matches"):
            print(f"   Top Matches: {', '.join([m.get('symbol', '') for m in result['top_matches'][:3]])}")
        if result.get("reason"):
            print(f"   Reason: {result['reason'][:100]}")
    
    print()
    print("=" * 80)
    print("✅ EVALUATION TEST COMPLETE")
    print("=" * 80)
    print()
    
    # Interpretation
    match_rate = (implemented + partially) / total * 100 if total > 0 else 0
    
    if match_rate > 50:
        print("🎉 SUCCESS! The evaluation function is working correctly!")
        print(f"   Found matches for {match_rate:.1f}% of requirements.")
        print("   This proves the behavior-based comparison method can detect")
        print("   when GDD requirements are implemented in the code.")
    elif match_rate > 0:
        print("⚠️  PARTIAL SUCCESS: Some matches found.")
        print(f"   Found matches for {match_rate:.1f}% of requirements.")
        print("   This could mean:")
        print("   - Some requirements are not yet implemented")
        print("   - Requirements are in different code files")
        print("   - The GDD and code are related but not directly matching")
    else:
        print("⚠️  NO MATCHES FOUND")
        print("   This could mean:")
        print("   - The GDD and code are not as related as expected")
        print("   - Requirements are implemented in different files")
        print("   - The behavior extraction needs tuning")
        print("   - Try with a different GDD/code combination")
    
    print()
    print("💡 Next Steps:")
    print("   1. If matches found: The evaluation function works! ✅")
    print("   2. If no matches: Try different GDD/code combinations")
    print("   3. Check the 'Top Matches' to see what the method found")
    print("   4. Review the 'Reason' field for explanation")
    print()

if __name__ == "__main__":
    asyncio.run(test_related_evaluation())

