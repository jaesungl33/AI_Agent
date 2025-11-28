using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace BehaviorTree.Tutorials
{
    /// <summary>
    /// Modifier (Decorator) đảo ngược kết quả của child node.
    /// Success -> Failure, Failure -> Success
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Inverter", 
        story: "Invert result of [Child]",
        description: "Inverts the result of the child node (Success <-> Failure)",
        category: "Flow/Modifiers",
        id: "c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8"
    )]
    public partial class InverterModifier : Modifier
    {
        [SerializeReference] public Node Child;

        protected override Status OnStart()
        {
            if (Child == null)
                return Status.Failure;
                
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            Status childStatus = Child.CurrentStatus;
            
            // Đảo ngược kết quả
            if (childStatus == Status.Success)
                return Status.Failure;
                
            if (childStatus == Status.Failure)
                return Status.Success;
            
            return Status.Running;
        }

        protected override void OnEnd()
        {
        }
    }
}
