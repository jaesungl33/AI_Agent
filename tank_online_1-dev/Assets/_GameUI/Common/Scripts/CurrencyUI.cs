using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    public InventoryItemType currencyType;
    [SerializeField] private TMPro.TextMeshProUGUI currencyAmountText;

    public void UpdateCurrencyAmount(int amount)
    {
        currencyAmountText.text = amount.ToString();
    }

    public void BuyItem()
    {
        // move to shop screen
        EventManager.TriggerEvent<UIIDs>(UIIDs.Shop);
    }
}
