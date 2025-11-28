using System;
using DG.Tweening;
using UnityEngine;
using GDOLib.UI;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using UniRx;
using BehaviorTree.Tutorials;

namespace GDO.View
{
	public class TutorialNpcPopup : PopupBase
	{
		[Serializable]
		private class NPC_UI
		{
			public Image npcAvatar;
			public TMP_Text npcName;
			public GameObject goAvatar;
			public GameObject goName;
		}

		[SerializeField] private CanvasGroup canvasGroup;
		[SerializeField] private float fadeTime = 0.25f;
		[SerializeField] private NPC_UI[] npcUIs;
		[SerializeField] private TMP_Text txtMessage;
		[SerializeField] private Color avatarGrayColor = Color.gray;
		[SerializeField] private float timeAnimText = 1f;
		[SerializeField] private float timeDelayClick = 1f;

		private TutorialNpcPopupParam param;
		private NPC_UI curNpcUI;
		private int curStepIndex = 0;
		private int curNpcIndex = 0;
		private bool clickAvailable;
        private NPCChatStatesData nPCChatStatesData;

        public override void Show(int additionalSortingOrder = 0, ScreenParam param = null)
		{
			base.Show(additionalSortingOrder, param);

			canvasGroup.alpha = 0f;
			canvasGroup.DOFade(1f, fadeTime);

			curStepIndex = 0;
			clickAvailable = true;

			if (curNpcUI != null)
			{
				curNpcUI.goAvatar.SetActive(false);
				curNpcUI.goName.SetActive(false);
			}

			foreach (var npcUI in npcUIs)
			{
				if (npcUI == null)
				{
					Debug.LogWarning("NPC UI is not assigned in the inspector.");
					continue;
				}
				npcUI.goAvatar.SetActive(false);
				npcUI.goName.SetActive(false);
			}

			if (param is TutorialNpcPopupParam popupParam)
			{
				this.param = popupParam;
				if (popupParam.chatSteps.Length > 0)
				{
					SetNpcChat(popupParam.chatSteps[curStepIndex]);
				}
				else
				{
					Debug.LogWarning("No chat steps provided in the popup parameter.");
				}
			}
		}

		private void SetNpcChat(TutorialNpcChatStep chatStep)
		{
			clickAvailable = false;
			DOVirtual.DelayedCall(timeDelayClick, () => clickAvailable = true, false);
			
			if (chatStep == null)
			{
				Debug.LogWarning("Chat step is null.");
				return;
			}

			var npcData = nPCChatStatesData.Data.FirstOrDefault(n => n.npcId.Equals(chatStep.npcId, StringComparison.OrdinalIgnoreCase));
			if (npcData == null)
			{
				Debug.LogWarning($"NPC data not found for ID: {chatStep.npcId}");
				return;
			}
			
			var spriteData = npcData.avatars.FirstOrDefault(i => i.state == chatStep.state);
			Sprite sprite;
			if (spriteData == null)
			{
				sprite = npcData.defaultAvatar;
			}
			else
			{
				sprite = spriteData.sprite;
			}
			var lastNpcIndex = curNpcIndex;
			if (curNpcUI != null)
			{
				curNpcUI.goName.SetActive(false);
				curNpcUI.npcAvatar.DOKill();
				curNpcUI.npcAvatar.DOColor(Color.clear, 0.5f);
			}
			curNpcUI = npcUIs[Mathf.Clamp(chatStep.npcIndex, 0, npcUIs.Length - 1)];
			curNpcUI.goAvatar.SetActive(true);
			curNpcUI.goName.SetActive(true);
			curNpcUI.goAvatar.transform.SetSiblingIndex(npcUIs.Length - 1);
			curNpcUI.npcAvatar.DOKill();
			if (lastNpcIndex != curNpcIndex)
			{
				curNpcUI.npcAvatar.color = Color.clear;
				curNpcUI.npcAvatar.DOColor(Color.white, 0.5f);
			}
			else
			{
				curNpcUI.npcAvatar.color = Color.white;
			}
			curNpcUI.npcAvatar.sprite = sprite;
			curNpcUI.npcName.text = npcData.npcName;

			 txtMessage.text = "";
			txtMessage.DOKill();
			DOTween.To(() => "", x => txtMessage.text = x, chatStep.message, timeAnimText).SetEase(Ease.Linear).SetUpdate(true);
		}

		public void OnNextStep()
		{
			if (!clickAvailable || curStepIndex >= param.chatSteps.Length)
				return;
			
			if (param == null || param.chatSteps == null || param.chatSteps.Length == 0)
			{
				Debug.LogWarning("No chat steps available to proceed.");
				return;
			}

			curStepIndex++;
			if (curStepIndex < param.chatSteps.Length)
			{
				SetNpcChat(param.chatSteps[curStepIndex]);
			}
			else
			{
				Debug.Log("No more chat steps available.");
				curNpcIndex = 0; // Reset NPC index
				curStepIndex = 0; // Reset step index
				
				//canvasGroup.alpha = 0f;
				canvasGroup.DOFade(0f, fadeTime);
				Observable.Timer(TimeSpan.FromSeconds(fadeTime)).Subscribe(_ => Hide());
			}
		}

		public void OnSkipTutorial()
		{
			if (!clickAvailable)
				return;
			
			clickAvailable = false;
			
			//canvasGroup.alpha = 0f;
			canvasGroup.DOFade(0f, fadeTime);
			Observable.Timer(TimeSpan.FromSeconds(fadeTime)).Subscribe(_ => Hide());
			EventManager.TriggerEvent<SkipTutorialEventData>(new SkipTutorialEventData { tutorialID = param.tutorialID });
		}
	}
	[Serializable]
	public class TutorialNpcChatStep
	{
		public string npcId;
		public NPCChatStates state;
		public string message;
		public int npcIndex; // Optional, can be used for ordering multiple NPCs
	}

	[Serializable]
	public class TutorialNpcPopupParam : ScreenParam
	{
		public TutorialNpcChatStep[] chatSteps;
		public string tutorialID;
	}
}