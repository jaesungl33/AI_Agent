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
[NodeDescription(name: "SpawnObject (Vector3)", story: "Spawn: [objectPrefab] at [spawnPosition]", category: "Action", id: "f1af08d0e3970b904a02325e7065ea60")]
public partial class SpawnObjectActionByPosition : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> objectPrefab;
    [SerializeReference]
    [Tooltip("Link to a String List variable from Blackboard")]
    public BlackboardVariable<Vector3> spawnPosition;

    [SerializeField] public BlackboardVariable<bool> destroyOnEnd;
    [SerializeReference] public BlackboardVariable<GameObject> spawnCache;
    [SerializeReference] public BlackboardVariable<string> functionName;
    [SerializeReference] public BlackboardVariable<SpawnObjectDataSO> Params;

    private GameObject spawnedObject;
    
    protected override Status OnStart()
    {
        // Validate inputs
        if (objectPrefab == null || objectPrefab.Value == null)
        {
            LogFailure("objectPrefab is not assigned!");
            return Status.Failure;
        }

        if (spawnPosition == null || string.IsNullOrWhiteSpace(spawnPosition.Value.ToString()))
        {
            LogFailure("spawnPosition is not assigned or empty!");
            return Status.Failure;
        }

        spawnedObject = null;

        // Find all ObjectPresenters in scene
        ObjectPresenter[] allPresenters = UnityEngine.Object.FindObjectsByType<ObjectPresenter>(FindObjectsSortMode.None);

        if (allPresenters.Length == 0)
        {
            LogFailure("No ObjectPresenters found in the scene!");
            return Status.Failure;
        }

        // Spawn object at presenter's position and rotation
        GameObject spawnedObj = UnityEngine.Object.Instantiate(
            objectPrefab.Value,
            spawnPosition.Value,
            Quaternion.identity
        );
        spawnedObj.SendMessage(functionName.Value, Params?.Value, SendMessageOptions.DontRequireReceiver);
        // Optional: Set parent to presenter's parent or keep world space
        // spawnedObj.transform.SetParent(presenter.transform.parent);

        spawnedObject = spawnedObj;
        if (spawnCache != null && spawnCache.Value != null)
        {
            spawnCache.Value = spawnedObj;
        }

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
            if (spawnedObject != null)
            {
                UnityEngine.Object.Destroy(spawnedObject);
            }
        }
    }
}

