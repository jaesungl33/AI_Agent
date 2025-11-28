using System;
using System.Collections;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine.Serialization;
using UnityEngine.UI;
using GDOLib.UI;
using GDO;
namespace BehaviorTree.Tutorials
{
    public class TutorialButtonPopup : PopupBase
    {
        [SerializeField] private NPCChatStatesData nPCChatStatesData;
        [Header("Message Settings")]
        [SerializeField] private GameObject goMessage;

        [SerializeField] private ButtonHandler messageSkipButton;
        [SerializeField] private Image imgNpc;
        [SerializeField] private TMP_Text txtMessage;
        [SerializeField] private float timeAnimText = 1f;
        [SerializeField] private Transform messageTransform;
        [SerializeField] private Transform messagePositionTop;
        [SerializeField] private Transform messagePositionCenter;
        [SerializeField] private Transform messagePositionBottom;

        [Header("Pointer Settings")]
        [SerializeField] private GameObject goPointer;
        [SerializeField] private Transform pointerLeft;
        [SerializeField] private Transform pointerRight;
        [SerializeField] private Transform pointerTop;
        [SerializeField] private Transform pointerBottom;

        [Header("Button Settings")]
        [SerializeField] private float timeDelayClick = 1f;

        private RectTransform target;
        private RectTransform clone; // Cloned button
        private Transform buttonContainer; // Parent container for clone

        private bool clickAvailable;
        private TutorialButtonPopupParam popupParam;

        private int curMessageIndex = 0;
        private Coroutine routineWaitButtonTarget;
        private Coroutine routineSetupButtonClone;
        
        private void OnDestroy()
        {
            CleanupClone();
        }

        public override void Show(int additionalSortingOrder = 0, ScreenParam param = null)
        {
            base.Show(additionalSortingOrder, param);

            curMessageIndex = 0;
            clickAvailable = true;

            CleanupClone();

            if (routineSetupButtonClone != null)
            {
                StopCoroutine(routineSetupButtonClone);
            }
            if (routineWaitButtonTarget != null)
            {
                StopCoroutine(routineWaitButtonTarget);
            }

            routineSetupButtonClone = null;
            routineWaitButtonTarget = null;

            pointerLeft.gameObject.SetActive(false);
            pointerRight.gameObject.SetActive(false);
            pointerTop.gameObject.SetActive(false);
            pointerBottom.gameObject.SetActive(false);

            goMessage.SetActive(false);

            if (param is TutorialButtonPopupParam popupParam)
            {
                this.popupParam = popupParam;

                messageSkipButton.gameObject.SetActive(popupParam.messages.Count(m => !string.IsNullOrWhiteSpace(m)) > 1 || string.IsNullOrWhiteSpace(popupParam.targetPresenterId));

                // Start waiting for target button first if needed
                if (!string.IsNullOrWhiteSpace(popupParam.targetPresenterId))
                {
                    routineWaitButtonTarget = StartCoroutine(DoWaitButton());
                }
                
                // Setup message after starting wait routine
                SetupMessage(0);
            }
        }

        private IEnumerator DoWaitButton()
        {
            UIButton uiButton; // Find most centered button
            while (true)
            {
                uiButton = FindObjectsByType<UIButton>(FindObjectsSortMode.None)
                    .Where(ui => ui.ID.Equals(popupParam.targetPresenterId))
                    .OrderBy(ui =>
                        (Camera.main.WorldToViewportPoint(ui.transform.position) - Vector3.one / 2f).sqrMagnitude)
                    .FirstOrDefault();
                if (uiButton == null)
                {
                    yield return new WaitForSeconds(0.15f);
                    continue;
                }
                Debug.Log($"TutorialButtonPopup: Found target button '{popupParam.targetPresenterId}' at position {uiButton.transform.position}");
                break;
            }

            target = uiButton.GetComponent<RectTransform>();
            routineWaitButtonTarget = null;
            
            // Only setup clone if no messages or all messages shown
            if ((popupParam.messages == null || popupParam.messages.Length == 0 || curMessageIndex >= popupParam.messages.Length - 1) && routineSetupButtonClone == null)
            {
                routineSetupButtonClone = StartCoroutine(SetupButtonClone());
            }
        }

