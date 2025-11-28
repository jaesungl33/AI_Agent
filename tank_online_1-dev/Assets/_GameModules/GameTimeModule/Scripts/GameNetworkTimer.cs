// Date: 28 June 2025
// AI-Generated: This code structure was created with AI assistance

using System;
using Fusion;
using Fusion.TankOnlineModule;
using FusionHelpers;
using TMPro;
using UnityEngine;

public class GameNetworkTimer : NetworkBehaviour
{
    [Networked] public int Timer { get; set; }
    [Networked] private TickTimer PassiveCurrencyTimer { get; set; }
    private GameServer gameServer;

    public override void Spawned()
    {
        DontDestroyOnLoad(this);
        Debug.Log($"GameNetworkTimer Spawned on Runner: {Runner}");
        if (Object.HasStateAuthority && Runner.IsSharedModeMasterClient)
        {
            DatabaseManager.GetDB<MatchmakingCollection>(collection =>
            {
                MatchmakingDocument matchmakingDocument = collection.GetActiveDocument();

                Timer = matchmakingDocument.MatchDuration;
            });
        }

        if (Runner.TryGetSingleton(out gameServer))
        {
            if (gameServer == null)
            {
                Debug.LogWarning("CoreGamePlay singleton not found, cannot update timer.");
            }
        }
    }

    private void OnDestroy()
    {
        Debug.Log($"GameNetworkTimer Destroyed on Runner: {Runner}");  
    }

    public override void FixedUpdateNetwork()
    {
        if (gameServer == null || gameServer.ActivePlayState != ServerState.LEVEL) return;
        
        if (Object.HasStateAuthority && Runner.IsSharedModeMasterClient)
        {
            if (PassiveCurrencyTimer.ExpiredOrNotRunning(Runner))
            {
                PassiveCurrencyTimer = TickTimer.CreateFromSeconds(Runner, 1);
                Timer -= 1;
                if (Timer < 0)
                    Timer = 0;
                RPC_ReceiveTimeUpdate(Timer);
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ReceiveTimeUpdate(int newTime)
    {
        EventManager.TriggerEvent(new GameServer.TimeEvent
        {
            TimeRemaining = newTime
        });
    }
}
