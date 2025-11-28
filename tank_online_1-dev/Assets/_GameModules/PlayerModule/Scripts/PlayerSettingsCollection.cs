using Fusion.GameSystems;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettingsCollection", menuName = "ScriptableObjects/PlayerSettingsCollection", order = 1)]
public class PlayerSettingsCollection: UserDataCollectionBase<PlayerSettingsDocument>, IStorage
{
    public PlayerSettingsDocument GetSettingsDocument(string id)
    {
        return FindDocumentByProperty(doc => doc.ID, id);
    }

    public PlayerSettingsDocument GetMine()
    {
        var roleID = PlayerPrefs.GetString(Constants.Key_RoleID);
        return GetSettingsDocument(roleID);
    }
}