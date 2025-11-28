using System;
using UnityEngine;
using GDO.Audio;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameDatabase", menuName = "ScriptableObjects/GameDatabase", order = 1)]
public class GameDatabase : ScriptableObject
{
    public TankCollection tankDatabase;
    public MapCollection mapDatabase;
    public TankWeaponCollection tankWeaponDatabase;
    public TankHullCollection tankHullDatabase;
    public TankAbilityCollection tankAbilityDatabase;
    public SoundCollection soundDatabase;
    public ProductCollection productDatabase;
    public MatchmakingCollection matchDatabase;
    public PackRewardCollection packRewardCollection;
    public GameAssetCollection gameAssetDatabase;
    public GameAssetCollection2 gameAssetCollection2;
    public SOMatchData soMatchDatabase;
    public TankUpgradeCollection tankUpgradeCollection;
    
    public PlayerCollection playerDatabase;
    public PlayerSettingsCollection playerSettingsCollection;
    public UserCollection userCollection;
    public TankWrapCollection tankWrapCollection;
    public TankStickerCollection tankStickerCollection;
    public DeviceQualityCollection deviceQualityCollection;
    
    private Dictionary<Type, ScriptableObject> collectionMap;

    private void InitializeCollectionMap()
    {
        collectionMap = new Dictionary<Type, ScriptableObject>
        {
            { typeof(MatchmakingCollection), matchDatabase },
            { typeof(TankCollection), tankDatabase },
            { typeof(PlayerCollection), playerDatabase },
            { typeof(PlayerSettingsCollection), playerSettingsCollection },
            { typeof(UserCollection), userCollection },
            { typeof(MapCollection), mapDatabase },
            { typeof(TankWeaponCollection), tankWeaponDatabase },
            { typeof(TankHullCollection), tankHullDatabase },
            { typeof(TankAbilityCollection), tankAbilityDatabase },
            { typeof(SoundCollection), soundDatabase },
            { typeof(ProductCollection), productDatabase },
            { typeof(GameAssetCollection), gameAssetDatabase },
            { typeof(GameAssetCollection2), gameAssetCollection2 },
            { typeof(SOMatchData), soMatchDatabase },
            { typeof(TankUpgradeCollection), tankUpgradeCollection },
            { typeof(PackRewardCollection), packRewardCollection },
            { typeof(TankWrapCollection), tankWrapCollection },
            { typeof(TankStickerCollection), tankStickerCollection },
            { typeof(DeviceQualityCollection), deviceQualityCollection },
        };
    }
    
    public T GetCollection<T>() where T : ScriptableObject
    {
        if (collectionMap == null)
        {
            InitializeCollectionMap();
        }

        if (collectionMap.TryGetValue(typeof(T), out var collection))
        {
            return collection as T;
        }

        Debug.LogError($"Collection of type {typeof(T)} not found in GameDatabase");
        return null;
    }

    private void OnDestroy()
    {
        if (playerDatabase != null)
        {
            DestroyImmediate(playerDatabase);
            playerDatabase = null;
        }

        if (playerSettingsCollection != null)
        {
            DestroyImmediate(playerSettingsCollection);
            playerSettingsCollection = null;
        }

        if (userCollection != null)
        {
            DestroyImmediate(userCollection);
            userCollection = null;
        }

        collectionMap?.Clear();
        collectionMap = null;
    }
    
    public void SaveAll()
    {
        if (collectionMap == null) InitializeCollectionMap();

        foreach (var collection in collectionMap.Values)
        {
            if (collection is IWriteable writeable)
            {
                writeable.Write();
            }
        }
    }

    public void LoadAll() 
    {
        if (collectionMap == null) InitializeCollectionMap();

        foreach (var collection in collectionMap.Values)
        {
            if (collection is IReadable readable)
            {
                readable.Read();
            }
        }
    }

    public void DeleteAll()
    {
        if (collectionMap == null) InitializeCollectionMap();

        foreach (var collection in collectionMap.Values)
        {
            if (collection is IDeletable deletable)
            {
                deletable.Delete();
            }
        }
    }
}

// Add these interfaces if not already defined
public interface IWriteable
{
    void Write();
}

public interface IReadable
{
    void Read();
}

public interface IDeletable
{
    void Delete();
}