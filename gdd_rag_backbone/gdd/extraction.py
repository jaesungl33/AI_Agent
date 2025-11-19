"""
GDD extraction functions using RAG + LLM to extract structured data.

This module provides functions to extract structured JSON checklists
from Game Design Documents using RAG queries and LLM processing.
"""
import json
from typing import List, Optional
from gdd_rag_backbone.gdd.schemas import GddObject, TankSpec, MapSpec
from gdd_rag_backbone.rag_backend.query_engine import ask_question
from gdd_rag_backbone.rag_backend.indexing import get_rag_instance_for_doc


async def extract_tanks(doc_id: str, *, llm_func=None) -> List[TankSpec]:
    """
    Extract tank specifications from a GDD using RAG + LLM.
    
    This function:
    1. Uses RAG to retrieve relevant context about tanks from the document
    2. Uses an LLM to generate structured JSON matching the TankSpec schema
    3. Parses and returns a list of TankSpec objects
    
    Args:
        doc_id: Document ID to extract from
        llm_func: Optional LLM function (if not provided, uses the one from RAG instance)
    
    Returns:
        List of TankSpec objects extracted from the document
    
    Example:
        >>> tanks = await extract_tanks("my_gdd_doc")
        >>> for tank in tanks:
        ...     print(f"{tank.name}: {tank.class_name}")
    """
    # Build a comprehensive query to gather tank-related context
    query = (
        "Extract all information about tanks, including tank classes, "
        "names, specifications (size, HP, armor, speed, firepower, range), "
        "special abilities, and gameplay notes. Include all tank-related "
        "content from the document."
    )
    
    # Get RAG instance to access the LLM function if not provided
    rag = get_rag_instance_for_doc(doc_id)
    if rag is None:
        raise ValueError(
            f"No RAG instance found for doc_id: {doc_id}. "
            "Make sure the document has been indexed first."
        )
    
    # Use provided llm_func or get from RAG instance
    if llm_func is None:
        if not hasattr(rag, 'llm_model_func') or rag.llm_model_func is None:
            raise ValueError(
                "LLM function is required but not available. "
                "Either provide llm_func parameter or ensure RAG instance has llm_model_func."
            )
        llm_func = rag.llm_model_func
    
    # Query for context using RAG
    context = await ask_question(doc_id, query, mode="mix")
    
    # Build a detailed prompt for structured extraction
    system_prompt = (
        "You are an expert at extracting structured data from game design documents. "
        "Extract all tank specifications and return them as a JSON array. "
        "Each tank should include: id, class_name, name, size_x, size_y, size_z, "
        "hp, armor, speed, firepower, range, special_abilities, gameplay_notes, source_section. "
        "Only include fields that are actually present in the document. "
        "Return only valid JSON array, no additional text or explanation."
    )
    
    extraction_prompt = f"""
Based on the following context from a Game Design Document, extract all tank specifications.

Context:
{context}

Extract all tanks mentioned in the context. For each tank, provide:
- id: A unique identifier (use tank name as base if no ID is given)
- class_name: Tank class (Heavy, Light, Medium, etc.)
- name: Tank name
- size_x, size_y, size_z: Dimensions in meters (if specified)
- hp: Hit points / Health (if specified)
- armor: Armor value (if specified)
- speed: Movement speed (if specified)
- firepower: Attack damage (if specified)
- range: Attack range in meters (if specified)
- special_abilities: Any special abilities or features
- gameplay_notes: Design notes and gameplay considerations
- source_section: The section or part of document where this tank appears

Return ONLY a JSON array of tank objects, nothing else. If no tanks are found, return an empty array [].
"""
    
    # Call LLM for extraction (await since llm_func is now async)
    response_text = await llm_func(
        prompt=extraction_prompt,
        system_prompt=system_prompt,
        temperature=0.1,  # Lower temperature for more structured output
    )
    
    # Parse JSON response
    try:
        # Try to extract JSON from the response (in case LLM adds extra text)
        response_text = response_text.strip()
        
        # Remove markdown code blocks if present
        if response_text.startswith("```"):
            lines = response_text.split("\n")
            response_text = "\n".join(lines[1:-1]) if lines[-1].strip() == "```" else "\n".join(lines[1:])
        
        # Parse JSON
        tanks_data = json.loads(response_text)
        
        # Validate and convert to TankSpec objects
        if not isinstance(tanks_data, list):
            tanks_data = [tanks_data]
        
        tanks = [TankSpec(**tank) for tank in tanks_data]
        return tanks
    
    except (json.JSONDecodeError, ValueError, TypeError) as e:
        # If parsing fails, return empty list and log error
        print(f"Warning: Failed to parse tank extraction response: {e}")
        print(f"Response was: {response_text[:500]}...")
        return []


