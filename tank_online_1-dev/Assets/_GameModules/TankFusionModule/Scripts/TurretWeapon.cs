using Fusion;
using Fusion.Utility;
using FusionHelpers;
using Unity.VisualScripting;
using UnityEngine;

namespace Fusion.TankOnlineModule
{
	/// <summary>
	/// The Weapon class controls how fast a weapon fires, which projectiles it uses
	/// and the start position and direction of projectiles.
	/// </summary>
	public class TurretWeapon : NetworkBehaviourWithState<TurretWeapon.NetworkState>
	{
		[Networked] public override ref NetworkState State => ref MakeRef<NetworkState>();
		public struct NetworkState : INetworkStruct
		{
			[Networked, Capacity(10)] 
			public NetworkArray<ShotState> bulletStates => default;
		}

		[SerializeField] private Transform[] _gunExits;
		[SerializeField] private float _rateOfFire;
		[SerializeField] private byte _ammo;
		[SerializeField] private bool _infiniteAmmo;
		[SerializeField] private LaserSightLine _laserSight;
		[SerializeField] private PowerupType _powerupType = PowerupType.DEFAULT;
		[SerializeField] private Shot _bulletPrefab;
		[SerializeField] private Turret _turret;
		[SerializeField] private Transform _target;

		private SparseCollection<ShotState, Shot> bullets;
		private float _visible;
		private bool _active;
		private Collider[] _areaHits = new Collider[4];

		public float Delay => _rateOfFire;
		public bool IsShowing => _visible >= 1.0f;
		public byte Ammo => _ammo;
		public bool InfiniteAmmo => _infiniteAmmo;
		public PowerupType powerupType => _powerupType;
		public ProjectileType projectileType;
		public MatchPlayerData data;

		public void Initialize(Turret turret, MatchPlayerData data)
		{
			this._turret = turret;
			this.data = data;

			_rateOfFire = 1 / data.FireRate;
			_ammo = data.ProjectileCount;
			if (_bulletPrefab != null)
				_bulletPrefab.OverrideData(data);
		}

		public override void Spawned()
		{
			bullets = new SparseCollection<ShotState, Shot>(State.bulletStates, _bulletPrefab);
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (_target != null)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawLine(_turret.transform.position, _target.position);
				if (_bulletPrefab != null)
				{
					Vector3 dir = (_target.position - _turret.transform.position).normalized;
					float t = (_target.position - _turret.transform.position).magnitude / _bulletPrefab.Speed;
					Vector3 p = _turret.transform.position + t * (_bulletPrefab.Speed * dir + 0.5f * t * _bulletPrefab.Gravity);
					Gizmos.color = Color.red;
					Gizmos.DrawLine(_turret.transform.position, p);
					//Debug.Log($"distance: {(_target.position - _turret.transform.position).magnitude}, time: {t}, target: {_target.position}, predicted: {p}");
				}
			}
		}
#endif
        public override void FixedUpdateNetwork()
		{
			bullets.Process(this, (ref ShotState bullet, int tick) => // process each bullet added to the collection
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
					float length = Mathf.Max(_bulletPrefab.Radius, data.ProjectileSpeed * Runner.DeltaTime);
					// float length = Mathf.Max(_bulletPrefab.Radius, _bulletPrefab.Speed * Runner.DeltaTime);
					if (Physics.Raycast(bullet.Position - length * dir, dir, out var hitinfo, length, _bulletPrefab.HitMask.value, QueryTriggerInteraction.Ignore))
					//					if (Runner.LagCompensation.Raycast(bullet.Position - length*dir, dir, length, Object.InputAuthority, out var hitinfo, _bulletPrefab.HitMask.value, HitOptions.IgnoreInputAuthority | HitOptions.IncludePhysX))
					{
						_target = hitinfo.transform;
						bullet.Position = hitinfo.point;
						bullet.EndTick = Runner.Tick;
						if (_bulletPrefab.ProjectileType == ProjectileType.Single)
							ApplySingleDamage(hitinfo.collider.transform.root.GetComponent<Player>());
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

			bullets.Render(this, from.bulletStates);
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

			if (show)
				transform.localScale = Tween.easeOutElastic(0, 1, _visible) * Vector3.one;
			else
				transform.localScale = Tween.easeInExpo(0, 1, _visible) * Vector3.one;
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
				bullets.Add(runner, new ShotState(_turret.transform.position, exit.position, exit.forward), _bulletPrefab.TimeToLive);
			}

			// bool impact;
			// if (runner.GameMode == GameMode.Shared)
			// {
			// 	impact = runner.GetPhysicsScene().Raycast(exit.position, exit.forward, out var hitinfo, _bulletPrefab.Range, _bulletPrefab.HitMask.value);
			// }
			// else
			// {
			// 	impact = Runner.LagCompensation.Raycast(exit.position, exit.forward, _bulletPrefab.Range, Object.InputAuthority, out var hitinfo, _bulletPrefab.HitMask.value, HitOptions.IgnoreInputAuthority | HitOptions.IncludePhysX);
			// }

			// if (impact)
			// 	bullets.Add(runner, new ShotState(exit.position, exit.forward), _bulletPrefab.TimeToLive);
		}
		
		private void ApplySingleDamage(Player target)
		{
			if (target != null && target.NetTeammateIndex != _turret.TurretTeamIndex)
			{
				var damageEvent = new DamageEvent { targetPlayerRef = MatchIndexs.Turret, damage = Random.Range(data.Damage[0], data.Damage[1]) };
				target.UpdateHPImmediately(damageEvent.damage);	
				target.RaiseEvent(damageEvent);
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
						if (target != null && target.NetTeammateIndex != _turret.TurretTeamIndex)
						{
							Vector3 impulse = other.transform.position - hitPoint;
							float l = Mathf.Clamp(_bulletPrefab.AOERadius - impulse.magnitude, 0, _bulletPrefab.AOERadius);
							impulse = _bulletPrefab.AreaImpulse * l * impulse.normalized;
							target.RaiseEvent(new DamageEvent { targetPlayerRef =  MatchIndexs.Turret, damage = Random.Range(data.Damage[0], data.Damage[1]) });
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