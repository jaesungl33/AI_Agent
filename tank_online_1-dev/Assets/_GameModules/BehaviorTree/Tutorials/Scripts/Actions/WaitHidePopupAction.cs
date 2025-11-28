using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitHidePopup", story: "Wait Hide PopupID: [popupId] ", category: "Action", id: "3487713dac0af3d1096bfacb1526ca5a")]
public partial class WaitHidePopupAction : Action
{
    [SerializeReference] public BlackboardVariable<PopupIDs> popupId; // Use enum as int
    private bool isHidden = false;
    protected override Status OnStart()
    {
        isHidden = false;
        EventManager.Register<PopPopupEvent>(HidePopupEventHandler);
        return Status.Running;
    }

    private void HidePopupEventHandler(PopPopupEvent @event)
    {
        if (@event.popupID == popupId.Value)
        {
            EventManager.Unregister<PopPopupEvent>(HidePopupEventHandler);
            isHidden = true;
        }
    }

    protected override Status OnUpdate()
    {
        if (!isHidden)
        {
            return Status.Running;
        }
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

