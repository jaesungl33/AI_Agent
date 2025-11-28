using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : Singleton<IAPManager>, IDetailedStoreListener
{
    private IStoreController storeController;
    private IExtensionProvider storeExtensionProvider;

    public ProductCollection productCollection;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        InitializePurchasing();
    }

    public void InitializePurchasing()
    {
        if (IsInitialized()) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        productCollection = CollectionExtensions.GetCollection<ProductCollection>();
        foreach (var product in productCollection.GetAllDocuments())
        {
            builder.AddProduct(product.productID, product.productType);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }

    public void BuyProduct(string productID)
    {
        if (IsInitialized())
        {
            Product product = storeController.products.WithID(productID);
            if (product != null && product.availableToPurchase)
            {
                storeController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log("BuyProduct: Product not found or unavailable.");
            }
        }
        else
        {
            Debug.Log("BuyProduct: IAP not initialized.");
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;
        Debug.Log("IAP initialized successfully.");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"IAP Initialization failed: {error}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string purchasedID = args.purchasedProduct.definition.id;
        Debug.Log($"Purchase successful: {purchasedID}");

        ProductDocument purchasedProduct = productCollection.GetAllDocuments().Find(p => p.productID == purchasedID);

        if (purchasedProduct != null)
        {
            GrantProduct(purchasedProduct);
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"Purchase failed: {product.definition.id}, Reason: {failureReason}");
    }

    void GrantProduct(ProductDocument product)
    {
        switch (product.category)
        {
            case ProductCategory.Tank:
                UnlockTank(product.productID);
                break;
            case ProductCategory.Currency:
                AddCurrency(product.value);
                break;
            case ProductCategory.NoAds:
                UnlockNoAds();
                break;
            case ProductCategory.Subscription:
                ActivateSubscription(product.productID);
                break;
            case ProductCategory.Bundle:
                GrantBundle(product.productID);
                break;
        }
    }

    void UnlockTank(string tankID)
    {
        Debug.Log($"Unlocked tank: {tankID}");
        // Implement unlock logic
    }

    void AddCurrency(int amount)
    {
        Debug.Log($"Added currency: {amount}");
        // Implement add currency logic
    }

    void UnlockNoAds()
    {
        Debug.Log("No Ads purchased.");
        // Implement no ads unlock logic
    }

    void ActivateSubscription(string subscriptionID)
    {
        Debug.Log($"Activated subscription: {subscriptionID}");
        // Implement subscription logic
    }

    void GrantBundle(string bundleID)
    {
        Debug.Log($"Granted bundle: {bundleID}");
        // Implement bundle logic
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        throw new System.NotImplementedException();
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        throw new System.NotImplementedException();
    }
}
