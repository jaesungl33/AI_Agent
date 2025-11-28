using System;
using System.Collections.Generic;
using Fusion;
using NUnit.Framework;
using UnityEngine;

public struct MatchPlayerUpgradeData
{
    public UpgradeType upgradeType;
    public int upgradeCount;
}

[System.Serializable]
public class MatchPlayerData
{
    [SerializeField] private string playerName;
    [SerializeField] int playerId;
    [SerializeField] private bool joined;
    [SerializeField] private bool isLocal;
    [SerializeField] private int teamIndex;
    [SerializeField] private int indexInTeam;
    [SerializeField] private string tankId;
    [SerializeField] private string avatarId;
    [SerializeField] private int itemIds;
    [SerializeField] private int kill;
    [SerializeField] private int killStreak; // Number of consecutive kills without dying (reset on death)
    [SerializeField] private int death;
    [SerializeField] private int destroyedTurrets;
    [SerializeField] private int gold;
    [SerializeField] private int hitpoints;
    [SerializeField] private int maxHitpoints;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float braking;
    [SerializeField] private float acceleration;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private int[] damage;
    [SerializeField] private int reloadTime;
    [SerializeField] private float fireRate;
    [SerializeField] private int projectileSpeed;
    [SerializeField] private int range;
    [SerializeField] private int magazineSize;
    [SerializeField] private byte projectileCount;
    [SerializeField] private float respawnInSeconds; // Time in seconds before the player can respawn
    [SerializeField] private string[] abilityIDs;

    [SerializeField] private int wrapId; //WrapDecalStickers Fixme

    public int PlayerId { get => playerId; set => playerId = value; }
    public bool IsJoined { get => joined; set => joined = value; }
    public bool IsLocalPlayer { get => isLocal; set => isLocal = value; }
    public int TeamIndex { get => teamIndex; set => teamIndex = value; }
    public string AvatarId { get => avatarId; set => avatarId = value; }
    public string TankId { get => tankId; set => tankId = value; }
    public int IndexInTeam { get => indexInTeam; set => indexInTeam = value; }
    public string PlayerName { get => playerName; set => playerName = value; }
    public int ItemIds { get => itemIds; set => itemIds = value; }
    public int Kill { get => kill; set => kill = value; }
    public int KillStreak { get => killStreak; set => killStreak = value; }
    public int Death { get => death; set => death = value; }
    public int DestroyedTurrets { get => destroyedTurrets; set => destroyedTurrets = value; }
    public int Gold { get => gold; set => gold = value; }
    public int HP { get => hitpoints; set => hitpoints = value; }
    public int MaxHitpoints { get => maxHitpoints; set => maxHitpoints = value; }
    public float MaxSpeed
    {
        get => Mathf.Max(0f, maxSpeed * (1 + ModifySpeedRatio));
        set => maxSpeed = value;
    }
    public int[] Damage { get => damage; set => damage = value; }
    public int ReloadTime { get => reloadTime; set => reloadTime = value; }
    public float FireRate
    {
        get => Mathf.Max(0f, fireRate * (1 + ModifyFireRateRatio));
        set => fireRate = value;
    }
    public int ProjectileSpeed { get => projectileSpeed; set => projectileSpeed = value; }
    public int Range { get => range; set => range = value; }
    public int MagazineSize { get => magazineSize; set => magazineSize = value; }
    public byte ProjectileCount { get => projectileCount; set => projectileCount = value; }
    public float RespawnInSeconds { get => respawnInSeconds; set => respawnInSeconds = value; }
    public float Braking { get => braking; set => braking = value; }
    public float Acceleration { get => acceleration; set => acceleration = value; }
    public float RotationSpeed { get => rotationSpeed; set => rotationSpeed = value; }
    public string[] AbilityIDs { get => abilityIDs; set => abilityIDs = value; }

    //WrapDecalStickers Fixme
    public int WrapId { get => wrapId; set => wrapId = value; }
    public void Upgrade(float damageMultiplier = 0, float speedMultiplier = 0, float hpMultiplier = 0, float fireRateMultiplier = 0)
    {
        Damage = new int[] { Damage[0] + Mathf.RoundToInt(Damage[0] * damageMultiplier / 100), Damage[1] + Mathf.RoundToInt(Damage[1] * damageMultiplier / 100) };
        MaxSpeed += MaxSpeed * speedMultiplier / 100;
        HP += Mathf.RoundToInt(MaxHitpoints * hpMultiplier / 100);
        MaxHitpoints += Mathf.RoundToInt(MaxHitpoints * hpMultiplier / 100);
        FireRate += FireRate * fireRateMultiplier / 100;
    }

    public int GetUpgradeMaxHP(float multiplier = 0)
    {
        return MaxHitpoints + Mathf.RoundToInt(MaxHitpoints * multiplier / 100);
    }


    #region Modify Stats Methods
    public float ModifySpeedRatio;
    public float ModifyFireRateRatio;
    #endregion
}