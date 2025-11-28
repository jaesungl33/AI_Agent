using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FusionHelpers;
using UnityEngine;
using Newtonsoft.Json;
using ExitGames.Client.Photon.StructWrapping;
namespace Fusion.TankOnlineModule
{
	/// <summary>
	/// The Player class represent the players avatar - in this case the Tank.
	/// </summary>
	[RequireComponent(typeof(NetworkCharacterController))]
	public class Player : FusionPlayer
	{
		#region Visuals
		[SerializeField] private List<TankVisual> _visualsArray;
		[SerializeField] private TankPartVisual tankPartVisual;
		[SerializeField] public AbilityManager abilityManager;
		[SerializeField] public EffectApplier EffectApplier;
		[SerializeField] public TankVisualsVfx TankVisualsVfx;
		public TankVisual visuals;
		public TankUI tankUI;
		public TankIndicator tankIndicator;
		#endregion

		#region Parameters
		private NetworkCharacterController ncc;
		private ChangeDetector changes;
		private NetworkInputData oldInput;
		private float respawnTime = 5f; // Default respawn time
		#endregion

		#region Properties
		[OnChangedRender(nameof(OnPlayerStageChanged))]
		[Networked] public Stage NetPlayerStage { get; set; }
		[Networked] public Angle AimDirection { get; set; }
		[Networked] public Angle HullDirection { get; set; }
		[Networked] private TickTimer RespawnTimer { get; set; }
		[Networked] private TickTimer invulnerabilityTimer { get; set; }
		[OnChangedRender(nameof(OnAnimStateChanged))]
		[Networked] public byte TankAnimState { get; set; }
		public bool IsDead => NetPlayerStage == Stage.Dead;
		public Player Target { get; }
		public bool IsCreated { get; set; }
		public int CurrentCurrency => PlayerGold;
		public bool IsActivated => (gameObject.activeInHierarchy && (NetPlayerStage == Stage.Active || NetPlayerStage == Stage.TeleportIn || NetPlayerStage == Stage.Invisible));
		public bool IsBlockMove => (EffectApplier != null
								&& (EffectApplier.HasEffect((int)EffectData.EffectType.Root)
								|| EffectApplier.HasEffect((int)EffectData.EffectType.Stun)
								|| EffectApplier.HasEffect((int)EffectData.EffectType.Knockback))
								) || IsDead;
		public bool IsBlockAim => (EffectApplier != null
								&& (EffectApplier.HasEffect((int)EffectData.EffectType.Stun)
								|| EffectApplier.HasEffect((int)EffectData.EffectType.Knockback))
								) || IsDead;
		public bool IsBlockAtk => (EffectApplier != null
								&& (EffectApplier.HasEffect((int)EffectData.EffectType.Stun)
								|| EffectApplier.HasEffect((int)EffectData.EffectType.Knockback))
								) || IsDead;
		public bool ExplosionOnDeath = true;
		public bool IsRespawningDone => NetPlayerStage == Stage.TeleportIn && RespawnTimer.Expired(Runner);
		public Material PlayerMaterial { get; set; }
		public Color PlayerColor { get; set; } = Color.white;
		public Vector3 Velocity => Object != null && Object.IsValid ? ncc.Velocity : Vector3.zero;
		public GameObject CameraTarget => ncc.gameObject;
		public int Direction { get { return PlayerTeamIndex != -1 ? (PlayerTeamIndex == 0 ? -1 : 1) : -1; } }

		public NetworkCharacterController NCC => ncc;

		public bool HasLocalSignal { get; set; } = false;
		#endregion

		#region Ability

		#endregion
		private void Awake()
		{
			DontDestroyOnLoad(this);
			ncc = GetComponent<NetworkCharacterController>();
			GameMatchData = DatabaseManager.GetDB<SOMatchData>();
		}

		protected override void OnNetPlayerIndexChanged() { }
		protected override void OnNetTankIdChanged()
		{
			UpdateTankData();
			SetVisual();
			ApplyNCCStats();
			abilityManager?.Initialize(this);
			UpdateUITankSelected();
		}
		protected override void OnNetWrapIdChanged()
		{
			Debug.Log($"[OnNetWrapIdChanged] PlayerWrapId: {PlayerWrap}");

			visuals?.LoadWrap(PlayerWrap, PlayerTankId);
			PlayerData.WrapId = PlayerWrap;
			UpdateDataChanged();
		}

		protected override void OnNetPlayerNameChanged()
		{
			tankUI.SetPlayerName(PlayerName);
			gameObject.name = $"pos[{PlayerId.AsIndex}]_name[{PlayerName}]";
			GameMatchData.AddPlayer(new MatchPlayerData { PlayerName = PlayerName, PlayerId = PlayerId.AsIndex, IndexInTeam = PlayerIndex, IsLocalPlayer = Object.HasInputAuthority });
		}

		protected override void OnNetTeamIndexChanged()
		{
			Debug.Log($"OnNetTeamIndexChanged {PlayerName} to team {PlayerTeamIndex}");
			PlayerData.TeamIndex = PlayerTeamIndex;
			tankUI.SetTeam(PlayerTeamIndex);

			SetReadyState();
		}

