using Fusion;
using FusionHelpers;
using UnityEngine;

public struct DamageEvent : INetworkEvent
{
    public int playerId;
    public int teamIndex;
    public int damage;
    public int targetPlayerRef;
}