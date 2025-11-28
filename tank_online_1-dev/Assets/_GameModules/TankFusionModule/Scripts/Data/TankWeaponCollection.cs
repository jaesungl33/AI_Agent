using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(TankWeaponCollection), menuName = "ScriptableObjects/" + nameof(TankWeaponCollection), order = 1)]
public class TankWeaponCollection : CollectionBase<TankWeaponDocument>
{
    public TankWeaponDocument GetDocByType(TankType tankType)
    {
        // Find the first tank weapon that matches the specified tank type
        TankWeaponDocument activeWeapon = FindDocumentByProperty(weapon => weapon.tankType == tankType, true);
        if (activeWeapon != null)
        {
            return activeWeapon;
        }
        else
        {
            Debug.LogWarning($"No active tank weapon found for type {tankType} in the collection.");
            return null;
        }
    }

    public TankWeaponDocument GetDocByID(string weaponId)
    {
        if (documents != null)
        {
            if (documents != null)
            {
                foreach (var weaponDocument in documents)
                {
                    if (weaponDocument.weaponID == weaponId)
                    {
                        return weaponDocument;
                    }
                }
            }
        }
        Debug.LogWarning($"Tank with ID {weaponId} not found in the collection.");
        return default(TankWeaponDocument);
    }
}