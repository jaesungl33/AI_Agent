using System;
using Unity.Behavior;
using UnityEngine;
using Modifier = Unity.Behavior.Modifier;
using Unity.Properties;
namespace BehaviorTree.Tutorials
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "TutorialInit", category: "Flow", id: "113288f317dc0ae4c2dbbc7110df0f3b")]
    public partial class TutorialInitModifier : Modifier
    {
        [SerializeReference] public BlackboardVariable<string> stepName;
        protected override Status OnStart()
        {
            if (Child == null)
            {
                return Status.Failure;
            }
            Status childStatus = StartNode(Child);
            return SucceedIfChildIsComplete(childStatus);
        }

        /// <inheritdoc cref="OnUpdate" />
        protected override Status OnUpdate()
        {
            return SucceedIfChildIsComplete(Child.CurrentStatus);
        }

        private Status SucceedIfChildIsComplete(Status childStatus)
        {
            if (childStatus is Status.Success or Status.Failure)
            {
                return Status.Success;
            }
            return Status.Waiting; // Parent waits for child to complete
        }
    }

}