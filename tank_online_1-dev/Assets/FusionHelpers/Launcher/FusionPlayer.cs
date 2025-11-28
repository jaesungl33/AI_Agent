using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using Fusion.TankOnlineModule;
using FusionHelpers;
using UnityEngine;

namespace FusionHelpers
{
	/// <summary>
	/// Base class for an object that will exist in exactly one instance per player.
	/// This *could* be the players avatar (visual game object), but it need not be - it's perfectly valid to just treat this as a data placeholder for the player.
	/// Implement the abstract InitNetworkState to initialize networked data on the State Authority.
	/// </summary>

	public abstract class FusionPlayer : NetworkBehaviour, IFusionObject
	{
		private SOMatchData soMatchPlayerData;
		public TickAlignedEventRelay _eventRelayPrefab;

		#region Networked Properties -> Not to be used directly, use the local properties instead.

		[Networked, OnChangedRender(nameof(OnPlayerIndexChanged))] public int NetPlayerIndex { get; set; } = -1;
		private void OnPlayerIndexChanged()
		{
			PlayerIndex = NetPlayerIndex;
			Debug.Log($"[FusionPlayer] OnPlayerIndexChanged: NetPlayerIndex={PlayerIndex}, PlayerIndex={PlayerIndex}");
			OnNetPlayerIndexChanged();
		}
		protected virtual void OnNetPlayerIndexChanged() { }


		[Networked, OnChangedRender(nameof(OnPlayerNameChanged))] public NetworkString<_32> NetPlayerName { get; set; }
		private void OnPlayerNameChanged()
		{
			var s = NetPlayerName.ToString();
			if (!string.IsNullOrEmpty(s))
			{
				PlayerName = s;
				Debug.Log($"[FusionPlayer] OnPlayerNameChanged: NetPlayerName={NetPlayerName}, PlayerName={PlayerName}");
				OnNetPlayerNameChanged();
			}
		}
		protected virtual void OnNetPlayerNameChanged() { }

		[Networked, OnChangedRender(nameof(OnWrapDecalStickerItemIdsChanged))] public int NetWrapId { get; set; }
		private void OnWrapDecalStickerItemIdsChanged()
		{
			Debug.Log($"[FusionPlayer] OnNetWrapIdChanged: NetWrap={NetWrapId}, PlayerWrap={PlayerWrap}");
			PlayerWrap = NetWrapId;
			OnNetWrapIdChanged();
		}
		protected virtual void OnNetWrapIdChanged() { }

		[Networked, OnChangedRender(nameof(OnPlayerAvatarIdChanged))] public NetworkString<_32> NetAvatarId { get; set; }
		private void OnPlayerAvatarIdChanged()
		{
			var s = NetAvatarId.ToString();
			if (!string.IsNullOrEmpty(s))
			{
				PlayerAvatarId = s;
				Debug.Log($"[FusionPlayer] OnPlayerAvatarIdChanged: NetAvatarId={NetAvatarId}, PlayerAvatarId={PlayerAvatarId}");
				OnNetAvatarIdChanged();
			}
		}
		protected virtual void OnNetAvatarIdChanged() { }


		[Networked, OnChangedRender(nameof(OnTeamIndexChanged))] public int NetTeammateIndex { get; set; } = -1;
		private void OnTeamIndexChanged()
		{
			PlayerTeamIndex = NetTeammateIndex;
			Debug.Log($"[FusionPlayer] OnTeamIndexChanged: NetTeammateIndex={NetTeammateIndex}, PlayerTeamIndex={PlayerTeamIndex}");
			OnNetTeamIndexChanged();
		}
		protected virtual void OnNetTeamIndexChanged() { }


		[Networked, OnChangedRender(nameof(OnTankIdChanged))] public NetworkString<_32> NetTankId { get; set; } = "";
		private void OnTankIdChanged()
		{
			var s = NetTankId.ToString();
			if (!string.IsNullOrEmpty(s))
			{
				PlayerTankId = s;
				Debug.Log($"[FusionPlayer] OnTankIdChanged: NetTankId={NetTankId}, PlayerTankId={PlayerTankId}");
				OnNetTankIdChanged();
			}
		}
		protected virtual void OnNetTankIdChanged() { }


