using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GarageDecoScreen : UIScreenBase
{
    [SerializeField] private TMPro.TextMeshProUGUI tankNameText;
    [SerializeField] private TankPreviewComp tankPreviewComp;
    [SerializeField] private Transform decoListParent;
    [SerializeField] private GarageDecoTankItem garageDecoTankItemPrefab;
    [SerializeField] private GarageDecoTankItem[] garageDecoItems;
    [SerializeField] private CurrencyUI[] currencyUIs;
    private GameAssetCollection gameAssetCollection;
    private GarageDecoScreenParam param;

    TankCollection tankCollection;
    int curWrapId;

    public override void Show(int additionalSortingOrder = 0, ScreenParam param = null)
    {
        base.Show(additionalSortingOrder, param);
        this.param = param as GarageDecoScreenParam;

        tankCollection = DatabaseManager.GetDB<TankCollection>();

        UpdateCurrencyUI();
        ShowDecoGroup(InventoryItemType.Wrap);

        if (this.param?.tankId != null)
        {
            string tankID = this.param.tankId;

            tankPreviewComp.ShowTankPreview(tankID);

            var tankDoc = tankCollection.GetTankByID(tankID);
            tankNameText.text = LocalizationHelper.GetString(nameof(LocKeys.Tank), tankID + ".name");

            int wrapId = PlayerDocument.GetMineWrapId(tankID);
            tankPreviewComp.ChangeWrap(wrapId, tankID);
        }
        else
        {
            tankPreviewComp.HideTankPreview();
            tankNameText.text = string.Empty;
        }
    }
    public override void Hide()
    {
        tankPreviewComp.HideTankPreview();
        base.Hide();
    }

    public void ShowGroupByInt(int inventoryItemTypeInt)
    {
        if (IsVisible == false) return;

        InventoryItemType inventoryItemType = (InventoryItemType)inventoryItemTypeInt;
        ShowDecoGroup(inventoryItemType);
    }

    public void ShowDecoGroup(InventoryItemType inventoryItemType)
    {
        var decorations = DatabaseManager.GetDB<TankWrapCollection>();

        // Lọc các decor hợp lệ
        var validDecors = decorations.documents
            .Where(decor => decor.itemType.ToString() == inventoryItemType.ToString())
            .Select(decor => new
            {
                decor,
                wrapDecal = decorations.GetByCatalogId(decor.itemCatalogId)?
                    .wrapDecalStickerData?.FirstOrDefault(w => w.tankId == this.param?.tankId)
            })
            .Where(x => x.wrapDecal != null)
            .ToList();

        // Clear các item cũ
        for (int j = 0; j < garageDecoItems.Length; j++)
        {
            if (garageDecoItems[j] != null)
                Destroy(garageDecoItems[j].gameObject);
        }

        // Khởi tạo mảng đúng số lượng
        garageDecoItems = new GarageDecoTankItem[validDecors.Count];

        // Khởi tạo các item mới
        for (int i = 0; i < validDecors.Count; i++)
        {
            var decor = validDecors[i].decor;
            Debug.Log($"ShowDecoList Showing decoration in garage: {decor}");
            garageDecoItems[i] = Instantiate(garageDecoTankItemPrefab, decoListParent);
            garageDecoItems[i].Init(decor.itemCatalogId, null, GarageTankState.Unlocked);
            garageDecoItems[i].SetCallback(ApplyDecoration);
            garageDecoItems[i].decorationType = inventoryItemType;
            garageDecoItems[i].gameObject.SetActive(true);
        }

        SetDefaultSelectedTank();
        garageDecoTankItemPrefab.gameObject.SetActive(false);
    }

    public void ApplyDecoration(GarageDecoTankItem item)
    {
        Debug.Log($"ApplyDecoration selected decoration: {item.DecoID}");

        //show highlight on selected tank
        foreach (var decoItem in garageDecoItems)
        {
            Debug.Log($"ApplyDecoration checking decoItem: {decoItem.DecoID} against selected decoID: {item.DecoID}");
            decoItem.Highlight(decoItem == item);
        }

        switch (item.decorationType)
        {
            case InventoryItemType.Wrap:
                curWrapId = PlayerDocument.GetMineWrapId(this.param.tankId);
                var docs = DatabaseManager.GetDB<TankWrapCollection>().GetByCatalogId(item.DecoID);
                //add docs.itemId to wrapDecalStickerIds if not existing
                if (docs != null)
                {
                    curWrapId = docs.itemId;
                }
                Debug.Log(string.Format("ApplyDecoration {0}", curWrapId));
                tankPreviewComp.ChangeWrap(curWrapId, this.param.tankId);
                break;
            //case InventoryItemType.Decal:
            //    ApplyDecalDecoration(item);
            //    break;
            default:
                Debug.LogWarning($"ApplyDecoration unknown decoration type: {item.decorationType}");
                break;
        }
    }

    private void SetDefaultSelectedTank()
    {
        // string selectedDecoID = InventoryHelper.GetSelectedDecoration();
        // foreach (var decoItem in garageDecoItems)
        // {
        //     Debug.Log($"SetDefaultSelectedTank checking decoItem: {decoItem.DecoID} against selectedDecoID: {selectedDecoID}");
        //     if (decoItem.DecoID == selectedDecoID)
        //     {
        //         ApplyDecoration(decoItem);
        //         break;
        //     }
        // }
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

    public void SaveDecor()
    {
        var playerDocument = DatabaseManager.GetDB<PlayerCollection>().GetMine();
        var tankId = this.param.tankId;
        bool updated = false;

        // Tìm và cập nhật nếu đã có formationTank cho tankId
        for (int i = 0; i < playerDocument.formationTanks.Count; i++)
        {
            var tank = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerDocument.FormationTanks>(playerDocument.formationTanks[i]);
            if (tank != null && tank.tankID == tankId)
            {
                tank.wrapId = curWrapId;
                playerDocument.formationTanks[i] = Newtonsoft.Json.JsonConvert.SerializeObject(tank);
                updated = true;
                Debug.Log($"[SaveDecor] Updated wrap for tankId: {tankId}, wrapIds: {string.Join(",", curWrapId)}");
                break;
            }
        }

        // Nếu chưa có thì thêm mới
        if (!updated)
        {
            var newFormationTank = new PlayerDocument.FormationTanks(tankId, curWrapId);
            playerDocument.formationTanks.Add(newFormationTank.ToJson());
            Debug.Log($"[SaveDecor] Added new formationTank for tankId: {tankId}, wrapIds: {string.Join(",", curWrapId)}");
        }

        _ = DatabaseManager.GetDB<PlayerCollection>().UpdateDocumentAsync(playerDocument);
    }
}

public class GarageDecoScreenParam : ScreenParam
{
    public string tankId;
    // Có thể thêm các thuộc tính khác nếu cần
    //contructor
    public GarageDecoScreenParam(string tankId = null)
    {
        this.tankId = tankId;
    }
}
