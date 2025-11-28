using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UniRx;
using GDOLib.UI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitingObjectSpawn", story: "Wait [ObjectId] Spawn [Interval]", category: "Action", id: "e5a1f12112178ac2a1e1a6b37144715d")]
public partial class WaitingObjectSpawnAction : Action
{
    [SerializeReference] public BlackboardVariable<string> ObjectId;
    [SerializeReference] public BlackboardVariable<float> Interval = new BlackboardVariable<float>(1f);
    private bool isSpawned = false;
    protected override Status OnStart()
    {
        isSpawned = false;
        // use observarble to wait for interval seconds to find object with objectId
        Observable.Timer(TimeSpan.FromSeconds(Interval.Value)).Subscribe(_ =>
        {
            // find object presenter by id
            ObjectPresenter[] allPresenters = UnityEngine.Object.FindObjectsByType<ObjectPresenter>(FindObjectsSortMode.None);
            ObjectPresenter targetObject = Array.Find(allPresenters, presenter => presenter.ID == ObjectId.Value);
            if (targetObject != null)
            {
                isSpawned = true;
            }
        });
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!isSpawned)
        {
            return Status.Running;
        }
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

