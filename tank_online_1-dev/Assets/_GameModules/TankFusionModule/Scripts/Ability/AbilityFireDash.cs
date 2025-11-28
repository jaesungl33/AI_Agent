using Fusion;
using Fusion.Utility;
using FusionHelpers;
using UnityEngine;

namespace Fusion.TankOnlineModule
{
	public class AbilityFireDash : AbilityBase
	{
		public override AbilityPropertyBase AbilityPropsBase { get; } = new FireDashAbilityProperty();
		public FireDashAbilityProperty FireDashProps => AbilityPropsBase as FireDashAbilityProperty;

		private Transform exit;
		Vector3 aimDir;
		EffectData subEffect_1;
		EffectData subEffect_2;

		private Vector3 startPos;
		private bool isDashing = false;
		private Vector3 startFlameFieldPos;
		private Vector3 lastFlameFieldPos;
		private float flameFieldDurationRemain = 0f;
		public override void Initialize(Player player)
		{
			base.Initialize(player);
			//_bullets.Add(player.Runner, new ShotState(Vector3.zero, Vector3.zero, Vector3.zero, scale: Vector3.one * radius), 0.1f);
		}
		public override void Active(NetworkRunner runner, PlayerRef owner, Vector3 ownerVelocity)
		{
			base.Active(runner, owner, ownerVelocity);
			aimDir = ownerVelocity.normalized;
			exit = GetExitPoint(Runner.Tick);
		}
		protected override void OnAbilityTrigged()
		{
			base.OnAbilityTrigged();

			//Self effect
			// Root + ImmuneCC
			subEffect_1 = player.EffectApplier?.ApplyEffect(new EffectData
			{
				effectId = (int)EffectData.EffectType.Root,
				duration = FireDashProps.duration,
				lifeTime = FireDashProps.duration,
				value = 1,
				forceCC = true,
				srcPlayerId = (byte)player.PlayerId.AsIndex
			});
			subEffect_2 = player.EffectApplier?.ApplyEffect(new EffectData
			{
				effectId = (int)EffectData.EffectType.ImmuneCC,
				duration = FireDashProps.duration,
				lifeTime = FireDashProps.duration,
				value = 1,
				forceCC = true,
				srcPlayerId = (byte)player.PlayerId.AsIndex
			});
			//-----------------------------
			//start dash
			_bullets.Add(runner, new ShotState(player.transform.position, exit.position, Vector3.zero, scale: Vector3.one * FireDashProps.radius), FireDashProps.flameFieldDuration);
			startPos = player.transform.position;
			isDashing = true;

			startFlameFieldPos = player.transform.position;
			lastFlameFieldPos = player.transform.position;
			flameFieldDurationRemain = FireDashProps.flameFieldDuration;
		}
		protected override void AbilityUpdateHandler(float deltaTime)
		{
			base.AbilityUpdateHandler(deltaTime);
			if (!trigged) return;
			if (HasStateAuthority && isDashing)
			{
				DashUpdate(aimDir, deltaTime);
				DamageTrigger();

				// Cập nhật lastFlameFieldPos chỉ khi đang dash
				if (isDashing)
					lastFlameFieldPos = player.transform.position;
			}
		}

		protected override void AbilityAllTimeUpdateHandler(float deltaTime)
		{
			base.AbilityAllTimeUpdateHandler(deltaTime);
			flameFieldDurationRemain -= deltaTime;
			if (flameFieldDurationRemain <= 0f)
			{
				flameFieldDurationRemain = 0f;
			}
			else
			{
				FireDashProps.flameFieldTick -= deltaTime;
				if (FireDashProps.flameFieldTick <= 0f)
				{
					FireDashProps.flameFieldTick = 0.25f;
					// Sử dụng aimDir làm hướng flame field
					// nếu lastFlameFieldPos!= startFlameFieldPos thì không tạo flame field
					if ((lastFlameFieldPos - startFlameFieldPos).sqrMagnitude <= 0.001f)
					{
						_bullets.Process(this, (ref ShotState bullet, int tick) =>
						{
							bullet.EndTick = Runner.Tick;
							return true;
						});
						return;
					}

					// //nếu lastFlameFieldPos!= startFlameFieldPos thì lastFlameFieldPos lùi lại 1 chút về phía startFlameFieldPos
					// if ((lastFlameFieldPos - startFlameFieldPos).sqrMagnitude > 0.01f)
					// {
					// 	Vector3 toStart = (startFlameFieldPos - lastFlameFieldPos).normalized;
					// 	lastFlameFieldPos += toStart * 0.5f; //lùi lại 0.5f
					// }

					Vector3 dir = aimDir.sqrMagnitude > 0.01f ? aimDir.normalized : Vector3.forward;
					Vector3 center = (lastFlameFieldPos + startFlameFieldPos) / 2 + dir;
					Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

					Collider[] colliders = Physics.OverlapBox(center, new Vector3(FireDashProps.flameFieldWidth / 2, 0.5f, FireDashProps.flameFieldRange / 2), rot, _bulletPrefab.HitMask.value);
					foreach (var hit in colliders)
					{
						if (hit == null) continue;
						float yDelta = Mathf.Abs(hit.transform.position.y - center.y);
						if (yDelta > 1.0f) continue;

						if (hit.transform.root.TryGetComponent<Player>(out Player target))
						{
							ApplySingleDamage(target, FireDashProps.flameFieldDamageRatio);
						}
					}
				}
			}
		}
		public override void Exit()
		{
			base.Exit();
			if (_bullets != null)
				_bullets.Process(this, (ref ShotState bullet, int tick) =>
				{
					//bullet.EndTick = Runner.Tick;
					return true;
				});
			player?.EffectApplier?.RemoveEffect(subEffect_1);
			player?.EffectApplier?.RemoveEffect(subEffect_2);
		}
		public override void Clear()
		{
			base.Clear();
			flameFieldDurationRemain = 0f;
			isDashing = false;
		}