		protected override void OnNetAvatarIdChanged() { }
		protected override void OnNetGoldChanged()
		{
			PlayerData.Gold = PlayerGold;
			EventManager.TriggerEvent<GameServer.GoldEvent>(new GameServer.GoldEvent { Gold = PlayerGold });
		}

		protected override void OnNetKillChanged()
		{
			PlayerData.Kill = PlayerKill;
			PlayerKillStreak++;
			tankUI.UpdateKillStreak(PlayerKillStreak);
			UpdateDataChanged();
		}

		protected override void OnNetDeathChanged()
		{
			Debug.Log($"OnNetDeathChanged {PlayerName} to death {PlayerDeath}");
			PlayerData.Death = PlayerDeath;
			PlayerKillStreak = 0;
			tankUI.UpdateKillStreak(PlayerKillStreak);
			UpdateDataChanged();
		}

		protected override void OnNetDestroyedOutpostChanged()
		{
			DestroyedTurrets = NetDestroyedOutpost;
			PlayerData.DestroyedTurrets = DestroyedTurrets;
			UpdateDataChanged();
		}

		protected override void OnNetHPChanged()
		{
			PlayerData.HP = PlayerHP;
			tankUI.ChangeHP(PlayerHP, PlayerMaxHP);
		}
		protected override void OnNetMaxHPChanged()
		{
			PlayerData.MaxHitpoints = PlayerMaxHP;
			PlayerData.HP = PlayerHP;
			tankUI.ChangeHP(PlayerHP, PlayerMaxHP);
			tankUI.SetLineDecorHP(PlayerMaxHP);
		}
		protected override void OnNetUpgradesChanged()
		{
			tankUI.UpdateLevel();
			ApplyNCCStats();
		}

		private void UpdateDataChanged()
		{
			EventManager.TriggerEvent<MatchPlayerData>(PlayerData);
			EventManager.TriggerEvent<GamePlayDataEvent>(new GamePlayDataEvent());
		}

		protected override void OnNetModifyStatsChanged()
		{
			if (PlayerData == null) return;

			if (!Mathf.Approximately(PlayerData.ModifySpeedRatio, MoveSpeedModifyRatio))
			{
				PlayerData.ModifySpeedRatio = MoveSpeedModifyRatio;
				ApplyNCCStats();
				Debug.Log($"[FusionPlayer] MoveSpeedModifyRatio changed: {PlayerData.ModifySpeedRatio}");
			}

			if (!Mathf.Approximately(PlayerData.ModifyFireRateRatio, FireRateModifyRatio))
			{
				PlayerData.ModifyFireRateRatio = FireRateModifyRatio;
				visuals.WeaponManager.UpgradeFireRate();
				Debug.Log($"[FusionPlayer] FireRateModifyRatio changed: {PlayerData.ModifyFireRateRatio}");
			}

		}
		public override void Spawned()
		{
			base.Spawned();
			UpdateTankData();

			EffectApplier?.Initialize(this);
			changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
			Runner.WaitForSingleton<GameServer>(session =>
			{
				session.AddPlayerAvatar(this);
			});

			RegisterEventListener((DamageEvent evt) => TakeDamage(evt));
			RegisterEventListener((PickupEvent evt) => OnPickup(evt));
			RegisterEventListener((EffectEvent evt) => OnEffectApply(evt));
		}

		/// <summary>
		/// Initializes the network state for the player.
		/// </summary>
		public override void InitNetworkState(string tankId, string playerName, int playerIndex = -1, int teamIndex = -1, string avatarUrl = "", int wrapId = 0)
		{
			Debug.Log($"Initializing Player Network State: TankId={tankId}, PlayerName={playerName}, PlayerIndex={playerIndex}, TeamIndex={teamIndex}, AvatarUrl={avatarUrl}, wrapId={wrapId}");

			InitData(tankId, playerName, avatarUrl, wrapId);
			// Proxies may not be in state "NEW" when they spawn, so make sure we handle the state properly, regardless of what it is
			NetPlayerStage = Stage.New; // first state
		}

		private void InitData(string tankId, string pName, string avatarUrl, int wrapId)
		{
			IsCreated = false;

			if (Object.HasStateAuthority)
			{
				Debug.Log($"[SyncVisual] InitData for Player {-1} with TankId {tankId} and Name {pName}");

				NetPlayerName = pName; // set data
				NetTankId = tankId; // set data
				NetDestroyedOutpost = 0; // set data
				NetAvatarId = avatarUrl; // set data
				NetKill = 0; // set data
				NetDeath = 0; // set data
				NetGold = 0; // set data
				NetWrapId = wrapId; // set data
			}

			IsCreated = true;
		}

