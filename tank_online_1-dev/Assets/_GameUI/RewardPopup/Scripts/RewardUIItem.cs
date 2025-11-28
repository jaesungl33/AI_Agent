using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardUIItem : MonoBehaviour
{
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TextMeshProUGUI rewardAmountText;
    
    public void SetRewardData(RewardData rewardData)
    {
        if (rewardData == null) return;
        
        // Set the reward amount
        if (rewardAmountText != null)
        {
            rewardAmountText.text = rewardData.amount.ToString();
        }
        
        // Get and set the icon from GameAssetCollection
        if (rewardIcon != null)
        {
            var gameAssetCollection = DatabaseManager.GetDB<GameAssetCollection>();
            var assetDocument = gameAssetCollection?.GetGameAssetDocumentById(rewardData.type);
            
            if (assetDocument?.icon != null)
            {
                rewardIcon.sprite = assetDocument.icon;
            }
            else
            {
                Debug.LogWarning($"No icon found for reward type: {rewardData.type}");
            }
        }
    }
}
