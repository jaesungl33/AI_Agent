using System;
using System.Collections.Generic;
using System.Linq;
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
        name: "Tutorial Selector",
        story: "Current step name: [currentStepName] . Executes child nodes [tutorialStepsRef] only when the current checkpoint matches the step name. Supports skip [skipTutorialRef] tutorial.",
        description: "Executes child nodes only when the current checkpoint matches the step name. Supports skip tutorial.",
        category: "Tutorial/Composite",
        id: "d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9"
    )]
    public partial class TutorialSelector : Composite
    {
        private bool isFailed = false;
        [SerializeReference] public BlackboardVariable<string> currentStepName;
        [SerializeReference] public BlackboardVariable<List<string>> tutorialStepsRef;
        [SerializeReference] public BlackboardVariable<bool> skipTutorialRef;
        [CreateProperty] int m_CurrentChild;
        protected override Status OnStart()
        {
            if (tutorialStepsRef == null)
            {
                Debug.LogError("tutorialStepKeySharedList is null");
            }
            else
            {
                var tutorialStepTasks = Children
                    .Where(i => i is TutorialStepModifier)
                    .Select(i => ((TutorialStepModifier)i).stepName).ToList();
                tutorialStepsRef.Value = tutorialStepTasks.Select(i => i.Value).ToList();
                Debug.Log("selection tutorialStepTasks:" + tutorialStepTasks.Count);
            }
            isFailed = false;
            Debug.Log("checkPointName.Value 1: " + currentStepName.Value);
            if (string.IsNullOrEmpty(currentStepName.Value))
            {
                currentStepName.Value = tutorialStepsRef.Value[0];
            }
            else if (currentStepName.Value == TutorialsManager.TUTORIAL_FINISH_VALUE)
            {
                isFailed = true;
            }
            Debug.Log("checkPointName.Value 2: " + currentStepName.Value);
            if (!isFailed)
            {
                skipTutorialRef.Value = false;
                m_CurrentChild = 0;
                Status childStatus;
                for (int i = 0; i < tutorialStepsRef.Value.Count; i++)
                {
                    childStatus = StartChild(m_CurrentChild);
                    if (childStatus== Status.Running || childStatus == Status.Waiting)
                    {
                        return Status.Waiting;
                    }
                    else
                    {
                        m_CurrentChild++;
                    }
                }
            }
            return Status.Failure;
        }
        protected override Status OnUpdate()
        {
            if (isFailed) return Status.Failure;
            
            var currentChild = Children[m_CurrentChild];
            Status childStatus = currentChild.CurrentStatus;
            if (childStatus == Status.Success || childStatus == Status.Failure)
            {
                return StartChild(++m_CurrentChild);
            }
            return childStatus == Status.Running ? Status.Waiting : childStatus;
        }
        protected Status StartChild(int childIndex)
        {
            if (m_CurrentChild >= Children.Count)
            {
                return Status.Success;
            }
            var childStatus = StartNode(Children[childIndex]);
            return childStatus switch
            {
                Status.Success => childIndex + 1 >= Children.Count ? Status.Success : Status.Running,
                Status.Running => Status.Waiting,
                _ => childStatus
            };
        }
    }
}
