using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DestroyObjects", story: "Destroy [Objects]", category: "Action", id: "566350baa03b3e1f3f1744212d9ed3fe")]
public partial class DestroyObjectsAction : Action
{
    [SerializeReference] public BlackboardVariable<List<GameObject>> Objects;

    protected override Status OnStart()
    {
        if (Objects == null || Objects.Value == null || Objects.Value.Count == 0)
        {
            Debug.LogWarning("DestroyObjectsAction: No objects to destroy.");
            return Status.Failure;
        }
        foreach (var obj in Objects.Value)
        {
            if (obj != null)
            {
                UnityEngine.Object.Destroy(obj);
                Debug.Log($"DestroyObjectsAction: Destroyed object '{obj.name}'.");
            }
            else
            {
                Debug.LogWarning("DestroyObjectsAction: Encountered a null object in the list.");
            }
        }
        Objects.Value.Clear();
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

