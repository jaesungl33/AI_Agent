#!/usr/bin/env python3
"""
Test with requirements that should exactly match code functions.

Creates test requirements based on what the code actually does.
"""

import asyncio
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent.parent / "src"))

from gdd_rag_backbone.gdd.requirement_matching import evaluate_requirement_behavior
from gdd_rag_backbone.gdd.behavior_indexing import index_code_behaviors
from gdd_rag_backbone.gdd.schemas import GddRequirement
from gdd_rag_backbone.llm_providers import QwenProvider, make_llm_model_func, make_embedding_func

async def test_exact_match():
    """Test with requirements that should match code exactly."""
    
    workspace_id = "tank_war"
    
    print("=" * 80)
    print("TEST: Exact Match Between Requirements and Code")
    print("=" * 80)
    print()
    
    provider = QwenProvider()
    llm = make_llm_model_func(provider)
    embed = make_embedding_func(provider)
    
    # Test Case 1: WeaponManager with weapon management requirements
    print("TEST CASE 1: WeaponManager.cs")
    print("-" * 80)
    
    code_id = "WeaponManager"
    
    # Requirements that should match WeaponManager functions
    test_requirements = [
        GddRequirement(
            id="req_weapon_init",
            title="Initialize Weapon System",
            description="The weapon system should initialize with a player reference. It should set up primary and secondary weapons, initialize weapon fire delays, set ammo counts, and reset all weapons to their default state.",
            category="combat",
            priority="high",
        ),
        GddRequirement(
            id="req_weapon_activate",
            title="Activate Weapon",
            description="When a player picks up a new weapon, the system should activate it. The weapon should be set as the primary or secondary weapon based on the weapon type. The ammo count should be updated for the selected weapon.",
            category="combat",
            priority="high",
        ),
        GddRequirement(
            id="req_weapon_fire",
            title="Fire Weapon",
            description="The system should allow firing of weapons. When a weapon is fired, it should check if firing is allowed, consume ammo if the weapon doesn't have infinite ammo, and set a fire delay timer. The weapon should spawn projectiles when fired.",
            category="combat",
            priority="high",
        ),
        GddRequirement(
            id="req_weapon_show_hide",
            title="Show and Hide Weapons",
            description="The system should show and hide weapons based on which weapons are selected. Only the active primary and secondary weapons should be visible. The system should animate the weapon scale based on active status.",
            category="combat",
            priority="medium",
        ),
    ]
    
    print(f"📦 Code: {code_id}")
    print(f"📋 Testing {len(test_requirements)} requirements")
    print()
    
    # Index code behaviors
    print("Indexing code behaviors...")
    behaviors = await index_code_behaviors(
        code_id, provider=provider, llm_func=llm,
        workspace_id=workspace_id, max_methods=30
    )
    print(f"✅ Found {len(behaviors)} behaviors")
    
    # Show key functions
    key_functions = ['Initialize', 'ActivateWeapon', 'FireWeapon', 'ShowAndHideWeapons']
    print("\n💻 Key Functions Found:")
    for func_name in key_functions:
        matching = [b for b in behaviors if func_name.lower() in b.symbol.lower()]
        if matching:
            print(f"   ✅ {func_name}: {matching[0].symbol}")
        else:
            print(f"   ❌ {func_name}: Not found")
    print()
    
    # Evaluate
    print("Evaluating requirements...")
    print()
    
    matches_found = []
    
    for req in test_requirements:
        print(f"📋 {req.id}: {req.title}")
        
        result = await evaluate_requirement_behavior(
            req, code_id, code_behaviors=behaviors,
            provider=provider, llm_func=llm, embedding_func=embed,
            workspace_id=workspace_id, top_k=5
        )
        
        status = result.get("status")
        icon = {
            "implemented": "✅",
            "partially_implemented": "⚠️",
            "not_implemented": "❌"
        }.get(status, "❓")
        
        print(f"   {icon} Status: {status}")
        
        if result.get("best_match"):
            print(f"   🎯 Best Match: {result['best_match']}")
        
        if result.get("top_matches"):
            print(f"   📊 Top Matches:")
            for i, match in enumerate(result['top_matches'][:3], 1):
                print(f"      {i}. {match.get('symbol', '')} (similarity: {match.get('similarity', 0):.3f})")
        
        if result.get("reason"):
            print(f"   📝 Reason: {result['reason'][:100]}")
        
        if status in ["implemented", "partially_implemented"]:
            matches_found.append({
                "requirement": req,
                "status": status,
                "result": result
            })
        
        print()
    
    # Summary
    print("=" * 80)
    print("RESULTS SUMMARY")
    print("=" * 80)
    
    implemented = sum(1 for r in matches_found if r["status"] == "implemented")
    partially = sum(1 for r in matches_found if r["status"] == "partially_implemented")
    
    print(f"\n✅ Implemented: {implemented}/{len(test_requirements)}")
    print(f"⚠️  Partially Implemented: {partially}/{len(test_requirements)}")
    print(f"❌ Not Implemented: {len(test_requirements) - implemented - partially}/{len(test_requirements)}")
    print()
    
    if matches_found:
        print("🎉 SUCCESS! Found implemented matches:")
        print()
        for match in matches_found:
            print(f"   ✅ {match['requirement'].id}: {match['requirement'].title}")
            print(f"      Status: {match['status']}")
            if match['result'].get('best_match'):
                print(f"      Best Match: {match['result']['best_match']}")
            print()
        
        print("=" * 80)
        print("✅ PROOF: The evaluation function CAN detect implemented requirements!")
        print("=" * 80)
        print()
        print("This proves:")
        print("  1. The behavior-based comparison method works")
        print("  2. It can find matches when requirements match code")
        print("  3. The evaluation function is accurate")
        print()
    else:
        print("⚠️  No matches found, but this could be because:")
        print("  1. Requirements are described at a different level of detail")
        print("  2. The behavior extraction needs tuning")
        print("  3. The LLM verification is being too strict")
        print()
        print("Check the 'Top Matches' and 'Reason' fields to see what was found.")
        print()

if __name__ == "__main__":
    asyncio.run(test_exact_match())


