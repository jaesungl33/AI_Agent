using Fusion.GameSystems;
using Fusion.TankOnlineModule;
using UnityEngine;
using UnityEngine.UI;

public class GarageTankItem : MonoBehaviour
{
    [SerializeField] private Image tankImage, tankClassImage, bgImage;
    [SerializeField] private GameObject selectedImage;
    [SerializeField] private Sprite[] tankStateSprites; // 0=locked, 1=unlocked
    [SerializeField] private Sprite[] tankClassSprites; // 0=scout, 1=assault, 2=heavy
    [SerializeField] private Button button;

    [SerializeField] private string tankId;
    [SerializeField] private GarageTankState state;
    private UnityEngine.Events.UnityAction<GarageTankItem> onSelectTank;
    public string TankId => tankId;

    private void Awake()
    {
        button.onClick.AddListener(OnButtonSelect);
    }

    private void OnButtonSelect()
    {
        // if (state != GarageTankState.Unlocked)
        // {
        //     //show dialog to inform tank is locked
        //     return;
        // }
        onSelectTank?.Invoke(this);
    }

    public void Highlight(bool highlight)
    {
        selectedImage.SetActive(highlight);
    }

    public void EnableSelection(GarageTankState garageTankState)
    {
        this.state = garageTankState;
        // button.interactable = this.state == GarageTankState.Unlocked;
        bgImage.sprite = tankStateSprites[(int)this.state];
    }

    public void Init(string tankId, TankType tankType, Sprite tankIcon, GarageTankState curState)
    {
        state = curState;
        this.tankId = tankId;
        tankImage.sprite = tankIcon;
        tankClassImage.sprite = tankClassSprites[(int)tankType - 1];
    }

    public void SetCallback(UnityEngine.Events.UnityAction<GarageTankItem> callback)
    {
        onSelectTank = callback;
    }
}
