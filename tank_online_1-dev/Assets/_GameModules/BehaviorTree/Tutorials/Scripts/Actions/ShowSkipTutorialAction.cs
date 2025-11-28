using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.EventSystems;
using GDO.View;
using UniRx;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ShowSkipTutorial", story: "ShowSkipTutorial: [hasSkipTutorial] ", category: "Action", id: "dec51d731578c1c67b1e45d4dca71b7e")]
public partial class ShowSkipTutorialAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> hasSkipTutorial;
    private bool hasDecided = false;
    protected override Status OnStart()
    {
        hasDecided = false;
        TutorialSkipPopupParam param = new TutorialSkipPopupParam();
        param.onSkip = OnSkipTutorialSelected;
        param.onContinue = OnContinueTutorialSelected;
        EventManager.TriggerEvent(new ShowPopupEvent(PopupIDs.TutorialSkipPopup, param));
        return Status.Running;
    }
    private void OnSkipTutorialSelected()
    {
        hasSkipTutorial.Value = true;
         hasDecided = true;
    }
    private void OnContinueTutorialSelected()
    {
        hasSkipTutorial.Value = false;
       hasDecided = true;
    }
    protected override Status OnUpdate()
    {
        if(!hasDecided)
        {
            return Status.Running;
        }
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

