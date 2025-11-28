using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using BehaviorTree.Tutorials;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TutorialDone", story: "DONEEEEE", category: "Action", id: "a754fee964b7c8832169964a31acc2a2")]
public partial class TutorialDoneAction : Action
{
    [SerializeField] private BlackboardVariable<string> tutorialID;
    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        EventManager.TriggerEvent(new DoneTutorialEventData { tutorialID = tutorialID.Value });
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

