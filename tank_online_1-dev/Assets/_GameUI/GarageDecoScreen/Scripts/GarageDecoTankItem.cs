using Fusion.GameSystems;
using Fusion.TankOnlineModule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GarageDecoTankItem : MonoBehaviour
{
    public InventoryItemType decorationType;
    [SerializeField] private Image textureImage, priceImage;
    [SerializeField] private TextMeshProUGUI titleText, priceText;
    [SerializeField] private GameObject selectedImage;
    [SerializeField] private Button button;

    [SerializeField] private string decoID;
    [SerializeField] private GarageTankState state;
    private UnityEngine.Events.UnityAction<GarageDecoTankItem> onSelectTank;
    public string DecoID => decoID;

    private void Awake()
    {
        button.onClick.AddListener(OnButtonSelect);
    }

    private void OnButtonSelect()
    {
        Debug.Log($"OnButtonSelect clicked decoration: {decoID} with state: {state}");
        if (state != GarageTankState.Unlocked)
        {
            //show dialog to inform tank is locked
            return;
        }

        onSelectTank?.Invoke(this);
    }

    public void Highlight(bool highlight)
    {
        selectedImage.SetActive(highlight);
    }

    public void EnableSelection(GarageTankState garageTankState)
    {
        this.state = garageTankState;
        button.interactable = this.state == GarageTankState.Unlocked;
    }

    public void Init(string decoID, Sprite sprite, GarageTankState curState)
    {
        state = curState;
        this.decoID = decoID;
        //textureImage.sprite = sprite;
        if (textureImage) textureImage.sprite = null;

        var tankWrapDecalStickerCollection = DatabaseManager.GetDB<TankWrapCollection>();
        var doc = tankWrapDecalStickerCollection.GetByCatalogId(decoID);
        if (doc == null)
        {
            Debug.LogError($"[GarageDecoTankItem] Không tìm thấy decoID: {decoID} trong database TankWrapCollection");
            if (textureImage)
                textureImage.sprite = null;
            return;
        }
        titleText.text = doc.itemTitle;
        TextureDownloader.Instance.LoadImage(doc.iconPath, textureImage.gameObject, onComplete: (downloadedIconTexture) =>
        {
            if (downloadedIconTexture != null)
            {
                textureImage.sprite = Sprite.Create((Texture2D)downloadedIconTexture, new Rect(0, 0, downloadedIconTexture.width, downloadedIconTexture.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                TextureDownloader.Instance.LoadImage(doc.texturePath, textureImage.gameObject, onComplete: (downloadedTexture) =>
                {
                    if (downloadedTexture != null)
                    {
                        textureImage.sprite = Sprite.Create((Texture2D)downloadedTexture, new Rect(0, 0, downloadedTexture.width, downloadedTexture.height), new Vector2(0.5f, 0.5f));
                    }
                });
            }
        });
    }
    public void SetCallback(UnityEngine.Events.UnityAction<GarageDecoTankItem> callback)
    {
        onSelectTank = callback;
    }
}
