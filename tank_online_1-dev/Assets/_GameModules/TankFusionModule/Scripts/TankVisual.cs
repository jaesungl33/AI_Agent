using UnityEngine;
using FusionHelpers;
using TMPro;
using System;

namespace Fusion.TankOnlineModule
{
    public class TankVisual : MonoBehaviour
    {
        public string tankId;
        #region Visuals
        [SerializeField] private Transform _hull;
        [SerializeField] private Transform _turret;
        [SerializeField] private Transform _visualParent;
        [SerializeField] private Transform _visualSpawnEffect;
        [SerializeField] private TankTeleportInEffect _teleportIn;
        [SerializeField] private TankTeleportOutEffect _teleportOutPrefab;
        [SerializeField] private GameObject _deathExplosionPrefab;
        [SerializeField] private WeaponManager weaponManager;
        [SerializeField] private Collider[] _collider;
        [SerializeField] private GameObject _deathExplosionInstance;
        [SerializeField] private TankDamageVisual _damageVisuals;
        [SerializeField] private TankPartVisual[] _tankPartVisuals;
        [SerializeField] private Transform[] _drivingDustParents;
        [SerializeField] private Animation[] tankAnims;

        public Transform[] DrivingDustParents { get => _drivingDustParents; private set => _drivingDustParents = value; }
        public Transform Hull { get => _hull; private set => _hull = value; }
        public Transform Turret { get => _turret; private set => _turret = value; }
        public Transform VisualParent { get => _visualParent; private set => _visualParent = value; }
        public TankTeleportInEffect TeleportIn { get => _teleportIn; private set => _teleportIn = value; }
        public TankTeleportOutEffect TeleportOutPrefab { get => _teleportOutPrefab; private set => _teleportOutPrefab = value; }
        public GameObject DeathExplosionPrefab { get => _deathExplosionPrefab; private set => _deathExplosionPrefab = value; }
        public WeaponManager WeaponManager { get => weaponManager; private set => weaponManager = value; }
        public Collider[] Collider { get => _collider; private set => _collider = value; }
        public GameObject DeathExplosionInstance { get => _deathExplosionInstance; set => _deathExplosionInstance = value; }
        public TankDamageVisual DamageVisuals { get => _damageVisuals; private set => _damageVisuals = value; }
        public TankPartVisual[] TankPartVisuals { get => _tankPartVisuals; private set => _tankPartVisuals = value; }
        #endregion

        private void Awake()
        {
            _collider = transform.root.GetComponentsInChildren<Collider>();
            _tankPartVisuals = GetComponentsInChildren<TankPartVisual>();
            tankAnims = GetComponentsInChildren<Animation>();
        }

        public void Initialize(Player player)
        {
            // Initialize the teleport in effect with the player
            TeleportIn.Initialize(player);

            // Initialize the damage visuals with the player's material
            DamageVisuals.Initialize(this, player);

            // Initialize the weapon manager with the player
            WeaponManager.Initialize(player);

            SetUpDeathExplosion(player);

            //WrapDecalStickers Fixme
            Debug.Log($"[TankVisual.Initialize] PlayerData.WrapId: {(player.PlayerData.WrapId == null ? "null" : player.PlayerData.WrapId.ToString())}");
            LoadWrap(player.PlayerData.WrapId, player.PlayerTankId);
        }

        internal void SetUpDeathExplosion(Player player)
        {
            if (DeathExplosionPrefab != null)
            {
                DeathExplosionInstance = Instantiate(DeathExplosionPrefab, transform.position, transform.rotation, player.transform);
                DeathExplosionInstance.SetActive(false);
                ColorChanger.ChangeColor(DeathExplosionInstance.transform, player.PlayerColor);
            }
        }

        internal void Dead(Player player, bool showExplosion = true)
        {
            if (showExplosion)
            {
                if (DeathExplosionInstance == null) SetUpDeathExplosion(player);
                DeathExplosionInstance.transform.position = transform.position;
                DeathExplosionInstance.SetActive(false); // dirty fix to reactivate the death explosion if the particlesystem is still active
                DeathExplosionInstance.SetActive(true);
            }

            VisualParent.gameObject.SetActive(false);
            DamageVisuals.OnDeath();
        }
        public void LoadWrap(int wrapId, string tankId)
        {
            Debug.Log($"[TankVisual.LoadWrap] wrapId: {wrapId}, tankId: {tankId}");
            if (TankPartVisuals == null || TankPartVisuals.Length == 0)
            {
                Debug.LogWarning("[TankVisual.LoadWrap] TankPartVisuals is null or empty");
                return;
            }
            foreach (var part in TankPartVisuals)
            {
                Debug.Log($"[TankVisual.LoadWrap] Call LoadWrap on part: {part.name}, tankId: {tankId}, wrapId: {wrapId}");
                part.LoadWrap(wrapId, tankId);
            }
        }
        public void EnableVisuals(bool status)
        {
            foreach (var part in TankPartVisuals)
            {
                part.EnableVisual(status);
            }
        }

        internal void EnableCollider(bool enable)
        {
            foreach (var col in Collider)
            {
                col.enabled = enable;
            }
        }

        internal void Active()
        {
            DamageVisuals.CleanUpDebris();
            TeleportIn.EndTeleport();
            VisualParent.gameObject.SetActive(true);
        }

        internal bool IsTurretAvailable()
        {
            return Turret != null;
        }

        internal void RenderVisuals(Player player)
        {
            var interpolated = new NetworkBehaviourBufferInterpolator(player);
            Turret.rotation = Quaternion.Euler(0, interpolated.Angle(nameof(player.AimDirection)), 0);

            float targetY = interpolated.Angle(nameof(player.HullDirection));
            Quaternion targetRot = Quaternion.Euler(0, targetY, 0);
            Hull.rotation = Quaternion.RotateTowards(
                Hull.rotation,
                targetRot,
                360f * Time.deltaTime // hoặc tốc độ xoay mong muốn, ví dụ: 180f * Time.deltaTime
            );
            DamageVisuals.CheckHPToShowVFX(player.PlayerHP, player.PlayerMaxHP);
        }

        internal void ShowVFXDamage(Player player, int damage)
        {
            DamageVisuals.CheckHPToShowVFX(player.PlayerHP, player.PlayerMaxHP);
        }

        public void DrivingFx(bool isDriving)
        {
            //DrivingDustParents
            foreach (var dust in DrivingDustParents)
            {
                if (dust != null && dust.gameObject.activeSelf != isDriving)
                    dust.gameObject.SetActive(isDriving);
            }
        }

        public void SetAnimation(byte anim)
        {
            if (tankAnims == null || tankAnims.Length <= 0) return;
            foreach (var a in tankAnims)
            {
                if (a != null)
                {
                    if (a.clip != null && anim > 0)
                    {
                        a.wrapMode = WrapMode.Loop;
                        a.Play();
                    }
                    else
                    {
                        a.Stop();
                    }
                }
            }
        }
    }
}
