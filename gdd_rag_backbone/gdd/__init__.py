"""
GDD (Game Design Document) extraction layer.

This module provides schemas and functions for extracting structured data
from Game Design Documents, such as objects, tanks, maps, etc.
"""

from gdd_rag_backbone.gdd.schemas import (
    GddObject,
    TankSpec,
    MapSpec,
)
from gdd_rag_backbone.gdd.extraction import (
    extract_objects,
    extract_breakable_objects,
    extract_hiding_objects,
    extract_tanks,
    extract_maps,
)

__all__ = [
    "GddObject",
    "TankSpec",
    "MapSpec",
    "extract_objects",
    "extract_breakable_objects",
    "extract_hiding_objects",
    "extract_tanks",
    "extract_maps",
]

