// Date: 28 June 2025
// AI-Generated: This code structure was created with AI assistance

using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.RemoteConfig;
using UnityEngine.Events;

public class FirestoreManager : Singleton<FirestoreManager>, IInitializableManager
{
    public FirebaseFirestore db;

    public UnityAction<bool> OnInitialized { get; set; }

    protected async override void Awake()
    {
        base.Awake();
    }
    
    public async void Initialize()
    {
        var dep = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dep == DependencyStatus.Available)
        {
            db = FirebaseFirestore.DefaultInstance;
            db.Settings.PersistenceEnabled = false;
            Debug.Log("Firestore Initialized");
            await FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync();
            OnInitialized?.Invoke(true);
        }
        else
        {
            Debug.LogError($"Firebase deps not available: {dep}");
            OnInitialized?.Invoke(false);
        }
    }
}
