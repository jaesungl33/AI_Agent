using GDO;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using GDOLib.UI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SpawnObjects (ids)", story: "Spawn: [objectPrefab] at [spawnPositions]", category: "Action", id: "f1af08d0e3970b904a02325e7065ea57")]
public partial class SpawnObjectsAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> objectPrefab;
    [SerializeReference]
    [Tooltip("Link to a String List variable from Blackboard")]
    public BlackboardVariable<List<string>> spawnPositions;
    
    [SerializeField] public BlackboardVariable<bool> destroyOnEnd;
    [SerializeReference] public BlackboardVariable<List<GameObject>> spawnCache;
    [SerializeReference] public BlackboardVariable<string> functionName;
    [SerializeReference] public BlackboardVariable<SpawnObjectDataSO> Params;
    
    private List<GameObject> spawnedObjects = new List<GameObject>();

    protected override Status OnStart()
    {
        // Validate inputs
        if (objectPrefab == null || objectPrefab.Value == null)
        {
            LogFailure("objectPrefab is not assigned!");
            return Status.Failure;
        }
        
        if (spawnPositions == null || spawnPositions.Value == null || spawnPositions.Value.Count == 0)
        {
            LogFailure("spawnPositions is empty or not assigned!");
            return Status.Failure;
        }
        
        // Clear previous spawned objects
        spawnedObjects.Clear();
        
        // Find all ObjectPresenters in scene
        ObjectPresenter[] allPresenters = UnityEngine.Object.FindObjectsByType<ObjectPresenter>(FindObjectsSortMode.None);
        
        if (allPresenters.Length == 0)
        {
            LogFailure("No ObjectPresenters found in the scene!");
            return Status.Failure;
        }
        
        // Spawn objects at matching positions
        int spawnCount = 0;
        foreach (string positionId in spawnPositions.Value)
        {
            if (string.IsNullOrWhiteSpace(positionId))
                continue;
            
            // Find presenter with matching ID
            ObjectPresenter presenter = allPresenters
                .FirstOrDefault(p => p.ID.Equals(positionId, StringComparison.OrdinalIgnoreCase));
            
            if (presenter == null)
            {
                Debug.LogWarning($"SpawnObjectsAction: No ObjectPresenter found with ID '{positionId}'");
                continue;
            }
            
            // Spawn object at presenter's position and rotation
            GameObject spawnedObj = UnityEngine.Object.Instantiate(
                objectPrefab.Value,
                presenter.transform.position,
                presenter.transform.rotation
            );
            spawnedObj.SendMessage(functionName.Value, Params?.Value, SendMessageOptions.DontRequireReceiver);
            // Optional: Set parent to presenter's parent or keep world space
            // spawnedObj.transform.SetParent(presenter.transform.parent);
            
            spawnedObjects.Add(spawnedObj);
            spawnCount++;
            if (spawnCache != null && spawnCache.Value != null)
            {
                spawnCache.Value.Add(spawnedObj);
            }
            Debug.Log($"SpawnObjectsAction: Spawned '{objectPrefab.Value.name}' at '{positionId}' (Position: {presenter.transform.position})");
        }
        
        if (spawnCount == 0)
        {
            LogFailure("Failed to spawn any objects. No matching ObjectPresenters found!");
            return Status.Failure;
        }
        
        Debug.Log($"SpawnObjectsAction: Successfully spawned {spawnCount} objects");
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
        if (destroyOnEnd != null && destroyOnEnd.Value)
        {
            foreach (var obj in spawnedObjects)
            {
                if (obj != null)
                {
                    UnityEngine.Object.Destroy(obj);
                }
            }
            spawnedObjects.Clear();
        }
    }
}

