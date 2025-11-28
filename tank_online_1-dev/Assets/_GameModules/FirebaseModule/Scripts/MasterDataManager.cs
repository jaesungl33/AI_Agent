// Date: 28 June 2025
// AI-Generated: This code structure was created with AI assistance
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Firestore;
using System.Collections.Generic;

public class MasterDataManager : MonoBehaviour
{
    FirebaseFirestore db => FirestoreManager.Instance.db;

    void Start()
    {
        CheckAndLoadMasterData();
    }

    async void CheckAndLoadMasterData()
    {
        DocumentReference docRef = db.Collection("masterData").Document("gameData");
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            int serverVersionCode = snapshot.GetValue<int>("versionCode");
            int localVersionCode = PlayerPrefs.GetInt("masterDataVersionCode", 0);

            Debug.Log($"Server version: {serverVersionCode}, Local version: {localVersionCode}");

            if (serverVersionCode > localVersionCode)
            {
                Debug.Log("Master data outdated. Downloading new data...");
                DownloadAndCacheMasterData(snapshot);
                PlayerPrefs.SetInt("masterDataVersionCode", serverVersionCode);

                long lastUpdated = snapshot.GetValue<long>("lastUpdated");
                PlayerPrefs.SetString("masterDataLastUpdated", lastUpdated.ToString());
            }
            else
            {
                Debug.Log("Master data is up to date. Loading from cache.");
                LoadMasterDataFromCache();
            }
        }
        else
        {
            Debug.LogError("Master data document does not exist!");
        }
    }

    void DownloadAndCacheMasterData(DocumentSnapshot snapshot)
    {
        // Example: parse tank data from Firestore document
        Dictionary<string, object> tanksDict = snapshot.GetValue<Dictionary<string, object>>("tanks");

        // Convert to JSON string and save to PlayerPrefs as cache (or save to file in production)
        string json = JsonUtility.ToJson(new Serialization<string, object>(tanksDict));
        PlayerPrefs.SetString("masterDataCache", json);
        PlayerPrefs.Save();

        Debug.Log("Downloaded and cached new master data.");
    }

    void LoadMasterDataFromCache()
    {
        string json = PlayerPrefs.GetString("masterDataCache", "{}");
        if (json != "{}")
        {
            // Example deserialize
            var tanks = JsonUtility.FromJson<Serialization<string, object>>(json);
            Debug.Log($"Loaded {tanks.dictionary.Count} tanks from cache.");
        }
        else
        {
            Debug.LogWarning("No master data cache found.");
        }
    }
}
