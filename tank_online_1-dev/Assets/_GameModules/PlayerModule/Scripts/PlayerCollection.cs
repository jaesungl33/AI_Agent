using Fusion.GameSystems;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCollection", menuName = "ScriptableObjects/PlayerCollection", order = 1)]
public class PlayerCollection : UserDataCollectionBase<PlayerDocument>, IStorage
{
    public PlayerDocument GetPlayerDocument(string id)
    {
        return FindDocumentByProperty(doc => doc.ID, id);
    }

    public PlayerDocument GetMine()
    {
        var roleID = PlayerPrefs.GetString(Constants.Key_RoleID);
        Debug.Assert(!string.IsNullOrEmpty(roleID), "RoleID is not set in PlayerPrefs.");
        return GetPlayerDocument(roleID);
    }

    public void DeleteAccountFromFirebase()
    {
        PlayerDocument mine = GetMine();
        if (mine != null)
        {
            _ = DeleteDocument(mine.roleID);
            Debug.Log("Player document deleted from Firebase.");
        }
        else
        {
            Debug.LogWarning("No player document found to delete.");
        }
    }
    
    public void UpdatePlayerElo(int newElo)
    {
        Debug.Log($"Updating player ELO by {newElo} points.");
        PlayerDocument mine = GetMine();
        if (mine != null)
        {
            mine.elo += newElo;
            _ = UpdateDocumentAsync(mine);
            Debug.Log($"Player ELO updated to {mine.elo}.");
        }
        else
        {
            Debug.LogWarning("No player document found to update ELO.");
        }
    }
}