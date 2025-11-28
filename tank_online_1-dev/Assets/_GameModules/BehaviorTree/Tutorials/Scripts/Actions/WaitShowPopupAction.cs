using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitShowPopup", story: "Wait Show Popup: [popupId] ", category: "Action", id: "12f1f2ff680fc59955abfb16d343551a")]
public partial class WaitShowPopupAction : Action
{
    [SerializeReference] public BlackboardVariable<PopupIDs> popupId; // Use enum as int
    private bool isShown = false;
    protected override Status OnStart()
    {
        isShown = false;
        EventManager.Register<ShowPopupEvent>(ShowPopupEventHandler);
        return Status.Running;
    }

    private void ShowPopupEventHandler(ShowPopupEvent @event)
    {
        if (@event.ID == popupId.Value)
        {
            EventManager.Unregister<ShowPopupEvent>(ShowPopupEventHandler);
            isShown = true;
        }
    }

    protected override Status OnUpdate()
    {
        if (!isShown)
        {
            return Status.Running;
        }
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

