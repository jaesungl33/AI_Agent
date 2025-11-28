using UnityEngine;
using UnityEngine.Purchasing;

[System.Serializable]
public class ProductDocument
{
    public string productID;//unique identifier for the product
    public string assetID; //optional, used to link to a GameAssetDocument
    public string displayName;
    public ProductType productType; // Unity IAP ProductType: Consumable, NonConsumable, Subscription
    public ProductCategory category;
    public int value; // e.g. amount of currency or stat value
    public int price; // optional if needed for display
    public int discountPrice; // optional, used for discounts
}
