using System;
using System.Collections.Generic;
using Fusion;
using Fusion.GameSystems;
using Fusion.TankOnlineModule;
using FusionHelpers;
using UnityEngine;

public class TurretSpawner : NetworkBehaviour
{
    [SerializeField] private Turret turretPrefab;
    [SerializeField] private OutpostObject[] outpostObjects;
    [SerializeField] private List<NetworkObject> networkObjects = new();
    private TankCollection tankCollection;

    public OutpostObject[] Outposts => outpostObjects;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        RegisterEvents(); 
    }

    public override void Spawned()
    {
        tankCollection = DatabaseManager.GetDB<TankCollection>();
        TankDocument[] tanks = tankCollection.CloneDocumentsByProperty(doc => doc.tankType == TankType.Outpost, true);
        for (int i = 0; i < tanks.Length; i++)
        {
            outpostObjects[i].outpostId = tanks[i].tankId;
        }
        base.Spawned();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        UnregisterEvents();
        base.Despawned(runner, hasState);
    }

    private void RegisterEvents()
    {
       EventManager.Register<TurretSpawnerEvent>(OnLevelEvent);
    }

    private void UnregisterEvents()
    {
        if (GameManager.IsApplicationQuitting) return;

        EventManager.Unregister<TurretSpawnerEvent>(OnLevelEvent);
    }

    private void OnLevelEvent(TurretSpawnerEvent @event)
    {
        SpawnTurrets();
    }

    public void DespawnTurrets()
    {
        foreach (var turret in networkObjects)
        {
            Runner.Despawn(turret);
        }
        networkObjects.Clear();
    }

    public void SpawnTurrets()
    {
        if (Object.HasStateAuthority && (Runner.IsServer || Runner.IsSharedModeMasterClient))
        {
            foreach (var outpost in outpostObjects)
            {
                Turret newTurret = Runner.Spawn(turretPrefab, outpost.position, outpost.rotation, inputAuthority: null);
                newTurret.Init(outpost.outpostId);
                networkObjects.Add(newTurret.Object);
            }

            Debug.Log($"Đã spawn {outpostObjects.Length} trụ.");
        }
        else
        {
            Debug.LogWarning("Chỉ có server hoặc master client mới có thể spawn trụ.");
            return;
        }
    }
}

public struct TurretSpawnerEvent : INetworkEvent
{
}

[System.Serializable]
public class OutpostObject
{
    public string outpostId;
    public Vector3 position;
    public Quaternion rotation = Quaternion.identity; //default
}