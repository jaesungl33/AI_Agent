using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardPopup : PopupBase
{
    [SerializeField] private List<RewardUIItem> rewardUIItems = new List<RewardUIItem>();
    [SerializeField] private Button collectButton;
    [SerializeField] private Transform rewardContainer;

    private RewardPopupParam currentParam;

    private void Start()
    {
        if (collectButton != null)
        {
            collectButton.onClick.AddListener(OnCollectButtonClicked);
        }
    }

    public override void Show(int additionalSortingOrder = 0, ScreenParam param = null)
    {
        base.Show(additionalSortingOrder);

        currentParam = param as RewardPopupParam;
        if (currentParam?.packReward?.rewards != null)
        {
            SetupRewardItems(currentParam.packReward.rewards);
        }

        // Auto collect after 3 seconds
        StartCoroutine(AutoCollectAfterDelay(3f));
    }
    
    private IEnumerator AutoCollectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnCollectButtonClicked();
    }

    private void SetupRewardItems(RewardData[] rewards)
    {
        // Ensure we have at least one preset RewardUIItem
        if (rewardUIItems.Count == 0)
        {
            Debug.LogError("RewardPopup: No preset RewardUIItem found in serialized list!");
            return;
        }

        // Show the first item and hide others initially
        for (int i = 0; i < rewardUIItems.Count; i++)
        {
            rewardUIItems[i].gameObject.SetActive(i == 0);
        }

        // Create additional items if needed by duplicating the first one
        while (rewardUIItems.Count < rewards.Length)
        {
            GameObject newRewardItem = Instantiate(rewardUIItems[0].gameObject, rewardContainer);
            RewardUIItem newRewardUIItem = newRewardItem.GetComponent<RewardUIItem>();
            rewardUIItems.Add(newRewardUIItem);
        }

        // Setup each reward item with data
        for (int i = 0; i < rewards.Length; i++)
        {
            rewardUIItems[i].gameObject.SetActive(true);
            rewardUIItems[i].SetRewardData(rewards[i]);
        }

        // Hide unused items
        for (int i = rewards.Length; i < rewardUIItems.Count; i++)
        {
            rewardUIItems[i].gameObject.SetActive(false);
        }

        // claim all rewards
        foreach (var reward in currentParam.packReward.rewards)
        {
            AddRewardToPlayer(reward);
        }
    }

    private void OnCollectButtonClicked()
    {
        if (currentParam?.packReward?.rewards == null) return;

        var playerCollection = DatabaseManager.GetDB<PlayerCollection>();
        var playerDocument = playerCollection.GetMine();

        if (playerDocument == null)
        {
            Debug.LogError("RewardPopup: Player document not found!");
            return;
        }
       
        // Pop this popup
        EventManager.TriggerEvent(new PopPopupEvent());
        currentParam.onClose?.Invoke();

        StopAllCoroutines();
    }

    private void AddRewardToPlayer(RewardData reward)
    {
        switch (reward.type)
        {
            case "Gold":
                InventoryHelper.RewardItem(InventoryItemType.Gold, reward.amount);
                break;

            case "Diamond":
                InventoryHelper.RewardItem(InventoryItemType.Diamond, reward.amount);
                break;

            case "Exp":
                InventoryHelper.RewardItem(InventoryItemType.EXP, reward.amount);
                break;

            default:
                Debug.LogWarning($"RewardPopup: Unknown reward type: {reward.type}");
                break;
        }
    }

    private void OnDestroy()
    {
        if (collectButton != null)
        {
            collectButton.onClick.RemoveListener(OnCollectButtonClicked);
        }
    }
}

public class RewardPopupParam : ScreenParam
{
    public PackRewardDocument packReward;
    public Action onClose;
}