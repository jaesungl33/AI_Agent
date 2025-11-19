"""
Tests for structured data extraction.
"""
import pytest
import asyncio
from gdd_rag_backbone.gdd import extract_tanks, extract_maps, extract_objects


def test_extract_tanks_nonexistent_doc():
    """Test that extracting tanks from nonexistent doc raises ValueError."""
    async def run_test():
        with pytest.raises(ValueError) as exc_info:
            await extract_tanks("nonexistent_doc")
        
        assert "No RAG instance found" in str(exc_info.value)
    
    asyncio.run(run_test())


def test_extract_tanks_structure():
    """Test that extract_tanks function exists and has correct signature."""
    import inspect
    from gdd_rag_backbone.gdd.extraction import extract_tanks
    
    sig = inspect.signature(extract_tanks)
    
    # Check required parameters
    assert "doc_id" in sig.parameters
    
    # Check it's async
    assert inspect.iscoroutinefunction(extract_tanks)
    
    # Check return type annotation suggests List
    return_annotation = sig.return_annotation
    assert "List" in str(return_annotation) or "list" in str(return_annotation).lower()


def test_extract_maps_structure():
    """Test that extract_maps function exists and has correct signature."""
    import inspect
    from gdd_rag_backbone.gdd.extraction import extract_maps
    
    sig = inspect.signature(extract_maps)
    
    # Check required parameters
    assert "doc_id" in sig.parameters
    
    # Check it's async
    assert inspect.iscoroutinefunction(extract_maps)


def test_extract_objects_structure():
    """Test that extract_objects function exists and has correct signature."""
    import inspect
    from gdd_rag_backbone.gdd.extraction import extract_objects
    
    sig = inspect.signature(extract_objects)
    
    # Check required parameters
    assert "doc_id" in sig.parameters
    
    # Check it's async
    assert inspect.iscoroutinefunction(extract_objects)


def test_extraction_functions_import():
    """Test that extraction functions can be imported correctly."""
    from gdd_rag_backbone.gdd.extraction import (
        extract_tanks,
        extract_maps,
        extract_objects,
        extract_breakable_objects,
        extract_hiding_objects,
    )
    
    assert extract_tanks is not None
    assert extract_maps is not None
    assert extract_objects is not None
    assert extract_breakable_objects is not None
    assert extract_hiding_objects is not None