		public override void FixedUpdateNetwork()
		{
			if (Object.HasStateAuthority)
			{
				CheckRespawn();

				if (IsRespawningDone)
					RevivalPlayer();

				ScanTargetInRange(30f);
			}

			if (GameServer.Instance == null || GameServer.Instance.gameObject == null || GameServer.Instance.CurrentPlayState != ServerState.LEVEL)
			{
				return; // Skip input processing if not in the level state
			}

			bool controlledByJoystick = false;
			if (InputController.fetchInput)
			{
				if (GetInput(out NetworkInputData input))
				{
					controlledByJoystick = input.aimDirection != Vector2.zero;
					bool allowClick = !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
					SetDirections(input.moveDirection.normalized, input.aimDirection.normalized);
					//Preset ability params
					var range = 0f;
					var radius = 0f;
					const int REMOVE_NORMALIZE_INDEX = 1;//NOTE: to convert to zero-based index OF ABILITY ARRAY
					if (!IsBlockAtk)
					{
						int btnDown = input.GetSlotBtnDownIndex();
						switch (btnDown)
						{
							case -1:
								tankIndicator?.ActiveIndicator(false);
								break;
							case 0:
								visuals.WeaponManager.FireWeapon(WeaponManager.WeaponInstallationType.PRIMARY);
								range = 5f;
								break;
							default:
								int abilitySlot = btnDown;
								var ability = abilityManager?.GetAbilityBySlot(abilitySlot - REMOVE_NORMALIZE_INDEX);
								range = ability?.GetCustomPropertiesRuntime().FirstOrDefault(p => p.propertyName == "Range")?.propertyValue ?? 0f;
								radius = ability?.GetCustomPropertiesRuntime().FirstOrDefault(p => p.propertyName == "Radius")?.propertyValue ?? 0f;
								break;
						}
						if (btnDown >= 0)
						{
							tankIndicator?.ActiveIndicator(true);
							if (input.aimDirection != Vector2.zero)
							{
								tankIndicator?.SetIndicatorSkillRotate(input.aimDirection.normalized, range);
								tankIndicator?.SetIndicatorAim(input.rangeRatio, radius, range);
							}
							else
								tankIndicator?.PresetIndicator(new Vector2(visuals.Turret.forward.x, visuals.Turret.forward.z), 0);
						}

						for (int i = 0; i <= 3; i++)
						{
							uint btn = (1u << i);
							if (input.WasReleased(btn, oldInput))
							{
								if ((input.CancelMask & btn) != 0)
								{
									continue;
								}
								if (btn == NetworkInputData.BUTTON_FIRE_PRIMARY)
								{

								}
								else
								{
									int abilityWasPressedSlot = i;
									var abilityWasPressed = abilityManager?.GetAbilityBySlot(abilityWasPressedSlot - REMOVE_NORMALIZE_INDEX);
									Vector3 aimDir = visuals.WeaponManager.GetMainWeapon().transform.forward;
									if (!IsDead)
										abilityManager?.ActivateAbilityBySlot(abilityWasPressedSlot - REMOVE_NORMALIZE_INDEX, aimDir);
								}
							}
						}
					}
					oldInput = input;
				}

				RotateTargetAutomatically(!(controlledByJoystick || abilityManager.IsAbilityBlockAutoAim));
			}
			CheckStayInBushes();
		}

		/// <summary>
		/// Render is the Fusion equivalent of Unity's Update() and unlike FixedUpdateNetwork which is very different from FixedUpdate,
		/// Render is in fact exactly the same. It even uses the same Time.deltaTime time steps. The purpose of Render is that
		/// it is always called *after* FixedUpdateNetwork - so to be safe you should use Render over Update if you're on a
		/// SimulationBehaviour.
		///
		/// Here, we use Render to update visual aspects of the Tank that does not involve changing of networked properties.
		/// </summary>
		public override void Render()
		{
			this.transform.position = new Vector3(this.transform.position.x, 0.08f, this.transform.position.z);

			if (visuals != null)
			{
				visuals.RenderVisuals(this);
				CheckInvisibilityTogether();
			}
		}

		private void SetReadyState()
		{
			if (string.IsNullOrEmpty(PlayerTankId) || PlayerTeamIndex == -1)
			{
				Debug.LogWarning($"1[RPC_SetReady] Player {PlayerName} cannot be ready. TankId: {PlayerTankId}, TeamIndex: {PlayerTeamIndex}");
				return;
			}
			Debug.Log($"1[RPC_SetReady] for Player {PlayerName}, TankId: {PlayerTankId}, TeamIndex: {PlayerTeamIndex}");
			PlayerData.IsJoined = true;
			Runner.WaitForSingleton<GameServer>(session =>
			{
				session.RPC_SetReady(PlayerId);
			});
		}

		private void UpdateUITankSelected()
		{
			Debug.Log($"UpdateUITankSelected for Player {PlayerName}, TankId: {PlayerTankId}, TeamIndex: {PlayerTeamIndex}");
			if (string.IsNullOrEmpty(PlayerTankId)) return;

			EventManager.TriggerEvent<ChooseTankBaseInfo>(
				new ChooseTankBaseInfo
				{
					IsLocalPlayer = IsLocalPlayer,
					TeammateIndex = PlayerTeamIndex,
					PlayerName = PlayerName,
					PlayerIndex = PlayerIndex,
					TankId = PlayerTankId,
					PlayerId = PlayerId.AsIndex,
				});
		}

