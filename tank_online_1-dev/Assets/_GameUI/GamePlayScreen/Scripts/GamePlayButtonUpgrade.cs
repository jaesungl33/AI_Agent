using System;
using Fusion.GameSystems;
using Fusion.TankOnlineModule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayButtonUpgrade : MonoBehaviour
{
    [SerializeField] private UpgradeType upgradeType;
    [SerializeField] private Image icon;
    [SerializeField] private Image stackIcon;
    [SerializeField] private TMP_Text txtCurUpgrade;
    [SerializeField] private TMP_Text txtUpgradeStats;
    [SerializeField] private TMP_Text txtUpgradeCost;
    [SerializeField] private Button btnSelect;
    public bool IsHidden { get; set; } = false;
    private TankDocument _curTankDocument;

    private Player player;
    private TankUpgradeStats _data;
    private int upgradeCount;

    private void Awake()
    {
        btnSelect.onClick.AddListener(OnButtonUpgrade);
    }

    public void DefineTankUpgrades()
    {
        _curTankDocument = DatabaseManager.GetDB<TankCollection>().GetActiveTank();
        if (_curTankDocument != null && !string.IsNullOrWhiteSpace(_curTankDocument.tankId))
        {
            _data = DatabaseManager.GetDB<TankUpgradeCollection>().GetDefinition(_curTankDocument.tankId)
                .GetStat(upgradeType);
        }
    }

    public void Enable(bool status)
    {
        IsHidden = !status;
    }

    public void EnableSelectButton(bool status)
    {
        btnSelect.interactable = status & !IsHidden;
        btnSelect.image.raycastTarget = status & !IsHidden;
    }

    private void OnEnable()
    {
       EventManager.Register<GameServer.GoldEvent>(OnGoldEvent);
    }

    private void OnDisable()
    {
        if (!GameManager.IsApplicationQuitting)
            EventManager.Unregister<GameServer.GoldEvent>(OnGoldEvent);
    }

    private void OnGoldEvent(GameServer.GoldEvent obj)
    {
        Refresh();
    }

    private void OnButtonUpgrade()
    {
        if (upgradeCount >= _data.upgradeValues.Length) return;    //max upgrades

        EventManager.TriggerEvent(new GameServer.UpgradeEvent() { UpgradeType = _data.type });
    }

    private void UpdateText()
    {
        if (upgradeCount >= _data.upgradeValues.Length)
        {
            txtUpgradeCost.text = "";
            txtUpgradeStats.text = "MAX";
        }
        else
        {
            string typeName = LocalizationHelper.GetString(nameof(LocKeys.UI_GamePlay), LocKeys.UI_GamePlay.GetUpgradeNameType(_data.type));
            txtUpgradeCost.text = _data.baseCost.ToString();
            txtUpgradeStats.text = $"+{_data.upgradeValues[upgradeCount]:#0.#}% " + typeName;
        }
    }

    public void Refresh()
    {
        if (_data == null) 
            DefineTankUpgrades();
        
        player = GameServer.Instance.MyTank;
        if (player == null) return;
        if (_data == null)
        {
            Debug.LogError("GamePlayButtonUpgrade|Refresh|_data is null");
            return;
        } 
        if (_data.upgradeValues == null)
        {
            Debug.LogError("GamePlayButtonUpgrade|Refresh|_data.upgradeValues is null");
            return;
        }

        upgradeCount = player.StatsLevel[(int)upgradeType];
        txtCurUpgrade.text = upgradeCount.ToString();
        UpdateStackIcon();
        if (upgradeCount >= _data.upgradeValues.Length)
        {
            UpdateText();
            txtUpgradeCost.text = "";
            EnableSelectButton(false);
            return;
        }
        EnableSelectButton(true);
        UpdateText();
    }

    private void UpdateStackIcon()
    {
        if (_data == null || stackIcon == null)
            return;
        int maxStack = _data.upgradeValues.Length;
        int currentStack = upgradeCount;
        stackIcon.gameObject.SetActive(currentStack > 0 && maxStack > 0);
        if(!stackIcon.gameObject.activeSelf) return;


        stackIcon.fillAmount = Mathf.Clamp01((float)currentStack / maxStack);
    }
}