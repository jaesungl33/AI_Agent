#!/usr/bin/env python3
"""
Find GDD requirements that directly match code functions.

This script:
1. Extracts requirements from a GDD
2. Extracts function names from code
3. Finds direct matches (same or similar names)
4. Shows which requirements have corresponding code functions
"""

import asyncio
import json
import re
import sys
from pathlib import Path

# Add src to path
sys.path.insert(0, str(Path(__file__).parent.parent.parent / "src"))

from gdd_rag_backbone.gdd.extraction import extract_all_requirements
from gdd_rag_backbone.gdd.behavior_indexing import extract_methods_from_chunk
from gdd_rag_backbone.rag_backend.chunk_qa import load_doc_chunks, load_doc_status
from gdd_rag_backbone.llm_providers import QwenProvider, make_llm_model_func

def normalize_name(name):
    """Normalize names for comparison."""
    # Remove special chars, convert to lowercase
    name = re.sub(r'[^a-zA-Z0-9]', '', name.lower())
    return name

def find_similar_names(req_name, code_names, threshold=0.7):
    """Find code names similar to requirement name."""
    req_normalized = normalize_name(req_name)
    matches = []
    
    for code_name in code_names:
        code_normalized = normalize_name(code_name)
        
        # Exact match
        if req_normalized == code_normalized:
            matches.append((code_name, 1.0, "exact"))
            continue
        
        # Substring match
        if req_normalized in code_normalized or code_normalized in req_normalized:
            similarity = min(len(req_normalized), len(code_normalized)) / max(len(req_normalized), len(code_normalized))
            if similarity >= threshold:
                matches.append((code_name, similarity, "substring"))
            continue
        
        # Word overlap
        req_words = set(req_normalized.split())
        code_words = set(code_normalized.split())
        if req_words and code_words:
            overlap = len(req_words & code_words) / len(req_words | code_words)
            if overlap >= threshold:
                matches.append((code_name, overlap, "word_overlap"))
    
    return sorted(matches, key=lambda x: x[1], reverse=True)

