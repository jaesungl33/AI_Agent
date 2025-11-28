using System.Linq;
using Fusion;
using Fusion.Utility;
using FusionHelpers;
using UnityEngine;
using System.Collections.Generic;
namespace Fusion.TankOnlineModule
{
    public enum AbilityUseType
    {
        Weapon,
        Instant,
        Direction,
        Ground,
        ChannelingDirection,
    }
    public class AbilityBase : NetworkBehaviourWithState<AbilityBase.NetworkState>
    {
        [Networked] public override ref NetworkState State => ref MakeRef<NetworkState>();
        public struct NetworkState : INetworkStruct
        {
            [Networked, Capacity(12)]
            public NetworkArray<ShotState> bulletStates => default;
        }
        #region Fields
        [Header("Fixed Ability Info")]
        [SerializeField] public string abilityID;
        public virtual AbilityPropertyBase AbilityPropsBase { get; } = new AbilityPropertyBase();
        // Ability metadata
        // [SerializeField] public string abilityID;
        // [SerializeField] public string abilityName;
        // [SerializeField] public string description;

        // // Ability timing
        // [SerializeField] public float castTime = 1f;
        // [SerializeField] public float duration = 2f;
        // [SerializeField] public float cooldown = 1f;

        [SerializeField] public AbilityUseType abilityUseType = AbilityUseType.Instant;
        [SerializeField] public bool IsBlockAutoAim { get; set; }
        // Projectile and firing
        [SerializeField] protected Shot _bulletPrefab;
        [SerializeField] protected Transform[] _gunExits;
        protected SparseCollection<ShotState, Shot> _bullets;

        // Area damage
        protected readonly Collider[] _areaHits = new Collider[16];

        // Owner
        protected Player player;
        protected NetworkRunner runner;
        protected PlayerRef owner;
        protected Vector3 ownerVelocity;
        // State
        protected float timeElapsed = 0f;
        protected float countingDown = 0f; //--> special param, don't reset on init becase tank dead not reset cooldown
        private float visible;
        private bool active;
        protected bool trigged;
        #endregion

        #region Properties

        public bool Visible => visible >= 1.0f;
        public virtual bool AvailableActive => Visible && countingDown <= 0f && timeElapsed <= 0f && !IsActive;
        public bool IsActive => active;
        public float CooldownRemaining => countingDown;
        public float TimeElapsed => timeElapsed;
        public bool IsTrigged => trigged;
        public Collider[] AreaHits => _areaHits;
        #endregion

        #region Unity & Fusion Methods
        public void Awake()
        {
            visible = 1.0f;
            timeElapsed = 0f;
            countingDown = 0f;
            active = false;
            trigged = false;

        }
        public virtual void Initialize(Player player)
        {
            this.player = player;
            visible = 1.0f;
            timeElapsed = 0f;
            active = false;
            trigged = false;
            //countingDown = 0f;// don't reset cooldown on init (dead)

            AbilityPropsBase.LoadProperties(abilityID);
        }
        //----------------
        public override void Spawned()
        {
            base.Spawned();
            _bullets = new SparseCollection<ShotState, Shot>(State.bulletStates, _bulletPrefab);
        }
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            if (!Visible)
                return;
            AbilityAllTimeUpdateHandler(Runner.DeltaTime);

            countingDown = countingDown < 0 ? 0 : countingDown - Runner.DeltaTime;
            if (countingDown > 0)
                return;

            if (!IsActive)
                return;

            timeElapsed += Runner.DeltaTime;

            AbilityUpdateHandler(Runner.DeltaTime);
            if (timeElapsed >= AbilityPropsBase.duration)
            {
                Exit();
                return;
            }
        }


        public override void Render()
        {
            base.Render();
            if (TryGetStateChanges(out var from, out var to))
            {
            }
            else
                TryGetStateSnapshots(out from, out _, out _, out _, out _);
            //Debug.Log($"AbilityBase Render: {abilityName} for player: {_player.name}, timeElapsed: {timeElapsed}");
            _bullets.Render(this, from.bulletStates);
        }
        //---------------
        #endregion

        #region Ability Lifecycle

