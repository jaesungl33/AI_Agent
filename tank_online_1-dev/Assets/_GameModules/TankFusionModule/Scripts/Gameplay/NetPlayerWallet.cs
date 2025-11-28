using Fusion;
using UnityEngine;

public class NetPlayerWallet : NetworkBehaviour
{
    [Networked] public int Gold { get; private set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Gold = 500;
        }
    }

    // Only called on StateAuthority
    public bool TrySpend(int amount)
    {
        if (amount <= 0) return true;
        if (Gold < amount) return false;
        Gold -= amount;
        return true;
    }
}