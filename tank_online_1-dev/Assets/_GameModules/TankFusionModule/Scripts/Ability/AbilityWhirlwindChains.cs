using System.Linq;
using Fusion;
using Fusion.Utility;
using FusionHelpers;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

namespace Fusion.TankOnlineModule
{
	public class AbilityWhirlwindChains : AbilityBase
	{
        // public float castTime = 0f;
        // public float duration = 5f;
        // public float cooldown = 1f;

		public override AbilityPropertyBase AbilityPropsBase { get; } = new WhirlwindAbilityProperty();
		public WhirlwindAbilityProperty WhirlwindProps => AbilityPropsBase as WhirlwindAbilityProperty;
		private Dictionary<Player, float> nextPlayerDamageTime = new();
		private Dictionary<Turret, float> nextTurretDamageTime = new();
		public override void Initialize(Player player)
		{
			base.Initialize(player);

			// WhirlwindProps.castTime = castTime;
			// WhirlwindProps.duration = duration;
			// WhirlwindProps.cooldown = cooldown;
		}
		public override void Active(NetworkRunner runner, PlayerRef owner, Vector3 ownerVelocity)
		{
			base.Active(runner, owner, ownerVelocity);
			IsBlockAutoAim = false;
		}
		protected override void OnAbilityTrigged()
		{
			base.OnAbilityTrigged();
			Transform exit = GetExitPoint(Runner.Tick);
			_bullets.Add(runner, new ShotState(player.transform.position, exit.position, Vector3.zero, scale: Vector3.one), WhirlwindProps.duration);
			if (player) player.TankAnimState = 1;
		}
		protected override void AbilityUpdateHandler(float deltaTime)
		{
			base.AbilityUpdateHandler(deltaTime);
			//follow player position
			_bullets.Process(this, (ref ShotState bullet, int tick) =>
			{
				bullet.Direction = Vector3.zero;
				bullet.IsAnimActive = (timeElapsed < WhirlwindProps.duration - 0.2f);
				bullet.Position = this.transform.position;
				int playerDamage = Random.Range(player.PlayerData.Damage[0], player.PlayerData.Damage[1]);
				int damage = (int)(WhirlwindProps.damageRatio * playerDamage);
				int cnt = ApplyAreaDamage(bullet.Position, WhirlwindProps.radius, damage, 0, _bulletPrefab.HitMask);
				return true;
			});
		}
		public override void Exit()
		{
			base.Exit();
			if (_bullets != null)
				_bullets.Process(this, (ref ShotState bullet, int tick) =>
				{
					bullet.EndTick = Runner.Tick;
					return true;
				});
			if (player) player.TankAnimState = 0;
			nextPlayerDamageTime.Clear();
			nextTurretDamageTime.Clear();
		}

		protected override int ApplyAreaDamage(Vector3 hitPoint, float radius, int damage, float areaImpulse, LayerMask hitMask)
		{
			int cnt = Physics.OverlapSphereNonAlloc(hitPoint, radius, _areaHits, hitMask.value, QueryTriggerInteraction.Ignore);
			var damagedPlayers = new HashSet<Player>();
			var damagedTurrets = new HashSet<Turret>();
			float now = Time.time;

			for (int i = 0; i < cnt; i++)
			{
				GameObject other = _areaHits[i].gameObject;
				if (other)
				{
					Player target = other.GetComponent<Player>();
					if (target != null && target != player && target.PlayerTeamIndex != player.PlayerTeamIndex && !damagedPlayers.Contains(target))
					{
						// Kiểm tra delay
						if (!nextPlayerDamageTime.TryGetValue(target, out float nextTime) || now >= nextTime)
						{
							damagedPlayers.Add(target);
							nextPlayerDamageTime[target] = now + WhirlwindProps.delayTicks;

							Vector3 impulse = other.transform.position - hitPoint;
							float l = Mathf.Clamp(radius - impulse.magnitude, 0, radius);
							impulse = areaImpulse * l * impulse.normalized;
							target.RaiseEvent(new DamageEvent { targetPlayerRef = player.PlayerId.AsIndex, damage = damage });

							Vector3 knockDir = (target.transform.position - player.transform.position).normalized;
							target.RaiseEvent(new Player.EffectEvent
							{
								effectId = (int)EffectData.EffectType.Knockback,
								duration = WhirlwindProps.knockDuration,
								value = WhirlwindProps.knockRange,
								angle = Player.EffectEvent.ConvertToAngle(knockDir),
								srcPlayerId = (byte)player.PlayerId.AsIndex
							});
						}
					}

					Turret turret = other.GetComponent<Turret>();
					if (turret != null && turret.TurretTeamIndex != player.PlayerTeamIndex && !damagedTurrets.Contains(turret))
					{
						if (!nextTurretDamageTime.TryGetValue(turret, out float nextTime) || now >= nextTime)
						{
							damagedTurrets.Add(turret);
							nextTurretDamageTime[turret] = now + WhirlwindProps.delayTicks;

							turret.RPC_DealDamage(new DamageEvent { playerId = player.PlayerId.AsIndex, teamIndex = player.PlayerTeamIndex, targetPlayerRef = player.PlayerId.AsIndex, damage = damage });
						}
					}
				}
			}
			return cnt;
		}

		public class WhirlwindAbilityProperty : AbilityPropertyBase
		{
			public float radius = 4f;
			public float damageRatio = 0.3f;
			public float knockDuration = 0.1f;
			public float knockRange = 1f;
			public float delayTicks = 0.5f; //0.5seconds
		}
	}
}