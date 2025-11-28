using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.TankOnlineModule;
using UnityEngine;
using UnityEngine.Events;

namespace FusionHelpers
{
	/// <summary>
	/// Base class for you per-session state class.
	/// You can use this to track and access player avatars on all peers.
	/// Override OnPlayerAvatarAdded/Removed to be notified of players joining/leaving *after* their avatar is created or removed.
	/// Use GetPlayer/GetPlayerByIndex/AllPlayers to access or iterate over players on all peers.
	/// Use Runner.GetSingleton/Runner.WaitForSingleton to get your custom session instance on all peers.
	/// </summary>

	public abstract class FusionSession : NetworkBehaviour
	{
		private const int MAX_PLAYERS = 10;

		[SerializeField] private FusionPlayer _playerPrefab;
		[SerializeField] protected TurretSpawner _turretSpawner;

		[OnChangedRender(nameof(OnActivePlayStateChanged))] [Networked] public ServerState ActivePlayState { get; set; }

		// [Networked, Capacity(MAX_PLAYERS)] public NetworkDictionary<int, PlayerRef> PlayerRefByIndex { get; }
		public Dictionary<PlayerRef, FusionPlayer> proxyPlayerList = new();
		public TurretSpawner turretSpawnerInstance;
		protected abstract void OnPlayerAvatarAdded(Player fusionPlayer);
		protected abstract void OnPlayerAvatarRemoved(FusionPlayer fusionPlayer);
		public IEnumerable<FusionPlayer> AllPlayers => proxyPlayerList.Values;
		public int PlayerCount => AllPlayers.Count();
		public TankCollection tankCollection { get; set; }
		public SOMatchData sOMatchData { get; set; }
		public MatchmakingDocument matchmakingDocument { get; set; }
		public ChangeDetector changes;
		public ServerState CurrentPlayState { get; set; }
		public Player MyTank => Runner?.GetPlayerObject(Runner.LocalPlayer)?.GetComponent<Player>();
		private Dictionary<PlayerRef, FusionPlayer> players = new Dictionary<PlayerRef, FusionPlayer>();

		private void Awake()
		{
			DontDestroyOnLoad(this);
			FillData();
		}

		public override void Spawned()
		{
			changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
			Debug.Log($"Spawned Network Session for Runner: {Runner}");

			if (Runner.IsSharedModeMasterClient && matchmakingDocument.MatchMode == MatchMode.CaptureBase)
				turretSpawnerInstance = Runner.Spawn(_turretSpawner);
		}

		private void FillData()
		{
			tankCollection = DatabaseManager.GetDB<TankCollection>();
			sOMatchData = DatabaseManager.GetDB<SOMatchData>();
			matchmakingDocument = DatabaseManager.GetDB<MatchmakingCollection>().GetActiveDocument();
		}


		// cả Master và Client đều nhận được sự kiện này
		// nhưng chỉ có Master mới được quyền thay đổi trạng thái của trận đấu
		// điều phối toàn bộ trận đấu và trạng thái của các client
		private void OnActivePlayStateChanged()
		{
			Debug.Log($"ActivePlayState changed to {ActivePlayState}");
			CurrentPlayState = ActivePlayState;

			CheckState();
		}

		private void CheckState()
        {
			switch (CurrentPlayState)
			{
				case ServerState.MAP_LOADING:
					EventManager.TriggerEvent<GamePhase>(GamePhase.MatchLoading);
					break;

				case ServerState.PICKING:
					EventManager.TriggerEvent<GamePhase>(GamePhase.MatchPicking);
					break;

				case ServerState.FINAL:
					EventManager.TriggerEvent<GamePhase>(GamePhase.MatchFinal);
					break;

				case ServerState.COUNTDOWN:
					EventManager.TriggerEvent<GamePhase>(GamePhase.MatchCountdown);
					break;

				case ServerState.GAMEOVER:
					EventManager.TriggerEvent<GameOverEvent>(new GameOverEvent());
					break;
			}
        }

		// private IEnumerator DelayedCheckState()
        // {
		// 	yield return new WaitForSeconds(1f);
		// 	switch (CurrentPlayState)
		// 	{
		// 		case ServerState.MAP_LOADING:
		// 			EventManager.Emit<GamePhase>(GamePhase.MatchLoading);
		// 			break;

