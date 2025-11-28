using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using Fusion;

public class ChooseTankPlayerCard : MonoBehaviour
{
    public bool isEmpty = false;
    [SerializeField] private TextMeshProUGUI playerNameText, tankNameText;
    [SerializeField] private Image tankClassIcon, tankBgClassIcon;
    [SerializeField] private Image tankIcon;
    [SerializeField] private TankPreviewComp tankPreviewComp;
    [SerializeField] private Sprite[] tankClassSprites; // 0=scout, 1=assault, 2=heavy, 3=outpost
    [SerializeField] private Sprite defaultTankSprite;
    public ChooseTankBaseInfo tankInfo { get; private set; }

    public void UpdateSelectedTank(string tankId)
    {
        GameAssetCollection gameAssetCollection = DatabaseManager.GetDB<GameAssetCollection>();
        gameAssetCollection.GetAssets(tankId, out Sprite mainIcon, out Sprite previewIcon, out Sprite classIcon);
        TankDocument tankDocument = DatabaseManager.GetDB<TankCollection>().GetTankByID(tankId);
        SetPlayerInfo(playerNameText.text, tankNameText.text, mainIcon, classIcon, tankClassSprites[(int)tankDocument.tankType - 1]);
    }

    public void SetPlayerInfo(string playerName = "", string tankName = "", Sprite tankIcon = null, Sprite classIcon = null, Sprite bgClassIcon = null)
    {
        // if (this.tankIcon != null)
        // {
        //     this.tankIcon.gameObject.SetActive(tankIcon != null);
        //     this.tankIcon.sprite = tankIcon == null ? defaultTankSprite : tankIcon;
        //     this.tankIcon.preserveAspect = true;
        // }

        if (playerNameText != null)
        {
            playerNameText.text = playerName;
        }

        if (tankNameText != null)
        {
            tankNameText.text = tankName ?? "Unknown";
        }

        // if (tankClassIcon != null)
        // {
        //     tankClassIcon.sprite = classIcon == null ? defaultTankSprite : classIcon;
        // }

        if (tankBgClassIcon != null)
        {
            tankBgClassIcon.gameObject.SetActive(bgClassIcon != null);
            tankBgClassIcon.sprite = bgClassIcon == null ? defaultTankSprite : bgClassIcon;
        }
    }

    public void SetTankPreviewActive(bool isActive, string tankId = null, int wrapId = 0)
    {
        if (tankPreviewComp != null)
        {
            tankPreviewComp.gameObject.SetActive(isActive);
        }
        if (isActive && tankPreviewComp != null)
        {
            tankPreviewComp.ShowTankPreview(tankId);
            tankPreviewComp.ChangeWrap(wrapId, tankId);
        }
    }

    public void SetInfo(ChooseTankBaseInfo info)
    {
        Debug.Log($"[SetInfo] PlayerName: {info.PlayerName}, TankId: {info.TankId}, PlayerIndex: {info.PlayerIndex}");
        tankInfo = info;

        TankDocument tankDocument = DatabaseManager.GetDB<TankCollection>().GetTankByID(tankInfo.TankId.ToString());
        GameAssetCollection gameAssetCollection = DatabaseManager.GetDB<GameAssetCollection>();
        var gameAssetDocument = gameAssetCollection.GetGameAssetDocumentById(tankDocument.tankId);
        Sprite tankIcon = null, classIcon = null;
        if (gameAssetDocument?.sprites != null)
        {
            var iconAsset = gameAssetDocument.sprites.FirstOrDefault(asset =>
            asset.type == AssetType.MainIcon);
            tankIcon = iconAsset?.asset as Sprite;

            classIcon = gameAssetDocument.sprites.FirstOrDefault(asset =>
                asset.type == AssetType.ClassIcon)?.asset as Sprite;
        }
        string tankName = LocalizationHelper.GetString(nameof(LocKeys.Tank), tankDocument.tankId + ".name");
        SetPlayerInfo(tankInfo.PlayerName.ToString(), tankName, tankIcon, classIcon, tankClassSprites[(int)tankDocument.tankType - 1]);

        SOMatchData sOMatchData = DatabaseManager.GetDB<SOMatchData>(); ;
        var matchPlayer = sOMatchData.MatchDataArray.FirstOrDefault(p => p.PlayerId == info.PlayerId);
        if (matchPlayer != null)
            SetTankPreviewActive(true, info.TankId.ToString(), wrapId: matchPlayer.WrapId);
    }

    public void ClearInfo()
    {
        SetPlayerInfo();

        // Hide the card or make it inactive
        // if (tankIcon != null)
        //     tankIcon.gameObject.SetActive(false);
        if (tankPreviewComp != null)
            tankPreviewComp.HideTankPreview();
    }
}