		public void DashUpdate(Vector3 direction, float deltaTime)
		{
			if (player == null || player.visuals == null) return;
			if (!isDashing) return;
			// Chuẩn hóa hướng dash trên mặt phẳng XZ
			direction.y = 0;
			direction = direction.normalized;

			// Tính phần trăm thời gian đã dash
			float elapsed = Mathf.Clamp01(timeElapsed / FireDashProps.duration);

			// Hàm ease out quad: tốc độ lớn lúc đầu, nhỏ dần về cuối
			float ease = 1 - (1 - elapsed) * (1 - elapsed);

			// Tính vị trí mới theo tỷ lệ quãng đường
			Vector3 start = startPos;
			Vector3 target = start + direction * FireDashProps.range;
			Vector3 nextPos = Vector3.Lerp(start, target, ease);

			// Di chuyển bằng ForceMove để đảm bảo đồng bộ vật lý và mạng
			player.NCC.ForceMoveInstant(nextPos);

			// Xoay hull theo hướng dash (xoay nhanh nhưng vẫn mượt)
			if (player.visuals.Hull != null && direction.sqrMagnitude > 0.001f)
			{
				float maxDegrees = 1000f * deltaTime;
				player.visuals.Hull.rotation = Quaternion.RotateTowards(
					player.visuals.Hull.rotation,
					Quaternion.LookRotation(-direction, Vector3.up),
					maxDegrees
				);
				player.HullDirection = player.visuals.Hull.rotation.eulerAngles.y;
			}
		}

		public void DamageTrigger()
		{
			// Kiểm tra va chạm, nếu có thì ApplyAreaDamage, không cần xử lý damage thủ công
			_bullets.Process(this, (ref ShotState bullet, int tick) =>
			{
				if (!isDashing)
					return false;
				//bullet.IsCustomHited = false;
				bullet.Position = exit.position;
				bullet.Direction = exit.forward.normalized;
				bullet.IsCancel = false;
				Collider[] colliders = new Collider[16];
				int cnt = Physics.OverlapSphereNonAlloc(
					bullet.Position,
					FireDashProps.radius,
					colliders,
					_bulletPrefab.HitMask.value,
					QueryTriggerInteraction.Ignore
				);
				// if (cnt > 0)
				// 	bullet.IsCustomHited = true;
				for (int i = 0; i < cnt; i++)
				{
					var hit = colliders[i];
					if (hit == null) continue;
					// Loại bỏ va chạm với chính bản thân player
					if (hit.transform == player.transform) continue;
					//nếu hit.mask có layer là Breakable thì continue

					if (hit.gameObject.layer == LayerMask.NameToLayer("Breakable"))
					{
						// bullet.IsCustomHited = true;
						continue;
					}
					if (hit.gameObject.layer == LayerMask.NameToLayer("Wall"))
					{
						bullet.IsCustomHited = true;
					}
					if (hit.transform.root.TryGetComponent<Player>(out Player target))
					{
						// target.RaiseEvent(new Player.EffectEvent
						// {
						// 	effectId = (int)EffectData.EffectType.Stun,
						// 	duration = knockDuration,
						// 	value = 1,
						// 	forceCC = false,
						// 	srcPlayerIndex = player.PlayerIndex
						// });
						// Vector3 knockPos là knockback 1 unit về phía sau target so với vị trí player
						Vector3 knockDir = (target.transform.position - player.transform.position).normalized;
						//Vector3 knockPos = target.transform.position + knockDir * 1f;

						target.RaiseEvent(new Player.EffectEvent
						{
							effectId = (int)EffectData.EffectType.Knockback,
							duration = FireDashProps.knockDuration,
							value = FireDashProps.knockRange,
							angle = Player.EffectEvent.ConvertToAngle(knockDir),
							srcPlayerId = (byte)player.PlayerId.AsIndex
						});
						ApplySingleDamage(target, FireDashProps.damageRatio);
						bullet.IsCustomHited = true;
						//isDashing = false;
						//ForceExit();
					}
					if (hit.transform.root.TryGetComponent<Turret>(out Turret turret))
					{
						ApplySingleDamage(turret, FireDashProps.damageRatio);
						bullet.IsCustomHited = true;
						//isDashing = false;
						//ForceExit();
					}
					isDashing = false;
					ForceExit();
					break;
				}
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
				propertyValue = FireDashProps.range
			};
			return props;
		}
		#endregion
	}

	public class FireDashAbilityProperty : AbilityPropertyBase
	{
		public float range = 10f;
		public float damageRatio = 1f;
		public float radius = 1f;
		public float knockDuration = 0.5f;
		public float knockRange = 2f;

		//Flame Field
		public float flameFieldDuration = 3f;
		public float flameFieldDamageRatio = 0.25f;
		public float flameFieldRange = 7f;
		public float flameFieldWidth = 1.5f;
		public float flameFieldTick = 0.25f; //0.25 seconds/ tick
	}
}