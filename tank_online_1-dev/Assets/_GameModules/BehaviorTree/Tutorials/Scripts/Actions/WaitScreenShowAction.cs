using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitScreenShow", story: "Wait [ScreenId] show", category: "Action", id: "7ae9de7d6a3a79cf8f7e304e988348a8")]
public partial class WaitScreenShowAction : Action
{
    [SerializeReference] public BlackboardVariable<UIIDs> ScreenId;
    private bool isHide;

    protected override Status OnStart()
    {
        isHide = false;
        EventManager.Register<UIEvent>(ShowUI);
        
        Debug.Log("WaitScreenShowAction: Started waiting for screen: " + ScreenId.Value);
        
        // Return Waiting so OnUpdate() will be called every frame
        return Status.Running;
    }

    private void ShowUI(UIEvent uiEvent)
    {
        if (uiEvent.ID == ScreenId.Value)
        {
            isHide = true;
            EventManager.Unregister<UIEvent>(ShowUI);
        }
    }

    protected override Status OnUpdate()
    {
        if (!isHide)
        {
            Debug.Log("Waiting for screen to show: " + ScreenId.Value);
            return Status.Running; // Keep waiting, OnUpdate() will be called next frame
        }
        
        Debug.Log("Screen shown: " + ScreenId.Value);
        return Status.Success;
    }

    protected override void OnEnd()
    {
        // Always unregister event to prevent memory leaks
        EventManager.Unregister<UIEvent>(ShowUI);
    }
}

