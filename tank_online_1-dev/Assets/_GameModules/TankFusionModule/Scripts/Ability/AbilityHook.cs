using Fusion;
using Fusion.Utility;
using FusionHelpers;
using UnityEngine;

namespace Fusion.TankOnlineModule
{
	public class AbilityHook : AbilityBase
	{
		public override AbilityPropertyBase AbilityPropsBase { get; } = new HookAbilityProperty();
		public HookAbilityProperty HookProps => AbilityPropsBase as HookAbilityProperty;

		private Transform exit;

		EffectData subEffect_1;
		EffectData subEffect_2;
		public override void Initialize(Player player)
		{
			base.Initialize(player);

			// foreach (var prop in AbilityPropsBase.GetType().GetFields())
			// {
			// 	Debug.LogWarning($"AbilityPropsBase - {prop.Name}: {prop.GetValue(HookProps)}");
			// }
			// //foreach HookProps và show giá trị props
			// foreach (var prop in HookProps.GetType().GetFields())
			// {
			// 	Debug.Log($"HookProps - {prop.Name}: {prop.GetValue(HookProps)}");
			// }
		}
		public override void Active(NetworkRunner runner, PlayerRef owner, Vector3 ownerVelocity)
		{
			base.Active(runner, owner, ownerVelocity);
			_isReturning = false;
		}
		protected override void OnAbilityTrigged()
		{
			base.OnAbilityTrigged();
			exit = GetExitPoint(Runner.Tick);
			//Self effect
			// Root + ImmuneCC
			subEffect_1 = player.EffectApplier?.ApplyEffect(new EffectData
			{
				effectId = (int)EffectData.EffectType.Root,
				duration = HookProps.duration,
				lifeTime = HookProps.duration,
				value = 1,
				forceCC = true,
				srcPlayerId = (byte)player.PlayerId.AsIndex
			});
			subEffect_2 = player.EffectApplier?.ApplyEffect(new EffectData
			{
				effectId = (int)EffectData.EffectType.ImmuneCC,
				duration = HookProps.duration,
				lifeTime = HookProps.duration,
				value = 1,
				forceCC = true,
				srcPlayerId = (byte)player.PlayerId.AsIndex
			});
			//-----------------------------

			_bullets.Add(Runner, new ShotState(player.transform.position, exit.position, exit.forward, ownerId: (byte)player.PlayerId.AsIndex), HookProps.duration);
		}


		bool _isReturning = false;
		protected override void AbilityUpdateHandler(float deltaTime)
		{
			base.AbilityUpdateHandler(deltaTime);
			if (!trigged) return;

			_bullets.Process(this, (ref ShotState bullet, int tick) =>
			{
				// Nếu đạn vượt quá tầm, cho quay về player
				if (bullet.IsOutOfRange(HookProps.range))
				{
					SetBulletReturnToPlayer(ref bullet);
					bullet.IsCustomHited = true;
					bullet.IsCancel = true;
					return true;
				}

				// Nếu đạn đang đi ra, kiểm tra va chạm
				if (!_bulletPrefab.IsHitScan && bullet.EndTick > Runner.Tick && !_isReturning)
				{
					Vector3 dir = bullet.Direction.normalized;
					float length = Mathf.Max(_bulletPrefab.Radius, player.PlayerData.ProjectileSpeed * Runner.DeltaTime);
					Vector3 rayOrigin = bullet.Position - length * dir;

					if (Physics.Raycast(rayOrigin, dir, out var hitinfo, length, _bulletPrefab.HitMask.value, QueryTriggerInteraction.Ignore))
					{
						bullet.Position = hitinfo.point;
						bool handled = false;

						// Va chạm với Player
						if (hitinfo.collider.transform.root.TryGetComponent<Player>(out Player target))
						{
							ApplySingleDamage(target, HookProps.damageRatio);
							float distance = Vector3.Distance(target.transform.position, player.transform.position);
							float hookDuration = distance * HookProps.hookDurationPerUnit;
							Vector3 hookDir = (player.transform.position - target.transform.position).normalized;

							target.RaiseEvent(new Player.EffectEvent
							{
								effectId = (int)EffectData.EffectType.Hook,
								duration = hookDuration,
								value = distance,
								angle = Player.EffectEvent.ConvertToAngle(hookDir),
								srcPlayerId = (byte)player.PlayerId.AsIndex
							});

							SetHookEnemy(ref bullet, hookDuration);
							handled = true;
						}
						// Va chạm với Turret
						else if (hitinfo.collider.TryGetComponent<Turret>(out Turret turret))
						{
							ApplySingleDamage(turret, HookProps.damageRatio);
							SetBulletReturnToPlayer(ref bullet);
							handled = true;
						}
						// Va chạm với vật thể khác
						else
						{
							SetBulletReturnToPlayer(ref bullet);
							handled = true;
						}

						if (handled)
						{
							bullet.IsCustomHited = true;
							return true;
						}
					}
				}
				return false;
			});
		}
		private void SetHookEnemy(ref ShotState bullet, float? customHookDuration = null)
		{
			// Dừng đạn lại, chờ kéo về
			float EXTEND_DURATION = 1f;
			float returnDuration = customHookDuration ?? (Vector3.Distance(bullet.Position, player.transform.position) * HookProps.hookDurationPerUnit);
			returnDuration += EXTEND_DURATION;
			bullet.Direction = Vector3.zero;
			bullet.EndTick = Runner.Tick + (int)(returnDuration / Runner.DeltaTime);
			_isReturning = true;
			//duration = returnDuration;
			//dựa vào returnDuration để set lại timeElapsed kết thúc ability
			timeElapsed = HookProps.duration - (returnDuration + 0.5f);
		}

		private void SetBulletReturnToPlayer(ref ShotState bullet)
		{
			// Kéo đạn về player
			float distance = Vector3.Distance(bullet.Position, player.transform.position);
			float returnDuration = distance * HookProps.hookDurationPerUnit;
			Vector3 moveDir = (player.transform.position - bullet.Position).normalized;
			float moveStep = distance * (Runner.DeltaTime / Mathf.Max(returnDuration, 0.01f));
			bullet.Direction = moveDir * moveStep;
			bullet.EndTick = Runner.Tick + (int)(returnDuration / Runner.DeltaTime);
			_isReturning = true;
			//duration = returnDuration + 0.5f;
			//dựa vào returnDuration để set lại timeElapsed kết thúc ability nới ra 0.5f
			timeElapsed = HookProps.duration - (returnDuration + 0.5f);
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
			player?.EffectApplier?.RemoveEffect(subEffect_1);
			player?.EffectApplier?.RemoveEffect(subEffect_2);
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
				propertyValue = HookProps.range
			};
			return props;
		}
		#endregion
	}

	public class HookAbilityProperty : AbilityPropertyBase
	{
		public float range = 20f;
		public float damageRatio = 1f;
		public float hookDurationPerUnit = 0.02f;
	}
}