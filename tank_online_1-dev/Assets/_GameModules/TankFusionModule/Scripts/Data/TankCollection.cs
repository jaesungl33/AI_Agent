using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TankCollection", menuName = "ScriptableObjects/TankCollection", order = 1)]
public class TankCollection : CollectionBase<TankDocument>
{
    public TankDocument GetActiveTank()
    {
        // get the selected tank from player document
        var selectedTank = DatabaseManager.GetDB<PlayerCollection>().GetMine().selectedTank;
        // Find the first tank that is selected
        return FindDocumentByProperty(tank => tank.tankId.Equals(selectedTank), true).DeepClone();
    }

    public TankDocument GetTankByID(TankType tankType)
    {
        if (documents != null)
        {
            foreach (var tank in documents)
            {
                if (tank.tankType == tankType)
                {
                    return tank.DeepClone();
                }
            }
        }
        Debug.LogWarning($"Tank of type {tankType} not found in the collection.");
        return default(TankDocument);
    }

    public TankDocument GetTankByID(string tankId)
    {
        if (documents != null)
        {
            foreach (var tank in documents)
            {
                if (tank.tankId == tankId)
                {
                    return tank.DeepClone();
                }
            }
        }
        Debug.LogWarning($"Tank with ID {tankId} not found in the collection.");
        return default(TankDocument);
    }
}