		private void UpdateTankData()
		{
			if (string.IsNullOrEmpty(PlayerTankId) || PlayerIndex == -1)
			{
				Debug.LogWarning("[UpdateTankData] PlayerTankId is null or empty or PlayerIndex is -1, cannot sync visual.");
				return;
			}
			// cập nhật dữ liệu về tank từ database vào GameMatchData với PlayerIndex là vị trí slot của người chơi
			MatchPlayerData matchPlayerData = DatabaseManager.CreateMatchPlayer(PlayerTankId);

			// Gán trực tiếp wrapId (kiểu int) cho MatchPlayerData
			matchPlayerData.WrapId = PlayerWrap;

			Debug.Log($"[UpdateTankData] WrapId: {matchPlayerData.WrapId}");

			GameMatchData.UpdatePlayerData(Object.InputAuthority, matchPlayerData);

			// cập nhật lại HP và max HP theo tank mới, chỉ người sở hữu quyền hạn mới được phép thay đổi dữ liệu mạng
			if (Object.HasStateAuthority)
			{
				respawnTime = PlayerRespawnTime = matchPlayerData.RespawnInSeconds; // đặt lại thời gian hồi sinh mới
				NetHP = NetMaxHP = matchPlayerData.MaxHitpoints; // đặt lại giá trị Network HP và MaxHP
			}
		}

		private TickTimer TargetDetectionTimer { get; set; }
		[SerializeField] private Player _tankTarget;
		[SerializeField] private Turret _turretTarget;
		private Collider[] _areaHits = new Collider[20];
		private void ScanTargetInRange(float customRange = -1)
		{
			if (NetPlayerStage != Stage.Active && NetPlayerStage != Stage.Invisible) return;

			if (!TargetDetectionTimer.ExpiredOrNotRunning(Runner))
				return;

			float TARGET_DETECTION_INTERVAL = Runner.DeltaTime * 5f; // *5 to reduce frame rate
			TargetDetectionTimer = TickTimer.CreateFromSeconds(Runner, TARGET_DETECTION_INTERVAL);

			float _range = customRange > 0 ? customRange : PlayerData.Range;
			float rangeSqr = _range * _range;
			int cnt = Physics.OverlapSphereNonAlloc(transform.position, _range, _areaHits, LayerMask.GetMask("Player", "Outpost"), QueryTriggerInteraction.Ignore);

			Player nearestEnemy = null;
			Turret nearestOutpost = null;
			float minEnemyDistSqr = float.MaxValue;
			float minOutpostDistSqr = float.MaxValue;

			for (int i = 0; i < cnt; i++)
			{
				var col = _areaHits[i];
				if (col == null) continue;
				if (CheckSignBlocked(col.transform)) continue;

				Player p = col.GetComponent<Player>();
				if (p != null)
				{
					if (p.PlayerTeamIndex == PlayerTeamIndex || p.IsDead || !p.HasLocalSignal) continue;
					float distSqr = (transform.position - p.visuals.Turret.position).sqrMagnitude;
					if (distSqr < minEnemyDistSqr)
					{
						minEnemyDistSqr = distSqr;
						nearestEnemy = p;
					}
					continue;
				}

				Turret t = col.GetComponent<Turret>();
				if (t != null)
				{
					if (t.TurretTeamIndex == PlayerTeamIndex || !t.IsAlive) continue;
					float distSqr = (transform.position - t.TurretHead.position).sqrMagnitude;
					if (distSqr < minOutpostDistSqr)
					{
						minOutpostDistSqr = distSqr;
						nearestOutpost = t;
					}
				}
			}

			_tankTarget = nearestEnemy;
			_turretTarget = nearestOutpost;
		}

		private void RotateTargetAutomatically(bool active = false)
		{
			if (!active)
				return;
			Vector3 turretPos = visuals.Turret.position;

			// Nếu không có target nào thì return
			if (_tankTarget == null && _turretTarget == null)
				return;

			// Chọn target gần nhất
			float tankDist = _tankTarget != null ? Vector3.Distance(turretPos, _tankTarget.visuals.Turret.position) : float.MaxValue;
			float turretDist = _turretTarget != null ? Vector3.Distance(turretPos, _turretTarget.TurretHead.position) : float.MaxValue;

			Vector3 targetPos = (tankDist < turretDist)
				? _tankTarget.visuals.Turret.position
				: _turretTarget.TurretHead.position;

			Vector3 direction = targetPos - turretPos;
			direction.y = 0;
			if (direction.sqrMagnitude > 0.01f)
			{
				visuals.Turret.forward = direction.normalized;
				AimDirection = visuals.Turret.rotation.eulerAngles.y;
			}

		}
		private bool CheckSignBlocked(Transform target)
		{
			if (target == null) return true;

			Vector3 from = transform.position + Vector3.up * 0.5f;
			Vector3 to = target.position + Vector3.up * 0.5f;
			Vector3 dir = (to - from);
			float length = dir.magnitude;

			// Nếu target rất gần thì bỏ qua kiểm tra chắn tường
			if (length < 0.5f)
				return false;

			dir /= length; // Chuẩn hóa

			int blockedLayer = LayerMask.GetMask("Wall");

			// Dùng SphereCast thay vì Raycast để ổn định hơn (bán kính nhỏ, ví dụ 0.2f)
			if (Physics.SphereCast(from, 0.2f, dir, out var hitinfo, length, blockedLayer, QueryTriggerInteraction.Ignore))
				return true;

			return false;
		}