		// 		case ServerState.PICKING:
		// 			EventManager.Emit<GamePhase>(GamePhase.MatchPicking);
		// 			break;

		// 		case ServerState.FINAL:
		// 			EventManager.Emit<GamePhase>(GamePhase.MatchFinal);
		// 			break;

		// 		case ServerState.COUNTDOWN:
		// 			EventManager.Emit<GamePhase>(GamePhase.MatchCountdown);
		// 			break;

		// 		case ServerState.GAMEOVER:
		// 			EventManager.Emit<GameOverEvent>(new GameOverEvent());
		// 			break;
		// 	}
        // }

		public override void Render()
		{
			// if (Runner && Runner.Topology == Topologies.Shared && players.Count != playerRefByIndex.Count)
			// 	MaybeSpawnNextAvatar();
		}

		private void CreateNewPlayer(NetworkRunner runner, ClientDataEvent data)
		{
			// int playerIndex = -1; // toàn bộ sẽ đưa vào slot 0 trước, chờ chia lại vị trí sau
			runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, data.playerRef, (newRunner, networkObject) =>
			{
				newRunner.SetPlayerObject(data.playerRef, networkObject);

				// cập nhật một số dữ liệu cơ bản cho người chơi này
				Player newPlayer = networkObject.GetComponent<Player>();

				Debug.Log("My tank: " + Runner.LocalPlayer + " - " + data.playerRef);
				if (newPlayer != null)
				{
					// if (playerIndex == -1) return;

					// int teamIndex = -1; // tạm đặt toàn bộ vào team 0, chờ chia lại đội sau

					// if (playerIndex % 2 == 0)
					// 	teamIndex = MatchIndexs.Defender; // 0,2,4,6,8
					// else
					// 	teamIndex = MatchIndexs.Attacker; // 1,3,5,7,9

					newPlayer.InitNetworkState(data.playerSelectedTankId.ToString(), data.playerName.ToString(), avatarUrl: data.playerSelectedAvatar.ToString(), wrapId: data.wrapId);
					Debug.Log("[FusionSession] Creating Player Avatar for PlayerRef " + data.playerRef + " at Index " + -1 + " with TankId " + data.playerSelectedTankId + " and Name " + data.playerName);
				}
				else
					Debug.LogError("Failed to get Player component from spawned object!");
			});
		}

