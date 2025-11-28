using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Firebase.Firestore;

[Serializable]
[FirestoreData]
public class UserDocument : IUserData
{
    [FirestoreProperty("userID")]
    public string userID { get; set; } // Unique identifier for the user
    
    [FirestoreProperty("roleID")]
    public string roleID { get; set; } // Unique identifier for the account
    
    [FirestoreProperty("inactiveRoleIDs")]
    public List<string> inactiveRoleIDs { get; set; } = new List<string>();

    [JsonIgnore]
    public string ID => userID;
    
    // Default constructor required for Firebase
    public UserDocument()
    {
        inactiveRoleIDs = new List<string>();
    }
}