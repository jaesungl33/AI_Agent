using UnityEngine;

/// <summary>
/// Help to find the place for set model and other designer related tasks.
/// </summary>
public class DesignerHelper : MonoBehaviour
{
    [Header("To set hull model")]
    [SerializeField] private Transform hullRoot;

    [Header("To set chain model")]
    [SerializeField] private Transform chainRoot;

    [Header("To set turret model")]
    [SerializeField] private Transform turretRoot;

    [Header("To set weapon model, drag weapon prefab to root")]
    [SerializeField] private Transform primaryWeaponRoot;
    [SerializeField] private Transform secondaryWeaponRoot;
}