async def extract_breakable_objects(doc_id: str, *, llm_func=None) -> List[GddObject]:
    """
    Extract breakable objects (BR category) from a GDD.
    
    Args:
        doc_id: Document ID to extract from
        llm_func: Optional LLM function (if not provided, uses the one from RAG instance)
    
    Returns:
        List of GddObject objects with category="BR"
    """
    query = (
        "Extract all information about breakable objects (BR category), "
        "including names, sizes (x, y, z), HP, destructibility, "
        "interaction rules (player/bullet pass-through), and special notes."
    )
    
    rag = get_rag_instance_for_doc(doc_id)
    if rag is None:
        raise ValueError(
            f"No RAG instance found for doc_id: {doc_id}. "
            "Make sure the document has been indexed first."
        )
    
    if llm_func is None:
        if not hasattr(rag, 'llm_model_func') or rag.llm_model_func is None:
            raise ValueError("LLM function is required but not available.")
        llm_func = rag.llm_model_func
    
    context = await ask_question(doc_id, query, mode="mix")
    
    system_prompt = (
        "Extract breakable objects (BR category) from game design documents. "
        "Return a JSON array with fields: id, category (always 'BR'), name, "
        "size_x, size_y, size_z, hp, player_pass_through, bullet_pass_through, "
        "destructible, special_notes, source_section. Return only valid JSON array."
    )
    
    extraction_prompt = f"""
Extract all breakable objects from this context:

{context}

For each object, provide:
- id: Unique identifier
- category: "BR"
- name: Object name
- size_x, size_y, size_z: Dimensions (if specified)
- hp: Hit points (if specified)
- player_pass_through: Can players pass through? (if specified)
- bullet_pass_through: Can bullets pass through? (if specified)
- destructible: Is it destructible? (if specified)
- special_notes: Any special notes
- source_section: Section where this appears

Return ONLY a JSON array, nothing else. If none found, return [].
"""
    
    response_text = await llm_func(
        prompt=extraction_prompt,
        system_prompt=system_prompt,
        temperature=0.1,
    )
    
    try:
        response_text = response_text.strip()
        if response_text.startswith("```"):
            lines = response_text.split("\n")
            response_text = "\n".join(lines[1:-1]) if lines[-1].strip() == "```" else "\n".join(lines[1:])
        
        objects_data = json.loads(response_text)
        if not isinstance(objects_data, list):
            objects_data = [objects_data]
        
        objects = [GddObject(id=obj.get("id", ""), category="BR", name=obj.get("name", ""), **{k: v for k, v in obj.items() if k not in ["id", "category", "name"]}) for obj in objects_data]
        return objects
    except (json.JSONDecodeError, ValueError, TypeError) as e:
        print(f"Warning: Failed to parse breakable objects extraction: {e}")
        return []


