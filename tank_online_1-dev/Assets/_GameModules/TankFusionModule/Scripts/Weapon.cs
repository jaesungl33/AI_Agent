using Fusion;
using Fusion.Utility;
using FusionHelpers;
using UnityEngine;

namespace Fusion.TankOnlineModule
{
	/// <summary>
	/// The Weapon class controls how fast a weapon fires, which projectiles it uses
	/// and the start position and direction of projectiles.
	/// </summary>

	public class Weapon : NetworkBehaviourWithState<Weapon.NetworkState>
	{
		[Networked] public override ref NetworkState State => ref MakeRef<NetworkState>();
		public struct NetworkState : INetworkStruct
		{
			[Networked, Capacity(12)]
			public NetworkArray<ShotState> bulletStates => default;
		}
		[SerializeField] private Transform[] _gunExits;
		[SerializeField] private float _rateOfFire;
		[SerializeField] private byte _ammo;
		[SerializeField] private bool _infiniteAmmo;
		[SerializeField] private LaserSightLine _laserSight;
		[SerializeField] private PowerupType _powerupType = PowerupType.DEFAULT;
		[SerializeField] private Shot _bulletPrefab;
		[SerializeField] private string _bulletId;
		private SparseCollection<ShotState, Shot> bullets;
		private float _visible;
		private bool _active;
		private Collider[] _areaHits = new Collider[4];
		private Player _player;

		public float delay => _rateOfFire;
		public bool isShowing => _visible >= 1.0f;
		public byte ammo => _ammo;
		public bool infiniteAmmo => _infiniteAmmo;

		public PowerupType powerupType => _powerupType;

		public void Initialize(Player player)
		{
			if (player == null)
			{
				Debug.LogError("Weapon Initialize failed: player is null");
				return;
			}
			
			_player = player;

			_rateOfFire = 1/_player.PlayerData.FireRate;
			_ammo = _player.PlayerData.ProjectileCount;
			if (_bulletPrefab != null)
				_bulletPrefab.OverrideData(_player.PlayerData);
		}

		public void UpdateFireRate()
		{
			_rateOfFire = 1/_player.PlayerData.FireRate;
		}

		public override void Spawned()
		{
			bullets = new SparseCollection<ShotState, Shot>(State.bulletStates, _bulletPrefab);
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			// show the red line to virtual target with range of bulletPrefab
			if (_bulletPrefab != null)
			{
				Vector3 targetPos = _gunExits[0].position + _gunExits[0].forward * _bulletPrefab.Range;
				Gizmos.color = Color.red;
				Gizmos.DrawLine(_gunExits[0].position, targetPos);
				Vector3 dir = (targetPos - _gunExits[0].position).normalized;
				float a = 0.5f * _bulletPrefab.Gravity.y;
				float b = _bulletPrefab.Speed * dir.y;
				float c = _gunExits[0].position.y;
				float d = b * b - 4 * a * c;
				if (d < 0)
					return;
				float t = (-b - Mathf.Sqrt(d)) / (2 * a);
				Vector3 p = _gunExits[0].position + t * (_bulletPrefab.Speed * dir + 0.5f * t * _bulletPrefab.Gravity);
				Gizmos.color = Color.red;
				Gizmos.DrawLine(_gunExits[0].position, p);
				//Debug.Log($"distance: {(targetPos - _gunExits[0].position).magnitude}, time: {t}, target: {targetPos}, predicted: {p}");
			}
		}
#endif

		public override void FixedUpdateNetwork()
		{
			bullets.Process(this, (ref ShotState bullet, int tick) =>
			{
				if (bullet.Position.y < -.15f)
				{
					bullet.EndTick = Runner.Tick;
					return true;
				}

				if (bullet.IsOutOfRange(_bulletPrefab.Range))
				{
					bullet.EndTick = Runner.Tick;
					return true;
				}

				if (!_bulletPrefab.IsHitScan && bullet.EndTick > Runner.Tick)
				{
					Vector3 dir = bullet.Direction.normalized;
					float length = Mathf.Max(_bulletPrefab.Radius, _player.PlayerData.ProjectileSpeed * Runner.DeltaTime);
					if (Physics.Raycast(bullet.Position - length * dir, dir, out var hitinfo, length, _bulletPrefab.HitMask.value, QueryTriggerInteraction.Ignore))
					//if (Runner.LagCompensation.Raycast(bullet.Position - length*dir, dir, length, Object.InputAuthority, out var hitinfo, _bulletPrefab.HitMask.value, HitOptions.IgnoreInputAuthority | HitOptions.IncludePhysX))
					{
						bullet.IsCustomHited = true;
						bullet.Position = hitinfo.point;
						bullet.EndTick = Runner.Tick;
						if (_bulletPrefab.ProjectileType == ProjectileType.Single)
						{
							if (hitinfo.collider.transform.root.TryGetComponent<Player>(out Player target))
							{
								ApplySingleDamage(target);
							}
							else if (hitinfo.collider.TryGetComponent<Turret>(out Turret turret))
							{
								ApplySingleDamage(turret);
							}
						}
						else if (_bulletPrefab.ProjectileType == ProjectileType.AOE)
							ApplyAreaDamage(hitinfo.point);
						return true;
					}
				}
				return false;
			});
		}

		public override void Render()
		{
			if (TryGetStateChanges(out var from, out var to))
				OnFireTickChanged();
			else
				TryGetStateSnapshots(out from, out _, out _, out _, out _);

			bullets.Render(this, from.bulletStates );
		}