        public virtual void Active(NetworkRunner runner, PlayerRef owner, Vector3 ownerVelocity)
        {
            if (!AvailableActive)
                return;
            timeElapsed = 0f; // Start timer
            active = true;
            trigged = false;
            countingDown = 0f; // Reset cooldown, will start after ability ends

            //Debug.LogError($"====== Ability {abilityName} On Active");
            // Optionally spawn projectile or trigger effect here
            this.runner = runner;
            this.owner = owner;
            this.ownerVelocity = ownerVelocity;
            IsBlockAutoAim = true;
        }
        protected virtual void OnAbilityTrigged()
        {
            // Optionally handle ability triggered logic here
        }

        public virtual void Exit()
        {
            timeElapsed = 0f;
            active = false;
            trigged = false;
            countingDown = AbilityPropsBase.cooldown;
            IsBlockAutoAim = false;
            //Debug.LogError($"====== Ability {abilityName} On Exit, cooldown: {countingDown}");
            // Optionally: cleanup or end effect logic
        }
        public void ForceExit()
        {
            timeElapsed = AbilityPropsBase.duration;
        }

        public virtual void OnPlayerDestroyed()
        {
            Exit();
        }

        public virtual void Clear()
        {
            Exit();
            countingDown = 0f;
            if (_bullets != null)
                _bullets.Process(this, (ref ShotState bullet, int tick) =>
                {
                    bullet.EndTick = Runner.Tick;
                    return true;
                });
        }
        #endregion

        #region Projectile Logic

        protected virtual void SpawnNetworkShot(NetworkRunner runner, PlayerRef owner, Transform exit, Vector3 ownerVelocity)
        {
            _bullets.Add(runner, new ShotState(player.transform.position, exit.position, exit.forward), _bulletPrefab.TimeToLive);
        }

        protected virtual void AbilityUpdateHandler(float deltaTime)
        {
            if (timeElapsed >= AbilityPropsBase.castTime && !trigged)
            {
                trigged = true; // ensure only add once
                OnAbilityTrigged();
            }
            if (!trigged)
                return;
        }

        // Called every frame, regardless of ability state
        // Useful for projectile updates after ability ends
        protected virtual void AbilityAllTimeUpdateHandler(float deltaTime)
        {

        }

        protected virtual Transform GetExitPoint(int tick)
        {
            return _gunExits[tick % _gunExits.Length];
        }

        #endregion

        #region Damage Logic

        protected virtual void ApplySingleDamage(IFusionObject target, float modifyRatio = 1.0f)
        {
            if (target != null && target.PlayerTeamIndex != player.PlayerTeamIndex)
            {
                int damage = Random.Range(player.PlayerData.Damage[0], player.PlayerData.Damage[1]);
                damage = Mathf.FloorToInt(damage * modifyRatio);
                Debug.Log($"Applying single damage to target: {target.PlayerName} with damage: {damage}");
                target.RaiseEvent(new DamageEvent { targetPlayerRef = player.PlayerId.AsIndex, damage = damage });
            }
        }

        protected virtual void ApplySingleDamage(Turret target, float modifyRatio = 1.0f)
        {
            if (target != null && target.TurretTeamIndex != player.PlayerTeamIndex)
            {
                int damage = Random.Range(player.PlayerData.Damage[0], player.PlayerData.Damage[1]);
                damage = Mathf.FloorToInt(damage * modifyRatio);
                Debug.Log($"Damaged turret: {target.name} with damage: {damage}");
                target.RPC_DealDamage(new DamageEvent { playerId = player.PlayerId.AsIndex, teamIndex = player.PlayerTeamIndex, targetPlayerRef = player.PlayerId.AsIndex, damage = damage });
            }
        }

        protected virtual int ApplyAreaDamage(Vector3 hitPoint, float radius, int damage, float areaImpulse, LayerMask hitMask)
        {
            int cnt = Physics.OverlapSphereNonAlloc(hitPoint, radius, _areaHits, hitMask.value, QueryTriggerInteraction.Ignore);

            // Dùng HashSet để tránh gây damage lặp cho cùng 1 Player/Turret (do nhiều collider con)
            var damagedPlayers = new HashSet<Player>();
            var damagedTurrets = new HashSet<Turret>();

            for (int i = 0; i < cnt; i++)
            {
                GameObject other = _areaHits[i].gameObject;
                if (other)
                {
                    Player target = other.GetComponent<Player>();
                    if (target != null && target != player && target.PlayerTeamIndex != player.PlayerTeamIndex && !damagedPlayers.Contains(target))
                    {
                        damagedPlayers.Add(target);
                        Vector3 impulse = other.transform.position - hitPoint;
                        float l = Mathf.Clamp(radius - impulse.magnitude, 0, radius);
                        impulse = areaImpulse * l * impulse.normalized;
                        target.RaiseEvent(new DamageEvent { targetPlayerRef = player.PlayerId.AsIndex, damage = damage });
                    }

                    Turret turret = other.GetComponent<Turret>();
                    if (turret != null && turret.TurretTeamIndex != player.PlayerTeamIndex && !damagedTurrets.Contains(turret))
                    {
                        damagedTurrets.Add(turret);
                        turret.RPC_DealDamage(new DamageEvent { playerId = player.PlayerId.AsIndex, teamIndex = player.PlayerTeamIndex, targetPlayerRef = player.PlayerId.AsIndex, damage = damage });
                    }
                }
            }
            return cnt;
        }


