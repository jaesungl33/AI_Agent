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
[NodeDescription(name: "SpawnObjects (Vector3)", story: "Spawn: [objectPrefab] at [spawnPositions]", category: "Action", id: "f1af08d0e3970b904a02325e7065ea58")]
public partial class SpawnObjectsActionByPosition : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> objectPrefab;
    [SerializeReference] public BlackboardVariable<Vector3> spawnPositions;
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

        // Clear previous spawned objects
        spawnedObjects.Clear();

        // Find all ObjectPresenters in scene
        ObjectPresenter[] allPresenters = UnityEngine.Object.FindObjectsByType<ObjectPresenter>(FindObjectsSortMode.None);

        if (allPresenters.Length == 0)
        {
            LogFailure("No ObjectPresenters found in the scene!");
            return Status.Failure;
        }
        GameObject spawnedObj = UnityEngine.Object.Instantiate(
                objectPrefab.Value,
                spawnPositions,
                Quaternion.identity
            );
        spawnedObj.SendMessage(functionName.Value, Params?.Value, SendMessageOptions.DontRequireReceiver);
        if (spawnCache != null && spawnCache.Value != null)
        {
            spawnCache.Value.Add(spawnedObj);
        }
        Debug.Log($"SpawnObjectsAction: Successfully spawned 1 object at position {spawnPositions}.");
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

