#!/usr/bin/env python3
"""
Quick test of evaluation function with related GDD and code.

Tests: Skill Design Document vs AbilityBase.cs
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

async def quick_test():
    workspace_id = "tank_war"
    gdd_id = "[Combat_Module]_[Tank_War]_Skill_Design_Document"
    code_id = "AbilityBase"
    
    print("=" * 80)
    print("QUICK EVALUATION TEST")
    print("=" * 80)
    print(f"GDD: {gdd_id}")
    print(f"Code: {code_id}")
    print()
    
    provider = QwenProvider()
    llm = make_llm_model_func(provider)
    embed = make_embedding_func(provider)
    
    # Extract requirements
    print("Extracting requirements...")
    spec = await extract_all_requirements(gdd_id, llm_func=llm, workspace_id=workspace_id)
    reqs_data = spec.get("requirements", [])[:3]  # Just 3 for quick test
    
    if not reqs_data:
        print("❌ No requirements found")
        return
    
    print(f"✅ Found {len(reqs_data)} requirements")
    for req in reqs_data:
        print(f"   - {req.get('id')}: {req.get('title', req.get('summary', ''))[:50]}")
    print()
    
    # Index code
    print("Indexing code behaviors...")
    behaviors = await index_code_behaviors(code_id, provider=provider, llm_func=llm, workspace_id=workspace_id, max_methods=20)
    print(f"✅ Found {len(behaviors)} behaviors")
    print()
    
    # Evaluate
    print("Evaluating...")
    print()
    
    for req_dict in reqs_data:
        req = GddRequirement(
            id=req_dict.get("id", ""),
            title=req_dict.get("title", req_dict.get("summary", "")),
            description=req_dict.get("description", req_dict.get("details", "")),
        )
        
        print(f"📋 {req.id}: {req.title[:50]}")
        
        result = await evaluate_requirement_behavior(
            req, code_id, code_behaviors=behaviors,
            provider=provider, llm_func=llm, embedding_func=embed,
            workspace_id=workspace_id, top_k=3
        )
        
        status = result.get("status")
        icon = {"implemented": "✅", "partially_implemented": "⚠️", "not_implemented": "❌"}.get(status, "❓")
        print(f"   {icon} {status}")
        
        if result.get("best_match"):
            print(f"   🎯 {result['best_match']}")
        if result.get("top_matches"):
            print(f"   📊 Top: {result['top_matches'][0].get('symbol', '')[:40]}")
        print()
    
    print("✅ Test complete!")

if __name__ == "__main__":
    asyncio.run(quick_test())


