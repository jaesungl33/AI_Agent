using UnityEngine;
using UnityEngine.UI;

public class GarageScreen : UIScreenBase
{
    [SerializeField] private GarageTankItem garageTankItemPrefab;
    [SerializeField] private Transform tankListParent;
    [SerializeField] private GarageTankItem[] garageTankItems;
    [SerializeField] private GarageArtifactItem[] garageArtifactItems;
    [SerializeField] private GarageAbilityItem[] garageAbilityItems;
    [SerializeField] private GarageStatItem[] garageStatItems;
    [SerializeField] private CurrencyUI[] currencyUIs;
    [SerializeField] private TMPro.TextMeshProUGUI tankNameText, classNameText;
    [SerializeField] private Image tankImage, tankClassImage;
    [SerializeField] private TankPreviewComp tankPreviewComp;
    [SerializeField] private Button setLobbyButton, modifyDecoButton;
    [SerializeField] private Button buyTankButton;
    private GarageTankItem currentSelectedTankItem;
    private int amountDemoToBuy = 15000;

    TankCollection tankCollection;
    GameAssetCollection gameAssetCollection;

    protected override void Awake()
    {
        base.Awake();
        setLobbyButton.onClick.AddListener(OnSetLobby);
        buyTankButton.onClick.AddListener(OnBuyTank);
    }

    public override void Show(int additionalSortingOrder = 0, ScreenParam param = null)
    {
        base.Show(additionalSortingOrder, param);
        tankCollection = DatabaseManager.GetDB<TankCollection>();
        gameAssetCollection = DatabaseManager.GetDB<GameAssetCollection>();

        UpdateCurrencyUI();
        ShowTankList();
    }
    public override void Hide()
    {
        base.Hide();
        tankPreviewComp.HideTankPreview();
    }
    public void ShowTankList()
    {
        PlayerDocument playerDocument = DatabaseManager.GetDB<PlayerCollection>().GetMine();
        garageTankItemPrefab.gameObject.SetActive(true);

        // Show the list of tanks
        var tanks = InventoryHelper.GetAllTanksInGame();
        Debug.Log($"ShowTankList Owned tanks count: {tanks.Count}");
        // clear existing items
        for (int j = 0; j < garageTankItems.Length; j++)
        {
            Destroy(garageTankItems[j].gameObject);
        }

        garageTankItems = new GarageTankItem[tanks.Count];
        int i = 0;
        foreach (var tankID in tanks)
        {
            Debug.Log($"ShowTankList Showing tank in garage: {tankID}");
            gameAssetCollection.GetAssets(tankID, out Sprite tankIcon, out Sprite previewIcon, out Sprite tankClassIcon);
            var tankDoc = tankCollection.GetTankByID(tankID);
            garageTankItems[i] = Instantiate(garageTankItemPrefab, tankListParent);
            garageTankItems[i].Init(tankID, tankDoc.tankType, previewIcon, GarageTankState.Unlocked);
            garageTankItems[i].SetCallback(ShowSelectedTank);
            garageTankItems[i].EnableSelection(InventoryHelper.IsTankOwned(tankID) ? GarageTankState.Unlocked : GarageTankState.Locked);
            i++;
        }
        SetDefaultSelectedTank();
        garageTankItemPrefab.gameObject.SetActive(false);
    }

    public void ShowSelectedTank(GarageTankItem item)
    {
        setLobbyButton.interactable = item.TankId != InventoryHelper.GetSelectedTank();
        currentSelectedTankItem = item;
        TankDocument tankDocument = tankCollection.GetTankByID(item.TankId);
        // Show the selected tank's details
        gameAssetCollection.GetAssets(item.TankId, out Sprite tankIcon, out Sprite previewIcon, out Sprite tankClassIcon);
        ShowBuyButton(item.TankId);
        ShowBaseInfo(item.TankId, tankDocument.tankName, tankDocument.tankType.ToString(), tankIcon, tankClassIcon);
        ShowStats(item.TankId);
        ShowAbilities(tankDocument.abilityIDs[0]);
        ShowArtifacts();
        //show highlight on selected tank
        foreach (var tankItem in garageTankItems)
        {
            tankItem.Highlight(tankItem == item);
        }
    }

    private void SetDefaultSelectedTank()
    {
        string selectedTankID = InventoryHelper.GetSelectedTank();
        foreach (var tankItem in garageTankItems)
        {
            Debug.Log($"SetDefaultSelectedTank checking tankItem: {tankItem.TankId} against selectedTankID: {selectedTankID}");
            if (tankItem.TankId == selectedTankID)
            {
                ShowSelectedTank(tankItem);
                break;
            }
        }
    }