		/// <summary>
		/// Chia players thành 2 teams với PlayerIndex riêng biệt cho mỗi team.
		/// Team Defender: PlayerIndex 0 -> (teamSize-1)
		/// Team Attacker: PlayerIndex 0 -> (teamSize-1)
		/// </summary>
		public void CheckCreateTeam()
		{
			Debug.Log($"[CheckCreateTeam] All called => SetTeam, PlayerCount={PlayerCount}, ActivePlayers/MaxPlayers={Runner.ActivePlayers.Count()} / {Runner.SessionInfo.MaxPlayers}.");
			
			// Chỉ Master Client mới được quyền chia team
			if (!Runner.IsSharedModeMasterClient)
			{
				Debug.LogWarning("[CheckCreateTeam] Not master client, skipping team creation");
				return;
			}

			// Đợi đủ số lượng players
			if (Runner.ActivePlayers.Count() < Runner.SessionInfo.MaxPlayers)
			{
				Debug.LogWarning($"[CheckCreateTeam] Not enough players yet: {Runner.ActivePlayers.Count()}/{Runner.SessionInfo.MaxPlayers}");
				return;
			}

			int totalPlayers = Runner.SessionInfo.MaxPlayers;
			
			// ========== TRAINING MODE (1 player) ==========
			if (totalPlayers == 1)
			{
				var player = AllPlayers.FirstOrDefault();
				if (player != null)
				{
					var setTeamEvent = new SetTeamEvent 
					{ 
						TeamIndex = MatchIndexs.Defender, 
						PlayerIndex = 0 
					};
					player.RPC_SetTeam(setTeamEvent);
					Debug.Log($"[CheckCreateTeam] ✅ Training mode: {player.NetPlayerName} assigned to Defender[0]");
				}
				else
				{
					Debug.LogError("[CheckCreateTeam] ❌ No player found for training mode!");
				}
				return;
			}

			// ========== NORMAL MODE (2, 4, 6, 8, 10 players) ==========
			int teamSize = totalPlayers / 2; // Luôn chia đều vì là số chẵn
			int defenderCount = 0; // PlayerIndex cho team Defender (0 -> teamSize-1)
			int attackerCount = 0; // PlayerIndex cho team Attacker (0 -> teamSize-1)
			int globalPlayerIndex = 0; // Chỉ để debug, không dùng trong logic

			Debug.Log($"[CheckCreateTeam] Creating balanced teams: Defender[0-{teamSize-1}] vs Attacker[0-{teamSize-1}]");

			foreach (var player in AllPlayers)
			{
				if (defenderCount < teamSize)
				{
					// ===== TEAM DEFENDER =====
					var setTeamEvent = new SetTeamEvent 
					{ 
						TeamIndex = MatchIndexs.Defender, 
						PlayerIndex = (byte)defenderCount // 0, 1, 2, ... (teamSize-1)
					};
					player.RPC_SetTeam(setTeamEvent);
					
					Debug.Log($"[CheckCreateTeam] Player #{globalPlayerIndex} ({player.NetPlayerName}) → Defender[{defenderCount}]");
					defenderCount++;
				}
				else if (attackerCount < teamSize)
				{
					// ===== TEAM ATTACKER =====
					var setTeamEvent = new SetTeamEvent 
					{ 
						TeamIndex = MatchIndexs.Attacker, 
						PlayerIndex = (byte)attackerCount // 0, 1, 2, ... (teamSize-1)
					};
					player.RPC_SetTeam(setTeamEvent);
					
					Debug.Log($"[CheckCreateTeam] Player #{globalPlayerIndex} ({player.NetPlayerName}) → Attacker[{attackerCount}]");
					attackerCount++;
				}
				else
				{
					// ===== SAFETY CHECK =====
					// Không bao giờ xảy ra với số chẵn, nhưng giữ lại để phát hiện bugs
					Debug.LogError($"[CheckCreateTeam] ❌ CRITICAL BUG: Player #{globalPlayerIndex} ({player.NetPlayerName}) couldn't be assigned! Defender={defenderCount}, Attacker={attackerCount}");
				}

				globalPlayerIndex++;
			}

			// ========== VALIDATION ==========
			bool isValid = (defenderCount == teamSize) && (attackerCount == teamSize);
			
			if (!isValid)
			{
				Debug.LogWarning($"[CheckCreateTeam] ❌ Team assignment FAILED! Expected {teamSize} vs {teamSize}, Got Defender={defenderCount} vs Attacker={attackerCount}");
			}
			else
			{
				Debug.Log($"[CheckCreateTeam] ✅ Teams created successfully!");
				Debug.Log($"    Defender Team: {defenderCount} players (PlayerIndex 0-{teamSize-1})");
				Debug.Log($"    Attacker Team: {attackerCount} players (PlayerIndex 0-{teamSize-1})");
			}
		}
		
		public void RemovePlayerInSession(PlayerRef playerRef)
		{
			if (Runner && !Runner.IsShutdown)
			{
				FusionPlayer player = GetPlayer<FusionPlayer>(playerRef);
				if (player && player.Object.IsValid)
				{
					Debug.Log($"RemovePlayerInSession for PlayerRef {player.NetPlayerName}");
					Runner.Despawn(player.Object);
				}
			}
		}


		public void AddPlayerAvatar(Player fusionPlayer)
		{
			OnPlayerAvatarAdded(fusionPlayer);
		}

		public void RemovePlayerAvatar(PlayerRef playerRef)
		{
			if (ActivePlayState == ServerState.LEVEL || ActivePlayState == ServerState.FINAL)
			{
				Debug.Log($"[FusionSession] In LEVEL state, not removing PlayerRef {playerRef} yet");
				return;
			}

			if (!proxyPlayerList.TryGetValue(playerRef, out FusionPlayer fusionPlayer))
			{
				Debug.LogWarning($"No player found for PlayerRef {playerRef}");
				return;
			}
		
			proxyPlayerList.Remove(playerRef);
			sOMatchData.RemovePlayerData(playerRef);

			if (Runner.TryGetPlayerObject(playerRef, out var obj))
				Runner.Despawn(obj);
			OnPlayerAvatarRemoved(fusionPlayer);
			Debug.Log($"Removing PlayerRef {playerRef}");
		}

