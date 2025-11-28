using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(GameAssetCollection2), menuName = "ScriptableObjects/" + nameof(GameAssetCollection2), order = 1)]
public class GameAssetCollection2 : CollectionBase<GameAssetGroup>
{
    public GameAssetDocument GetGameAssetDocumentById(string groupID, string assetID)
    {
        if (string.IsNullOrEmpty(assetID))
        {
            Debug.LogWarning("Asset ID is null or empty.");
            return null;
        }
        var group = documents.FirstOrDefault(g => g.groupID == groupID);
        if (group != null)
        {
            return group.assetDocuments.Find(doc => doc.id == assetID);
        }
        Debug.LogWarning($"Asset with ID {assetID} not found in group {groupID}.");
        return null;
    }

    // public Sprite GetClassIcon(string assetID, AssetType assetType = AssetType.ClassIcon)
    // {
    //     var assetDoc = GetGameAssetDocumentById(assetID);
    //     if (assetDoc != null)
    //     {
    //         return assetDoc.sprites.FirstOrDefault(s => s.type == assetType)?.asset as Sprite;
    //     }
    //     Debug.LogWarning($"Sprite for asset ID {assetID} not found.");
    //     return null;
    // }

    // public Sprite GetMainIcon(string assetID)
    // {
    //     var assetDoc = GetGameAssetDocumentById(assetID);
    //     if (assetDoc != null)
    //     {
    //         return assetDoc.sprites.FirstOrDefault(s => s.type == AssetType.MainIcon)?.asset as Sprite;
    //     }
    //     Debug.LogWarning($"Main icon for asset ID {assetID} not found.");
    //     return null;
    // }

    // public void GetAssets(string assetID, out Sprite mainIcon, out Sprite previewIcon, out Sprite classIcon)
    // {
    //     mainIcon = null;
    //     previewIcon = null;
    //     classIcon = null;
    //     var assetDoc = GetGameAssetDocumentById(assetID);
    //     if (assetDoc != null)
    //     {
    //         mainIcon = assetDoc.sprites.FirstOrDefault(s => s.type == AssetType.MainIcon)?.asset as Sprite;
    //         classIcon = assetDoc.sprites.FirstOrDefault(s => s.type == AssetType.ClassIcon)?.asset as Sprite;
    //         previewIcon = assetDoc.sprites.FirstOrDefault(s => s.type == AssetType.PreviewIcon)?.asset as Sprite;
    //         return;
    //     }
    //     Debug.LogWarning($"Main icon for asset ID {assetID} not found.");
    // }

    // public Sprite GetPreviewIcon(string assetID)
    // {
    //     var assetDoc = GetGameAssetDocumentById(assetID);
    //     if (assetDoc != null)
    //     {
    //         return assetDoc.sprites.FirstOrDefault(s => s.type == AssetType.PreviewIcon)?.asset as Sprite;
    //     }
    //     Debug.LogWarning($"Preview icon for asset ID {assetID} not found.");
    //     return null;
    // }
}