async def extract_hiding_objects(doc_id: str, *, llm_func=None) -> List[GddObject]:
    """
    Extract hiding objects (HI category) from a GDD.
    
    Args:
        doc_id: Document ID to extract from
        llm_func: Optional LLM function (if not provided, uses the one from RAG instance)
    
    Returns:
        List of GddObject objects with category="HI"
    """
    query = (
        "Extract all information about hiding objects (HI category), "
        "including names, sizes (x, y, z), HP, interaction rules, "
        "and tactical/stealth notes."
    )
    
    rag = get_rag_instance_for_doc(doc_id)
    if rag is None:
        raise ValueError(
            f"No RAG instance found for doc_id: {doc_id}. "
            "Make sure the document has been indexed first."
        )
    
    if llm_func is None:
        if not hasattr(rag, 'llm_model_func') or rag.llm_model_func is None:
            raise ValueError("LLM function is required but not available.")
        llm_func = rag.llm_model_func
    
    context = await ask_question(doc_id, query, mode="mix")
    
    system_prompt = (
        "Extract hiding objects (HI category) from game design documents. "
        "Return a JSON array with fields: id, category (always 'HI'), name, "
        "size_x, size_y, size_z, hp, player_pass_through, bullet_pass_through, "
        "destructible, special_notes, source_section. Return only valid JSON array."
    )
    
    extraction_prompt = f"""
Extract all hiding objects from this context:

{context}

For each object, provide:
- id: Unique identifier
- category: "HI"
- name: Object name
- size_x, size_y, size_z: Dimensions (if specified)
- hp: Hit points (if specified)
- player_pass_through: Can players pass through? (if specified)
- bullet_pass_through: Can bullets pass through? (if specified)
- destructible: Is it destructible? (if specified)
- special_notes: Any special tactical/stealth notes
- source_section: Section where this appears

Return ONLY a JSON array, nothing else. If none found, return [].
"""
    
    response_text = await llm_func(
        prompt=extraction_prompt,
        system_prompt=system_prompt,
        temperature=0.1,
    )
    
    try:
        response_text = response_text.strip()
        if response_text.startswith("```"):
            lines = response_text.split("\n")
            response_text = "\n".join(lines[1:-1]) if lines[-1].strip() == "```" else "\n".join(lines[1:])
        
        objects_data = json.loads(response_text)
        if not isinstance(objects_data, list):
            objects_data = [objects_data]
        
        objects = [GddObject(id=obj.get("id", ""), category="HI", name=obj.get("name", ""), **{k: v for k, v in obj.items() if k not in ["id", "category", "name"]}) for obj in objects_data]
        return objects
    except (json.JSONDecodeError, ValueError, TypeError) as e:
        print(f"Warning: Failed to parse hiding objects extraction: {e}")
        return []


async def extract_objects(doc_id: str, category: Optional[str] = None, *, llm_func=None) -> List[GddObject]:
    """
    Extract game objects from a GDD, optionally filtered by category.
    
    Categories include: BR (Breakable), BO (Blocking), DE (Decorative),
    GR (Ground), HI (Hiding), OP (Objective Points), TA (Tactical)
    
    Args:
        doc_id: Document ID to extract from
        category: Optional category filter (BR, HI, TA, etc.)
        llm_func: Optional LLM function (if not provided, uses the one from RAG instance)
    
    Returns:
        List of GddObject objects
    """
    if category:
        # Use category-specific extraction if available
        if category == "BR":
            return await extract_breakable_objects(doc_id, llm_func=llm_func)
        elif category == "HI":
            return await extract_hiding_objects(doc_id, llm_func=llm_func)
        # For other categories, fall through to general extraction
    
    query = (
        "Extract all information about game objects, including categories "
        "(BR, BO, DE, GR, HI, OP, TA), names, sizes, HP, interaction rules, "
        "and special notes."
    )
    
    rag = get_rag_instance_for_doc(doc_id)
    if rag is None:
        raise ValueError(
            f"No RAG instance found for doc_id: {doc_id}. "
            "Make sure the document has been indexed first."
        )
    
    if llm_func is None:
        if not hasattr(rag, 'llm_model_func') or rag.llm_model_func is None:
            raise ValueError("LLM function is required but not available.")
        llm_func = rag.llm_model_func
    
    context = await ask_question(doc_id, query, mode="mix")
    
    category_filter = f" with category '{category}'" if category else ""
    
    system_prompt = (
        "Extract game objects from game design documents. "
        "Return a JSON array with fields: id, category, name, "
        "size_x, size_y, size_z, hp, player_pass_through, bullet_pass_through, "
        "destructible, special_notes, source_section. Return only valid JSON array."
    )
    
    extraction_prompt = f"""
Extract all game objects{category_filter} from this context:

{context}

For each object, provide:
- id: Unique identifier
- category: Object category (BR, BO, DE, GR, HI, OP, TA)
- name: Object name
- size_x, size_y, size_z: Dimensions (if specified)
- hp: Hit points (if specified)
- player_pass_through: Can players pass through? (if specified)
- bullet_pass_through: Can bullets pass through? (if specified)
- destructible: Is it destructible? (if specified)
- special_notes: Any special notes
- source_section: Section where this appears

Return ONLY a JSON array, nothing else. If none found, return [].
"""
    
    response_text = await llm_func(
        prompt=extraction_prompt,
        system_prompt=system_prompt,
        temperature=0.1,
    )
    
    try:
        response_text = response_text.strip()
        if response_text.startswith("```"):
            lines = response_text.split("\n")
            response_text = "\n".join(lines[1:-1]) if lines[-1].strip() == "```" else "\n".join(lines[1:])
        
        objects_data = json.loads(response_text)
        if not isinstance(objects_data, list):
            objects_data = [objects_data]
        
        objects = [GddObject(id=obj.get("id", ""), category=obj.get("category", ""), name=obj.get("name", ""), **{k: v for k, v in obj.items() if k not in ["id", "category", "name"]}) for obj in objects_data]
        
        # Filter by category if specified
        if category:
            objects = [obj for obj in objects if obj.category == category]
        
        return objects
    except (json.JSONDecodeError, ValueError, TypeError) as e:
        print(f"Warning: Failed to parse objects extraction: {e}")
        return []


