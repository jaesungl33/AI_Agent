using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using BehaviorTree.Tutorials;
namespace BehaviorTree.Tutorials
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Tutorial Step Done", story: "Step Done", category: "Action", id: "8aad2b1f7ac090ab4e027675edd88fed")]
    public partial class TutorialStepDoneAction : Action
    {
        [SerializeReference] public BlackboardVariable<string> currentStepName;
        [SerializeReference] public BlackboardVariable<string> tutorialID;
        protected override Status OnStart()
        {
            EventManager.TriggerEvent(new TutorialStepCompletedEventData(
                tutorialID.Value,
                currentStepName.Value));
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
}
