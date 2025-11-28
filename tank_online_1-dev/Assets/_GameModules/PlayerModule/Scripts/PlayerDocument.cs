using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Firebase.Firestore;

[Serializable]
[FirestoreData]
public class PlayerDocument : IUserData
{
    [FirestoreProperty("roleID")]
    public string roleID { get; set; } // Unique identifier for the account

    [FirestoreProperty("elo")]
    [Min(500)]
    public int elo { get; set; } // all players start with 1000 elo

    [FirestoreProperty("playerName")]
    public string playerName { get; set; }

    [FirestoreProperty("playerAvatar")]
    public string playerAvatar { get; set; }

    [FirestoreProperty("gold")]
    public int gold { get; set; }

    [FirestoreProperty("diamond")]
    public int diamond { get; set; }

    [FirestoreProperty("exp")]
    public long exp { get; set; }

    [FirestoreProperty("level")]
    public int level { get; set; }

    [FirestoreProperty("tutorialCount")]
    public int tutorialCount { get; set; }
    
    [FirestoreProperty("tutorialCompleted")]
    public bool tutorialCompleted { get; set; }

    [FirestoreProperty("selectedModeIndex")]
    public int selectedModeIndex { get; set; } // Convert enum to int for Firebase compatibility

    [FirestoreProperty("selectedTank")]
    public string selectedTank { get; set; }

    [FirestoreProperty("tanks")]
    public List<string> tanks { get; set; } = new List<string>();

    [FirestoreProperty("ownedWraps")]
    public List<string> ownedWraps { get; set; } = new List<string>();

    [FirestoreProperty("ownedDecals")]
    public List<string> ownedDecals { get; set; } = new List<string>();

    [FirestoreProperty("ownedStickers")]
    public List<string> ownedStickers { get; set; } = new List<string>();

    [FirestoreProperty("ownedAIStickers")]
    public List<string> ownedAIStickers { get; set; } = new List<string>();

    [FirestoreProperty("formationTanks")]
    public List<string> formationTanks { get; set; } = new List<string>();

    [JsonIgnore]
    public string ID => roleID;

    // Helper property for enum conversion
    [JsonIgnore]
    public MatchMode SelectedMode
    {
        get => (MatchMode)selectedModeIndex;
        set => selectedModeIndex = (int)value;
    }

    // Default constructor required for Firebase
    public PlayerDocument()
    {
        tanks = new List<string>();
    }


    #region Helper Methods
    public List<FormationTanks> GetFormationTanks()
    {
        var result = new List<FormationTanks>();
        foreach (var json in formationTanks)
        {
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<FormationTanks>(json);
                    if (obj != null)
                        result.Add(obj);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Invalid FormationTanks JSON: {json}\n{ex}");
                }
            }
        }
        return result;
    }
    public FormationTanks GetFormationTankByTankId(string tankId)
    {
        foreach (var json in formationTanks)
        {
            Debug.Log($"[GetFormationTankByTankId] Checking JSON: {json}");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<FormationTanks>(json);
                    Debug.Log($"[GetFormationTankByTankId] Deserialized tankID: {obj?.tankID}");
                    if (obj != null && obj.tankID == tankId)
                    {
                        Debug.Log($"[GetFormationTankByTankId] Found tankID: {tankId}");
                        return obj;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Invalid FormationTanks JSON: {json}\n{ex}");
                }
            }
        }
        Debug.LogWarning($"[GetFormationTankByTankId] Not found tankID: {tankId}");
        return null;
    }
    public int GetWrapId(string tankId)
    {
        var formationTank = GetFormationTankByTankId(tankId);
        if (formationTank != null)
        {
            Debug.Log($"[GetWrapId] tankId: {tankId}, wrapId: {formationTank.wrapId}");
            return formationTank.wrapId;
        }
        Debug.LogWarning($"[GetWrapId] Not found tankId: {tankId}");
        return 0;
    }
    public class FormationTanks
    {
        public string tankID;
        public int wrapId;
        public FormationTanks(string tankID, int wrapId)
        {
            this.tankID = tankID;
            this.wrapId = wrapId;
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
        public string SkinIdsToJson()
        {
            return JsonConvert.SerializeObject(this.wrapId);
        }
    }

    public static int GetMineWrapId(string tankId)
    {
        var playerCollection = DatabaseManager.GetDB<PlayerCollection>();
        var playerDocument = playerCollection?.GetMine();
        if (playerDocument == null)
        {
            Debug.LogWarning("[GetMineWrapId] playerDocument is null");
            return 0;
        }

        var formationTank = playerDocument.GetFormationTankByTankId(tankId);
        if (formationTank != null)
        {
            Debug.Log($"[GetMineWrapId] tankId: {tankId}, wrapId: {formationTank.wrapId}");
            return formationTank.wrapId;
        }
        Debug.LogWarning($"[GetMineWrapId] Not found tankId: {tankId}");
        return 0;
    }
    #endregion
}