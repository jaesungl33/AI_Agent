"""
Structured dataclasses for the GDD pipeline.
"""

from dataclasses import asdict, dataclass, field
from typing import List, Optional


@dataclass
class GddObject:
    id: str
    name: str
    category: Optional[str] = None
    description: Optional[str] = None
    size_x: Optional[float] = None
    size_y: Optional[float] = None
    size_z: Optional[float] = None
    hp: Optional[int] = None
    armor: Optional[int] = None
    speed: Optional[float] = None
    player_pass_through: Optional[bool] = None
    bullet_pass_through: Optional[bool] = None
    destructible: Optional[bool] = None
    special_rules: Optional[str] = None
    source_note: Optional[str] = None

    def to_dict(self) -> dict:
        return asdict(self)


@dataclass
class TankSpec:
    id: str
    name: Optional[str] = None
    class_name: Optional[str] = None
    size_x: Optional[float] = None
    size_y: Optional[float] = None
    size_z: Optional[float] = None
    hp: Optional[int] = None
    armor: Optional[int] = None
    speed: Optional[float] = None
    firepower: Optional[float] = None
    range: Optional[float] = None
    special_abilities: Optional[str] = None
    gameplay_notes: Optional[str] = None
    source_note: Optional[str] = None

    def to_dict(self) -> dict:
        return asdict(self)


@dataclass
class GddMap:
    id: str
    name: str
    mode: Optional[str] = None
    scene: Optional[str] = None
    size_x: Optional[float] = None
    size_y: Optional[float] = None
    player_count: Optional[int] = None
    objective_locations: Optional[str] = None
    spawn_points: Optional[int] = None
    cover_elements: Optional[str] = None
    special_features: Optional[str] = None
    gameplay_notes: Optional[str] = None
    source_note: Optional[str] = None

    def to_dict(self) -> dict:
        return asdict(self)


@dataclass
class GddSystem:
    id: str
    name: str
    description: Optional[str] = None
    mechanics: Optional[str] = None
    objectives: Optional[str] = None
    related_objects: List[str] = field(default_factory=list)
    interactions: List[str] = field(default_factory=list)
    source_note: Optional[str] = None

    def to_dict(self) -> dict:
        return asdict(self)


@dataclass
class GddInteraction:
    id: str
    summary: str
    description: Optional[str] = None
    trigger: Optional[str] = None
    effect: Optional[str] = None
    related_objects: List[str] = field(default_factory=list)
    related_systems: List[str] = field(default_factory=list)
    source_note: Optional[str] = None

    def to_dict(self) -> dict:
        return asdict(self)


@dataclass
class GddLogicRule:
    id: str
    statement: str
    applies_to: List[str] = field(default_factory=list)
    condition: Optional[str] = None
    result: Optional[str] = None
    priority: Optional[str] = None
    source_note: Optional[str] = None

    def to_dict(self) -> dict:
        return asdict(self)


@dataclass
class GddRequirement:
    id: str
    title: str
    description: str
    summary: Optional[str] = None
    category: Optional[str] = None
    priority: Optional[str] = None
    status: Optional[str] = None
    acceptance_criteria: Optional[str] = None
    related_objects: List[str] = field(default_factory=list)
    related_systems: List[str] = field(default_factory=list)
    source_note: Optional[str] = None
    # Extended structured fields
    triggers: List[str] = field(default_factory=list)
    effects: List[str] = field(default_factory=list)
    entities_involved: List[str] = field(default_factory=list)
    expected_code_anchors: List[str] = field(default_factory=list)  # e.g., ["Class.Method", "OtherClass.fn"]

    def to_dict(self) -> dict:
        return asdict(self)


@dataclass
class RequirementSpec:
    """Backwards-compatible alias used by older helpers."""

    id: str
    summary: str
    category: Optional[str] = None
    details: Optional[str] = None
    priority: Optional[str] = None
    status: Optional[str] = None
    acceptance_criteria: Optional[str] = None
    source_section: Optional[str] = None

    def to_dict(self) -> dict:
        return asdict(self)


@dataclass
class BehaviorRequirement:
    """
    QA-style structured behavior requirement extracted from GDD.
    This is the bridge between GDD and code - focuses on behavior, not implementation details.
    """
    id: str
    summary: str
    triggers: List[str] = field(default_factory=list)
    effects: List[str] = field(default_factory=list)
    entities: List[str] = field(default_factory=list)
    conditions: List[str] = field(default_factory=list)
    priority: Optional[str] = None
    expected_code_anchor: Optional[str] = None  # Optional hint, not strict
    source_requirement_id: Optional[str] = None  # Link back to original GddRequirement
    
    def to_dict(self) -> dict:
        return asdict(self)
    
    def to_behavior_text(self) -> str:
        """Convert to a structured text representation for embedding/matching."""
        return f"""Feature: {self.summary or 'Unknown'}
Triggers: {', '.join(self.triggers) if self.triggers else 'None'}
Effects: {', '.join(self.effects) if self.effects else 'None'}
Entities: {', '.join(self.entities) if self.entities else 'None'}
Conditions: {', '.join(self.conditions) if self.conditions else 'None'}""".strip()


@dataclass
class CodeBehavior:
    """
    Behavior description extracted from a code method/function.
    This is a lightweight representation of what a code method does.
    """
    symbol: str  # e.g., "HidingGrass.OnTriggerEnter"
    description: str  # What this method does
    trigger_patterns: List[str] = field(default_factory=list)  # e.g., ["OnTriggerEnter", "enter"]
    effect_patterns: List[str] = field(default_factory=list)  # e.g., ["player invisible", "stealth"]
    entities: List[str] = field(default_factory=list)  # e.g., ["Player", "Grass", "HidingSystem"]
    embedding: Optional[List[float]] = None  # cached embedding for fast similarity
    file_path: Optional[str] = None
    chunk_id: Optional[str] = None  # Link to original chunk
    
    def to_dict(self) -> dict:
        return asdict(self)
    
    def to_behavior_text(self) -> str:
        """Convert to a structured text representation for embedding/matching."""
        return f"""Feature: {self.description or self.symbol}
Symbol: {self.symbol}
Triggers: {', '.join(self.trigger_patterns) if self.trigger_patterns else 'None'}
Effects: {', '.join(self.effect_patterns) if self.effect_patterns else 'None'}
Entities: {', '.join(self.entities) if self.entities else 'None'}""".strip()


@dataclass
class EvaluationResult:
    """
    Unified evaluation result for behavior-based coverage.
    """
    requirement_id: str
    status: str  # implemented | partially_implemented | not_implemented | error
    best_match: Optional[str] = None  # symbol of the best code behavior match
    similarity: Optional[float] = None
    llm_reason: Optional[str] = None
    missing_triggers: List[str] = field(default_factory=list)
    missing_effects: List[str] = field(default_factory=list)
    coverage_type: str = "behavior"
    matched_symbols: List[str] = field(default_factory=list)

    def to_dict(self) -> dict:
        return asdict(self)


__all__ = [
    "GddObject",
    "TankSpec",
    "GddMap",
    "GddSystem",
    "GddInteraction",
    "GddLogicRule",
    "GddRequirement",
    "RequirementSpec",
    "BehaviorRequirement",
    "CodeBehavior",
    "EvaluationResult",
]
