#!/usr/bin/env python3
"""
Test if GDD requirements directly match code functions.

This creates a simple test case where:
- GDD requirement: "Activate ability" or "Exit ability"  
- Code function: Active() or Exit()
- Should match!
"""

import asyncio
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent.parent / "src"))

from gdd_rag_backbone.gdd.schemas import GddRequirement
from gdd_rag_backbone.gdd.requirement_matching import evaluate_requirement_behavior
from gdd_rag_backbone.gdd.behavior_indexing import index_code_behaviors
from gdd_rag_backbone.llm_providers import QwenProvider, make_llm_model_func, make_embedding_func

async def test_direct_match():
    """Test with requirements that should directly match code functions."""
    
    print("=" * 80)
    print("TEST: Direct Match Between GDD Requirements and Code Functions")
    print("=" * 80)
    print()
    
    workspace_id = "tank_war"
    code_file = "AbilityBase"
    
    # Initialize
    provider = QwenProvider()
    llm_func = make_llm_model_func(provider)
    embedding_func = make_embedding_func(provider)
    
    print(f"📁 Code File: {code_file}")
    print(f"📄 Testing with requirements that should match code functions")
    print()
    
    # Create test requirements that should match AbilityBase.cs functions
    test_requirements = [
        GddRequirement(
            id="req_activate_ability",
            title="Activate Ability",
            description="When player triggers an ability, the system should activate it. The ability should become active and start its duration timer.",
            category="combat",
            priority="high",
        ),
        GddRequirement(
            id="req_exit_ability",
            title="Exit Ability",
            description="When an ability finishes or is cancelled, it should exit. The ability should become inactive and start its cooldown timer.",
            category="combat",
            priority="high",
        ),
        GddRequirement(
            id="req_initialize_ability",
            title="Initialize Ability",
            description="When an ability is created or assigned to a player, it should be initialized with the player reference and load its properties.",
            category="combat",
            priority="high",
        ),
        GddRequirement(
            id="req_apply_damage",
            title="Apply Damage",
            description="When an ability hits a target, it should apply damage to that target based on the player's damage stats.",
            category="combat",
            priority="high",
        ),
    ]
    
    print("📋 Test Requirements:")
    for i, req in enumerate(test_requirements, 1):
        print(f"   {i}. {req.id}: {req.title}")
    print()
    
    # Index code behaviors
    print("=" * 80)
    print("STEP 1: Index Code Behaviors")
    print("=" * 80)
    
    print(f"📦 Extracting behaviors from: {code_file}")
    code_behaviors = await index_code_behaviors(
        code_file,
        provider=provider,
        llm_func=llm_func,
        workspace_id=workspace_id,
        max_methods=30,
    )
    
    print(f"✅ Extracted {len(code_behaviors)} code behaviors")
    
    # Show functions that should match
    print("\n💻 Key Functions Found:")
    key_functions = ['Active', 'Exit', 'Initialize', 'ApplySingleDamage', 'OnAbilityTrigged']
    for func_name in key_functions:
        matching = [b for b in code_behaviors if func_name.lower() in b.symbol.lower()]
        if matching:
            print(f"   ✅ {func_name}: {matching[0].symbol}")
            print(f"      Description: {matching[0].description[:60]}")
        else:
            print(f"   ❌ {func_name}: Not found")
    print()
    
    # Evaluate each requirement
    print("=" * 80)
    print("STEP 2: Evaluate Requirements")
    print("=" * 80)
    
    results = []
    for i, requirement in enumerate(test_requirements, 1):
        print(f"\n🔍 Testing {i}/{len(test_requirements)}: {requirement.id}")
        print(f"   Requirement: {requirement.title}")
        
        try:
            result = await evaluate_requirement_behavior(
                requirement,
                code_file,
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
            
            print(f"   ✅ Status: {status}")
            if best_match:
                print(f"   🎯 Best Match: {best_match}")
            if top_matches:
                print(f"   📊 Top Matches:")
                for j, match in enumerate(top_matches[:3], 1):
                    symbol = match.get("symbol", "")
                    similarity = match.get("similarity", 0)
                    print(f"      {j}. {symbol} (similarity: {similarity:.3f})")
            if reason:
                print(f"   📝 Reason: {reason[:100]}")
            
            results.append({
                "requirement": requirement,
                "status": status,
                "best_match": best_match,
                "reason": reason,
            })
            
        except Exception as e:
            print(f"   ❌ Error: {e}")
            import traceback
            traceback.print_exc()
            results.append({
                "requirement": requirement,
                "status": "error",
                "error": str(e),
            })
    
    # Summary
    print()
    print("=" * 80)
    print("STEP 3: Results Summary")
    print("=" * 80)
    
    implemented = sum(1 for r in results if r.get("status") == "implemented")
    partially = sum(1 for r in results if r.get("status") == "partially_implemented")
    not_implemented = sum(1 for r in results if r.get("status") == "not_implemented")
    errors = sum(1 for r in results if r.get("status") == "error")
    
    print(f"\n📊 Results:")
    print(f"   ✅ Implemented: {implemented}/{len(test_requirements)}")
    print(f"   ⚠️  Partially Implemented: {partially}/{len(test_requirements)}")
    print(f"   ❌ Not Implemented: {not_implemented}/{len(test_requirements)}")
    print(f"   🚫 Errors: {errors}/{len(test_requirements)}")
    print()
    
    print("📋 Detailed Results:")
    for result in results:
        req = result["requirement"]
        status = result.get("status")
        
        status_icon = {
            "implemented": "✅",
            "partially_implemented": "⚠️",
            "not_implemented": "❌",
            "error": "🚫",
        }.get(status, "❓")
        
        print(f"\n{status_icon} {req.id}: {req.title}")
        print(f"   Status: {status}")
        if result.get("best_match"):
            print(f"   Best Match: {result['best_match']}")
        if result.get("reason"):
            print(f"   Reason: {result['reason'][:80]}")
    
    print()
    print("=" * 80)
    print("✅ TEST COMPLETE")
    print("=" * 80)
    print()
    
    if implemented > 0 or partially > 0:
        print("🎉 SUCCESS! The method detected matches between GDD requirements")
        print("   and code functions. This proves the behavior-based comparison works!")
    else:
        print("⚠️  No matches detected. This could mean:")
        print("   1. The requirements don't match the code (expected for some cases)")
        print("   2. The behavior extraction needs tuning")
        print("   3. Try with different requirements that more closely match the code")
    print()

if __name__ == "__main__":
    asyncio.run(test_direct_match())


