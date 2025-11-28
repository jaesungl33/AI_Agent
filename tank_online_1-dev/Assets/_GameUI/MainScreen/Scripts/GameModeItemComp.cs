using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
public class GameModeItemComp : MonoBehaviour
{
    [SerializeField] private TMP_Text txtModeName;
    [SerializeField] private TMP_Text txtModeDesc;
    [SerializeField] private TMP_Text txtComingSoon;

    [SerializeField] private Image imgModeIcon;
    [SerializeField] private Image selectedFrame;
    [SerializeField] private Image selectedIcon;
    [SerializeField] private Image nonSelectedFrame;
    [SerializeField] private Button btnSelectMode;
    [SerializeField] private Sprite[] modeIcons; // Assume we have different icons for different modes

    private GameModeItemData modeData;
    public GameModeItemData ModeData => modeData;
    public MatchMode GetMatchMode()
    {
        return modeData.MatchMode;
    }

    public MatchType GetMatchType()
    {
        // For simplicity, we assume all modes here are of type Normal
        return modeData.MatchType;
    }
    
    public void Init(GameModeItemData data, Action<bool> onSelectMode)
    {
        this.gameObject.SetActive(true);
        modeData = data;
        txtModeName.text = modeData.ModeName;
        txtModeDesc.text = modeData.ModeDesc;
        Sprite iconSprite = null;
        switch (modeData.MatchMode)
        {
            case MatchMode.CaptureBase:
                iconSprite = modeIcons[1];
                break;
            default:
            case MatchMode.TeamDeathmatch:
                iconSprite = modeIcons[2];
                break;
        }
        if (!data.IsActiveMode)
            iconSprite = modeIcons[0];
        if (iconSprite)
            imgModeIcon.sprite = iconSprite;
        selectedFrame.gameObject.SetActive(modeData.IsSelected);
        selectedIcon.gameObject.SetActive(modeData.IsSelected);
        nonSelectedFrame.gameObject.SetActive(!modeData.IsSelected && modeData.IsActiveMode);
        txtComingSoon.gameObject.SetActive(!modeData.IsActiveMode);
        btnSelectMode.interactable = !modeData.IsSelected;

        btnSelectMode.onClick.RemoveAllListeners();
        btnSelectMode.onClick.AddListener(() =>
        {
            if (!modeData.IsSelected && modeData.IsActiveMode)
                onSelectMode?.Invoke(true);
        });
        gameObject.name = modeData.ModeId;
        btnSelectMode.gameObject.name = modeData.ModeId;
    }

    public void SetSelected(bool isSelected)
    {
        modeData.IsSelected = isSelected;
        selectedFrame.gameObject.SetActive(modeData.IsSelected);
        selectedIcon.gameObject.SetActive(modeData.IsSelected);
        nonSelectedFrame.gameObject.SetActive(!modeData.IsSelected && modeData.IsActiveMode);
        btnSelectMode.interactable = !modeData.IsSelected;
    }
}
public class GameModeItemData
{
    public string Region;
    public string ModeId;
    public string ModeName;
    public string ModeDesc;
    public MatchMode MatchMode;
    public MatchType MatchType;
    public bool IsActiveMode;
    public bool IsSelected;
}