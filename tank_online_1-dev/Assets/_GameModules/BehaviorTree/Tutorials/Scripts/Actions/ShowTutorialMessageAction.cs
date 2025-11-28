using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace BehaviorTree.Tutorials
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "ShowTutorialMessage",
        story: "Message [messages]",
        category: "Action",
        id: "60e42aba6ac7ab72354e9c6b64e20af4"
    )]
    public partial class ShowTutorialMessageAction : Action
    {
        [SerializeReference] public BlackboardVariable<string> npcId;
        [SerializeReference] public BlackboardVariable<string> messages; // Or use List<string> if supported
        private bool isDone = false;
        protected override Status OnStart()
        {
            isDone = false;
            // Create TutorialButton from blackboard variables
            if(string.IsNullOrWhiteSpace(messages?.Value))
            {
                Debug.LogError("ShowTutorialMessageAction: messages is null or empty.");
                return Status.Failure;
            }
            TutorialButtonPopupParam buttonParam = new TutorialButtonPopupParam
            {
                targetPresenterId = "",
                npcId = npcId?.Value ?? "",
                messages = string.IsNullOrEmpty(messages?.Value) ? new string[0] : messages.Value.Split('|'),
            };
            EventManager.TriggerEvent(new ShowPopupEvent(PopupIDs.TutorialButtonPopup, buttonParam));
            // TODO: Show tutorial button with the created data
            Debug.Log($"Showing tutorial button: {buttonParam.targetPresenterId}");
            EventManager.Register<PopPopupEvent>(PopPopupEventHandler);
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