using Fusion.GameSystems;
using UnityEngine;

[CreateAssetMenu(fileName = "UserCollection", menuName = "ScriptableObjects/UserCollection", order = 1)]
public class UserCollection : UserDataCollectionBase<UserDocument>, IStorage
{
    public UserDocument GetUserDocument(string userId)
    {
        return FindDocumentByProperty(doc => doc.ID, userId);
    }

    public UserDocument GetMine()
    {
        var userId = PlayerPrefs.GetString(Constants.Key_UserID);
        return GetUserDocument(userId);
    }
}