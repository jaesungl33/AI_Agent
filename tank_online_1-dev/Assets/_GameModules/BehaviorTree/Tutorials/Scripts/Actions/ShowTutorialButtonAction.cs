using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace BehaviorTree.Tutorials
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "ShowTutorialButton",
        story: "Button [targetPresenterId]",
        category: "Action",
        id: "60e42aba6ac7ab72354e9c6b64e20af5"
    )]
    public partial class ShowTutorialButtonAction : Action
    {
        [SerializeReference] public BlackboardVariable<string> targetPresenterId;
        [SerializeReference] public BlackboardVariable<bool> hideTargetButton;
        [SerializeReference] public BlackboardVariable<TutorialButtonPopupParam.PointerPosition> pointerPosition; // Use enum as int
        [SerializeReference] public BlackboardVariable<TutorialButtonPopupParam.MessagePosition> messagePosition; // Use enum as int
        private bool isDone = false;
        protected override Status OnStart()
        {
            isDone = false;
            // Create TutorialButton from blackboard variables
            EventManager.Register<PopPopupEvent>(PopPopupEventHandler);
            if(string.IsNullOrEmpty(targetPresenterId.Value))
            {
                Debug.LogError("ShowTutorialButtonAction: targetPresenterId is null or empty.");
                return Status.Failure;
            }
            TutorialButtonPopupParam buttonParam = new TutorialButtonPopupParam
            {
                targetPresenterId = targetPresenterId.Value,
                npcId = "",
                messages = new string[0],
                pointerPosition = (TutorialButtonPopupParam.PointerPosition)(pointerPosition?.Value ?? 0),
                messagePosition = (TutorialButtonPopupParam.MessagePosition)(messagePosition?.Value ?? 0)
            };
            EventManager.TriggerEvent(new ShowPopupEvent(PopupIDs.TutorialButtonPopup, buttonParam));
            // TODO: Show tutorial button with the created data
            Debug.Log($"Showing tutorial button: {buttonParam.targetPresenterId}");
           
            return Status.Running;
        }

        private void PopPopupEventHandler(PopPopupEvent @event)
        {
            EventManager.Unregister<PopPopupEvent>(PopPopupEventHandler);
            isDone = true;
        }

        protected override Status OnUpdate()
        {
            // TODO: Check if tutorial button interaction is complete
            if (!isDone)
            {
                return Status.Running;
            }
            return Status.Success;
        }

        protected override void OnEnd()
        {
            // TODO: Hide tutorial button if needed
        }
    }

}