using System;
using ExitGames.Client.Photon.StructWrapping;
using FusionHelpers;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

namespace Fusion.TankOnlineModule
{
    [RequireComponent(typeof(NetworkTransform))]
    public class Turret : NetworkBehaviour
    {
        [SerializeField] private Animator animator;
        public TurretWeapon weapon;
        public float detectTime = 1f;
        public float rotationSpeed = 5f;
        public Vector3 velocity = Vector3.zero;
        private TickTimer primaryFireDelay;
        private int teamIndex = MatchIndexs.Attacker;
        private Collider[] _areaHits = new Collider[10];
        [SerializeField] private Player _target;
        [SerializeField] private Transform area;
        [SerializeField] private SpriteRenderer areaRenderer;
        [SerializeField] private Color[] areaColor;
        [SerializeField] private Transform turretHead;
        [SerializeField] private Transform explosion;
        [Networked] public NetworkString<_32> NetOutpostId { get; set; }
        [Networked] private Angle NetworkedY { get; set; }
        [Networked] private Angle NetworkedX { get; set; }
        [Networked] public int NetHP { get; set; }
        [Networked] public int NetMaxHP { get; set; }
        [Networked] public float NetRange { get; set; }
        [Networked] private TickTimer TargetDetectionTimer { get; set; }
        [Networked] public bool IsAlive { get; set; }
        private int lastHP { get; set; }
        public int TurretTeamIndex { get => teamIndex; }
        public Transform TurretHead { get => turretHead; private set => turretHead = value; }
        private ChangeDetector _changes;
        [SerializeField] private DamageVisual damageVisual;
        [SerializeField] private UnityEngine.UI.Image virtualHP, realHP;
        [SerializeField] private GameObject hpBar;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public override void Spawned()
        {
            if (Object.HasStateAuthority)
            {
                TargetDetectionTimer = TickTimer.CreateFromSeconds(Runner, detectTime);
                IsAlive = true;
            }
            Debug.Log("Turret spawned and initialized.");

            _areaHits = new Collider[10];
            _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

            teamIndex = DatabaseManager.GetDB<MatchmakingCollection>().GetActiveDocument().MaxPlayers <= 1 ? MatchIndexs.Attacker : MatchIndexs.Defender;
        }

        private void OnValueChanged()
		{
            var interpolated = new NetworkBehaviourBufferInterpolator(this);
			// Detect changes in player properties
            foreach (var change in _changes.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(NetOutpostId):
                        Init(NetOutpostId.ToString());
                        break;

                    case nameof(NetHP):
                    case nameof(NetMaxHP):
                        OnHealthChanged();
                        break;

                    case nameof(NetworkedX):
                        turretHead.rotation = Quaternion.Euler(interpolated.Angle(nameof(NetworkedX)), interpolated.Angle(nameof(NetworkedY)), 0);
                        break;

                    case nameof(NetworkedY):
                        turretHead.rotation = Quaternion.Euler(interpolated.Angle(nameof(NetworkedX)), interpolated.Angle(nameof(NetworkedY)), 0);
                        break;
                }
            }
		}

        public void Init(string id)
        {
            NetOutpostId = id;
            MatchPlayerData data = DatabaseManager.CreateMatchPlayer(id);
            NetHP = data.HP;
            lastHP = data.HP;
            NetMaxHP = data.MaxHitpoints;
            weapon.Initialize(this, data);
            animator.SetFloat("attack_speed", 1.4f * 1 / weapon.data.FireRate);
            NetRange = data.Range;
            detectTime = weapon.Delay / 100f;
            explosion?.gameObject?.SetActive(false);
            hpBar.SetActive(true);
        }

        private void ShowArea()
        {
            var localPlayer = GameServer.Instance.MyTank;
            var pos = localPlayer?.transform.position;
            var rangeToLocalPlayer = Vector3.Distance(transform.position, pos ?? Vector3.zero);
            var rangeActive = NetRange + 4f;
            var fadeRange = rangeActive - NetRange;
            bool sameTeam = localPlayer != null && localPlayer.PlayerTeamIndex == TurretTeamIndex;
            //Debug.LogError("Turret ShowArea range: " + rangeToLocalPlayer);
            bool active = IsAlive && (rangeActive > rangeToLocalPlayer);

            if (area.gameObject.activeSelf != active)
                area.gameObject.SetActive(active);
            area.localScale = new Vector3(NetRange * 2, 1, NetRange * 2);
            if (areaColor.Length >= 2)
                areaRenderer.GetComponent<SpriteRenderer>().color = sameTeam ? areaColor[0] : areaColor[1];
            if(active)
            {
                if (rangeToLocalPlayer > NetRange)
                {
                    float alpha = 1f - (rangeToLocalPlayer - NetRange) / fadeRange;
                    var c = areaRenderer.GetComponent<SpriteRenderer>().color;
                    c.a = alpha;
                    areaRenderer.GetComponent<SpriteRenderer>().color = c;
                }
            }
        }

        public override void Render()
        {
            OnValueChanged();
            ShowArea();
            if (hpBar.activeSelf != IsAlive)
                hpBar.SetActive(IsAlive);
        }

        public override void FixedUpdateNetwork()
        {
            if (!IsAlive) return;
            if (Object.HasStateAuthority)
            {
                if (!IsAlive) return;
                LookAtTarget();
                ScanTargetInRange();
                Fire();
            }
        }