		private void ShootTargetAutomatically()
		{
			Vector3 turretPos = visuals.Turret.position;
			float tankDist = (_tankTarget != null && !_tankTarget.IsDead)
				? Vector3.Distance(turretPos, _tankTarget.visuals.Turret.position)
				: float.MaxValue;
			float turretDist = (_turretTarget != null && _turretTarget.IsAlive)
				? Vector3.Distance(turretPos, _turretTarget.TurretHead.position)
				: float.MaxValue;

			// Không có mục tiêu hợp lệ thì return
			if (tankDist == float.MaxValue && turretDist == float.MaxValue)
				return;

			Transform targetTransform = null;
			Vector3 direction;
			if (tankDist < turretDist)
				targetTransform = _tankTarget.visuals.Turret;
			else
				targetTransform = _turretTarget.TurretHead;

			if (!targetTransform) return;
			if (CheckSignBlocked(targetTransform))
				return;

			direction = targetTransform.position - turretPos;
			direction.y = 0;
			if (direction.sqrMagnitude > 0.01f)
			{
				visuals.Turret.forward = direction.normalized;
				AimDirection = visuals.Turret.rotation.eulerAngles.y;
			}
		}

		// hiện thị tank mà nguời chơi đã chọn theo TankId
		private void SetVisual()
		{
			visuals = _visualsArray.Find(tank => tank.tankId.Equals(PlayerTankId.ToString()));
			if (visuals == null)
			{
				Debug.LogError($"No TankVisual found for TankId {PlayerTankId}. Check if the TankVisuals are set up correctly.");
				return;
			}

			for (int i = 0; i < _visualsArray.Count; i++)
			{
				_visualsArray[i].gameObject.SetActive(false);
			}

			visuals.gameObject.SetActive(true);
			visuals.Initialize(this);
		}

		private void ApplyNCCStats()
		{
			ncc.maxSpeed = PlayerData.MaxSpeed;
			ncc.braking = PlayerData.Braking;
			ncc.rotationSpeed = PlayerData.RotationSpeed;
			ncc.acceleration = PlayerData.Acceleration;
		}

