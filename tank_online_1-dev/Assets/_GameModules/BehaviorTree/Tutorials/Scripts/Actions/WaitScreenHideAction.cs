using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitScreenHide", story: "Wait [ScreenId] hide", category: "Action", id: "7ae9de7d6a3a79cf8f7e304e988348a9")]
public partial class WaitScreenHideAction : Action
{
    [SerializeReference] public BlackboardVariable<UIIDs> ScreenId;
    private bool isHide;

    protected override Status OnStart()
    {
        isHide = false;
        EventManager.Register<HideUIEvent>(HideUIHandler);
        return Status.Running;
    }

    private void HideUIHandler(HideUIEvent uiEvent)
    {
        if (uiEvent.ID == ScreenId.Value)
        {
            EventManager.Unregister<HideUIEvent>(HideUIHandler);
            isHide = true;
        }
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

