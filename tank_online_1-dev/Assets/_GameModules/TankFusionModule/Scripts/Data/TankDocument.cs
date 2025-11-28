// Date: 28 June 2025
// AI-Generated: This code structure was created with AI assistance

using UnityEngine;
using System;
using Fusion;

[Serializable]
public class TankDocument
{
    public string tankName;
    public string tankId;
    public TankType tankType;
    public string description;
    public string[] abilityIDs; // Array of ability IDs associated with the tank
    public string hullID; // Unique identifier for the tank hull
    public string primaryWeaponID; // Unique identifier for the primary weapon
    public string secondaryWeaponID; // Unique identifier for the secondary weapon
}

[Serializable]
public enum TankType
{
    None,
    Scout,
    Assault,
    Heavy,
    Outpost,
}