		public IEnumerator ClearAllPlayerAvatars()
		{
			yield return new WaitForSeconds(1f);
			foreach (var player in proxyPlayerList.Values.ToList())
			{
				proxyPlayerList.Remove(player.PlayerId);

				if (Runner.TryGetPlayerObject(player.PlayerId, out var obj))
					Runner.Despawn(obj);
				OnPlayerAvatarRemoved(player);
			}
			proxyPlayerList.Clear();
			// PlayerRefByIndex.Clear();

			// foreach (var p in Runner.ActivePlayers) {
			// 	if (Runner.TryGetPlayerObject(p, out var obj)) {
			// 		Runner.Despawn(obj);
			// 	}
			// }
		}

		public T GetPlayer<T>(PlayerRef plyRef) where T : FusionPlayer
		{
			proxyPlayerList.TryGetValue(plyRef, out FusionPlayer ply);
			return (T)ply;
		}
		public T GetPlayer<T>(byte plyRef) where T : FusionPlayer
		{
			// Find player by PlayerRef.AsIndex
			PlayerRef playerRef = PlayerRef.FromIndex(plyRef);
			proxyPlayerList.TryGetValue(playerRef, out FusionPlayer ply);
			return (T)ply;
		}

		public T GetPlayerByIndex<T>(int idx) where T : FusionPlayer
		{
			Debug.Log($"GetPlayerByIndex({proxyPlayerList.Values.Count})");
			foreach (FusionPlayer player in proxyPlayerList.Values)
			{
				if (player.Object != null && player.Object.IsValid && player.PlayerId.AsIndex == idx)
					return (T)player;
			}
			return default;
		}

		// private int GetNextPlayerIndex()
		// {
		// 	for (int idx = 0; idx < Runner.Config.Simulation.PlayerCount; idx++)
		// 	{
		// 		if (!PlayerRefByIndex.TryGet(idx, out _))
		// 			return idx;
		// 	}
		// 	Debug.LogWarning("No free player index!");
		// 	return -1;
		// }

		public void PlayerLeft(PlayerRef playerRef)
		{
			Debug.Log($"Player {playerRef} Left");
			// Only remove player avatar if we are not in PICKING or LEVEL or FINAL state
			// If joining/leaving during the game, we keep the avatar until the game ends
			if (CurrentPlayState != ServerState.PICKING
				&& CurrentPlayState != ServerState.LEVEL
				&& CurrentPlayState != ServerState.FINAL)
			{
				RemovePlayerAvatar(playerRef);
			}
			// if (Runner && !Runner.IsShutdown)
			// {
			// 	FusionPlayer player = GetPlayer<FusionPlayer>(playerRef);
			// 	if (player && player.Object.IsValid)
			// 	{
			// 		Debug.Log($"Despawning PlayerAvatar for PlayerRef {player.PlayerId}");
			// 		Runner.Despawn(player.Object);
			// 		Runner.Despawn(turretSpawnerInstance.Object);
			// 	}

			// 	// This means only on player remains
			// 	if (Runner.SessionInfo.PlayerCount == 1)
			// 	{
			// 		await Runner.Shutdown(false);
			// 	}
			// }
		}

		public void HandleJoinedPlayer(NetworkRunner runner, PlayerRef newPlayer, string playerName, string tankId, string avatarUrl, int wrapId)
		{
			if (newPlayer == PlayerRef.None)
			{
				Debug.LogError("PlayerRef is None, cannot add player");
				return;
			}
			
			Debug.Log($"[FusionSession] Master -> {Object.StateAuthority}");
			ClientDataEvent data = new ClientDataEvent
			{
				playerIndex = -1,
				playerRef = newPlayer,
				playerName = new NetworkString<_16>(playerName),
				playerSelectedTankId = new NetworkString<_16>(tankId),
				playerSelectedAvatar = new NetworkString<_16>(avatarUrl),
				wrapId = wrapId
			};
			CreateNewPlayer(runner, data);
		}
	}
}