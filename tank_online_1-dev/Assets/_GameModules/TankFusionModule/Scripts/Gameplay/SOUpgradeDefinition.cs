using System;
using UnityEngine;

public enum UpgradeType
{
    MaxHP,
    Damage,
    MovementSpeed,
    FireRate,
    Count,
}

[Serializable]
public class SOUpgradeDefinition
{
    public string tankId;
    public TankUpgradeStats[] tankUpgradeStats;

    public TankUpgradeStats GetStat(UpgradeType type)
    {
        foreach (var stats in tankUpgradeStats)
        {
            if (stats.type == type)
            {
                return stats.DeepClone();
            }
        }
        return null;
    }

    public int GetCost(UpgradeType type, int nextLevel)
    {
        foreach (var stats in tankUpgradeStats)
        {
            if (stats.type == type)
            {
                return stats.GetCost(nextLevel);
            }
        }
        return 0;
    }

    public int GetBonusAtLevel(UpgradeType type, int level)
    {
        foreach (var stats in tankUpgradeStats)
        {
            if (stats.type == type)
            {
                return stats.GetBonusAtLevel(level);
            }
        }
        return 0;
    }
}

[System.Serializable]
public class TankUpgradeStats
{
    public UpgradeType type;

    [Header("Price at Level=1")]
    public int baseCost = 100;

    [Header("Values for each level (1 to maxLevel) (%)")]
    public int[] upgradeValues;

    [Header("Default = 1 (%)")]
    public int costMultiplier = 100;

    public int GetCost(int nextLevel)
    {
        // geometric
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, nextLevel - 1));
    }

    public int GetBonusAtLevel(int level) => upgradeValues[level - 1];
}