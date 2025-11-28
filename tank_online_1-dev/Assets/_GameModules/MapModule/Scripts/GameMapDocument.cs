using Fusion;
using UnityEngine;

[System.Serializable]
public class GameMapDocument
{
    public string mapID; // Unique identifier for the map
    public MapType mapType; // Type of the map (e.g., Desert, Forest, Urban)
    public MapMode mapMode; // Game mode associated with the map (e.g., Capture
    public string mapName; // Name of the map
    public string description; // Description of the map
}

public enum MapType
{
    None = -1,
    Desert,
    Forest,
    Urban
}

public enum MapMode
{
    None = -1,
    CaptureTheFlag,
    TeamDeathmatch,
    KingOfTheHill
}