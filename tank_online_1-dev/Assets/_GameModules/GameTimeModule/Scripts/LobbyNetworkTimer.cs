using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.TankOnlineModule;
using FusionHelpers;
using UnityEngine;

public class LobbyNetworkTimer : NetworkBehaviour
{
    // [Networked] public int Timer { get; set; }
    // [Networked] private TickTimer PassiveCurrencyTimer { get; set; }
    
    // private ELobbyState lobbyState = ELobbyState.None;
    // private List<FusionPlayer> playersJoined = new();
    // private MatchmakingDocument matchmakingDocument;
    // private GameServer coreGamePlay;

    // public override void Spawned()
    // {
    //     DontDestroyOnLoad(this);
    //     Debug.Log($"LobbyNetworkTimer Spawned on Runner: {Runner}");
    //     playersJoined.Clear();
    //     lobbyState = ELobbyState.None;

    //     PassiveCurrencyTimer = TickTimer.CreateFromSeconds(Runner, 1);
    //     if (Object != null && Object.IsValid && Object.HasStateAuthority && (Runner.IsSharedModeMasterClient))
    //     {
    //         DatabaseManager.GetDB<MatchmakingCollection>(collection =>
    //         {
    //             matchmakingDocument = collection.GetActiveDocument();
    //             Timer = matchmakingDocument.MaxWaitPlayersTime;
    //             Debug.Log($"LobbyNetworkTimer: MaxWaitPlayersTime = {Timer}");
    //             lobbyState = ELobbyState.WaitPlayers;
    //             RPC_ChangeState(lobbyState);
    //         });
    //     }

    //     if (Runner.TryGetSingleton(out coreGamePlay))
    //     {
    //         if (coreGamePlay == null)
    //         {
    //             Debug.LogWarning("CoreGamePlay singleton not found, cannot update timer.");
    //             return;
    //         }
    //     }
    // }

    // private void OnDestroy()
    // {
    //     Debug.Log($"LobbyNetworkTimer Destroyed on Runner: {Runner}");
    // }

    // public override void FixedUpdateNetwork()
    // {
    //     ELobbyState newState = ELobbyState.None;
    //     if (lobbyState == ELobbyState.None || coreGamePlay == null) return;

    //     if (Object != null && Object.IsValid && Object.HasStateAuthority && Runner.IsSharedModeMasterClient)
    //     {
    //         if (coreGamePlay.ActivePlayState == GameServerState.LOBBY && PassiveCurrencyTimer.ExpiredOrNotRunning(Runner))
    //         {
    //             PassiveCurrencyTimer = TickTimer.CreateFromSeconds(Runner, 1);
    //             if (Timer > 0)
    //             {
    //                 Timer -= 1;
    //                 if (Timer < 0)
    //                 {
    //                     Timer = 0;
    //                 }
    //                 RPC_ReceiveTimeUpdate(Timer, lobbyState);

    //                 // switch (_lobbyState)
    //                 // {
    //                 //     case ELobbyState.WaitPlayers:
    //                 //     case ELobbyState.SelectTank:
    //                 //         _playersJoined = MatchmakingManager.Instance.GetAllPlayers()
    //                 //             .Where(p => p != null && p.Object != null && p.Object.IsValid && !string.IsNullOrWhiteSpace(p.PlayerTankId))
    //                 //             .ToList();

    //                 //         if (_lobbyState == ELobbyState.WaitPlayers &&
    //                 //             _playersJoined.Count == _matchmakingDocument.MaxPlayers)
    //                 //         {
    //                 //             Timer = 0;  //next frame will change state
    //                 //         }
    //                 //         break;
    //                 // }
    //             }
    //             else
    //             {
    //                 switch (lobbyState)
    //                 {
    //                     case ELobbyState.WaitPlayers:
    //                         Timer = matchmakingDocument.SelectTankTime;
    //                         Debug.Log($"LobbyNetworkTimer: SelectTankTime = {Timer}");
    //                         newState = ELobbyState.SelectTank;
    //                         break;

    //                     case ELobbyState.SelectTank:
    //                         Timer = 0; //matchmakingDocument.TargetReadingTime; => removed time for reading
    //                         Debug.Log($"LobbyNetworkTimer: TargetReadingTime = {Timer}");
    //                         newState = ELobbyState.GameEntering;
    //                         break;

    //                     case ELobbyState.GameEntering:
    //                         Timer = matchmakingDocument.TargetReadingTime;
    //                         //EventManager.Emit<GamePhase>(GamePhase.MatchLoading);
    //                         newState = ELobbyState.EnterGame;
    //                         break;

    //                     case ELobbyState.EnterGame:
    //                         newState = ELobbyState.None;
    //                         break;
    //                 }
    //                 if(newState != lobbyState)
    //                 {
    //                     Debug.Log($"LobbyNetworkTimer: State changed from {lobbyState} to {newState}");
    //                     lobbyState = newState;
    //                     EventManager.Emit<ChooseTankStateEvent>(new ChooseTankStateEvent()
    //                     {
    //                         State = lobbyState
    //                     });
    //                     RPC_ChangeState(lobbyState);
    //                 }
    //             }
    //         }
    //     }
    // }

    // [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    // public void RPC_ReceiveTimeUpdate(int newTime, ELobbyState state)
    // {
    //     EventManager.Emit<ChooseTankEvent>(new ChooseTankEvent
    //     {
    //         Seconds = newTime,
    //         State = state
    //     });
    // }
    
    // [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
    // public void RPC_ChangeState(ELobbyState state)
    // {
    //     EventManager.Emit<ChooseTankStateEvent>(new ChooseTankStateEvent()
    //     {
    //         State = state
    //     });
    // }
    
    // [Serializable]
    // public enum ELobbyState
    // {
    //     None,
    //     WaitPlayers,
    //     SelectTank,
    //     GameEntering,
    //     EnterGame
    // }
}
