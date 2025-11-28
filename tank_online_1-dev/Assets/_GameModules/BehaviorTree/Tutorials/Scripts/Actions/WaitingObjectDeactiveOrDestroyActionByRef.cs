using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UniRx;
using GDOLib.UI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Waiting Object hide", story: "Wait [ObjectRef] hide", category: "Action", id: "e5a1f12112178ac2a1e1a6b37144716d")]
public partial class WaitingObjectDeactiveOrDestroyActionByRef : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> ObjectRef;
    [SerializeReference] public BlackboardVariable<float> Interval = new BlackboardVariable<float>(1f);
    private bool isHide = false;
    private GameObject targetObject;
    protected override Status OnStart()
    {
        if (ObjectRef == null || ObjectRef.Value == null)
        {
            isHide = true;
            return Status.Running;
        }
        else
        {
            targetObject = ObjectRef.Value;
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

