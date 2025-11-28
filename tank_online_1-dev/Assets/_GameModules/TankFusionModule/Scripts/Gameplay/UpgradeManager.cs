using Fusion;
using UnityEngine;

public class UpgradeManager : NetworkBehaviour
{
    public SOUpgradeDefinition scout;
    public SOUpgradeDefinition assault;
    public SOUpgradeDefinition heavy;
    public NetTankStats _stats;
    public NetPlayerWallet _wallet;

    SOUpgradeDefinition GetDef(UpgradeType t) =>
        t == UpgradeType.MaxHP ? scout :
        t == UpgradeType.Damage ? assault : heavy;

    int GetCurrentLevel(UpgradeType t) =>
        t == UpgradeType.MaxHP ? _stats.NetMaxHPLevel :
        t == UpgradeType.Damage ? _stats.NetDamageLevel :
        t == UpgradeType.MovementSpeed ? _stats.NetMovementSpeedLevel :
        t == UpgradeType.FireRate ? _stats.NetFireRateLevel : -1;

    // CLIENT gọi hàm này; nó gửi RPC tới StateAuthority.
    public void RequestUpgrade(UpgradeType type)
    {
        RPC_RequestUpgrade(type);
    }

    // Client -> StateAuthority
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestUpgrade(UpgradeType type, RpcInfo info = default)
    {
        if (!Object.HasStateAuthority) return;

        SOUpgradeDefinition def = GetDef(type);
        int cur = GetCurrentLevel(type);
        int next = cur + 1;

        // Validate
        if (def == null) { RPC_UpgradeResult(false, "No def", type); return; }
        if (next > def.tankUpgradeStats.Length) { RPC_UpgradeResult(false, "Max level", type); return; }

        int cost = def.GetCost(type, next);

        // Trừ tiền (server-side)
        if (_wallet == null || !_wallet.TrySpend(cost))
        {
            RPC_UpgradeResult(false, "Not enough coins", type);
            return;
        }

        // Áp dụng nâng cấp
        _stats.Upgrade(type, def, next);

        // Phản hồi cho client sở hữu (InputAuthority)
        RPC_UpgradeResult(true, "", type);
    }

    // StateAuthority -> InputAuthority (feedback UI)
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    void RPC_UpgradeResult(bool success, string message, UpgradeType type, RpcInfo info = default)
    {
        // Tại client: báo UI
        if (!success)
            Debug.LogWarning($"Upgrade {type} failed: {message}");
        else
            Debug.Log($"Upgrade {type} success!");
    }
}