// Date: 28 June 2025
// AI-Generated: This code structure was created with AI assistance

using Fusion.TankOnlineModule;
using UnityEngine;

[System.Serializable]
public class TankWeaponDocument
{
    public string weaponName; // Name of the weapon
    public string weaponID; // Unique identifier for the weapon
    public TankType tankType; // Type of tank this weapon is compatible with
    public WeaponType weaponType; // Type of the weapon (e.g., primary, secondary)
    public PowerupType powerupType;
    public DamageAreaType damageAreaType; // Type of damage area (e.g., single target, area of effect)
    public int damageMultiplier; // %
    public int rangeMultiplier; // %
    public int fireRateMultiplier; // %
    public int reloadTimeMultiplier; // %
    public int projectileSpeedMultiplier; // %
    public int magazineSizeMultiplier; // %
    public int projectileCountMultiplier; // %
}

public enum WeaponType
{
    None = -1,
    Primary, // Main weapon of the tank
    Secondary, // Secondary weapon (e.g., machine gun, missile launcher)
    Support // Support weapon (e.g., smoke launcher, flare gun)
}

public enum DamageAreaType
{
    None = -1,
    SingleTarget, // Weapon targets a single enemy
    AreaOfEffect, // Weapon affects an area (e.g., splash damage)
    Continuous // Weapon provides continuous fire (e.g., laser, beam)
}