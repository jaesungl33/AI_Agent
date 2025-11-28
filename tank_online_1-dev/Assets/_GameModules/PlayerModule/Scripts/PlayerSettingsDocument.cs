using System;
using Newtonsoft.Json;
using Firebase.Firestore;

[Serializable]
[FirestoreData]
public class PlayerSettingsDocument : IUserData
{
    [FirestoreProperty("roleID")]
    public string roleID { get; set; } // Unique identifier for the account
    
    [FirestoreProperty("graphicSetting")]
    public int graphicSetting { get; set; } = 0;
    
    [FirestoreProperty("enableSfx")]
    public bool enableSfx { get; set; } = true;
    
    [FirestoreProperty("enableBgm")]
    public bool enableBgm { get; set; } = true;
    
    [FirestoreProperty("enableVibrate")]
    public bool enableVibrate { get; set; } = true;
    
    [FirestoreProperty("enableNotification")]
    public bool enableNotification { get; set; } = true;
    
    [JsonIgnore]
    public string ID => roleID;
    
    // Default constructor required for Firebase
    public PlayerSettingsDocument()
    {
        graphicSetting = 0;
        enableSfx = true;
        enableBgm = true;
        enableVibrate = true;
        enableNotification = true;
    }
}