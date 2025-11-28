using UnityEngine;

[CreateAssetMenu(fileName = "PackRewardCollection", menuName = "ScriptableObjects/PackRewardCollection", order = 1)]
public class PackRewardCollection : CollectionBase<PackRewardDocument>
{
    public PackRewardDocument GetPackRewardByID(string id)
    {
        if (documents != null)
        {
            foreach (var packReward in documents)
            {
                if (packReward.id == id)
                {
                    return packReward;
                }
            }
        }
        Debug.LogWarning($"PackReward with ID {id} not found in the collection.");
        return null;
    }
}