using Fusion;
using FusionHelpers;
using UnityEngine;

namespace Fusion.TankOnlineModule
{
	public struct ShotState : ISparseState<Shot>
	{
		/// <summary>
		/// Generic sparse state properties required by the interface
		/// </summary>
		public int StartTick { get; set; }
		public int EndTick { get; set; }

		/// <summary>
		/// Shot specific sparse properties
		/// </summary>
		public Vector3 Position;
		public Vector3 Direction;
		public Vector3 StartPosition { get; set; }

		public Vector3 Scale;

		public bool IsCancel;
		public bool IsCustomHited;

		public bool IsAnimActive;

		public int OwnerId;
		public int TeamIndex;
		public ShotState(Vector3 parentPosition, Vector3 startPosition, Vector3 direction, Vector3 scale = default, int ownerId = 1, int teamIndex = -1, bool isAnimActive = false)
		{
			StartPosition = parentPosition;
			StartTick = 0;
			EndTick = 0;
			Position = startPosition;
			Direction = direction;
			Scale = scale;
			IsCancel = false;
			IsCustomHited = false;
			OwnerId = ownerId;
			TeamIndex = teamIndex;
			IsAnimActive = isAnimActive;
		}

		public bool IsOutOfRange(float range)
		{
			return Vector3.Distance(StartPosition, Position) > range;
		}

		public void Extrapolate(float t, Shot prefab)
		{
			Position = GetPositionAt(t, prefab);
			Direction = GetDirectionAt(t, prefab);
		}

		public Vector3 GetTargetPosition(Shot prefab)
		{
			float a = 0.5f * prefab.Gravity.y;
			float b = prefab.Speed * Direction.y;
			float c = Position.y;
			float d = b * b - 4 * a * c;
			float t = (-b - Mathf.Sqrt(d)) / (2 * a);
			Vector3 p = GetPositionAt(t, prefab);
			p.y = 0.05f; // Return the position with a slight y offset to avoid placing target where it will end up z-fighting with the ground;
			return p;
		}

		private Vector3 GetPositionAt(float t, Shot prefab) => Position + t * (prefab.Speed * Direction + 0.5f * t * prefab.Gravity);
		private Vector3 GetDirectionAt(float t, Shot prefab) => prefab.Speed == 0 ? Direction : (prefab.Speed * Direction + t * prefab.Gravity).normalized;
	}

	public class Shot : MonoBehaviour, ISparseVisual<ShotState, Shot>
	{
		[Header("Settings")]
		[SerializeField] private ProjectileType _projectileType;
		[SerializeField] private Vector3 _gravity;
		[SerializeField] private float _speed;
		[SerializeField] private float _radius;
		[SerializeField] private LayerMask _hitMask;
		[SerializeField] private float _range;
		[SerializeField] private float _areaRadius;
		[SerializeField] private float _areaImpulse;
		[SerializeField] private int[] _areaDamage;
		[SerializeField] private float _timeToLive;
		[SerializeField] private bool _isHitScan;

		[Header("GameObject")]
		[SerializeField] protected Transform panelPrefab;
		[Header("Fx Prefabs")]
		[SerializeField] private ExplosionFX _detonationPrefab;
		[SerializeField] private ExplosionFX customHitedPrefab;
		[SerializeField] private TargetMarker _targetPrefab;
		[SerializeField] private MuzzleFlash _muzzleFxPrefab;

		public Vector3 Gravity => _gravity;
		public float Speed => _speed;
		public float Radius => _radius;
		public LayerMask HitMask => _hitMask;
		public float Range { get => _range; set => _range = value; }
		public float AOERadius => _areaRadius;
		public float AreaImpulse => _areaImpulse;
		public int[] AreaDamage => _areaDamage;
		public float TimeToLive => _timeToLive;
		public bool IsHitScan => _isHitScan;
		public ProjectileType ProjectileType => _projectileType;

		private Transform _xform;
		protected bool isCustomHited = false;
		private void Awake()
		{
			_xform = transform;
		}

		public virtual void ApplyStateToVisual(NetworkBehaviour owner, ShotState state, float t, bool isFirstRender, bool isLastRender)
		{
			if (isLastRender)
			{
				OnLastRender(state);
			}
			if (!state.IsCustomHited) isCustomHited = false;
			if (state.IsCustomHited && !isCustomHited)
			{
				OnCustomHited(state);
				isCustomHited = true;
			}
			if (isFirstRender)
			{
				OnFirstRender(state);
			}
			_xform.forward = state.Direction;
			_xform.position = state.Position;
			if (state.Scale != default)
				_xform.localScale = state.Scale;
		}
		protected virtual void OnFirstRender(ShotState state)
		{
			if (_targetPrefab) LocalObjectPool.Acquire(_targetPrefab, state.GetTargetPosition(this), Quaternion.identity);
			if (_muzzleFxPrefab) LocalObjectPool.Acquire(_muzzleFxPrefab, state.Position, Quaternion.LookRotation(state.Direction)).OnFire(state);
			isCustomHited = false;
		}
		protected virtual void OnLastRender(ShotState state)
		{
			if (!state.IsCancel)
			{
				if (IsHitScan && _detonationPrefab)
					LocalObjectPool.Acquire(_detonationPrefab, state.Position + state.Direction, Quaternion.identity);
				else if (_detonationPrefab)
					LocalObjectPool.Acquire(_detonationPrefab, state.Position, Quaternion.identity);
			}
		}
		protected virtual void OnCustomHited(ShotState state)
		{
			if (customHitedPrefab && !state.IsCancel)
				LocalObjectPool.Acquire(customHitedPrefab, state.Position, Quaternion.identity);
		}
		public void OverrideData(MatchPlayerData data)
		{
			_speed = data.ProjectileSpeed;
			Range = data.Range;
			_areaDamage = data.Damage;
		}

		public virtual bool IsMatchTeammate(int teamIndex)
		{
			var localPlayer = GameServer.Instance.MyTank;
			if (!localPlayer)
				return true;
			if (localPlayer.PlayerTeamIndex < 0 || teamIndex < 0)
				return true;
			return localPlayer.PlayerTeamIndex == teamIndex;
		}
	}
}