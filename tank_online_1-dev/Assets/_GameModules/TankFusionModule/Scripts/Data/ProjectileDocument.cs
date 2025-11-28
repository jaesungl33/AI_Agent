using Fusion.TankOnlineModule;
using UnityEngine;

[System.Serializable]
public class ProjectileDocument
{
    public string projectileName; // Name of the projectile
    public int projectileID; // Unique identifier for the projectile
    public ProjectileType projectileType; // Type of the projectile (e.g., standard, explosive, laser)
    public int gravity; // Gravity affecting the projectile
    [SerializeField] float speed;
    [SerializeField] float radius;
    [SerializeField] float range;
    [SerializeField] LayerMask hitMask; // Layer mask for detecting hits
    [SerializeField] float areaRadius; // Radius for area of effect damage
    [SerializeField] float areaImpulse; // Impulse applied to objects in the area of effect
    [SerializeField] int[] areaDamage; // Damage values for area of effect, can be a range
    [SerializeField] float timeToLive; // Time before the projectile is destroyed
    [SerializeField] bool isHitScan;
    [SerializeField] private ExplosionFX _detonationPrefab;
    [SerializeField] private TargetMarker _targetPrefab;
    [SerializeField] private MuzzleFlash _muzzleFxPrefab;


    public Vector3 Gravity => new Vector3(0, gravity, 0); // Gravity vector for the projectile
    public float Speed => speed;
    public float Radius => radius;
    public float Range => range;
    public LayerMask HitMask => hitMask;
    public float AOERadius => areaRadius;
    public float AreaImpulse => areaImpulse;
    public int[] AreaDamage => areaDamage;
    public float TimeToLive => timeToLive;
    public bool IsHitScan => isHitScan;
    public ProjectileType ProjectileType => projectileType;
}
public enum ProjectileType
{
    None = -1,
    Single, // Standard projectile with basic properties
    AOE
}
