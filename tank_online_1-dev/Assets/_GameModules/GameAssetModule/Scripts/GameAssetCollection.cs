using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(GameAssetCollection), menuName = "ScriptableObjects/" + nameof(GameAssetCollection), order = 1)]
public class GameAssetCollection : CollectionBase<GameAssetDocument>
{
    public GameAssetDocument GetGameAssetDocumentById(string assetID)
    {
        if (string.IsNullOrEmpty(assetID))
        {
            Debug.LogWarning("Asset ID is null or empty.");
            return null;
        }
        return documents.Find(doc => doc.id == assetID);
        // foreach (var assetDoc in documents)
        // {
        //     if (assetDoc.id == assetID)
        //     {
        //         return assetDoc;
        //     }
        // }

        // Debug.LogWarning($"Game asset document with ID {assetID} not found in collection.");
        // return null;
    }

    public Sprite GetClassIcon(string assetID, AssetType assetType = AssetType.ClassIcon)
    {
        var assetDoc = GetGameAssetDocumentById(assetID);
        if (assetDoc != null)
        {
            return assetDoc.sprites.FirstOrDefault(s => s.type == assetType)?.asset as Sprite;
        }
        Debug.LogWarning($"Sprite for asset ID {assetID} not found.");
        return null;
    }

    public Sprite GetMainIcon(string assetID)
    {
        var assetDoc = GetGameAssetDocumentById(assetID);
        if (assetDoc != null)
        {
            return assetDoc.sprites.FirstOrDefault(s => s.type == AssetType.MainIcon)?.asset as Sprite;
        }
        Debug.LogWarning($"Main icon for asset ID {assetID} not found.");
        return null;
    }

    public void GetAssets(string assetID, out Sprite mainIcon, out Sprite previewIcon, out Sprite classIcon)
    {
        mainIcon = null;
        previewIcon = null;
        classIcon = null;
        var assetDoc = GetGameAssetDocumentById(assetID);
        if (assetDoc != null)
        {
            mainIcon = assetDoc.sprites.FirstOrDefault(s => s.type == AssetType.MainIcon)?.asset as Sprite;
            classIcon = assetDoc.sprites.FirstOrDefault(s => s.type == AssetType.ClassIcon)?.asset as Sprite;
            previewIcon = assetDoc.sprites.FirstOrDefault(s => s.type == AssetType.PreviewIcon)?.asset as Sprite;
            return;
        }
        Debug.LogWarning($"Main icon for asset ID {assetID} not found.");
    }

    public Sprite GetPreviewIcon(string assetID)
    {
        var assetDoc = GetGameAssetDocumentById(assetID);
        if (assetDoc != null)
        {
            return assetDoc.sprites.FirstOrDefault(s => s.type == AssetType.PreviewIcon)?.asset as Sprite;
        }
        Debug.LogWarning($"Preview icon for asset ID {assetID} not found.");
        return null;
    }
}
