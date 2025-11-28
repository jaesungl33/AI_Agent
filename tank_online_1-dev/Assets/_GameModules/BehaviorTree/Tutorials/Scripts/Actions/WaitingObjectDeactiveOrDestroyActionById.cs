using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UniRx;
using GDOLib.UI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Waiting Object hide", story: "Wait [ObjectId] hide", category: "Action", id: "e5a1f12112178ac2a1e1a6b37144717d")]
public partial class WaitingObjectDeactiveOrDestroyActionById : Action
{
    [SerializeReference] public BlackboardVariable<string> ObjectId;
    [SerializeReference] public BlackboardVariable<float> Interval = new BlackboardVariable<float>(1f);
    private bool isHide = false;
    private GameObject targetObject;
    protected override Status OnStart()
    {
        isHide = false;
        // use observarble to wait for interval seconds to find object with objectId
        // find object presenter by id
        ObjectPresenter[] allPresenters = UnityEngine.Object.FindObjectsByType<ObjectPresenter>(FindObjectsSortMode.None);
        ObjectPresenter targetPresenter = Array.Find(allPresenters, presenter => presenter.ID == ObjectId.Value);
        targetObject = targetPresenter != null ? targetPresenter.gameObject : null;
        if (targetObject == null)
        {
            isHide = true;
        }
        else
        {
            Observable.Timer(TimeSpan.FromSeconds(Interval.Value)).Subscribe(_ =>
            {
                if(targetObject == null || !targetObject.activeSelf)
                {
                    isHide = true;
                }
            });
        }
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!isHide)
        {
            return Status.Running;
        }
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

