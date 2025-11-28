using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TankAbilityCollection", menuName = "ScriptableObjects/TankAbilityCollection", order = 1)]
public class TankAbilityCollection : CollectionBase<TankAbilityDocument>
{
    public TankAbilityDocument GetByAbilityId(string abilityID)
    {
        foreach (var ability in documents)
        {
            if (ability.abilityID == abilityID)
            {
                return ability;
            }
        }
        return null;
    }
    public TankAbilityDocument GetTankAbilityDocumentById(string abilityID)
    {
        if (string.IsNullOrEmpty(abilityID))
        {
            Debug.LogWarning("Ability ID is null or empty.");
            return null;
        }
        return documents.Find(doc => doc.abilityID == abilityID);
    }
}
