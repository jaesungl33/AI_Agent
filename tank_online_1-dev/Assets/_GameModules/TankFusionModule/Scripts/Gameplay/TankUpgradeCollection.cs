using UnityEngine;

[CreateAssetMenu(menuName = "Game/SO TankUpgradeData")]
public class TankUpgradeCollection : CollectionBase<SOUpgradeDefinition>
{
    public SOUpgradeDefinition GetDefinition(string tankId)
    {
        foreach (var definition in GetAllDocuments())
        {
            if (definition.tankId == tankId)
            {
                return definition;
            }
        }
        Debug.LogWarning($"Upgrade definition for tank ID {tankId} not found.");
        return null;
    }
}