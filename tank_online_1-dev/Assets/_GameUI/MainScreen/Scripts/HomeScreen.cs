using System;
using System.Collections;
using System.Collections.Generic;
using Fusion.GameSystems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreen : UIScreenBase
{
    [SerializeField] private Sprite[] playerCountSprites; // 0=empty, 1=joined
    [SerializeField] private Image[] playerCountImages;
    [SerializeField] private Button findMatchButton, cancelMatchButton;
    [SerializeField] private TMP_Dropdown dropdown;

    [SerializeField] private TMP_Text txtPlayerName, txtGold, txtDiamond, txtElo, txtCurModeName;
    [SerializeField] private Image playerAvatarImage;
    [SerializeField] private Sprite defaultAvatar;
    [SerializeField] private TankPreviewComp myTankPreview;

    [Header("Popup Change Mode")]
    [SerializeField] private Transform popupChangeMode;
    [SerializeField] private Transform scrollviewModeContent;
    [SerializeField] private GameObject gameModeItemPrefab;
    [SerializeField] private Toggle multiplayerToggle;

    private MatchmakingCollection matchmakingCollection;
    private bool IsFinding { get; set; }
    private List<GameModeItemComp> gameModeItemComps = new List<GameModeItemComp>();

    private void OnDestroy()
    {
        UnregisterEvents();
    }

    public override void RegisterEvents()
    {
        EventManager.Register<MainMenuEvent>(DisplayFindMatchButton);
    }

    private void UnregisterEvents()
    {
        EventManager.Unregister<MainMenuEvent>(DisplayFindMatchButton);
    }

    public override void Initialize()
    {
        findMatchButton.onClick.AddListener(OnFindMatchClicked);
        cancelMatchButton.onClick.AddListener(OnCancelMatchClicked);
        RegisterEvents();
        base.Initialize();
    }

    public override void Show(int additionalSortingOrder = 0, ScreenParam param = null)
    {
        base.Show();
        RegisterEvents();

        matchmakingCollection = DatabaseManager.GetDB<MatchmakingCollection>();
        dropdown.value = matchmakingCollection.GetSelectedMatchIndex();
        IsFinding = MatchmakingManager.IsFinding;
        if (!IsFinding)
        {
            findMatchButton.gameObject.SetActive(true);
            cancelMatchButton.gameObject.SetActive(false);
            DisplayFindMatchButton(MainMenuEvent.StopFindMatch());
        }
        UpdatePlayerInfo();
        UpdateMatchMode();
    }

    public override void Hide()
    {
        //UnregisterEvents();
        // remove tank preview
        myTankPreview.HideTankPreview();

        base.Hide();
    }

    public void ShowChooseModePopup()
    {
        popupChangeMode.gameObject.SetActive(true);
        multiplayerToggle.isOn = true;
        ShowMatchModesByType(new MatchType[] { MatchType.Ranked });
    }

    public void ToggleChangeModePopup(int index) // index = 0 is multiplayer, 1 is solo
    {
        if (index == 0)
        {
            ShowMatchModesByType(new MatchType[] { MatchType.Ranked });
        }
        else
        {
            ShowMatchModesByType(new MatchType[] { MatchType.Normal });
        }
    }
    
    private void ShowMatchModesByType(MatchType[] types)
    {
        popupChangeMode.gameObject.SetActive(true);
        // Hide all items first
        for (int i = 0; i < scrollviewModeContent.childCount; i++)
        {
            scrollviewModeContent.GetChild(i).gameObject.SetActive(false);
        }

        // Show only items matching the selected mode type
        for (int i = 0; i < gameModeItemComps.Count; i++)
        {
            var itemComp = gameModeItemComps[i];
            if (itemComp.ModeData.Region.ToLower() != GameManager.fixedRegion.ToLower())
            {
                continue; // skip items not in the fixed region
            }
            
            if (itemComp != null && System.Array.Exists(types, type => type == itemComp.GetMatchType()))
            {
                itemComp.gameObject.SetActive(true);
            }
        }
    }

    private void ShowMatchModesByType(MatchMode[] modeType)
    {
        popupChangeMode.gameObject.SetActive(true);
        // Hide all items first
        for (int i = 0; i < scrollviewModeContent.childCount; i++)
        {
            scrollviewModeContent.GetChild(i).gameObject.SetActive(false);
        }

        // Show only items matching the selected mode type
        for (int i = 0; i < gameModeItemComps.Count; i++)
        {
            var itemComp = gameModeItemComps[i];
            if (itemComp.ModeData.Region.ToLower() != GameManager.fixedRegion.ToLower())
            {
                continue; // skip items not in the fixed region
            }
            
            if (itemComp != null && System.Array.Exists(modeType, mode => mode == itemComp.GetMatchMode()))
            {
                itemComp.gameObject.SetActive(true);
            }
        }
    }

    private void UpdateMatchMode()
    {
        List<MatchmakingDocument> documents = DatabaseManager.GetDB<MatchmakingCollection>().GetAllDocuments();
        // filter follow GameManager.fixedRegion
        Debug.Log("Fixed Region: " + GameManager.fixedRegion);

        txtCurModeName.text = documents.Find(doc => doc.IsSelected)?.matchName ?? string.Empty;
        //scrollviewModeContent add scroll items
        // Only destroy redundant items for better performance
        int childCount = scrollviewModeContent.childCount;
        int docCount = documents.Count;

        // Destroy extra items if there are more children than needed
        for (int i = docCount; i < childCount; i++)
        {
            Destroy(scrollviewModeContent.GetChild(i).gameObject);
        }
        gameModeItemComps = new List<GameModeItemComp>();
        // Reuse or create items as needed
        for (int i = 0; i < docCount; i++)
        {
            int index = i;
            GameObject item;
            if (i < scrollviewModeContent.childCount)
            {
                // Reuse existing item
                item = scrollviewModeContent.GetChild(i).gameObject;
                item.SetActive(true);
            }
            else
            {
                // Instantiate new item if not enough
                item = Instantiate(gameModeItemPrefab, scrollviewModeContent);
            }
            string modeKey = "match." + Enum.GetName(typeof(MatchMode), documents[i].MatchMode).ToLower();
            string matchName = LocalizationHelper.GetSmartString(nameof(LocKeys.UI_Common),  modeKey + ".name", new Dictionary<string, object>
                {
                    { "Total", documents[i].maxPlayers }
                });

            string matchDes = LocalizationHelper.GetSmartString(nameof(LocKeys.UI_Common), modeKey + ".description", new Dictionary<string, object>
                {
                    { "Second", documents[i].MatchDuration}
                });

            GameModeItemData itemData = new GameModeItemData
            {
                Region = documents[i].FixedRegion,
                ModeId = documents[i].matchID,
                ModeName = matchName,
                ModeDesc = matchDes,
                MatchMode = documents[i].matchMode,
                MatchType = documents[i].matchType,
                IsActiveMode = documents[i].IsActive,
                IsSelected = documents[i].IsSelected
            };
            var itemComp = item.GetComponent<GameModeItemComp>();
            itemComp.Init(itemData, (isSelected) =>
            {
                if (isSelected)
                {
                    matchmakingCollection.SetMatchIndex(index);
                    foreach (var comp in gameModeItemComps)
                    {
                        if (comp != itemComp)
                            comp.SetSelected(false);
                        else
                        {
                            itemComp.SetSelected(true);
                            txtCurModeName.text = itemData.ModeName;
                        }
                    }
                }
            });
            gameModeItemComps.Add(itemComp);
        }
        
        // hide all matches not match the fixed region
        for (int i = 0; i < gameModeItemComps.Count; i++)
        {
            var itemComp = gameModeItemComps[i];
            Debug.Log("Match Region: " + itemComp.ModeData.Region + " vs Fixed Region: " + GameManager.fixedRegion);
            itemComp.gameObject.SetActive(itemComp.ModeData.Region.ToLower() == GameManager.fixedRegion.ToLower());
        }
    }

    private void UpdatePlayerInfo()
    {
        var playerDocument = DatabaseManager.GetDB<PlayerCollection>().GetMine();

        if (playerDocument != null)
        {
            txtGold.text = playerDocument.gold.ToString();
            txtDiamond.text = playerDocument.diamond.ToString();
            txtElo.text = playerDocument.elo.ToString();
            txtPlayerName.text = playerDocument.playerName;

            if (playerAvatarImage != null)
            {
                var playerAvatar = !string.IsNullOrWhiteSpace(playerDocument.playerAvatar) ? DatabaseManager.GetDB<GameAssetCollection>()
                    .GetGameAssetDocumentById(playerDocument.playerAvatar).icon : null;
                playerAvatarImage.sprite = playerAvatar != null ? playerAvatar : defaultAvatar;
            }
        }

        // remove tank preview
        myTankPreview.HideTankPreview();

        // show preview of selected tank
        myTankPreview.ShowTankPreview(playerDocument.selectedTank);
        int wrapId = PlayerDocument.GetMineWrapId(playerDocument.selectedTank);
        myTankPreview.ChangeWrap(wrapId, playerDocument.selectedTank);
    }

    public void OnChangeMode(int index)
    {
        matchmakingCollection.SetMatchIndex(index);
    }


    private void EnableFinding(bool status)
    {
        if (status == IsFinding) return;

        if (status)
        {
            EventManager.TriggerEvent(GamePhase.MatchSearching);
        }
        else
            EventManager.TriggerEvent(GamePhase.MatchIdling);

        IsFinding = status;

        findMatchButton.gameObject.SetActive(!IsFinding);
        cancelMatchButton.gameObject.SetActive(IsFinding);
    }

    private IEnumerator DelayChangeFindMatchStatus(Button button, int playerCount, int maxPlayers, string buttonLabel, float delayTime, bool isCanTouch, bool isCanChangeMode, bool isCanSeePlayerCount = true)
    {
        yield return new WaitForSeconds(delayTime);
        dropdown.interactable = isCanChangeMode;

        for (int i = 0; i < playerCountImages.Length; i++)
        {
            if (i + 1 > maxPlayers)
            {
                playerCountImages[i].gameObject.SetActive(false);
                continue;
            }
            else
            {
                playerCountImages[i].gameObject.SetActive(true);
                if (i < playerCount)
                    playerCountImages[i].sprite = playerCountSprites[1];
                else
                    playerCountImages[i].sprite = playerCountSprites[0];
            }
        }
    }

    private void DisplayFindMatchButton(MainMenuEvent args)
    {
        //if (IsVisible == false) return;

        Debug.Log($"Matchmaking Status: {args.status}, Player Count: {args.playerCount}/{args.maxPlayers}");
        if (args.status == MatchmakingStatus.Found || args.status == MatchmakingStatus.Updating)
        {
            StartCoroutine(DelayChangeFindMatchStatus(cancelMatchButton, args.playerCount, args.maxPlayers, "Matching...", 0.1f, isCanTouch: true, false));
        }
        else if (args.status == MatchmakingStatus.Finding)
        {
            StartCoroutine(DelayChangeFindMatchStatus(cancelMatchButton, args.playerCount, args.maxPlayers, "Matching...", 0, isCanTouch: false, false, false));
        }
        else
        {
            StartCoroutine(DelayChangeFindMatchStatus(findMatchButton, args.playerCount, args.maxPlayers, "Find Match", 1, isCanTouch: true, true, false));
        }
    }

    public void OnFindMatchClicked()
    {
        EnableFinding(true);
    }

    public void OnCancelMatchClicked()
    {
        EnableFinding(false);
    }

    public void DeleteAccount()
    {
        // Delete account on firebase (if applicable)
        DatabaseManager.GetDB<PlayerCollection>().DeleteAccountFromFirebase();

        // Clear local player preferences
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("All player preferences have been deleted.");
        EventManager.TriggerEvent(GamePhase.Auth);
    }

    private void OnValidate()
    {
        if (findMatchButton == null)
        {
            Debug.LogError("[MainMenuScreen] findMatchButton is not assigned.");
        }

        if (cancelMatchButton == null)
        {
            Debug.LogError("[MainMenuScreen] cancelMatchButton is not assigned.");
        }
    }
}