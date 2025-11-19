"""
Pydantic schemas for structured GDD data extraction.

This module defines the data models for various game design elements
that can be extracted from Game Design Documents.
"""
from typing import Optional
from dataclasses import dataclass, asdict
from pydantic import BaseModel, Field


@dataclass
class GddObject:
    """
    Represents a game object with physical and interaction properties.
    
    Categories include:
    - BR: Breakable objects
    - BO: Blocking objects
    - DE: Decorative objects
    - GR: Ground objects
    - HI: Hiding objects
    - OP: Objective points
    - TA: Tactical objects
    """
    id: str
    category: str  # e.g., "BR", "HI", "TA"
    name: str
    size_x: Optional[float] = None
    size_y: Optional[float] = None
    size_z: Optional[float] = None
    hp: Optional[int] = None
    player_pass_through: Optional[bool] = None
    bullet_pass_through: Optional[bool] = None
    destructible: Optional[bool] = None
    special_notes: Optional[str] = None
    source_section: Optional[str] = None
    
    def to_dict(self) -> dict:
        """Convert to dictionary."""
        return asdict(self)


class TankSpec(BaseModel):
    """
    Represents a tank specification with class, attributes, and gameplay notes.
    """
    id: str = Field(description="Unique identifier for the tank")
    class_name: str = Field(description="Tank class (e.g., 'Heavy', 'Light', 'Medium')")
    name: str = Field(description="Tank name")
    size_x: Optional[float] = Field(None, description="Length in meters")
    size_y: Optional[float] = Field(None, description="Width in meters")
    size_z: Optional[float] = Field(None, description="Height in meters")
    hp: Optional[int] = Field(None, description="Hit points / Health")
    armor: Optional[int] = Field(None, description="Armor value")
    speed: Optional[float] = Field(None, description="Movement speed")
    firepower: Optional[int] = Field(None, description="Attack damage")
    range: Optional[float] = Field(None, description="Attack range in meters")
    special_abilities: Optional[str] = Field(None, description="Special abilities or features")
    gameplay_notes: Optional[str] = Field(None, description="Design notes and gameplay considerations")
    source_section: Optional[str] = Field(None, description="Section of GDD where this was found")


class MapSpec(BaseModel):
    """
    Represents a map specification with mode, scene, size, and layout information.
    """
    id: str = Field(description="Unique identifier for the map")
    name: str = Field(description="Map name")
    mode: Optional[str] = Field(None, description="Game mode (e.g., 'CTF', 'TDM', 'Assault')")
    scene: Optional[str] = Field(None, description="Scene or environment type")
    size_x: Optional[float] = Field(None, description="Map length in meters")
    size_y: Optional[float] = Field(None, description="Map width in meters")
    player_count: Optional[int] = Field(None, description="Recommended player count")
    objective_locations: Optional[str] = Field(None, description="Description of objective points")
    spawn_points: Optional[int] = Field(None, description="Number of spawn points")
    cover_elements: Optional[str] = Field(None, description="Description of cover and tactical elements")
    special_features: Optional[str] = Field(None, description="Unique map features")
    gameplay_notes: Optional[str] = Field(None, description="Design notes and gameplay considerations")
    source_section: Optional[str] = Field(None, description="Section of GDD where this was found")