async def extract_maps(doc_id: str, *, llm_func=None) -> List[MapSpec]:
    """
    Extract map specifications from a GDD using RAG + LLM.
    
    Args:
        doc_id: Document ID to extract from
        llm_func: Optional LLM function (if not provided, uses the one from RAG instance)
    
    Returns:
        List of MapSpec objects extracted from the document
    """
    query = (
        "Extract all information about maps, including map names, game modes, "
        "scene/environment types, sizes, player counts, objective locations, "
        "spawn points, cover elements, special features, and gameplay notes."
    )
    
    rag = get_rag_instance_for_doc(doc_id)
    if rag is None:
        raise ValueError(
            f"No RAG instance found for doc_id: {doc_id}. "
            "Make sure the document has been indexed first."
        )
    
    if llm_func is None:
        if not hasattr(rag, 'llm_model_func') or rag.llm_model_func is None:
            raise ValueError("LLM function is required but not available.")
        llm_func = rag.llm_model_func
    
    context = await ask_question(doc_id, query, mode="mix")
    
    system_prompt = (
        "Extract map specifications from game design documents. "
        "Return a JSON array with fields: id, name, mode, scene, size_x, size_y, "
        "player_count, objective_locations, spawn_points, cover_elements, "
        "special_features, gameplay_notes, source_section. Return only valid JSON array."
    )
    
    extraction_prompt = f"""
Extract all map specifications from this context:

{context}

For each map, provide:
- id: Unique identifier
- name: Map name
- mode: Game mode (CTF, TDM, Assault, etc.)
- scene: Environment type
- size_x, size_y: Map dimensions in meters (if specified)
- player_count: Recommended player count (if specified)
- objective_locations: Description of objective points
- spawn_points: Number of spawn points (if specified)
- cover_elements: Description of cover and tactical elements
- special_features: Unique map features
- gameplay_notes: Design notes and gameplay considerations
- source_section: Section where this appears

Return ONLY a JSON array, nothing else. If none found, return [].
"""
    
    response_text = await llm_func(
        prompt=extraction_prompt,
        system_prompt=system_prompt,
        temperature=0.1,
    )
    
    try:
        response_text = response_text.strip()
        if response_text.startswith("```"):
            lines = response_text.split("\n")
            response_text = "\n".join(lines[1:-1]) if lines[-1].strip() == "```" else "\n".join(lines[1:])
        
        maps_data = json.loads(response_text)
        if not isinstance(maps_data, list):
            maps_data = [maps_data]
        
        maps = [MapSpec(**map_data) for map_data in maps_data]
        return maps
    except (json.JSONDecodeError, ValueError, TypeError) as e:
        print(f"Warning: Failed to parse maps extraction: {e}")
        return []

