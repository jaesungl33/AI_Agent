using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "LockUI", story: "Lock [IsLock]", category: "Action", id: "c60cd16a9e7329942a06f8a42d9ea1a7")]
public partial class LockUiAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> IsLock;

    protected override Status OnStart()
    {
        if(IsLock.Value)
        {
            EventManager.TriggerEvent(new ShowPopupEvent(PopupIDs.LockUIPopup));
        }
        else
        {
            EventManager.TriggerEvent(new PopPopupEvent() { popupID = PopupIDs.LockUIPopup });
        }
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