		[Networked, OnChangedRender(nameof(OnKillChanged))] public int NetKill { get; set; }
		private void OnKillChanged()
		{
			PlayerKill = NetKill;
			Debug.Log($"[FusionPlayer] OnKillChanged: NetKill={NetKill}, PlayerKill={PlayerKill}");
			OnNetKillChanged();
		}
		protected virtual void OnNetKillChanged() { }


		[Networked, OnChangedRender(nameof(OnDeathChanged))] public int NetDeath { get; set; }
		private void OnDeathChanged()
		{
			PlayerDeath = NetDeath;
			Debug.Log($"[FusionPlayer] OnDeathChanged: NetDeath={NetDeath}, PlayerDeath={PlayerDeath}");
			OnNetDeathChanged();
		}
		protected virtual void OnNetDeathChanged() { }


		[Networked, OnChangedRender(nameof(OnDestroyedOutpostChanged))] public int NetDestroyedOutpost { get; set; }
		private void OnDestroyedOutpostChanged()
		{
			DestroyedTurrets = NetDestroyedOutpost;
			Debug.Log($"[FusionPlayer] OnDestroyedOutpostChanged: NetDestroyedOutpost={NetDestroyedOutpost}, DestroyedTurrets={DestroyedTurrets}");
			OnNetDestroyedOutpostChanged();
		}
		protected virtual void OnNetDestroyedOutpostChanged() { }


		[Networked, OnChangedRender(nameof(OnGoldChanged))] public int NetGold { get; set; }
		private void OnGoldChanged()
		{
			PlayerGold = NetGold;
			OnNetGoldChanged();
		}
		protected virtual void OnNetGoldChanged() { }


		[Networked, OnChangedRender(nameof(OnHPChanged))] public int NetHP { get; set; }
		private void OnHPChanged()
		{
			PlayerHP = NetHP;
			Debug.Log($"[FusionPlayer] OnHPChanged: NetHP={NetHP}, PlayerHP={PlayerHP}");
			OnNetHPChanged();
		}
		protected virtual void OnNetHPChanged() { }


		[Networked, OnChangedRender(nameof(OnMaxHPChanged))] public int NetMaxHP { get; set; }
		private void OnMaxHPChanged()
		{
			PlayerMaxHP = NetMaxHP;
			Debug.Log($"[FusionPlayer] OnMaxHPChanged: NetMaxHP={NetMaxHP}, PlayerMaxHP={PlayerMaxHP}");
			OnNetMaxHPChanged();
		}
		protected virtual void OnNetMaxHPChanged() { }

		[Networked, Capacity((int)UpgradeType.Count), OnChangedRender(nameof(OnUpgradesChanged))] public NetworkArray<int> NetUpgrades { get; }
		private void OnUpgradesChanged()
		{
			StatsLevel = NetUpgrades.ToArray();
			Debug.Log($"[FusionPlayer] OnUpgradesChanged: NetUpgrades=[{string.Join(",", NetUpgrades)}], StatsLevel=[{string.Join(",", StatsLevel)}]");
			OnNetUpgradesChanged();
		}
		protected virtual void OnNetUpgradesChanged() { }

		[Networked] public TickAlignedEventRelay NetEventRelay { get; set; }


		[Networked, OnChangedRender(nameof(OnModifyStatsChanged))] public float MoveSpeedModifyRatio { get; set; }
		[Networked, OnChangedRender(nameof(OnModifyStatsChanged))] public float FireRateModifyRatio { get; set; }


		[Networked, OnChangedRender(nameof(OnDecorsChanged))] public NetworkDecors NetDecors { get; set; }
		private void OnDecorsChanged()
		{
			//PlayerSticker = NetSticker;
			Debug.Log($"[FusionPlayer] OnDecorsChanged: NetDecors={NetDecors}");
			OnNetDecorsChanged();
		}
		protected virtual void OnNetDecorsChanged() { }
		
		private void OnModifyStatsChanged()
		{
			// Áp dụng giá trị mới cho các thuộc tính gameplay
			OnNetModifyStatsChanged();
		}
		protected virtual void OnNetModifyStatsChanged()
		{

		}
		#endregion

		#region Local Properties -> Use these to access the player data.
		public MatchPlayerData PlayerData { get => GameMatchData.GetPlayerAtIndex(PlayerId); }

		public SOMatchData GameMatchData
		{
			get => soMatchPlayerData;
			set => soMatchPlayerData = value;
		}

