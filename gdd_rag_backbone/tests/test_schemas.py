"""
Tests for GDD schemas.
"""
import pytest
from gdd_rag_backbone.gdd.schemas import GddObject, TankSpec, MapSpec


def test_gdd_object_creation():
    """Test creating a GddObject."""
    obj = GddObject(
        id="obj_1",
        category="BR",
        name="Barrel",
        size_x=1.0,
        size_y=1.0,
        size_z=1.5,
        hp=100,
        destructible=True,
    )
    
    assert obj.id == "obj_1"
    assert obj.category == "BR"
    assert obj.name == "Barrel"
    assert obj.size_x == 1.0
    assert obj.destructible is True
    
    # Test to_dict conversion
    obj_dict = obj.to_dict()
    assert isinstance(obj_dict, dict)
    assert obj_dict["id"] == "obj_1"


def test_tank_spec_creation():
    """Test creating a TankSpec."""
    tank = TankSpec(
        id="tank_1",
        class_name="Heavy",
        name="Tiger Tank",
        hp=500,
        armor=100,
        speed=30.0,
    )
    
    assert tank.id == "tank_1"
    assert tank.class_name == "Heavy"
    assert tank.name == "Tiger Tank"
    assert tank.hp == 500
    assert tank.armor == 100


def test_map_spec_creation():
    """Test creating a MapSpec."""
    map_spec = MapSpec(
        id="map_1",
        name="Desert Oasis",
        mode="CTF",
        scene="Desert",
        player_count=16,
    )
    
    assert map_spec.id == "map_1"
    assert map_spec.name == "Desert Oasis"
    assert map_spec.mode == "CTF"
    assert map_spec.player_count == 16


def test_schemas_import():
    """Test that schemas can be imported correctly."""
    from gdd_rag_backbone.gdd.schemas import GddObject, TankSpec, MapSpec
    
    assert GddObject is not None
    assert TankSpec is not None
    assert MapSpec is not None

