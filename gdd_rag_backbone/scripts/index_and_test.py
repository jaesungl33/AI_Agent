#!/usr/bin/env python3
"""
Script to index a GDD document and run test queries.

This script demonstrates the basic workflow:
1. Index a document using RAG-Anything
2. Ask questions about the document
3. Optionally extract structured data (tanks, maps, objects)

Usage:
    python scripts/index_and_test.py [doc_path] [doc_id]
    
    Or set environment variables:
    - GDD_DOC_PATH: Path to the GDD document
    - GDD_DOC_ID: Document ID (defaults to filename without extension)
"""
import asyncio
import sys
from pathlib import Path

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).parent.parent.parent))

from gdd_rag_backbone.config import DEFAULT_DOCS_DIR
from gdd_rag_backbone.llm_providers import QwenProvider, make_llm_model_func, make_embedding_func
from gdd_rag_backbone.rag_backend import index_document, ask_question
from gdd_rag_backbone.gdd import extract_tanks, extract_maps


async def main():
    """Main function to index and test a GDD document."""
    import os
    
    # Get document path and ID from args or environment
    if len(sys.argv) > 1:
        doc_path = Path(sys.argv[1])
        doc_id = sys.argv[2] if len(sys.argv) > 2 else doc_path.stem
    else:
        doc_path_env = os.getenv("GDD_DOC_PATH")
        if doc_path_env:
            doc_path = Path(doc_path_env)
        else:
            # Look for sample GDD in docs directory
            doc_path = DEFAULT_DOCS_DIR / "sample_gdd.pdf"
            if not doc_path.exists():
                print(f"Error: No document found. Please provide a document path.")
                print(f"Usage: python {sys.argv[0]} <doc_path> [doc_id]")
                print(f"Or set GDD_DOC_PATH environment variable.")
                print(f"Or place a document at {doc_path}")
                sys.exit(1)
        
        doc_id = os.getenv("GDD_DOC_ID", doc_path.stem)
    
    print(f"Indexing document: {doc_path}")
    print(f"Document ID: {doc_id}")
    print("-" * 80)
    
    # Initialize LLM provider (use Qwen as default, or create a mock if API key not set)
    try:
        provider = QwenProvider()
        llm_func = make_llm_model_func(provider)
        embedding_func = make_embedding_func(provider)
        print("Using Qwen provider")
    except ValueError:
        print("Warning: QWEN_API_KEY not set. Using placeholder/mock functions.")
        print("Set QWEN_API_KEY environment variable to use real LLM.")
        
        # Create mock functions for testing structure without real API
        def mock_llm_func(prompt: str, system_prompt: str = None, **kwargs) -> str:
            return f"[Mock LLM Response] This is a placeholder response for testing. Prompt: {prompt[:100]}..."
        
        def mock_embedding_func(text_list: list) -> list:
            # Return dummy embeddings (1536 dimensions)
            return [[0.0] * 1536 for _ in text_list]
        
        llm_func = mock_llm_func
        embedding_func = mock_embedding_func
    
    # Get parser from args or use default
    parser = "docling" if "--parser" in sys.argv or "docling" in sys.argv else None
    
    # Index the document
    try:
        await index_document(
            doc_path=doc_path,
            doc_id=doc_id,
            llm_func=llm_func,
            embedding_func=embedding_func,
            parser=parser,
        )
        print(f"\n✓ Document indexed successfully!")
    except Exception as e:
        print(f"\n✗ Error indexing document: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)
    
    # Test queries
    print("\n" + "=" * 80)
    print("Running test queries...")
    print("=" * 80)
    
    test_queries = [
        "What is this document about?",
        "List all tanks mentioned in the document.",
        "What maps are described in this document?",
        "What game objects are mentioned?",
    ]
    
    for i, query in enumerate(test_queries, 1):
        print(f"\n{'─' * 80}")
        print(f"Query {i}: {query}")
        print("─" * 80)
        
        try:
            answer = await ask_question(doc_id, query, debug=False)
            print(f"Answer:\n{answer}")
        except Exception as e:
            print(f"Error: {e}")
            import traceback
            traceback.print_exc()
    
    # Test structured extraction
    print("\n" + "=" * 80)
    print("Testing structured data extraction...")
    print("=" * 80)
    
    # Extract tanks
    print("\nExtracting tank specifications...")
    try:
        tanks = await extract_tanks(doc_id, llm_func=llm_func)
        print(f"Found {len(tanks)} tank(s):")
        for tank in tanks:
            print(f"  - {tank.name} ({tank.class_name})")
            if tank.hp:
                print(f"    HP: {tank.hp}")
            if tank.special_abilities:
                print(f"    Abilities: {tank.special_abilities}")
    except Exception as e:
        print(f"Error extracting tanks: {e}")
        import traceback
        traceback.print_exc()
    
    # Extract maps
    print("\nExtracting map specifications...")
    try:
        maps = await extract_maps(doc_id, llm_func=llm_func)
        print(f"Found {len(maps)} map(s):")
        for map_spec in maps:
            print(f"  - {map_spec.name}")
            if map_spec.mode:
                print(f"    Mode: {map_spec.mode}")
            if map_spec.scene:
                print(f"    Scene: {map_spec.scene}")
    except Exception as e:
        print(f"Error extracting maps: {e}")
        import traceback
        traceback.print_exc()
    
    print("\n" + "=" * 80)
    print("Done!")
    print("=" * 80)


if __name__ == "__main__":
    asyncio.run(main())

