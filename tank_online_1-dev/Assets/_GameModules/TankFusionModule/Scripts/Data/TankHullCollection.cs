using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TankHullCollection", menuName = "ScriptableObjects/TankHullCollection", order = 1)]
public class TankHullCollection : CollectionBase<TankHullDocument>
{
    public TankHullDocument GetByType(TankType tankType)
    {
        // Find the first tank hull that matches the specified tank type
        TankHullDocument activeHull = FindDocumentByProperty(hull => hull.tankType == tankType, true);
        if (activeHull != null)
        {
            return activeHull;
        }
        else
        {
            Debug.LogWarning($"No active tank hull found for type {tankType} in the collection.");
            return null;
        }
    }

    public TankHullDocument GetDocByID(string hullId)
    {
        if (documents != null)
        {
            if (documents != null)
            {
                foreach (var hullDocument in documents)
                {       
                    if (hullDocument.hullID == hullId)
                    {
                        return hullDocument;
                    }
                }
            }
        }
        Debug.LogWarning($"Tank with ID {hullId} not found in the collection.");
        return default(TankHullDocument);
    }
}