async def find_matches():
    """Find GDD requirements that match code functions."""
    
    workspace_id = "tank_war"
    
    print("=" * 80)
    print("FINDING GDD REQUIREMENTS THAT MATCH CODE FUNCTIONS")
    print("=" * 80)
    print()
    
    # Initialize
    provider = QwenProvider()
    llm_func = make_llm_model_func(provider)
    
    # Get status
    status = load_doc_status(workspace_id=workspace_id)
    
    # Find code files (exclude GDDs - they usually have [Asset, UI] or [Module] in name)
    code_files = []
    gdd_files = []
    
    for k, meta in status.items():
        if meta.get('status') not in ['indexed', 'processed']:
            continue
        
        # Heuristic: code files don't have brackets or are explicitly marked
        name_lower = k.lower()
        if (k.startswith('code_') or 
            '[' not in k or  # Simple code files don't have brackets
            ('ability' in name_lower and 'base' in name_lower) or
            ('weapon' in name_lower and 'manager' in name_lower) or
            ('player' in name_lower and 'network' in name_lower)):
            code_files.append(k)
        else:
            gdd_files.append(k)
    
    print(f"📁 Found {len(code_files)} code files")
    print(f"   Sample: {', '.join(code_files[:5])}")
    
    # Find GDDs related to combat/abilities
    combat_gdds = [g for g in gdd_files if any(kw in g.lower() for kw in ['skill', 'ability', 'combat', 'shooting', 'weapon'])]
    
    print(f"📄 Found {len(combat_gdds)} combat-related GDDs")
    print()
    
    # Extract code function names
    print("=" * 80)
    print("STEP 1: Extracting Code Function Names")
    print("=" * 80)
    
    all_code_functions = {}
    
    for code_id in code_files[:5]:  # Limit to 5 for test
        print(f"\n📦 Processing: {code_id}")
        try:
            chunks = load_doc_chunks(code_id, workspace_id=workspace_id)
            functions = []
            
            for chunk in chunks:
                methods = extract_methods_from_chunk(chunk.content, chunk.chunk_id)
                for method in methods:
                    func_name = method.get('symbol', '')
                    if func_name:
                        functions.append(func_name)
            
            all_code_functions[code_id] = functions
            print(f"   ✅ Found {len(functions)} functions")
            if functions:
                print(f"   Sample: {', '.join(functions[:5])}")
        except Exception as e:
            print(f"   ❌ Error: {e}")
    
    print(f"\n✅ Total unique functions: {sum(len(f) for f in all_code_functions.values())}")
    
    # Extract GDD requirements
    print()
    print("=" * 80)
    print("STEP 2: Extracting GDD Requirements")
    print("=" * 80)
    
    all_requirements = {}
    
    for gdd_id in combat_gdds[:3]:  # Limit to 3 for test
        print(f"\n📄 Processing: {gdd_id}")
        try:
            spec_data = await extract_all_requirements(gdd_id, llm_func=llm_func, workspace_id=workspace_id)
            requirements = spec_data.get("requirements", [])
            all_requirements[gdd_id] = requirements
            print(f"   ✅ Found {len(requirements)} requirements")
            if requirements:
                for i, req in enumerate(requirements[:3], 1):
                    print(f"   {i}. {req.get('id', 'unknown')}: {req.get('title', req.get('summary', 'no title'))[:60]}")
        except Exception as e:
            print(f"   ❌ Error: {e}")
            import traceback
            traceback.print_exc()
    
    # Find matches
    print()
    print("=" * 80)
    print("STEP 3: Finding Matches")
    print("=" * 80)
    
    all_code_names = []
    for funcs in all_code_functions.values():
        all_code_names.extend(funcs)
    
    matches_found = []
    
    for gdd_id, requirements in all_requirements.items():
        print(f"\n🔍 Checking GDD: {gdd_id}")
        
        for req in requirements:
            req_id = req.get('id', '')
            req_title = req.get('title', req.get('summary', ''))
            req_desc = req.get('description', req.get('details', ''))
            
            # Search in requirement text
            search_text = f"{req_id} {req_title} {req_desc}".lower()
            
            # Find matching code functions
            code_matches = find_similar_names(search_text, all_code_names, threshold=0.5)
            
            if code_matches:
                best_match = code_matches[0]
                matches_found.append({
                    'gdd_id': gdd_id,
                    'requirement_id': req_id,
                    'requirement_title': req_title,
                    'requirement_desc': req_desc[:100],
                    'code_function': best_match[0],
                    'similarity': best_match[1],
                    'match_type': best_match[2],
                    'all_matches': code_matches[:5],
                })
                print(f"   ✅ {req_id}: {req_title[:50]}")
                print(f"      → Matches: {best_match[0]} ({best_match[2]}, {best_match[1]:.2f})")
    
    # Summary
    print()
    print("=" * 80)
    print("STEP 4: Summary")
    print("=" * 80)
    
    print(f"\n📊 Found {len(matches_found)} potential matches:")
    print()
    
    for i, match in enumerate(matches_found, 1):
        print(f"{i}. {match['requirement_id']}: {match['requirement_title'][:60]}")
        print(f"   GDD: {match['gdd_id']}")
        print(f"   Code Function: {match['code_function']}")
        print(f"   Match Type: {match['match_type']} (similarity: {match['similarity']:.2f})")
        if len(match['all_matches']) > 1:
            print(f"   Other matches: {', '.join([m[0] for m in match['all_matches'][1:3]])}")
        print()
    
    if matches_found:
        print("✅ These are good test cases! The requirements have corresponding code functions.")
        print("   You can test the comparison method with these to verify it detects matches.")
    else:
        print("⚠️  No direct name matches found.")
        print("   This is normal - requirements are often described differently than code.")
        print("   The behavior-based method should still find semantic matches!")
    
    print()
    print("=" * 80)
    print("✅ COMPLETE")
    print("=" * 80)

if __name__ == "__main__":
    asyncio.run(find_matches())

