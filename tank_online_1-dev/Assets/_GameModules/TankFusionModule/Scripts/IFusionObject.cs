using System;
using Fusion;
using FusionHelpers;

public interface IFusionObject : IDamagable, INetworkStruct
{
    [Networked] public int NetTeammateIndex { get; set; }
    public string PlayerName { get; set; }
    public int PlayerIndex { get; set; }
    public int PlayerTeamIndex { get; set; }
    public void RaiseEvent<T>(T evt) where T : unmanaged, INetworkEvent { }
    public void RegisterEventListener<T>(Action<T> listener) where T : unmanaged, INetworkEvent { }
    public void UpdateHPImmediately(int damage) { }
}