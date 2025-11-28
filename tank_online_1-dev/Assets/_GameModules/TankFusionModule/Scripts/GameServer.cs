using UnityEngine;
using FusionHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Fusion.GameSystems;
using Fusion;

namespace Fusion.TankOnlineModule
{
	public class GameServer : FusionSession
	{
		public static GameServer Instance { get; private set; }
		[Networked, Capacity(10)] private NetworkArray<PlayerStats> allPlayerStates => default;
		[Networked, Capacity(10)] private NetworkDictionary<PlayerRef, int> teamReadyStates => default;
		[Networked] private TickTimer PassiveCurrencyTimer { get; set; }
		[Networked] private TickTimer CheckGameOverTimer { get; set; }
		public Player lastPlayerStanding { get; set; }
		public List<string> TurretsAlive { get; private set; } = new();
		[SerializeField] private PingDebug pingDebug;
		private RenderTexture renderTexture;
		[SerializeField] private GameObject pickingTimerPrefab, gameTimerPrefab;
		public override void Spawned()
		{
			Instance = this;
			Runner.RegisterSingleton(this);
			base.Spawned();

			ResetOutpost();
			RegisterEvents();
			pingDebug.SetRunner(Runner);
			ChangeStateByMaster(ServerState.MATCH_FINDING);
			CheckGameOverTimer = TickTimer.CreateFromSeconds(Runner, 0.3f);
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			pingDebug.SetRunner(null);
			Debug.Log("CoreGamePlay Despawned called");
			UnRegisterEvents();
			if (runner != null && runner.IsRunning)
				runner.UnregisterSingleton(this);
			base.Despawned(runner, hasState);
		}

