using System.Linq;
using Fusion;
using Fusion.Utility;
using FusionHelpers;
using Unity.VisualScripting;
using UnityEngine;

namespace Fusion.TankOnlineModule
{
	public class AbilityBearTrap : AbilityBase
	{
		public override AbilityPropertyBase AbilityPropsBase { get; } = new BearTrapAbilityProperty();
		public BearTrapAbilityProperty BearTrapProps => AbilityPropsBase as BearTrapAbilityProperty;

		// Ability config
		private int trapStack = 0;
		private float trapStackTimer;

		const float TRAP_STACK_RECOVERY_TIME = 10f; // seconds

		public override bool AvailableActive
		{
			get
			{
				if (!player)
					return false;

				// Kiểm tra số lượng trap hiện tại trên bản đồ và khoảng cách tối thiểu
				Vector3 playerPos = player.transform.position;
				//int trapsCount = 0;
				bool tooClose = false;
				foreach (var item in State.bulletStates)
				{
					if (item.EndTick > Runner.Tick)
					{
						//trapsCount++;
						if (Vector3.Distance(playerPos, item.Position) < BearTrapProps.minTrapDistance)
						{
							tooClose = true;
							break;
						}
					}
				}
				return base.AvailableActive
					&& trapStack > 0
					&& !tooClose
					&& countingDown <= 0f;
			}
		}
		public override void Initialize(Player player)
		{
			base.Initialize(player);
			trapStack = BearTrapProps.maxTrapsStack;
			trapStackTimer = TRAP_STACK_RECOVERY_TIME;
		}
		public override void Active(NetworkRunner runner, PlayerRef owner, Vector3 ownerVelocity)
		{
			if (!AvailableActive)
			{
				Debug.LogWarning("Cannot place trap: not available (stack, max traps, or too close).");
				return;
			}
			base.Active(runner, owner, ownerVelocity);

		}
		protected override void OnAbilityTrigged()
		{
			base.OnAbilityTrigged();
			// Không dùng OnAbilityTrigged để xử lý trap, vì trap cần luôn update để phát hiện mục tiêu
			Vector3 playerPos = player.transform.position;

			// Lấy StartTick cũ nhất từ activeTraps
			var activeTraps = State.bulletStates
				.Where(item => item.EndTick > Runner.Tick)
				.ToList();

			if (activeTraps.Count >= BearTrapProps.maxTrapExits)
			{
				// Tìm trap có EndTick nhỏ nhất (cũ nhất)
				int oldestEndTick = int.MaxValue;
				foreach (var trap in activeTraps)
				{
					if (trap.EndTick < oldestEndTick)
						oldestEndTick = trap.EndTick;
				}
				// Dùng _bullets.Process để end tick trap cũ nhất
				_bullets.Process(this, (ref ShotState bullet, int tick) =>
				{
					if (bullet.EndTick == oldestEndTick && bullet.EndTick > Runner.Tick)
					{
						bullet.IsCancel = true;
						bullet.EndTick = Runner.Tick;
						//Debug.LogErrorFormat("Cancel oldest trap at {0}", bullet.Position);
						return true;
					}
					return false; // Dừng khi đã hủy trap cũ nhất
				});
			}
			// Tạo trap mới
			Transform exit = GetExitPoint(Runner.Tick);
			_bullets.Add(runner, new ShotState(playerPos, exit.position, Vector3.zero, teamIndex: player.PlayerTeamIndex), BearTrapProps.trapDuration);
			if (trapStack >= BearTrapProps.maxTrapsStack)
				trapStackTimer = 0f;
			trapStack--;
		}
		// Luôn được gọi, dùng để hồi trap stack và xử lý trap phát hiện mục tiêu.
		protected override void AbilityAllTimeUpdateHandler(float deltaTime)
		{
			// Hồi lại trap stack theo thời gian, chỉ khi chưa đầy
			if (trapStack < BearTrapProps.maxTrapsStack)
			{
				trapStackTimer += deltaTime;
				if (trapStackTimer >= BearTrapProps.coolDownStack)
				{
					trapStack++;
					// Nếu vừa đầy thì giữ timer, không reset về 0
					if (trapStack < BearTrapProps.maxTrapsStack)
						trapStackTimer = 0f;
				}
			}

			// Xử lý trap phát hiện mục tiêu
			_bullets.Process(this, (ref ShotState bullet, int tick) =>
			{
				if (!_bulletPrefab.IsHitScan && bullet.EndTick > Runner.Tick)
				{
					var trapDelayTicks = bullet.StartTick + (int)(BearTrapProps.trapDelay / Runner.DeltaTime);
					if (Runner.Tick < trapDelayTicks)
					{
						// Trap is still in delay period, do not activate
						return false;
					}

					var target = FindClosestEnemy(bullet.Position, BearTrapProps.radius, _bulletPrefab.HitMask);
					if (target != null)
					{
						var playerTarget = target.GetComponent<Player>();
						if (playerTarget != null)
						{
							// Apply root effect and damage
							playerTarget.RaiseEvent(new Player.EffectEvent
							{
								effectId = (int)EffectData.EffectType.Root,
								duration = BearTrapProps.rootEffectDuration,
								value = 1,
								srcPlayerId = (byte)player.PlayerId.AsIndex
							});
							ApplySingleDamage(playerTarget, BearTrapProps.damageRatio);
							bullet.EndTick = Runner.Tick;
							return true;
						}
					}
				}
				return false;
			});
		}

		public override void Exit()
		{
			base.Exit();
			// Không xóa trap ngay lập tức khi thoát ability
		}

		public override void Clear()
		{
			base.Clear();
			trapStack = BearTrapProps.maxTrapsStack;
			trapStackTimer = TRAP_STACK_RECOVERY_TIME;
		}
		// Hàm lấy số lượng trap stack hiện tại (nếu cần cho UI)
		public int GetTrapStack()
		{
			return trapStack;
		}

		#region  Helpers
		// return base GetCustomPropertiesRuntime, and add more param GetTrapStack(), and add more _trapStackTimer	
		public override CustomProperty[] GetCustomPropertiesRuntime()
		{
			var baseProps = base.GetCustomPropertiesRuntime();
			//Debug.LogWarningFormat("GetCustomPropertiesRuntime: trapStack={0}, trapStackTimer={1}, coolDownStack={2}", trapStack, trapStackTimer, coolDownStack);
			var extraProps = new[]
			{
				new CustomProperty { propertyName = "Stack", propertyValue = GetTrapStack() },
				new CustomProperty { propertyName = "StackTimer", propertyValue = BearTrapProps.coolDownStack - trapStackTimer },
				new CustomProperty { propertyName = "CoolDownStack", propertyValue = BearTrapProps.coolDownStack }
			};
			return baseProps.Concat(extraProps).ToArray();
		}
		#endregion
	}

	public class BearTrapAbilityProperty : AbilityPropertyBase
	{
		// Trap config
		public float trapDuration = 10f;
		public float trapDelay = 0.5f;
		public float radius = 2f;
		public float damageRatio = 1f;
		public float rootEffectDuration = 3f;
		public float minTrapDistance = 2f;

		// Trap stack & limits
		public int maxTrapExits = 3;
		public int maxTrapsStack = 3;
		public int coolDownStack = 10; // seconds
	}
}