		/// <summary>
		/// Control the visual appearance of the weapon. This is controlled by the Player based
		/// on the currently selected weapon, so the boolean parameter is entirely derived from a
		/// networked property (which is why nothing in this class is sync'ed).
		/// </summary>
		/// <param name="show">True if this weapon is currently active and should be visible</param>
		public void Show(bool show)
		{
			if (_active && !show)
			{
				ToggleActive(false);
			}
			else if (!_active && show)
			{
				ToggleActive(true);
			}

			_visible = Mathf.Clamp(_visible + (show ? Time.deltaTime : -Time.deltaTime) * 5f, 0, 1);
			gameObject.SetActive(show);
		}

		private void ToggleActive(bool value)
		{
			_active = value;

			if (_laserSight != null)
			{
				if (_active)
				{
					_laserSight.SetDuration(0.5f);
					_laserSight.Activate();
				}
				else
					_laserSight.Deactivate();
			}
		}

		public void Fire(NetworkRunner runner, PlayerRef owner, Vector3 ownerVelocity)
		{
			if (powerupType == PowerupType.EMPTY || _gunExits.Length == 0)
				return;
			
			Transform exit = GetExitPoint(Runner.Tick);
			
			Debug.DrawLine(exit.position, exit.position+exit.forward, Color.blue, 1.0f);
			SpawnNetworkShot(runner, owner, exit, ownerVelocity);
		}

		private void OnFireTickChanged()
		{
			// Recharge the laser sight if this weapon has it
			if (_laserSight != null)
				_laserSight.Recharge();
		}

		private void SpawnNetworkShot(NetworkRunner runner, PlayerRef owner, Transform exit, Vector3 ownerVelocity)
		{
			if(_bulletPrefab.ProjectileType == ProjectileType.Single)
			{
				bullets.Add(runner, new ShotState(_player.transform.position, exit.position, exit.forward), _bulletPrefab.TimeToLive);
			}

			// if (_bulletPrefab.IsHitScan)
			// {
			// 	bool impact;
			// 	Vector3 hitPoint = exit.position + _bulletPrefab.Range * exit.forward;
			// 	if (runner.GameMode == GameMode.Shared)
			// 	{
			// 		impact = runner.GetPhysicsScene().Raycast(exit.position, exit.forward, out var hitinfo, _bulletPrefab.Range, _bulletPrefab.HitMask.value);
			// 		hitPoint = hitinfo.point;
			// 	}
			// 	else
			// 	{
			// 		impact = Runner.LagCompensation.Raycast(exit.position, exit.forward, _bulletPrefab.Range, Object.InputAuthority, out var hitinfo, _bulletPrefab.HitMask.value, HitOptions.IgnoreInputAuthority | HitOptions.IncludePhysX);
			// 		hitPoint = hitinfo.Point;
			// 	}

			// 	if (impact)
			// 	{
			// 		ApplyAreaDamage(hitPoint);
			// 	}

			// 	bullets.Add( runner, new ShotState(exit.position, hitPoint-exit.position), 0);
			// }
			// else
			// 	bullets.Add(runner, new ShotState(exit.position, exit.forward), _bulletPrefab.TimeToLive);
		}
		
		private void ApplySingleDamage(IFusionObject target)
		{
			if (target != null && target.PlayerTeamIndex != _player.PlayerTeamIndex)
			{
				// int damage = Random.Range(_bulletPrefab.AreaDamage[0], _bulletPrefab.AreaDamage[1]);
				int damage = Random.Range(_player.PlayerData.Damage[0], _player.PlayerData.Damage[1]);
				Debug.Log("Applying single damage to target: " + target.PlayerName + " with damage: " + damage);
				var damageEvent = new DamageEvent { targetPlayerRef = _player.PlayerId.AsIndex, damage = damage };
				target.UpdateHPImmediately(damageEvent.damage);
				target.RaiseEvent(damageEvent);
			}
		}

		private void ApplySingleDamage(Turret target)
		{
			Debug.Log($"Applying single damage to turret: {target.name} at index: {target.TurretTeamIndex}");
			if (target != null && target.TurretTeamIndex != _player.PlayerTeamIndex)
			{
				int damage = Random.Range(_player.PlayerData.Damage[0], _player.PlayerData.Damage[1]);
				Debug.Log("Damaged to turret: " + target.name + " with damage: " + damage);
				var damageEvent = new DamageEvent { playerId = _player.PlayerId.AsIndex, teamIndex = _player.PlayerTeamIndex, targetPlayerRef = _player.PlayerId.AsIndex, damage = damage };
				//target.UpdateHPImmediately(damageEvent.damage);
				target.RPC_DealDamage(damageEvent);
			}
		}

		private void ApplyAreaDamage(Vector3 hitPoint)
		{
			int cnt = Physics.OverlapSphereNonAlloc(hitPoint, _bulletPrefab.AOERadius, _areaHits, _bulletPrefab.HitMask.value, QueryTriggerInteraction.Ignore);
			if (cnt > 0)
			{
				for (int i = 0; i < cnt; i++)
				{
					GameObject other = _areaHits[i].gameObject;
					if (other)
					{
						Player target = other.GetComponent<Player>();
						if (target != null && target != _player
						&& target.PlayerTeamIndex != _player.PlayerTeamIndex)
						{
							Vector3 impulse = other.transform.position - hitPoint;
							float l = Mathf.Clamp(_bulletPrefab.AOERadius - impulse.magnitude, 0, _bulletPrefab.AOERadius);
							var damageEvent = new DamageEvent { targetPlayerRef = _player.PlayerId.AsIndex, damage = Random.Range(_player.PlayerData.Damage[0], _player.PlayerData.Damage[1]) };
							target.UpdateHPImmediately(damageEvent.damage);
							target.RaiseEvent(damageEvent);
						}
					}
				}
			}
		}

		public Transform GetExitPoint(int tick)
		{
			Transform exit = _gunExits[tick% _gunExits.Length];
			return exit;
		}
	}
}