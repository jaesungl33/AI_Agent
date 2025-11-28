using Fusion.GameSystems;
using Fusion.TankOnlineModule;
using UnityEngine;
using UnityEngine.UI;

public class ChooseTankItem : MonoBehaviour
{
    [SerializeField] private Image tankImage, bgImage, tankClassImage, tankBgClassImage;
    [SerializeField] private Sprite[] buttonSprites; // 0=normal, 1=highlight
    [SerializeField] private Sprite[] tankClassSprites; // 0=scout, 1=assault, 2=heavy
    [SerializeField] private Button button;

    [SerializeField] private string tankId;
    [SerializeField] private ServerState serverState;
    private UnityEngine.Events.UnityAction<string> onSelectTank;
    public string TankId => tankId;

    private void Awake()
    {
        button.onClick.AddListener(OnButtonSelect);
    }

    public void OnButtonSelect()
    {
        if (serverState != ServerState.PICKING) return;

        InventoryHelper.SetSelectedTank(tankId);
        PlayerDocument playerDocument = DatabaseManager.GetDB<PlayerCollection>().GetMine();
        int wrapId = playerDocument.GetWrapId(tankId);
        GameServer.Instance.MyTank.RPC_ChooseTank(tankId, wrapId);
        onSelectTank?.Invoke(tankId);
    }

    public void Highlight(bool highlight)
    {
        bgImage.sprite = highlight ? buttonSprites[1] : buttonSprites[0];
    }

    public void EnableSelection(ServerState serverState)
    {
        Debug.Log($"[ChooseTankItem] EnableSelection: {serverState}");
        this.serverState = serverState;
        button.interactable = this.serverState == ServerState.PICKING;
    }

    public void Init(string tankId, TankType tankType, Sprite tankIcon, Sprite classIcon, ServerState curState)
    {
        serverState = curState;
        this.tankId = tankId;
        tankImage.sprite = tankIcon;
        bgImage.sprite = buttonSprites[0];
        tankClassImage.sprite = classIcon;
        tankBgClassImage.sprite = tankClassSprites[(int)tankType - 1];
        gameObject.SetActive(true);
        gameObject.name = $"{tankId}";
    }

    public void SetCallback(UnityEngine.Events.UnityAction<string> callback)
    {
        onSelectTank = callback;
    }
}