        private IEnumerator SetupButtonClone()
        {
            if (routineWaitButtonTarget == null && (target == null || !target.gameObject.activeInHierarchy))
                yield return DoWaitButton();
            yield return new WaitUntil(() => target != null && target.gameObject.activeInHierarchy);

            // Force rebuild layout to ensure target has correct size
            if (target.parent != null)
            {
                var parentRect = target.parent.GetComponent<RectTransform>();
                if (parentRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
                }
            }
            
            // Wait one frame to ensure layout is fully applied
            yield return null;

            // Store target's world rect corners before cloning
            Vector3[] targetWorldCorners = new Vector3[4];
            target.GetWorldCorners(targetWorldCorners);
            Vector2 targetSize = target.rect.size;
            Vector3 targetWorldCenter = target.TransformPoint(target.rect.center);

            // Clone the target button
            clone = Instantiate(target.gameObject, transform).GetComponent<RectTransform>();
            clone.name = "TutorialButtonClone";
            
            // Store reference to button container (parent of target)
            buttonContainer = target.parent;
            
            // Setup RectTransform to match target's world rect exactly
            // Use anchored position with center anchors for consistent positioning
            clone.anchorMin = new Vector2(0.5f, 0.5f);
            clone.anchorMax = new Vector2(0.5f, 0.5f);
            clone.pivot = new Vector2(0.5f, 0.5f);
            
            // Set size explicitly (independent of anchors)
            clone.sizeDelta = targetSize;
            
            // Position clone at target's world center
            clone.position = targetWorldCenter;
            clone.rotation = target.rotation;
            
            // Match the world scale
            Vector3 targetWorldScale = target.lossyScale;
            Vector3 cloneParentScale = transform.lossyScale;
            clone.localScale = new Vector3(
                targetWorldScale.x / cloneParentScale.x,
                targetWorldScale.y / cloneParentScale.y,
                targetWorldScale.z / cloneParentScale.z
            );
            
            Debug.Log($"TutorialButtonPopup: Cloned button at world position {targetWorldCenter} with size {targetSize}, anchors at center");

            // Setup click listener on clone
            var cloneUIButton = clone.GetComponent<UIButton>();
            if (cloneUIButton != null)
            {
                // Clear all existing listeners on clone button
                cloneUIButton.Button.onClick.RemoveAllListeners();
                // Add our click handler
                cloneUIButton.Button.onClick.AddListener(OnButtonClick);
            }

            // Set pointer position
            var targetUIButton = target.GetComponent<UIButton>();
            SetPointerPosition(targetUIButton.Button.targetGraphic != null ? targetUIButton.Button.targetGraphic.rectTransform : target, popupParam.pointerPosition);
            
            routineSetupButtonClone = null;
        }

        private void SetupMessage(int index)
        {
            clickAvailable = false;
            DOVirtual.DelayedCall(timeDelayClick, () => clickAvailable = true, false);
            if (index < 0 || index >= popupParam.messages.Length)
            {
                goMessage.SetActive(false);
                OnShowFinalMessage();
                return;
            }

            curMessageIndex = index;

            var message = popupParam.messages[index];
            if (string.IsNullOrWhiteSpace(message))
            {
                goMessage.SetActive(false);
                OnShowFinalMessage();
                return;
            }

            goMessage.SetActive(true);
            txtMessage.text = "";
            txtMessage.DOKill();
            DOTween.To(() => "", x => txtMessage.text = x, message, timeAnimText).SetEase(Ease.Linear).SetUpdate(true);
            messageTransform.SetParent(
                popupParam.messagePosition switch
                {
                    TutorialButtonPopupParam.MessagePosition.Top => messagePositionTop,
                    TutorialButtonPopupParam.MessagePosition.Center => messagePositionCenter,
                    TutorialButtonPopupParam.MessagePosition.Bottom => messagePositionBottom,
                    _ => throw new ArgumentOutOfRangeException(nameof(popupParam.messagePosition),
                        popupParam.messagePosition, null)
                }, false);
            messageTransform.localPosition = Vector3.zero;
            var npcData = nPCChatStatesData.Data.FirstOrDefault(n => n.npcId.Equals(popupParam.npcId, StringComparison.OrdinalIgnoreCase));
            // var npcData = ResourcesManager.Instance.GetNPCData(popupParam.npcId);
            imgNpc.sprite = npcData?.smallAvatar;
            imgNpc.gameObject.SetActive(imgNpc.sprite != null);

            if (index >= popupParam.messages.Length - 1)
            {
                OnShowFinalMessage();
            }
        }

