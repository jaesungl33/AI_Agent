using Fusion;
using Fusion.Utility;
using FusionHelpers;
using UnityEngine;
using System.Collections.Generic;

namespace Fusion.TankOnlineModule
{
	public class AbilityStorm : AbilityBase
	{
		public override AbilityPropertyBase AbilityPropsBase { get; } = new StormAbilityProperty();
		public StormAbilityProperty StormProps => AbilityPropsBase as StormAbilityProperty;

		private Transform exit;
		public override void Initialize(Player player)
		{
			base.Initialize(player);
		}
		public override void Active(NetworkRunner runner, PlayerRef owner, Vector3 ownerVelocity)
		{
			base.Active(runner, owner, ownerVelocity);
		}
		protected override void OnAbilityTrigged()
		{
			base.OnAbilityTrigged();
			exit = GetExitPoint(Runner.Tick);
			//-----------------------------

			_bullets.Add(Runner, new ShotState(player.transform.position, exit.position, exit.forward, ownerId: (byte)player.PlayerId.AsIndex), StormProps.duration);
		}

		protected override void AbilityUpdateHandler(float deltaTime)
		{
			base.AbilityUpdateHandler(deltaTime);
			if (!trigged) return;
			_bullets.Process(this, (ref ShotState bullet, int tick) =>
			{
				if (bullet.IsOutOfRange(StormProps.range))
				{
					// Set direction về phía player và thời gian tồn tại (đồng bộ với ForceMove)
					//bullet.IsCustomHited = true;
					//bullet.IsCancel = true;
					ApplyDamage(bullet.Position);
					bullet.EndTick = Runner.Tick;
					return true;
				}

				if (!_bulletPrefab.IsHitScan && bullet.EndTick > Runner.Tick)
				{
					Vector3 dir = bullet.Direction.normalized;
					float length = Mathf.Max(_bulletPrefab.Radius, player.PlayerData.ProjectileSpeed * Runner.DeltaTime);
					if (Physics.Raycast(bullet.Position - length * dir, dir, out var hitinfo, length, _bulletPrefab.HitMask.value, QueryTriggerInteraction.Ignore))
					{
						bullet.Position = hitinfo.point;
						ApplyDamage(bullet.Position);
						//--------------------------
						bullet.EndTick = Runner.Tick;
						return true;
					}
				}
				return false;
			});
		}
		public void ApplyDamage(Vector3 position)
		{
			int playerDamage = Random.Range(player.PlayerData.Damage[0], player.PlayerData.Damage[1]);
			int damage = (int)(StormProps.damageRatio * playerDamage);
			int cnt = ApplyAreaDamage(position, StormProps.radiusExplosion, damage, 0, _bulletPrefab.HitMask);
			var affectedPlayers = new HashSet<Player>();
			for (int i = 0; i < cnt; i++)
			{
				GameObject other = AreaHits[i].gameObject;
				if (other == null || other == this.player.gameObject)
					continue;
				Player targetPlayer = other.GetComponent<Player>();
				if (targetPlayer != null && targetPlayer.PlayerTeamIndex != player.PlayerTeamIndex && !affectedPlayers.Contains(targetPlayer))
				{
					affectedPlayers.Add(targetPlayer);
					targetPlayer.RaiseEvent(new Player.EffectEvent
					{
						effectId = (int)EffectData.EffectType.SlowStorm,
						duration = StormProps.slowDuration,
						value = StormProps.slowValue,
						srcPlayerId = (byte)player.PlayerId.AsIndex
					});
				}
			}
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
		}
		protected override Transform GetExitPoint(int tick)
		{
			var mainWeapon = player.visuals.WeaponManager.GetMainWeapon();
			if (mainWeapon != null)
			{
				return mainWeapon.GetExitPoint(tick);
			}
			else
			{
				Debug.LogWarning("Main weapon not found on player " + player.name);
				return null;
			}
		}

		#region  Helpers
		public override CustomProperty[] GetCustomPropertiesRuntime()
		{
			var baseProps = base.GetCustomPropertiesRuntime();
			var props = new CustomProperty[baseProps.Length + 1];
			baseProps.CopyTo(props, 0);
			props[baseProps.Length] = new CustomProperty
			{
				propertyName = "Range",
				propertyValue = StormProps.range
			};
			return props;
		}
		#endregion
	}

	public class StormAbilityProperty : AbilityPropertyBase
	{
		public float range = 15f;
		public float radiusExplosion = 2f;
		public float damageRatio = 0.5f;

		public float slowValue = 0.4f;
		public float slowDuration = 3f;
	}
}