using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

namespace BehaviorTree.Tutorials
{
    /// <summary>
    /// Modifier (Decorator) thêm cooldown cho child node.
    /// Child chỉ được chạy khi hết cooldown.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Cooldown", 
        story: "Wait [CooldownTime] seconds before running [Child]",
        description: "Adds a cooldown period before executing the child node",
        category: "Flow/Modifiers",
        id: "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6"
    )]
    public partial class CooldownModifier : Modifier
    {
        [SerializeReference] public BlackboardVariable<float> CooldownTime = new(3f);
        [SerializeReference] public Node Child;
        
        private float lastExecutionTime = -Mathf.Infinity;

        protected override Status OnStart()
        {
            // Kiểm tra xem đã hết cooldown chưa
            if (Time.time - lastExecutionTime < CooldownTime.Value)
            {
                return Status.Failure; // Còn cooldown, không chạy
            }
            
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            // Kiểm tra lại cooldown
            if (Time.time - lastExecutionTime < CooldownTime.Value)
            {
                return Status.Failure;
            }

            // Chạy child node
            Status childStatus = Child.CurrentStatus;
            
            // Khi child hoàn thành, lưu thời gian
            if (childStatus == Status.Success || childStatus == Status.Failure)
            {
                lastExecutionTime = Time.time;
                return childStatus;
            }
            
            return Status.Running;
        }

        protected override void OnEnd()
        {
            // Cleanup nếu cần
        }
    }
}
