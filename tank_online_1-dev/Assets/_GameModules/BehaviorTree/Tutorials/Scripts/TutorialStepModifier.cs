using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace BehaviorTree.Tutorials
{
    /// <summary>
    /// Modifier (Decorator) that checks tutorial step condition.
    /// Executes child node only when the current checkpoint matches the step name.
    /// Supports skip tutorial functionality.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Tutorial Step Modifier",
        story:"Step: [stepName]",
        description: "Executes child nodes only when the current checkpoint matches the step name. Supports skip tutorial.",
        category: "Tutorial/Modifiers",
        id: "d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s10"
    )]
    public partial class TutorialStepModifier : Modifier
    {
        [SerializeReference] 
        [Tooltip("The name of this tutorial step")]
        public BlackboardVariable<string> stepName;
        
        [SerializeReference] 
        [Tooltip("Reference to current step name variable (REQUIRED)")]
        public BlackboardVariable<string> currentStepNameRef;
        
        [SerializeReference] 
        [Tooltip("Reference to skip tutorial flag variable (REQUIRED)")]
        public BlackboardVariable<bool> skipTutorialRef;
        

        protected override Status OnStart()
        {
            // Validate required references
            if (currentStepNameRef == null)
            {
                Debug.LogError("TutorialStepModifier: currentStepNameRef is not assigned!", GameObject);
                return Status.Failure;
            }
            
            if (skipTutorialRef == null)
            {
                Debug.LogError("TutorialStepModifier: skipTutorialRef is not assigned!", GameObject);
                return Status.Failure;
            }
            
            // Register skip tutorial event
            EventManager.Register<SkipTutorialEventData>(SkipTutorialEventHandle);
            
            // Check if current step matches this step
            if (currentStepNameRef.Value != stepName.Value)
            {
                return Status.Failure; // Not this step's turn yet
            }
            
            // Check skip tutorial flag
            if (skipTutorialRef.Value)
            {
                return Status.Success; // Skip this step
            }
            
            // Start the child node
            if (Child == null)
            {
                Debug.LogError("TutorialStepModifier: Child node is null!", GameObject);
                return Status.Failure;
            }
            
            Status childStatus = StartNode(Child);
            
            // If child completes immediately, return its status
            if (childStatus == Status.Success || childStatus == Status.Failure)
            {
                return childStatus;
            }
            
            // Otherwise, wait for child to complete
            return Status.Waiting;
        }

        

        private void SkipTutorialEventHandle(SkipTutorialEventData data)
        {
            skipTutorialRef.Value = true;
        }

        protected override Status OnUpdate()
        {
            if (string.IsNullOrEmpty(stepName.Value)) return Status.Failure;
            if(currentStepNameRef.Value != stepName.Value)
            {
                return Status.Failure;
            }

            // Check skip tutorial flag
            if (skipTutorialRef != null && skipTutorialRef.Value)
            {
                return Status.Success; // Skip this step
            }

            // Get child's current status (DON'T call StartNode here!)
            Status childStatus = Child.CurrentStatus;

            // Handle child result
            if (childStatus == Status.Success || childStatus == Status.Failure)
            {
                return childStatus;
            }

            return Status.Waiting;
        }

        protected override void OnEnd()
        {
            // Always unregister event
            EventManager.Unregister<SkipTutorialEventData>(SkipTutorialEventHandle);
            
            // Reset skip tutorial flag if needed
            if (skipTutorialRef != null)
            {
                skipTutorialRef.Value = false;
            }
        }

    }
}