        public void ChangeHP(int damage, int life, int maxLife)
        {
            if (damage > 0)
                damageVisual.ShowText(damage.ToString(), Color.red);

            DoRealHP(life, maxLife);
            lastHP = life;
        }

        private void DoVirtualHP(int life, int maxLife)
        {
            virtualHP.DOFillAmount((float)life / (float)maxLife, 0.5f);
        }

        private void DoRealHP(int life, int maxLife)
        {
            realHP.DOFillAmount((float)life / (float)maxLife, 0.5f).OnComplete(() => DoVirtualHP(life, maxLife));
        }

        private void OnHealthChanged()
        {
            Debug.Log($"Turret health changed: {NetHP}");
            ChangeHP(lastHP - NetHP, NetHP, NetMaxHP);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_DealDamage(DamageEvent @event)
        {
            TakeDamage(@event);
        }

        public void TakeDamage(DamageEvent @event)
        {
            Debug.Log($"Turret {Object.InputAuthority} took damage: {@event.damage}");
            if (!IsAlive) return;
            if (Object.HasStateAuthority)
            {
                NetHP = Mathf.Max(NetHP - @event.damage, 0);
                if (NetHP <= 0)
                {
                    IsAlive = false;
                    //check left alive turrets
                    if (Runner.TryGetSingleton(out GameServer game))
                    {
                        game.RPC_DestroyedOutpost(new GameServer.OutpostEvent { teamIndex = @event.teamIndex, playerId = @event.playerId, turretId = NetOutpostId.ToString() });
                    }
                    animator.SetTrigger("dead");
                    explosion?.gameObject?.SetActive(true);
                    // DOVirtual.DelayedCall(1f, () =>
                    // {
                    //     //Runner.Despawn(Object);
                    // }, ignoreTimeScale: false).SetTarget(this);
                }
            }
        }

        // for show on attacker side immediately
		public void UpdateHPImmediately(int damage)
		{
			ChangeHP(damage, NetHP, NetMaxHP);
		}

        private void ScanTargetInRange()
        {
            if (TargetDetectionTimer.ExpiredOrNotRunning(Runner))
            {
                TargetDetectionTimer = TickTimer.CreateFromSeconds(Runner, detectTime);
                if (_target != null)
                {
                    if (_target.IsDead || Vector3.Distance(transform.position, _target.visuals.Turret.position) > NetRange) // out of range
                    {
                        _target = null; //reset for find new target
                    }
                    else
                    {
                        return; //target is still valid
                    }
                }

                int cnt = Physics.OverlapSphereNonAlloc(transform.position, NetRange, _areaHits, LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore);
                if (cnt > 0)
                {
                    List<Player> enemies = new List<Player>();
                    for (int i = 0; i < cnt; i++)
                    {
                        if (_areaHits[i] != null && _areaHits[i].TryGetComponent(out Player p2))
                        {
                            if (p2.PlayerTeamIndex == TurretTeamIndex) continue;
                            if (p2.IsDead) continue;
                            if (CheckSignBlocked(p2.transform)) continue;
                            enemies.Add(p2);
                        }
                    }

                    // calculate the distance from turret to enemies and get nearest
                    if (enemies.Count > 0)
                    {
                        _target = enemies.OrderBy(e => e.visuals == null ? float.MaxValue : Vector3.Distance(transform.position, e.visuals.Turret.position)).First();
                        if(_target.visuals == null) 
                        {
                            _target = null;
                        }
                        else primaryFireDelay = TickTimer.CreateFromSeconds(Runner, weapon.Delay);
                    }
                }
                else _target = null;
            }
        }

        private void LookAtTarget()
        {
            if (_target != null)
            {
                Vector3 direction = _target.visuals.Turret.position - turretHead.position;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                Quaternion newRotation = Quaternion.Slerp(turretHead.rotation, lookRotation, Time.deltaTime * rotationSpeed);
                NetworkedX = newRotation.eulerAngles.x;
                NetworkedY = newRotation.eulerAngles.y;
            }
        }

        private void Fire()
        {
            if (_target == null || weapon == null)
                return;
            if (CheckSignBlocked(_target.transform))
            {
                _target = null;
                return;
            }

            if (primaryFireDelay.ExpiredOrNotRunning(Runner))
            {
                primaryFireDelay = TickTimer.CreateFromSeconds(Runner, weapon.Delay);
                if (_target.TryGetComponent(out Player p1))
                {
                    if (p1.IsDead || p1.PlayerTeamIndex == TurretTeamIndex)
                    {
                        _target = null;
                        return;
                    }
                }
                animator.SetTrigger("fire");
                weapon.Fire(Runner, PlayerRef.MasterClient, velocity);
            }
        }

        private bool CheckSignBlocked(Transform target)
        {
            if (target == null) return true;

            // Hướng từ turret đến target

            Vector3 start = turretHead.position + Vector3.up * 0.5f;
            Vector3 end = target.position;
            Vector3 dir = (end - start).normalized;
            float length = Vector3.Distance(start, end);

            int blockedLayer = LayerMask.GetMask("Wall");

            // Raycast kiểm tra bị chắn bởi tường
            if (Physics.Raycast(start, dir, out var hitinfo, length, blockedLayer, QueryTriggerInteraction.Ignore))
            {
                // Nếu hitinfo không phải target thì bị chắn
                if (hitinfo.transform != target)
                {
                    return true;
                }
            }

            // Không bị chắn
            return false;
        }
    }
}