		public PlayerRef PlayerId { get; set; } = PlayerRef.None;
		public int[] StatsLevel { get; set; }
		public int PlayerIndex { get; set; }
		public string PlayerAvatarId { get; set; }
		public string PlayerName { get; set; }
		public bool IsJoined { get; set; }
		public int PlayerTeamIndex { get; set; }
		public string PlayerTankId { get; set; }
		public int PlayerHP { get; set; }
		public int PlayerMaxHP { get; set; }
		public int DestroyedTurrets { get; set; }
		public int PlayerKill { get; set; }
		public int PlayerKillStreak { get; set; }
		public int PlayerDeath { get; set; }
		public int PlayerGold { get; set; }
		public float PlayerRespawnTime { get; set; }
		public TickAlignedEventRelay EventReplay { get; set; }
		public bool IsLocalPlayer => Runner.LocalPlayer == PlayerId;
		public int PlayerWrap { get; set; }
		#endregion

		public override void Spawned()
		{
			PlayerId = Object.InputAuthority;

			OnPlayerIndexChanged();
			OnPlayerNameChanged();
			OnWrapDecalStickerItemIdsChanged();
			OnPlayerAvatarIdChanged();
			OnTeamIndexChanged();
			OnTankIdChanged();
			OnKillChanged();
			OnDeathChanged();
			OnDestroyedOutpostChanged();
			OnGoldChanged();
			OnHPChanged();
			OnMaxHPChanged();
			OnUpgradesChanged();

			// The EventRelay only makes sense in shared mode, and most games don't support both hosted and shared,
			// so this particular check is very specific to Tanknarok.
			if (Runner.Topology == Topologies.Shared)
			{
				if (HasStateAuthority)
					NetEventRelay = Runner.Spawn(_eventRelayPrefab);
				EventReplay = NetEventRelay;
				EventReplay.transform.SetParent(transform);
			}
			else
			{
				// For hosted mode, we avoid spawning the event relay since it doesn't really do anything except calling the local peer.
				EventReplay = gameObject.AddComponent<TickAlignedEventRelay>();
			}
		}

		public virtual void TakeDamage(DamageEvent @event)
		{
		}

		public void RegisterEventListener<T>(Action<T> listener) where T : unmanaged, INetworkEvent
		{
			EventReplay.RegisterEventListener(listener);
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void RPC_SetTeam(SetTeamEvent setTeamEvent)
		{
			Debug.Log($"[FusionPlayer] RPC_SetTeam {setTeamEvent.TeamIndex}");
			if (NetTeammateIndex != setTeamEvent.TeamIndex)
				NetTeammateIndex = setTeamEvent.TeamIndex;
			if (NetPlayerIndex != setTeamEvent.PlayerIndex)
				NetPlayerIndex = setTeamEvent.PlayerIndex;
		}

		/// <summary>
		/// Raises an event for the local player.
		/// This is useful for sending events that should only be processed by the local player,
		/// such as UI updates or local game state changes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="evt"></param>
		public void RaiseEvent<T>(T evt) where T : unmanaged, INetworkEvent
		{
			FusionPlayer localFusionPlayer = Runner.GetPlayerObject(Runner.LocalPlayer).GetComponent<FusionPlayer>();
			localFusionPlayer.EventReplay.RaiseEventFor(EventReplay, evt);
		}

		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		public void RPC_AddGold(int goldReward)
		{
			if (NetGold >= goldReward)
			{
				//Debug.Log($"RPC_AddGold: PlayerIndex={PlayerIndex}, goldReward={goldReward}");
				NetGold += goldReward;
				PlayerGold += goldReward;
				EventManager.TriggerEvent<GameServer.GoldEvent>(new GameServer.GoldEvent { Gold = PlayerGold });
			}
		}

		public virtual void UpdateHPImmediately(int damage)
		{
		}

		public abstract void InitNetworkState(string tankId, string playerName, int playerIndex, int teamIndex, string avatarUrl, int wrapId);
	}
}

public struct SetTeamEvent : INetworkStruct
{
	public byte TeamIndex;
	public byte PlayerIndex;
}
public struct Decors : INetworkStruct
{
	public byte decorId;
	public byte slot;
}
public struct NetworkDecors : INetworkStruct
{
	[Networked, Capacity(16)]
	public NetworkArray<Decors> decors => default;
}