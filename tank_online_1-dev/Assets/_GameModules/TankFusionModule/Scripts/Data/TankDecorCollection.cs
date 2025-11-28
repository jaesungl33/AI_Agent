using System.Collections.Generic;
using UnityEngine;

public class TankDecorCollection<TDocument> : CollectionBase<TDocument>
    where TDocument : TankDecorDocument
{
    public TankDecorDocument GetByCatalogId(string itemCatalogId)
    {
        foreach (var item in documents)
        {
            if (item.itemCatalogId == itemCatalogId)
            {
                return item;
            }
        }
        return null;
    }
    public TankDecorDocument GetTankCustomLiveryDocumentById(int itemId)
    {
        if (itemId < 0)
        {
            Debug.LogWarning("LiveryID ID is invalid.");
            return null;
        }
        return documents.Find(doc => doc.itemId == itemId);
    }

    public List<TDocument> GetAllWraps()
    {
        return documents.FindAll(doc => doc.itemType == TankDecorDocument.ItemType.Wrap);
    }
}