    private void ShowTank(string tankId)
    {
        foreach (var tankItem in garageTankItems)
        {
            Debug.Log($"ShowTank checking tankItem: {tankItem.TankId} against tankId: {tankId}");
            if (tankItem.TankId == tankId)
            {
                ShowSelectedTank(tankItem);
                break;
            }
        }
    }

    private void ShowBuyButton(string tankId)
    {
        bool isOwned = InventoryHelper.IsTankOwned(tankId);
        buyTankButton.gameObject.SetActive(!isOwned);
        setLobbyButton.gameObject.SetActive(isOwned);
        modifyDecoButton.gameObject.SetActive(isOwned);
    }

    private void ShowArtifacts()
    {
        // later
    }

    private void ShowAbilities(string abilityID)
    {
        TankAbilityDocument tankAbilityDocument = DatabaseManager.GetDB<TankAbilityCollection>().GetByAbilityId(abilityID);
        gameAssetCollection.GetAssets(abilityID, out Sprite mainAsset, out Sprite previewAsset, out Sprite classAsset);
        // Show the tank's abilities
        foreach (var ability in garageAbilityItems)
        {
            ability.gameObject.SetActive(true);
            string des = LocalizationHelper.GetString(nameof(LocKeys.Tank), tankAbilityDocument.abilityID + ".description");
            ability.Init(des, mainAsset);
        }
    }

    private void ShowStats(string tankId)
    {
        MatchPlayerData matchPlayer = DatabaseManager.CreateMatchPlayer(tankId);
        Debug.Log($"ShowStats for tankId: {tankId} with data: {JsonUtility.ToJson(matchPlayer)}");
        foreach (var statItem in garageStatItems)
        {
            // Update each stat item with the corresponding value from the player document
            switch (statItem.statType)
            {
                case StatType.MovementSpeed:
                    statItem.Init(matchPlayer.MaxSpeed / 10, matchPlayer.MaxSpeed);
                    break;

                case StatType.FireRate:
                    statItem.Init(matchPlayer.FireRate / 3, matchPlayer.FireRate);
                    break;

                case StatType.Damage:
                    statItem.InitDmg((float) matchPlayer.Damage[1]/200, matchPlayer.Damage);
                    break;
            }
        }
    }

    private void ShowBaseInfo(string tankId, string tankName, string className, Sprite tankIcon, Sprite tankClassIcon)
    {
        tankNameText.text = LocalizationHelper.GetString(nameof(LocKeys.Tank), tankId + ".name");
        classNameText.text = LocalizationHelper.GetString(nameof(LocKeys.Tank), className.ToLower());
        //tankImage.sprite = tankIcon;
        tankClassImage.sprite = tankClassIcon;
        currentSelectedTankItem.EnableSelection(InventoryHelper.IsTankOwned(currentSelectedTankItem.TankId) ? GarageTankState.Unlocked : GarageTankState.Locked);
        tankPreviewComp.HideTankPreview();
        tankPreviewComp.ShowTankPreview(tankId);

        int wrapId = PlayerDocument.GetMineWrapId(tankId);
        tankPreviewComp.ChangeWrap(wrapId, tankId);
    }

    private void UpdateCurrencyUI()
    {
        foreach (CurrencyUI currencyUI in currencyUIs)
        {
            switch (currencyUI.currencyType)
            {
                case InventoryItemType.Gold:
                    currencyUI.UpdateCurrencyAmount(InventoryHelper.GetGold());
                    break;
                case InventoryItemType.Diamond:
                    currencyUI.UpdateCurrencyAmount(InventoryHelper.GetDiamond());
                    break;
            }
        }
    }

    private void RefreshUIAfterPurchase()
    {
        UpdateCurrencyUI();
        ShowTank(currentSelectedTankItem.TankId);
    }

    private void OnBuyTank()
    {
        InventoryHelper.BuyTank(InventoryItemType.Gold, amountDemoToBuy, currentSelectedTankItem.TankId);
        // Refresh UI
        RefreshUIAfterPurchase();
    }

    public void OnSetLobby()
    {
        InventoryHelper.SetSelectedTank(currentSelectedTankItem.TankId);
        setLobbyButton.interactable = false;
    }
    public void ShowGarageDecoScreen()
    {
        EventManager.Instance.Invoke<UIEvent>(new UIEvent(UIIDs.GarageDeco, new GarageDecoScreenParam(currentSelectedTankItem.TankId)));
    }

    private string GetStringByKey(string key)
    {
        return LocalizationHelper.GetString(nameof(LocKeys.UI_Garage), key);
    }

}