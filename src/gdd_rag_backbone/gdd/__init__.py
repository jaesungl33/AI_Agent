"""
GDD (Game Design Document) extraction layer.

This module provides schemas and functions for extracting structured data
from Game Design Documents, such as objects, tanks, maps, etc.
"""

from gdd_rag_backbone.gdd.schemas import (
    GddObject,
    TankSpec,
    GddMap,
    GddSystem,
    GddInteraction,
    GddRequirement,
    GddLogicRule,
    RequirementSpec,
    BehaviorRequirement,
    CodeBehavior,
)
from gdd_rag_backbone.gdd.extraction import (
    extract_objects,
    extract_breakable_objects,
    extract_hiding_objects,
    extract_tanks,
    extract_maps,
    extract_requirements,
    extract_all_requirements,
    convert_to_behavior_requirement,
    extract_behavior_requirements,
)
from gdd_rag_backbone.gdd.requirement_matching import (
    evaluate_requirement,
    evaluate_all_requirements,
    generate_code_queries,
    search_code_chunks,
    classify_requirement_coverage,
    evaluate_requirement_behavior,
    evaluate_all_requirements_behavior,
)
from gdd_rag_backbone.gdd.behavior_indexing import (
    index_code_behaviors,
    save_behavior_index,
    load_behavior_index,
)
from gdd_rag_backbone.gdd.behavior_matching import (
    find_matching_behaviors,
    batch_find_matching_behaviors,
)
from gdd_rag_backbone.gdd.analysis import analyze_gdd
from gdd_rag_backbone.gdd.todo import generate_todo_list

__all__ = [
    "GddObject",
    "TankSpec",
    "GddMap",
    "GddSystem",
    "GddInteraction",
    "GddRequirement",
    "GddLogicRule",
    "RequirementSpec",
    "BehaviorRequirement",
    "CodeBehavior",
    "extract_objects",
    "extract_breakable_objects",
    "extract_hiding_objects",
    "extract_tanks",
    "extract_maps",
    "extract_requirements",
    "extract_all_requirements",
    "convert_to_behavior_requirement",
    "extract_behavior_requirements",
    "analyze_gdd",
    "generate_todo_list",
    "evaluate_requirement",
    "evaluate_all_requirements",
    "evaluate_requirement_behavior",
    "evaluate_all_requirements_behavior",
    "generate_code_queries",
    "search_code_chunks",
    "classify_requirement_coverage",
    "index_code_behaviors",
    "save_behavior_index",
    "load_behavior_index",
    "find_matching_behaviors",
    "batch_find_matching_behaviors",
]