		public override void FixedUpdateNetwork()
		{
			base.FixedUpdateNetwork();
			if (Runner.IsSharedModeMasterClient && CurrentPlayState == ServerState.LEVEL && PassiveCurrencyTimer.ExpiredOrNotRunning(Runner))
			{
				RPC_MasterAddGoldState(matchmakingDocument.GoldPerSecond);
				PassiveCurrencyTimer = TickTimer.CreateFromSeconds(Runner, 1);

			}

			if (Runner.IsSharedModeMasterClient && CurrentPlayState == ServerState.LEVEL && CheckGameOverTimer.ExpiredOrNotRunning(Runner))
			{
				OnConditionsChecking();
				CheckGameOverTimer = TickTimer.CreateFromSeconds(Runner, 0.3f);
			}
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void RPC_SetReady(PlayerRef playerRef)
		{
			Debug.Log($"2[RPC_SetReady] Player {playerRef} is ready.");
			if (!Object.HasStateAuthority) return;

			if (playerRef == PlayerRef.None) return;

			teamReadyStates.Add(playerRef, 1);

			// Check if all players are ready
			bool allReady = true;

			if (teamReadyStates.Count < Runner.ActivePlayers.Count())
			{
				allReady = false;
			}

			foreach (var state in teamReadyStates)
			{
				if (state.Value == 0)
				{
					allReady = false;
					break;
				}
			}

			if (allReady)
			{
				Debug.Log("[RPC_SetReady] All players are ready. Starting the game...");
				EventManager.TriggerEvent(new ServerStateEvent { NewState = ServerState.PICKING });
			}
		}

		public void AddPickingTimer()
		{
			if (Object.HasStateAuthority && Runner.IsSharedModeMasterClient)
			{
				Runner.Spawn(pickingTimerPrefab);
			}
		}

		public void AddGameTimer()
		{
			if (Object.HasStateAuthority && Runner.IsSharedModeMasterClient)
			{
				Runner.Spawn(gameTimerPrefab);
			}
		}

		private void RegisterEvents()
		{
			EventManager.Register<LevelEvent>(ClearData);
			EventManager.Register<TimeEvent>(OnTimeUpdated);
			EventManager.Register<ServerStateEvent>(OnServerStateChanged);
		}

		private void UnRegisterEvents()
		{
			if (GameManager.IsApplicationQuitting) return;

			EventManager.Unregister<LevelEvent>(ClearData);
			EventManager.Unregister<TimeEvent>(OnTimeUpdated);
			EventManager.Unregister<ServerStateEvent>(OnServerStateChanged);
		}

		private void OnUpdateAllInfos(GamePlayDataEvent matchData)
		{
			OnConditionsChecking();
		}

		private void OnTimeUpdated(TimeEvent @event)
		{
			if (@event.TimeRemaining <= 0)
			{
				GameOver();
			}
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void RPC_CreatePlayer(ClientDataEvent clientDataEvent)
		{

		}

		/// <summary>
		/// Add gold to player has state authority
		/// </summary>
		/// <param name="bonus"></param>
		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		public void RPC_MasterAddGoldState(int bonus)
		{
			foreach (var player in AllPlayers)
			{
				Player p = player as Player;
				if (p && p.Object && p.Object.HasStateAuthority) // only work on player has state authority
				{
					p.IncreaseGold(bonus);
				}
			}
		}

		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		public void RPC_DestroyedOutpost(OutpostEvent @event)
		{
			if (TurretsAlive.Contains(@event.turretId.ToString()))
				TurretsAlive.Remove(@event.turretId.ToString());
			Debug.Log($"RPC_DestroyedOutpost: TeamIndex={@event.teamIndex}, PlayerId={@event.playerId}, TurretId={@event.turretId}");
			foreach (FusionPlayer fusionPlayer in AllPlayers)
			{
				Player player = (Player)fusionPlayer;
				// update score for player
				if (player.PlayerId.AsIndex == @event.playerId && player.Object.HasStateAuthority)
				{
					lastPlayerStanding = player;
					player.NetDestroyedOutpost++;
				}

				// add gold if player is in the same team
				if (player && player.Object && player.Object.HasStateAuthority && player.PlayerTeamIndex == @event.teamIndex)
				{
					player.RPC_ReceiveCurrencyTeam(matchmakingDocument.GoldDestroyOutpost);
				}
			}
		}

		[Rpc(RpcSources.All, RpcTargets.All)]
		internal void RPC_UpdateKillDeath(PlayerKillEvent @event)
		{
			Debug.Log($"UpdateKillDeath: killerId={@event.KillerId}, targetId={@event.TargetId}");

			// Update kill/death stats for the players involved
			if (@event.TargetId != -1)
			{
				PlayerDeath(@event.TargetId);
			}

			//don't count when kill himself
			if (@event.KillerId == @event.TargetId)
				return;

			if (@event.KillerId != -1 && @event.KillerId != MatchIndexs.Turret)
			{
				PlayerKill(@event.KillerId);
			}
		}

		private void ResetOutpost()
		{
			TurretsAlive.Clear();
			TurretsAlive = tankCollection.CloneDocumentsByProperty(doc => doc.tankType == TankType.Outpost, true)
				.Select(t => t.tankId)
				.ToList();
		}

		public void CheckEndGame()
		{
			if (ActivePlayState != ServerState.LEVEL) return;

			// count alive players
			int alivePlayers = Runner.ActivePlayers.Count();

			if (alivePlayers <= 1)
			{
				Debug.Log("[MatchmakingManager] Ending game due to all players dead");
				GameOver();
			}

			// count players in each team, if one team has no players in active, end game
			int countTeam0 = 0;
			int countTeam1 = 0;
			foreach (var player in AllPlayers)
			{
				Player p = player as Player;
				if (p && p.Object && p.Object.HasStateAuthority) // only count player has state authority and alive
				{
					if (p.PlayerTeamIndex == 0)
						countTeam0++;
					else if (p.PlayerTeamIndex == 1)
						countTeam1++;
				}
			}

			if(countTeam0 == 0 || countTeam1 == 0)
			{
				Debug.Log("[MatchmakingManager] Ending game due to all players in one team dead");
				GameOver();
			}
		}

		private void GameOver()
		{
			if (CurrentPlayState != ServerState.LEVEL || !Object.HasStateAuthority || !Runner.IsSharedModeMasterClient) return;
			ChangeStateByMaster(ServerState.GAMEOVER);
		}

		private void OnServerStateChanged(ServerStateEvent newStateEvent)
		{
			ChangeStateByMaster(newStateEvent.NewState);

			// ngay lập tức xử lý sự kiện thời gian nếu đây là đối tượng giữ quyền state authority
			switch (newStateEvent.NewState)
			{
				case ServerState.PICKING:
					AddPickingTimer();
					break;

				case ServerState.LEVEL:
					AddGameTimer();
					break;

				case ServerState.FINAL:
					StartCoroutine(ClearAllPlayerAvatars());
					break;
			}
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void RPC_ChangeStateByMaster(ServerState newState)
		{
			if (Object != null && Object.IsValid && Object.HasStateAuthority && Runner.IsSharedModeMasterClient && ActivePlayState != newState)
			{
				ActivePlayState = newState;
			}
		}

		public void ChangeStateByMaster(ServerState newState)
		{
			if (Object != null && Object.IsValid && Object.HasStateAuthority && Runner.IsSharedModeMasterClient && ActivePlayState != newState)
			{
				ActivePlayState = newState;
			}
		}

		private void ClearData(LevelEvent gameplaySceneEvent)
		{
			if (!HasStateAuthority)
				return;

			for (int i = 0; i < allPlayerStates.Length; i++)
			{
				var playerStats = new PlayerStats();
				var player = GetPlayerByIndex<Player>(i);
				if (player != null)
				{
					playerStats.team = player.PlayerTeamIndex;
					allPlayerStates.Set(i, playerStats);
				}
			}
		}

		protected override void OnPlayerAvatarAdded(Player fusionPlayer)
		{
			Debug.Log($"[FusionSession] OnPlayerAvatarAdded: {fusionPlayer.PlayerId} - team:{fusionPlayer.PlayerTeamIndex} - index_in_team:{fusionPlayer.PlayerIndex}");
			proxyPlayerList[fusionPlayer.Object.InputAuthority] = fusionPlayer;
			Debug.Log($"[FusionSession] OnPlayerAvatarAdded Current Players: {proxyPlayerList.Count}");
			CheckPickingPhase();
			if (!fusionPlayer.HasInputAuthority)
			{
				return;
			}
			if (Runner.GetLevelManager())
			{
				Runner.GetLevelManager().IsometricCamera.Initialize(fusionPlayer);
			}
		}

		protected override void OnPlayerAvatarRemoved(FusionPlayer fusionPlayer)
		{
			// remove ready state
			teamReadyStates.Remove(fusionPlayer.PlayerId);
			// đang tìm trận thì đơn giản là cập nhật lại số người chơi cho những người chơi còn lại
			if (ActivePlayState == ServerState.MATCH_FINDING)
			{
				EventManager.TriggerEvent(new MainMenuEvent
				{
					status = MatchmakingStatus.Updating,
					playerCount = Runner.ActivePlayers.Count(),
					maxPlayers = Runner.SessionInfo.MaxPlayers,
					immediate = true
				});
				return;
			}

			// nếu đang trong giai đoạn chọn xe tank thì đưa người chơi về lại trạng thái chờ
			// lý do là nếu đang chọn xe thì phòng đã khóa, nên phải hủy bỏ cả trận đấu và người chơi tìm trận lại
			if (ActivePlayState == ServerState.PICKING)
			{
				EventManager.TriggerEvent(GamePhase.MatchIdling);
				return;
			}
		}

		private void CheckPickingPhase()
		{
			if (Runner.ActivePlayers.Count() < Runner.SessionInfo.MaxPlayers) return;
			if (Object.HasStateAuthority)
			{
				CheckCreateTeam();
				Debug.Log("[MatchmakingManager] Starting tank picking phase");
			}
		}


		/// <summary>
		/// Check the conditions for winning the game
		/// </summary>
		public void OnConditionsChecking()
		{
			Debug.Log("[GameServer] GameOver! called & ActivePlayState:" + ActivePlayState + ", Object.HasStateAuthority: " + Object.HasStateAuthority);

			switch (matchmakingDocument.MatchMode)
			{
				case MatchMode.TeamDeathmatch:
					CheckTeamDeathmatchWinCondition();
					break;

				case MatchMode.CaptureBase:
					CheckCaptureBaseWinCondition();
					break;
			}
		}

		private void CheckCaptureBaseWinCondition()
		{
			if (Object.HasStateAuthority == false) return;

			if (ActivePlayState == ServerState.LEVEL)
			{
				int deadTeam1 = 0, deadTeam2 = 0;
				int destroyedTurrets = 0;
				for (int i = 0; i < matchmakingDocument.MaxPlayers; i++)
				{
					MatchPlayerData data = sOMatchData.GetPlayerAtIndex(i);
					if (data.TeamIndex == 0)
						deadTeam1 += data.Death;
					else
						deadTeam2 += data.Death;

					destroyedTurrets += data.DestroyedTurrets;
				}
				Debug.Log($"[GameServer] GameOver! deadTeam1={deadTeam1}, deadTeam2={deadTeam2}, destroyedTurrets={destroyedTurrets}");
				// win if kill 20 or destroy all turrets
				if (deadTeam1 >= matchmakingDocument.KillsNeededToWin
				|| deadTeam2 >= matchmakingDocument.KillsNeededToWin
				|| destroyedTurrets >= matchmakingDocument.TargetsNeededToWin)
				{
					GameOver();
				}
				else lastPlayerStanding = null;
			}
		}
		
		private void CheckTeamDeathmatchWinCondition()
		{
			if (Object.HasStateAuthority == false) return;
			
			if (ActivePlayState == ServerState.LEVEL)
			{
				int deadTeam1 = 0, deadTeam2 = 0;
				for (int i = 0; i < matchmakingDocument.MaxPlayers; i++)
				{
					MatchPlayerData data = sOMatchData.GetPlayerAtIndex(i);
					if (data.TeamIndex == 0)
						deadTeam1 += data.Death;
					else
						deadTeam2 += data.Death;
				}
				Debug.Log($"[GameServer] GameOver! deadTeam1={deadTeam1}, deadTeam2={deadTeam2}");
				// win if kill 20
				if (deadTeam1 >= matchmakingDocument.KillsNeededToWin
				|| deadTeam2 >= matchmakingDocument.KillsNeededToWin)
				{
					GameOver();
				}
				else lastPlayerStanding = null;
			}
		}

		#region Update Player Stats

		public void PlayerKill(int playerId)
		{
			foreach (var player in AllPlayers)
			{
				Player p = player as Player;
				if (p && p.Object && p.Object.HasStateAuthority && p.PlayerId.AsIndex == playerId) // only work on player has state authority
				{
					p.IncreaseKill();
					p.IncreaseGold(matchmakingDocument.GoldPerKill);
				}
			}
		}

		public void PlayerDeath(int playerId)
		{
			foreach (var player in AllPlayers)
			{
				Player p = player as Player;
				if (p && p.Object && p.Object.HasStateAuthority && p.PlayerId.AsIndex == playerId) // only work on player has state authority
				{
					p.IncreaseDeath();
				}
			}
		}

		#endregion

		private void OnDestroy()
		{
			Debug.Log("CoreGamePlay OnDestroy called");
			// Release render texture only when the session ends
			if (renderTexture != null)
			{
				renderTexture.Release();
				renderTexture = null;
			}
			if (Runner != null && Runner.IsRunning)
				Runner.UnregisterSingleton(this);
		}

		[Serializable]
		public struct PlayerStats : INetworkStruct
		{
			public int team;
			public int Kill;
			public int Death;
			public int OutpostsDestroyed;
		}

		public struct PlayerKillEvent : INetworkStruct
		{
			public int KillerId { get; set; }
			public int TargetId { get; set; }
		}

		public struct OutpostEvent : INetworkStruct
		{
			public int teamIndex { get; set; }
			public int playerId { get; set; }
			public NetworkString<_16> turretId { get; set; }
		}
		public struct TimeEvent
		{
			public int TimeRemaining { get; set; }
		}

		public struct GoldEvent : INetworkStruct
		{
			public int Gold { get; set; }
		}

		public struct UpgradeEvent : INetworkStruct
		{
			public UpgradeType UpgradeType { get; set; }
		}

		
	}
}

public enum ServerState { WAITING, MATCH_FINDING, PICKING, COUNTDOWN, LEVEL, MAP_LOADING, FINAL, GAMEOVER }

public struct ServerStateEvent : INetworkStruct
{
	public ServerState NewState { get; set; }
}