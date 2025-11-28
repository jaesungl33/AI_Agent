using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.TankOnlineModule;
using FusionHelpers;
using UnityEngine;

public class PickingTimer : NetworkBehaviour
{
    [Networked] public int Timer { get; set; }
    [Networked] private TickTimer OneSecond { get; set; }
    private MatchmakingDocument matchmakingDocument;
    private GameServer gameServer;

    public override void Spawned()
    {
        DontDestroyOnLoad(this);
        Debug.Log($"PickingTimer Spawned on Runner: {Runner}");

        OneSecond = TickTimer.CreateFromSeconds(Runner, 1);
        if (Object != null && Object.IsValid && Object.HasStateAuthority && Runner.IsSharedModeMasterClient)
        {
            DatabaseManager.GetDB<MatchmakingCollection>(collection =>
            {
                matchmakingDocument = collection.GetActiveDocument();
                Timer = matchmakingDocument.PickingTime;
                Debug.Log($"PickingTimer: SelectTankTime = {Timer}");
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

    public override void FixedUpdateNetwork()
    {
        if (gameServer == null || gameServer.ActivePlayState != ServerState.PICKING) return;

        if (Object != null && Object.IsValid && Object.HasStateAuthority && Runner.IsSharedModeMasterClient)
        {
            if (OneSecond.ExpiredOrNotRunning(Runner))
            {
                OneSecond = TickTimer.CreateFromSeconds(Runner, 1);
                if (Timer > 0)
                {
                    Timer -= 1;
                    if (Timer <= 0)
                    {
                        Timer = 0;
                    }
                    RPC_UpdateTimeAll(Timer);
                }
                else
                {
                    StartCoroutine(DelayedAction(1f, () =>
                    {
                        Debug.Log("[PickingTimer] Time's up! Changing state to MAP_LOADING.");
                        gameServer.ChangeStateByMaster(ServerState.MAP_LOADING);
                    }));
                }
            }
        }
    }

    private IEnumerator DelayedAction(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateTimeAll(int newTime)
    {
        EventManager.TriggerEvent<ChooseTankTimeEvent>(new ChooseTankTimeEvent
        {
            Seconds = newTime
        });
    }
}