		[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
		public void RPC_AddUpgrade(GameServer.UpgradeEvent @event)
		{
			var upgrade = DatabaseManager.GetDB<TankUpgradeCollection>().GetDefinition(PlayerTankId).GetStat(@event.UpgradeType);

			var upgradeCount = NetUpgrades[(int)upgrade.type];
			var nextUpgradeIndex = upgradeCount;

			if (nextUpgradeIndex >= upgrade.upgradeValues.Length) return;

			var nextUpgradeValue = upgrade.upgradeValues[nextUpgradeIndex];
			var nextUpgradeCost = upgrade.baseCost;

			var gold = NetGold;
			if (gold < nextUpgradeCost) return;

			Debug.Log($"RPC: Upgrade {upgrade.type} | {nextUpgradeIndex + 1}");
			RPC_AddGold(-nextUpgradeCost);

			NetUpgrades.Set((int)upgrade.type, nextUpgradeIndex + 1);
			switch (upgrade.type)
			{
				case UpgradeType.MaxHP:
					int newMaxHP = PlayerData.GetUpgradeMaxHP(nextUpgradeValue);
					int deltaHP = newMaxHP - PlayerData.MaxHitpoints;
					NetMaxHP = newMaxHP;
					NetHP += deltaHP;
					break;

				case UpgradeType.Damage:
					PlayerData.Upgrade(damageMultiplier: nextUpgradeValue);
					break;

				case UpgradeType.FireRate:
					PlayerData.Upgrade(fireRateMultiplier: nextUpgradeValue);
					visuals.WeaponManager.UpgradeFireRate();
					break;

				case UpgradeType.MovementSpeed:
					PlayerData.Upgrade(speedMultiplier: nextUpgradeValue);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			Debug.Log($"Upgraded Player {PlayerId} stats: MaxSpeed={nextUpgradeValue}, Damage={PlayerData.Damage[0]}");
		}

		[Rpc(RpcSources.InputAuthority, RpcTargets.All)]
		public void RPC_ChooseTank(string tankId, int wrapId)
		{
			Debug.Log($"Player select tank {tankId}");
			if (PlayerTankId.ToString().Equals(tankId)) return;
			NetTankId = tankId;

			// Cập nhật lại wrap decal sticker khi đổi tank
			//get from db PlayerDocument
			Debug.Log($"RPC wrap {tankId}, old wrap: {NetWrapId}");
			NetWrapId = wrapId;
		}

		[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
		public void RPC_ReceiveCurrencyTeam(int bonus)
		{
			Debug.Log($"RPC_ReceiveCurrencyTeam: PlayerIndex={PlayerIndex}, bonus={bonus}, team={PlayerTeamIndex}");
			Debug.Log($"RPC_ReceiveCurrencyTeam: Adding {bonus} currency to PlayerIndex={PlayerIndex}");
			NetGold += bonus;
		}

		public void IncreaseKill()
		{
			NetKill++;
		}

		public void IncreaseDeath()
		{
			NetDeath++;
		}

		public void IncreaseGold(int bonus)
		{
			NetGold += bonus;
		}

		[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
		public void RPC_RequestPurchase(int cost, int powerupIndex)
		{
			if (PlayerGold >= cost)
			{
				PlayerGold -= cost;
				RPC_AddGold(PlayerGold); // Only notify the requesting player
				RaiseEvent(new PickupEvent { powerup = powerupIndex });
				Debug.Log($"Purchase approved: -{cost} coins");
			}
			else
			{
				Debug.Log("Not enough coins (server validated)");
			}
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			Debug.Log($"Despawned PlayerAvatar for PlayerRef {PlayerId}");
			base.Despawned(runner, hasState);
			SpawnTeleportOutFx();
		}

		private void OnDestroy()
		{
			if (visuals != null && visuals.DeathExplosionInstance != null)
				Destroy(visuals.DeathExplosionInstance);
		}

		private void OnPickup(PickupEvent evt)
		{
			PowerupElement powerup = PowerupSpawner.GetPowerup(evt.powerup);

			if (powerup.powerupType == PowerupType.HEALTH)
			{
				PlayerHP = PlayerData.MaxHitpoints;
				EventManager.TriggerEvent<Player>(this);
			}
			else
				visuals.WeaponManager.InstallWeapon(powerup);
		}
		private void OnEffectApply(EffectEvent evt)
		{
			if (EffectApplier == null)
				return;

			if (EffectApplier.HasEffect((int)EffectData.EffectType.ImmuneCC) && EffectData.IsCCEffect(evt.effectId))
				return;

			//add more logic here if needed
			EffectApplier?.ApplyEffect(new EffectData
			{
				effectId = evt.effectId,
				duration = evt.duration,
				lifeTime = evt.duration,
				value = evt.value,
				dir = Player.EffectEvent.ConvertToDir(evt.angle),
				srcPlayerId = evt.srcPlayerId,
			});
		}

		private void HideVisuals(bool hide)
		{
			tankPartVisual.EnableVisual(hide);
		}

		/// <summary>
		/// Set the direction of movement and aim
		/// </summary>
		private void SetDirections(Vector2 moveVector, Vector2 aimVector)
		{
			if (!IsActivated)
				return;

			if (!IsBlockMove)
			{
				Vector3 moveDirection = new Vector3(moveVector.x, 0, moveVector.y);
				ncc.Move(moveDirection, this.visuals.Hull);
				HullDirection = this.visuals.Hull.rotation.eulerAngles.y;

				if (moveVector.sqrMagnitude > 0)
					visuals.DrivingFx(true);
				else
					visuals.DrivingFx(false);
			}
			if (!IsBlockAim)
			{
				if (aimVector.sqrMagnitude > 0)
					visuals.Turret.forward = new Vector3(aimVector.x, 0, aimVector.y);
				AimDirection = visuals.Turret.rotation.eulerAngles.y;
			}
		}

		public override void TakeDamage(DamageEvent @event)
		{
			base.TakeDamage(@event);
			Debug.Log($"Player {PlayerId} took {@event.damage} damage, life = {PlayerHP}, max = {PlayerMaxHP}");
			if (!IsActivated || !invulnerabilityTimer.Expired(Runner))
				return;

			if (HasStateAuthority && Runner.TryGetSingleton(out GameServer game))
			{
				if (game == null)
				{
					Debug.LogWarning("CoreGamePlay singleton not found, cannot apply damage.");
					return;
				}

				if (@event.damage >= PlayerHP)
				{
					if (game.ActivePlayState == ServerState.LEVEL)
					{
						game.RPC_UpdateKillDeath(new GameServer.PlayerKillEvent { KillerId = @event.targetPlayerRef, TargetId = PlayerId.AsIndex });
						NetHP = 0;
						NetPlayerStage = Stage.Dead; // killed by another player or outpost
						Respawn(PlayerRespawnTime);
					}
				}
				else
				{
					NetHP -= @event.damage;
				}
			}

			if (visuals != null)
				visuals.ShowVFXDamage(this, @event.damage);

			if (@event.targetPlayerRef != PlayerId.AsIndex)// only show damage if not self-inflicted
				tankUI.ShowTextDamage(@event.damage);
		}

		// for show on attacker side immediately
		public override void UpdateHPImmediately(int damage)
		{
			tankUI.ChangeHP(NetHP - damage, NetMaxHP);
		}

		/// <summary>
		/// Apply damage to Tank with an associated impact impulse
		/// </summary>
		/// <param name="impulse"></param>
		/// <param name="damage"></param>
		/// <param name="attacker"></param>
		public void ApplyAreaDamage(int killerIndex, Vector3 impulse, int damage)
		{
			//closed 

			// if (!IsActivated || !invulnerabilityTimer.Expired(Runner))
			// 	return;

			// if (HasStateAuthority && Runner.TryGetSingleton(out CoreGamePlay game))
			// {
			// 	_ncc.Velocity += impulse / 10.0f; // Magic constant to compensate for not properly dealing with masses
			// 	_ncc.Move(Vector3.zero); // Velocity property is only used by CC when steering, so pretend we are, without actually steering anywhere

			// 	if (damage >= PlayerHP)
			// 	{
			// 		if (game.ActivePlayState == CoreGamePlay.PlayState.LEVEL)
			// 		{
			// 			PlayerDeath += 1;
			// 			game.RPC_UpdateKillDeath(killerIndex, PlayerIndex);

			// 			PlayerHP = 0;
			// 			NetPlayerStage = Stage.Dead;
			// 			Respawn(PlayerRespawnTime);
			// 		}
			// 	}
			// 	else
			// 	{
			// 		PlayerHP -= damage;
			// 		Debug.Log($"Player {PlayerId} took {damage} damage, life = {PlayerHP}");
			// 	}
			// }

			// visuals.DamageVisuals.UpdateText(damage);
			// visuals.DamageVisuals.CheckHealth(PlayerHP, PlayerMaxHP);

			// invulnerabilityTimer = TickTimer.CreateFromSeconds(Runner, 0.1f);
		}

		public void Reset()
		{
			Debug.Log($"Resetting player #{PlayerIndex} ID:{PlayerId}");
		}

		public void Respawn(float inSeconds = 0)
		{
			respawnTime = inSeconds;
		}

		private void CheckRespawn()
		{
			if (respawnTime >= 0)
			{
				respawnTime -= Runner.DeltaTime;
				tankUI.UpdateRevivalText(Mathf.CeilToInt(respawnTime));

				if (respawnTime <= 0)
				{
					SpawnPoint spawnpt = Runner.GetLevelManager().GetPlayerSpawnPoint(PlayerTeamIndex, PlayerIndex);
					if (spawnpt == null)
					{
						respawnTime = Runner.DeltaTime;
						return;
					}

					Debug.Log($"Respawning Player #{PlayerIndex} ID:{PlayerId}, life={PlayerHP}, kills={PlayerKill}, deaths={PlayerDeath}, hasStateAuth={Object.HasStateAuthority} from state={NetPlayerStage} @{spawnpt}");

					// Make sure we don't get in here again, even if we hit exactly zero
					respawnTime = -1;

					// Restore health
					NetHP = NetMaxHP;

					// Start the respawn timer and trigger the teleport in effect
					RespawnTimer = TickTimer.CreateFromSeconds(Runner, 1); // Add a small delay to ensure the timer is not expired immediately
					invulnerabilityTimer = TickTimer.CreateFromSeconds(Runner, 3);

					// Place the tank at its spawn point. This has to be done in FUN() because the transform gets reset otherwise
					Transform spawn = spawnpt.transform;
					ncc.Teleport(spawn.position, spawn.rotation);

					// If the player was already here when we joined, it might already be active, in which case we don't want to trigger any spawn FX, so just leave it ACTIVE
					if (NetPlayerStage != Stage.Active)
						NetPlayerStage = Stage.TeleportIn;
					Debug.Log($"Respawned player {PlayerId} @ {spawn.position}, tick={Runner.Tick}, timer={RespawnTimer.IsRunning}:{RespawnTimer.TargetTick}, life={PlayerHP}, kills={PlayerKill}, deaths={PlayerDeath}, hasStateAuth={Object.HasStateAuthority} to state={NetPlayerStage}");
				}
			}
		}

		private void OnAnimStateChanged()
		{
			visuals?.SetAnimation(TankAnimState);
		}

		public void OnPlayerStageChanged()
		{
			if (visuals == null) return;

			switch (NetPlayerStage)
			{
				case Stage.TeleportIn:
					Debug.Log($"Starting teleport for player {PlayerId} @ {transform.position} cc@ {ncc.Data.Position}, tick={Runner.Tick}");
					DOVirtual.DelayedCall(0.7f, () =>
					{
						if (TankVisualsVfx) TankVisualsVfx.SetVfx(TankVisualsVfx.VfxType.SpawnVfxBlue, true, 1f);
					}).SetTarget(this);
					break;

				case Stage.Active:
					visuals.Active();
					break;

				case Stage.Invisible:
					if (!Object.HasInputAuthority)
					{
						// Đồng minh có thể nhìn thấy nhau ở bất kỳ đâu
						// Cùng trong bụi thì sẽ nhìn thấy nhau
						//HideVisuals(false);
					}
					break;

				case Stage.Dead:
					visuals.Dead(this, ExplosionOnDeath);
					abilityManager?.ExitAllAbility();
					ExplosionOnDeath = true;
					break;

				case Stage.TeleportOut:
					SpawnTeleportOutFx();
					break;
			}

			tankUI.EnableUI(NetPlayerStage != Stage.Dead);
			visuals.EnableCollider(NetPlayerStage != Stage.Dead);
		}

		private void SpawnTeleportOutFx()
		{
			if (visuals == null || visuals.TeleportOutPrefab == null) return;

			TankTeleportOutEffect teleout = LocalObjectPool.Acquire(visuals.TeleportOutPrefab, transform.position, transform.rotation, null);
			teleout.StartTeleport(PlayerColor, visuals.Turret.rotation, visuals.Hull.rotation);
		}

		private void RevivalPlayer()
		{
			if (visuals == null || visuals.WeaponManager == null) return;

			Debug.Log($"Resetting player {PlayerId}, tick={Runner.Tick}, timer={RespawnTimer.IsRunning}:{RespawnTimer.TargetTick}, life={PlayerHP}, kills={PlayerKill}, deaths={PlayerDeath}, hasStateAuth={Object.HasStateAuthority} to state={NetPlayerStage}");
			visuals.WeaponManager.ResetAllWeapons();
			NetPlayerStage = Stage.Active;
		}

		[SerializeField] private LayerMask grassLayer, bushLayer;
		[SerializeField] private float radius = 5f;

#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, radius);
		}
#endif

		void CheckStayInBushes()
		{
			if (Object.HasInputAuthority == false) return;

			Collider[] hits = Physics.OverlapSphere(transform.position, radius, grassLayer);
			Player mine = GameServer.Instance.MyTank;
			if (mine == null || mine.IsDead) return;

			if (hits.Length > 0)
			{
				HideOnBushes(true);
			}
			else
			{
				HideOnBushes(false);
			}
		}

		public BushArea currentBush;
		public Player mine;
		Collider[] hits = new Collider[20];
		void CheckInvisibilityTogether()
		{
			mine = GameServer.Instance.MyTank;
			if (mine == null) return;

			if (IsDead) return; // dead can not see anyone

			int cnt = Physics.OverlapSphereNonAlloc(transform.position, radius, hits, bushLayer, QueryTriggerInteraction.Collide);
			if (cnt > 0)
			{
				for (int i = 0; i < cnt; i++)
				{
					if (hits[i] != null && hits[i].TryGetComponent(out BushArea bush))
					{
						currentBush = bush;
						break;
					}
				}
			}
			else currentBush = null;

			// Same team can always see each other
			if (PlayerTeamIndex == mine.PlayerTeamIndex)
			{
				tankPartVisual.EnableVisual(true);
				visuals.EnableVisuals(true);
				return;
			}
			else // different team
			{
				if (mine.currentBush == null && NetPlayerStage == Stage.Invisible) // you stand outside of bush, you can not see enemy in bush
				{
					tankPartVisual.EnableVisual(false);
					visuals.EnableVisuals(false);
					HasLocalSignal = false;
				}
				else
				{
					// case1: enemy is outside of bush, you can see him
					if (NetPlayerStage == Stage.Active)
					{
						// can see him
						tankPartVisual.EnableVisual(true);
						visuals.EnableVisuals(true);
						HasLocalSignal = true;
						return;
					}

					// case2: both players are in the same bush, they can see each other
					if (mine.currentBush != null && currentBush != null &&
					mine.NetPlayerStage == Stage.Invisible &&
					NetPlayerStage == Stage.Invisible &&
					mine.currentBush.GetInstanceID() == currentBush.GetInstanceID())
					{
						// can see him
						tankPartVisual.EnableVisual(true);
						visuals.EnableVisuals(true);
						HasLocalSignal = true;
						return;
					}

					// case3: enemy is in bush, you are outside of bush, you can not see him
					if (NetPlayerStage == Stage.Invisible || mine.NetPlayerStage == Stage.Active)
					{
						// can not see him
						tankPartVisual.EnableVisual(false);
						visuals.EnableVisuals(false);
						HasLocalSignal = false;
						return;
					}
				}
			}
		}

		public void HideOnBushes(bool isHide)
		{
			if (NetPlayerStage == Stage.Active && isHide == true)
			{
				NetPlayerStage = Stage.Invisible;
			}
			else if (isHide == false && NetPlayerStage == Stage.Invisible)
			{
				NetPlayerStage = Stage.Active;
			}
		}

		public void TeleportOut()
		{
			if (NetPlayerStage == Stage.Dead || NetPlayerStage == Stage.TeleportOut || NetPlayerStage == Stage.New)
				return;

			if (Object.HasStateAuthority)
				NetPlayerStage = Stage.TeleportOut;
		}

		public enum Stage
		{
			New,
			TeleportOut,
			TeleportIn,
			Active,
			Dead,
			Invisible
		}

		public struct PickupEvent : INetworkEvent
		{
			public int powerup;
		}

		public struct EffectEvent : INetworkEvent
		{
			public byte effectId;         // 1
			public byte srcPlayerId;      // 1
			public float duration;        // 4
			public float value;           // 4
			public float angle;           // 4 hướng knockback/hook

			public static Vector3 ConvertToDir(float angle)
			{
				float rad = angle * Mathf.Deg2Rad;
				return new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad));
			}
			public static float ConvertToAngle(Vector3 dir)
			{
				return Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
			}
		}
	}
}