        private void OnShowFinalMessage()
        {
            if (!string.IsNullOrWhiteSpace(popupParam.targetPresenterId) && routineSetupButtonClone == null)
            {
                routineSetupButtonClone = StartCoroutine(SetupButtonClone());
                messageSkipButton.gameObject.SetActive(false);
            }
        }
        private void CleanupClone()
        {
            if (clone != null)
            {
                Debug.Log("TutorialButtonPopup: Destroying clone");
                Destroy(clone.gameObject);
                clone = null;
            }
        }

        private void OnButtonClick()
        {
            // Invoke the original target button's onClick
            if (target != null)
            {
                var targetUIButton = target.GetComponent<UIButton>();
                if (targetUIButton != null)
                {
                    Debug.Log("TutorialButtonPopup: Invoking target button onClick");
                    targetUIButton.Button.onClick.Invoke();
                }
            }

            CleanupClone();
            ClosePopup();
        }        
        public void OnNextMessage()
        {
            if (!clickAvailable) return;
            if (curMessageIndex >= popupParam.messages.Length - 1)
            {
                if (string.IsNullOrWhiteSpace(popupParam.targetPresenterId))
                    OnButtonClick();
                return;
            }

            SetupMessage(curMessageIndex + 1);
        }

        private void SetPointerPosition(RectTransform target, TutorialButtonPopupParam.PointerPosition pointerPosition)
        {
            // Deactivate all pointers first
            pointerLeft.gameObject.SetActive(false);
            pointerRight.gameObject.SetActive(false);
            pointerTop.gameObject.SetActive(false);
            pointerBottom.gameObject.SetActive(false);

            LayoutRebuilder.ForceRebuildLayoutImmediate(target.transform.parent.GetComponent<RectTransform>());

            Transform pointer = null;
            var offset = Vector3.zero;
            var size = target.rect.size;
            var pivot = target.pivot;

            switch (pointerPosition)
            {
                case TutorialButtonPopupParam.PointerPosition.TopLeft:
                    pointer = pointerTop;
                    offset = new Vector3(-size.x / 2, size.y / 2, 0);
                    // Adjust for pivot
                    break;
                case TutorialButtonPopupParam.PointerPosition.TopRight:
                    pointer = pointerTop;
                    offset = new Vector3(size.x / 2, size.y / 2, 0);
                    break;
                case TutorialButtonPopupParam.PointerPosition.BottomLeft:
                    pointer = pointerBottom;
                    offset = new Vector3(-size.x / 2, -size.y / 2, 0);
                    break;
                case TutorialButtonPopupParam.PointerPosition.BottomRight:
                    pointer = pointerBottom;
                    offset = new Vector3(size.x / 2, -size.y / 2, 0);
                    break;
                case TutorialButtonPopupParam.PointerPosition.MiddleLeft:
                    pointer = pointerLeft;
                    offset = new Vector3(-size.x / 2, 0, 0);
                    break;
                case TutorialButtonPopupParam.PointerPosition.MiddleRight:
                    pointer = pointerRight;
                    offset = new Vector3(size.x / 2, 0, 0);
                    break;
                case TutorialButtonPopupParam.PointerPosition.MiddleTop:
                    pointer = pointerTop;
                    offset = new Vector3(0, size.y / 2, 0);
                    break;
                case TutorialButtonPopupParam.PointerPosition.MiddleBottom:
                    pointer = pointerBottom;
                    offset = new Vector3(0, -size.y / 2, 0);
                    break;
                case TutorialButtonPopupParam.PointerPosition.MiddleCenter:
                    pointer = pointerTop;
                    offset = Vector3.zero;
                    break;
            }

            offset.x -= size.x * (pivot.x - 0.5f);
            offset.y -= size.y * (pivot.y - 0.5f);

            if (pointer != null && target != null)
            {
                pointer.gameObject.SetActive(true);
                // Convert target local position to world, then to pointer's parent space
                var targetWorldPos = target.TransformPoint(offset);
                pointer.position = targetWorldPos;
            }
        }
    }
    [Serializable]
    public class TutorialButtonPopupParam : ScreenParam
    {
        [Serializable]
        public enum PointerPosition
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            MiddleLeft,
            MiddleRight,
            MiddleTop,
            MiddleBottom,
            MiddleCenter
        }

        [Serializable]
        public enum MessagePosition
        {
            Top,
            Center,
            Bottom
        }

        public string targetPresenterId;
        public string npcId;
        [FormerlySerializedAs("multiMessages")] public string[] messages;
        public PointerPosition pointerPosition;
        public MessagePosition messagePosition;
    }
}
