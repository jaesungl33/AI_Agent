using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DatabaseManager : Singleton<DatabaseManager>, IInitializableManager
{
    private static GameDatabase gameDatabase;
    private Dictionary<System.Type, object> _runtimeCollections = new();
    public UnityAction<bool> OnInitialized { get; set; }

    public static UnityAction<object> OnCollectionUpdated;
    private static GameDatabase DB
    {
        get
        {
            if (gameDatabase == null)
            {
                gameDatabase = CollectionExtensions.GetCollection<GameDatabase>();
                if (gameDatabase == null)
                {
                    Debug.LogError("GameDatabase not found in Resources folder.");
                }
            }
            return gameDatabase;
        }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        OnInitialized?.Invoke(true);
    }

    public static void GetDB<T>(UnityAction<T> callback) where T : ScriptableObject
    {
        if (DB == null)
        {
            Debug.LogError("GameDatabase is not initialized.");
            callback?.Invoke(null);
            return;
        }

        T db = DB.GetCollection<T>();
        if (db == null)
        {
            Debug.LogError($"Document of type {typeof(T)} not found in GameDatabase.");
        }

        callback?.Invoke(db);
    }
    
    public static void UpdateRuntimeData<TDocument, TCollection>(string json) 
    where TCollection : CollectionBase<TDocument>
    where TDocument : class
    {
        var type = typeof(TDocument);
        if (!Instance._runtimeCollections.TryGetValue(type, out var collection))
        {
            // Get original collection
            var originalCollection = DB.GetCollection<TCollection>();
            if (originalCollection == null)
            {
                Debug.LogError($"Original collection not found for type {typeof(TCollection)}");
                return;
            }
 
            // Create runtime collection
            collection = new RuntimeCollection<TDocument>(originalCollection);
            Instance._runtimeCollections[type] = collection;
        }

        // Update runtime data
        ((RuntimeCollection<TDocument>)collection).UpdateFromJson(json);

        //Debug.LogError($"Runtime collection for {typeof(TDocument)} updated.");
        OnCollectionUpdated?.Invoke(collection);
    }
    
    // Remove the conflicting GetRuntimeDocument<T> method and keep only the generic version
    public static TDocument GetRuntimeDocument<TDocument, TCollection>(string id)
        where TCollection : CollectionBase<TDocument>
        where TDocument : class
    {
        var type = typeof(TDocument);
        if (Instance._runtimeCollections.TryGetValue(type, out var collection))
        {
            return ((RuntimeCollection<TDocument>)collection).GetDocument(id);
        }

        Debug.LogWarning($"No runtime collection found for {type}, falling back to original data");
        var originalCollection = DB.GetCollection<TCollection>();
        return originalCollection?.FindDocumentByProperty(d => GetDocumentId(d) == id, true);
    }

    // Add convenience method for getting runtime documents with type inference
    public static TDocument GetRuntimeDocumentFromCollection<TDocument>(string id, CollectionBase<TDocument> collection) 
        where TDocument : class
    {
        var type = typeof(TDocument);
        if (Instance._runtimeCollections.TryGetValue(type, out var runtimeCollection))
        {
            return ((RuntimeCollection<TDocument>)runtimeCollection).GetDocument(id);
        }

        Debug.LogWarning($"No runtime collection found for {type}, falling back to original data");
        return collection?.FindDocumentByProperty(d => GetDocumentId(d) == id, true);
    }

    // Helper method to get document ID
    private static string GetDocumentId<T>(T document) where T : class
    {
        var idField = typeof(T).GetField("tankId") ??
                        typeof(T).GetField("hullID") ??
                        typeof(T).GetField("weaponID") ??
                        typeof(T).GetField("abilityID") ??
                        typeof(T).GetField("projectileID") ??
                        typeof(T).GetField("matchID") ??
                        typeof(T).GetField("$id");
        
        return idField?.GetValue(document)?.ToString();
    }

    public static T GetRuntimeDocument<T>(string id) where T : class
    {
        var type = typeof(T);
        if (Instance._runtimeCollections.TryGetValue(type, out var collection))
        {
            return ((RuntimeCollection<T>)collection).GetDocument(id);
        }

        Debug.LogWarning($"No runtime collection found for {type}, falling back to original data");
        return DB.GetCollection<CollectionBase<T>>()?.FindDocumentByProperty(d => GetDocumentId(d) == id, true);
    }

    public static T GetDB<T>() where T : ScriptableObject
    {
        T db = DB.GetCollection<T>();
        return db;
    }

    private static MatchPlayerData GetMatchPlayerData(MatchmakingDocument matchDoc, TankDocument tankDoc)
    {
        MatchPlayerData matchPlayerData = new MatchPlayerData { };
        TankHullDocument hullDoc = DB.tankHullDatabase.GetDocByID(tankDoc.hullID);
        TankWeaponDocument weaponDoc = DB.tankWeaponDatabase.GetDocByID(tankDoc.primaryWeaponID);

        // Populate matchPlayerData with the necessary information
        matchPlayerData.Gold = matchDoc.GoldStarting;
        matchPlayerData.TankId = tankDoc.tankId;
        matchPlayerData.HP = (int)(hullDoc.hpMultiplier * TankStatBaseData.hp / 100f);
        matchPlayerData.MaxHitpoints = (int)(hullDoc.hpMultiplier * TankStatBaseData.hp / 100f);
        matchPlayerData.MaxSpeed = (int)(hullDoc.speedMultiplier * TankStatBaseData.maxSpeed / 100f);
        matchPlayerData.Damage = new int[2];
        matchPlayerData.Damage[0] = (int)(weaponDoc.damageMultiplier * TankStatBaseData.minDMG / 100f);
        matchPlayerData.Damage[1] = (int)(weaponDoc.damageMultiplier * TankStatBaseData.maxDMG / 100f);
        matchPlayerData.FireRate = weaponDoc.fireRateMultiplier * TankStatBaseData.fireRate / 100f;
        matchPlayerData.ReloadTime = (int)(weaponDoc.reloadTimeMultiplier * TankStatBaseData.reloadTime / 100f);
        matchPlayerData.Braking = hullDoc.brakingMultiplier * TankStatBaseData.braking / 100f;
        matchPlayerData.Acceleration = hullDoc.accelerationMultiplier * TankStatBaseData.acceleration / 100f;
        matchPlayerData.RotationSpeed = hullDoc.rotationSpeedMultiplier * TankStatBaseData.rotationSpeed / 100f;
        matchPlayerData.ProjectileSpeed = (int)(weaponDoc.projectileSpeedMultiplier * TankStatBaseData.projectileSpeed / 100f);
        matchPlayerData.Range = (int)(weaponDoc.rangeMultiplier * TankStatBaseData.range / 100f);
        matchPlayerData.ProjectileCount = (byte)(weaponDoc.projectileCountMultiplier * TankStatBaseData.projectileCount / 100f);
        matchPlayerData.MagazineSize = (int)(weaponDoc.magazineSizeMultiplier * TankStatBaseData.magazineSize / 100f);
        matchPlayerData.RespawnInSeconds = matchDoc.RespawnInSeconds;
        matchPlayerData.AbilityIDs = tankDoc.abilityIDs;
        return matchPlayerData;
    }

    public static MatchPlayerData CreateMatchPlayer(string tankId)
    {
        TankDocument tankDoc = DB.tankDatabase.GetTankByID(tankId);
        MatchmakingDocument matchDoc = DB.matchDatabase.GetActiveDocument();
        return GetMatchPlayerData(matchDoc, tankDoc);
    }
}