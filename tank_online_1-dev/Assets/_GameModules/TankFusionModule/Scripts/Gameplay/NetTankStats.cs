using Fusion;
using Fusion.TankOnlineModule;
using UnityEngine;

public class NetTankStats : NetworkBehaviour
{
    #region LEVEL
    [Networked] public int NetMaxHPLevel { get; private set; }
    [Networked] public int NetDamageLevel { get; private set; }
    [Networked] public int NetMovementSpeedLevel { get; private set; }
    [Networked] public int NetFireRateLevel { get; private set; }
    #endregion


    #region NETWORKED PROPERTIES
    [Networked] public int NetPlayerIndex { get; set; }
    [Networked] public NetworkString<_32> NetPlayerName { get; set; }
    [Networked] public int NetAvatarId { get; set; }
    [Networked] public int NetTeammateIndex { get; set; } = -1;
    [Networked] public NetworkString<_32> NetTankId { get; set; } = "";
    [Networked] public int NetMinDamage { get; private set; }
    [Networked] public int NetMaxDamage { get; private set; }
    [Networked] public float NetMovementSpeed { get; private set; } // max speed
    [Networked] public float NetFireRate { get; private set; } // fire rate
    [Networked] public int NetKill { get; set; }
    [Networked] public int NetDeath { get; set; }
    [Networked] public int NetDestroyedOutpost { get; set; }
    [Networked] public int NetMaxHP { get; private set; }
    [Networked] public int NetCurrentHP { get; set; }
    #endregion

    [SerializeField] private SOMatchPlayerData playerData;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            playerData.data = DatabaseManager.CreateMatchPlayer(NetTankId.ToString());
        }
    }

    public void Upgrade(UpgradeType type, SOUpgradeDefinition def, int nextLevel)
    {
        switch (type)
        {
            case UpgradeType.MaxHP:
                NetMaxHPLevel = nextLevel;
                break;
            case UpgradeType.Damage:
                NetDamageLevel = nextLevel;
                break;
            case UpgradeType.MovementSpeed:
                NetMovementSpeedLevel = nextLevel;
                break;
            case UpgradeType.FireRate:
                NetFireRateLevel = nextLevel;
                break;
        }
        Recalculate(def, type);
    }

    private void Recalculate(SOUpgradeDefinition changedDef, UpgradeType type)
    {
        if (changedDef != null)
        {
            switch (type)
            {
                case UpgradeType.MaxHP:
                    NetMaxHP += changedDef.GetBonusAtLevel(type, NetMaxHPLevel);
                    break;

                case UpgradeType.Damage:
                    NetMinDamage += changedDef.GetBonusAtLevel(type, NetDamageLevel);
                    NetMaxDamage += changedDef.GetBonusAtLevel(type, NetDamageLevel);
                    break;

                case UpgradeType.MovementSpeed:
                    NetMovementSpeed += changedDef.GetBonusAtLevel(type, NetMovementSpeedLevel);
                    break;
                    
                case UpgradeType.FireRate:
                    NetFireRate += changedDef.GetBonusAtLevel(type, NetFireRateLevel);
                    break;
            }

            ApplyData();
        }
    }

    private void ApplyData()
    {
        if (playerData.data != null)
        {
            playerData.data.MaxHitpoints = NetMaxHP;
            playerData.data.Damage[0] = NetMinDamage;
            playerData.data.Damage[1] = NetMaxDamage;
            playerData.data.MaxSpeed = NetMovementSpeed;
            playerData.data.FireRate = NetFireRate;
        }
    }
}