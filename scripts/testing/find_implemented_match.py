#!/usr/bin/env python3
"""
Find a GDD requirement that is actually implemented in a code file.

This script:
1. Looks at what code files actually implement
2. Finds GDD requirements that match
3. Tests the evaluation to see if it detects the match
"""

import asyncio
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent.parent / "src"))

from gdd_rag_backbone.gdd.extraction import extract_all_requirements
from gdd_rag_backbone.gdd.requirement_matching import evaluate_requirement_behavior
from gdd_rag_backbone.gdd.behavior_indexing import index_code_behaviors
from gdd_rag_backbone.gdd.schemas import GddRequirement
from gdd_rag_backbone.llm_providers import QwenProvider, make_llm_model_func, make_embedding_func
from gdd_rag_backbone.rag_backend.chunk_qa import load_doc_status

async def find_implemented_match():
    """Find a GDD requirement that matches a code implementation."""
    
    workspace_id = "tank_war"
    
    print("=" * 80)
    print("FINDING GDD REQUIREMENT IMPLEMENTED IN CODE")
    print("=" * 80)
    print()
    
    provider = QwenProvider()
    llm = make_llm_model_func(provider)
    embed = make_embedding_func(provider)
    
    status = load_doc_status(workspace_id=workspace_id)
    
    # Test cases: code files that should have matching GDDs
    test_cases = [
        {
            "code": "WeaponManager",
            "gdd": "[Combat_Module]_[Tank_War]_Shooting_Logic",
            "expected": "Weapon management, firing, weapon selection"
        },
        {
            "code": "AbilityBase",
            "gdd": "[Combat_Module]_[Tank_War]_Skill_Design_Document",
            "expected": "Ability activation, ability lifecycle"
        },
        {
            "code": "Weapon",
            "gdd": "[Combat_Module]_[Tank_War]_Shooting_Logic",
            "expected": "Weapon firing, projectile spawning"
        },
    ]
    
    for test_case in test_cases:
        code_id = test_case["code"]
        gdd_id = test_case["gdd"]
        expected = test_case["expected"]
        
        if code_id not in status or gdd_id not in status:
            continue
        
        print(f"🔍 Testing: {code_id} vs {gdd_id}")
        print(f"   Expected: {expected}")
        print()
        
        try:
            # Extract requirements
            print("   📄 Extracting requirements...")
            spec = await extract_all_requirements(gdd_id, llm_func=llm, workspace_id=workspace_id)
            reqs = spec.get("requirements", [])[:3]  # Test first 3
            
            if not reqs:
                print("   ⚠️  No requirements found")
                print()
                continue
            
            print(f"   ✅ Found {len(reqs)} requirements")
            
            # Index code
            print("   📦 Indexing code behaviors...")
            behaviors = await index_code_behaviors(
                code_id, provider=provider, llm_func=llm,
                workspace_id=workspace_id, max_methods=20
            )
            print(f"   ✅ Found {len(behaviors)} behaviors")
            
            # Test each requirement
            print("   🔍 Evaluating requirements...")
            print()
            
            found_match = False
            
            for req_dict in reqs:
                req = GddRequirement(
                    id=req_dict.get("id", ""),
                    title=req_dict.get("title", req_dict.get("summary", "")),
                    description=req_dict.get("description", req_dict.get("details", "")),
                )
                
                print(f"      📋 {req.id}: {req.title[:50]}")
                
                result = await evaluate_requirement_behavior(
                    req, code_id, code_behaviors=behaviors,
                    provider=provider, llm_func=llm, embedding_func=embed,
                    workspace_id=workspace_id, top_k=3
                )
                
                status_val = result.get("status")
                icon = {
                    "implemented": "✅",
                    "partially_implemented": "⚠️",
                    "not_implemented": "❌"
                }.get(status_val, "❓")
                
                print(f"      {icon} {status_val}")
                
                if result.get("best_match"):
                    print(f"      🎯 {result['best_match']}")
                
                if status_val in ["implemented", "partially_implemented"]:
                    found_match = True
                    print()
                    print("   🎉 FOUND A MATCH!")
                    print(f"      Code: {code_id}")
                    print(f"      GDD: {gdd_id}")
                    print(f"      Requirement: {req.id} - {req.title}")
                    print(f"      Status: {status_val}")
                    print()
                    return {
                        "code": code_id,
                        "gdd": gdd_id,
                        "requirement": req,
                        "status": status_val,
                        "result": result
                    }
                
                print()
            
            if not found_match:
                print(f"   ❌ No matches found in this combination")
                print()
        
        except Exception as e:
            print(f"   ❌ Error: {e}")
            import traceback
            traceback.print_exc()
            print()
            continue
    
    print("=" * 80)
    print("⚠️  No implemented matches found in tested combinations")
    print("=" * 80)
    print()
    print("This could mean:")
    print("  1. Requirements are in different code files")
    print("  2. GDDs describe features not yet implemented")
    print("  3. Need to test with more GDD/code combinations")
    print()
    print("💡 Try testing with:")
    print("  - Multiple code files at once")
    print("  - Different GDDs")
    print("  - The full codebase instead of single files")
    print()
    
    return None

if __name__ == "__main__":
    result = asyncio.run(find_implemented_match())
    if result:
        print("=" * 80)
        print("✅ SUCCESS: Found an implemented match!")
        print("=" * 80)
        print(f"Code File: {result['code']}")
        print(f"GDD: {result['gdd']}")
        print(f"Requirement: {result['requirement'].id} - {result['requirement'].title}")
        print(f"Status: {result['status']}")
        print()
        print("This proves the evaluation function can detect when")
        print("GDD requirements are actually implemented in code! 🎉")