        protected virtual GameObject FindClosestEnemy(Vector3 position, float maxDistance, LayerMask hitMask)
        {
            GameObject closest = null;
            float closestDistSqr = maxDistance * maxDistance;
            int cnt = Physics.OverlapSphereNonAlloc(position, maxDistance, _areaHits, hitMask.value, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < cnt; i++)
            {
                GameObject other = _areaHits[i].gameObject;
                if (other)
                {
                    Player target = other.GetComponent<Player>();
                    if (target != null && target != player && target.PlayerTeamIndex != player.PlayerTeamIndex)
                    {
                        float distSqr = (other.transform.position - position).sqrMagnitude;
                        if (distSqr < closestDistSqr)
                        {
                            closestDistSqr = distSqr;
                            closest = other;
                        }
                    }
                }
            }
            return closest;
        }

        #endregion

        #region  Helpers
        // return CooldownRemaining, abilityUseType
        public virtual CustomProperty[] GetCustomPropertiesRuntime()
        {
            return new CustomProperty[]
            {
                new CustomProperty
                {
                    propertyName = "CooldownRemaining",
                    propertyValue = CooldownRemaining
                },
                new CustomProperty
                {
                    propertyName = "Cooldown",
                    propertyValue = AbilityPropsBase.cooldown
                },
                new CustomProperty
                {
                    propertyName = "AbilityUseType",
                    propertyValue = (int)abilityUseType
                }
            };
        }
        public ushort GetRandomUshortWithPlayer()
        {
            ushort randomId = (ushort)(((byte)player.PlayerId.AsIndex << 8) | UnityEngine.Random.Range(0, 256));
            return randomId;
        }
        public ushort GetAbilityIdUshort()
        {
            ushort randomId = (ushort)(((byte)player.PlayerId.AsIndex << 8) | UnityEngine.Random.Range(0, 256));
            return randomId;
        }
        #endregion
    }

    public class AbilityPropertyBase
    {
        public string abilityID;
        public string abilityName;
        public string description;
        public float castTime = 0f;
        public float duration = 2f;
        public float cooldown = 1f;

        public virtual void LoadProperties(string _abilityID)
        {
            DatabaseManager.GetDB<TankAbilityCollection>(result =>
            {
                if (result != null)
                {
                    var propDoc = result.GetTankAbilityDocumentById(_abilityID);
                    if (propDoc != null)
                    {
                        //gán các giá trị từ propDoc vào các trường tương ứng
                        abilityID = propDoc.abilityID;
                        abilityName = propDoc.abilityName;
                        description = propDoc.description;
                        castTime = propDoc.castTime;
                        duration = propDoc.duration;
                        cooldown = propDoc.cooldown;
                        // các properties khác khai báo ở lớp con sẽ được load từ propDoc.customProperties
                        foreach (var customProp in propDoc.customProperties)
                        {
                            var field = this.GetType().GetField(customProp.propertyName);
                            if (field != null)
                            {
                                if (field.FieldType == typeof(float))
                                {
                                    field.SetValue(this, customProp.propertyValue);
                                }
                                else if (field.FieldType == typeof(int))
                                {
                                    field.SetValue(this, (int)customProp.propertyValue);
                                }
                                // Thêm các kiểu dữ liệu khác nếu cần
                            }
                        }
                        Debug.LogWarning("ability load Done: " + abilityID);
                    }
                    else
                    {
                        Debug.LogWarning($"AbilityPropertyBase: No TankAbilityDocument found for abilityID: {abilityID}");
                    }
                }
            });
        }
    }
}