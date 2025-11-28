using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.TankOnlineModule;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChooseTankScreen : UIScreenBase
{
    [SerializeField] private Transform tankItemParent;
    [SerializeField] private TextMeshProUGUI countdownText, stateText, teamText, targetText, killText;
    [SerializeField] private Image targetIcon, killIcon;
    [SerializeField] private Sprite[] targetSprites, killSprites; // 0=defender, 1=attacker
    [SerializeField] private ChooseTankPlayerCard[] playerCards; // my team, my position is index=0
    [SerializeField] private ChooseTankItem[] tankItems;
    [SerializeField] private ChooseTankItem chooseTankItemPrefab, selectedTankItem;
    private UnityAction<string> onSelectTank;
    private int localPlayerTeamIndex = -1, slotIndex = -1; // Store local player's team index and player index
    private ServerState serverState;

    public override void Initialize()
    {
        RegisterEvents();
        ClearAllPlayerCards();
        onSelectTank = OnTankSelected;
        base.Initialize();
    }

    public override void RegisterEvents()
    {
        EventManager.Register<ChooseTankBaseInfo>(DisplayExistingPlayers);
        EventManager.Register<ChooseTankTimeEvent>(OnUpdateCountdown);
        //EventManager.Register<ServerStateEvent>(OnChooseTankState);
        EventManager.Register<ChooseTankEvent>(OnAllowChooseTank);
    }

    private void OnAllowChooseTank(ChooseTankEvent chooseTankEvent)
    {
        Debug.Log($"[OnAllowChooseTank] ChangeState to {chooseTankEvent.State}");
        serverState = chooseTankEvent.State;
        switch (chooseTankEvent.State)
        {
            case ServerState.PICKING:
                DisplayExistingPlayers(new ChooseTankBaseInfo());
                break;
        }
    }

    private void OnChooseTankState(ServerStateEvent stateEvent)
    {
        Debug.Log($"[OnChooseTankState] ChangeState to {stateEvent.NewState}");
        //if (IsVisible == false) return;
        serverState = stateEvent.NewState;
        switch (stateEvent.NewState)
        {
            case ServerState.PICKING:
                DisplayExistingPlayers(new ChooseTankBaseInfo());
                break;
        }
    }

    public override void Show(int additionalSortingOrder = 0, ScreenParam param = null)
    {
        base.Show(additionalSortingOrder, param);
        CreateTankItems();
        for (int i = 0; i < playerCards.Length; i++)
        {
            playerCards[i].gameObject.SetActive(false);
        }
    }

    public override void Hide()
    {
        localPlayerTeamIndex = -1;
        ClearAllPlayerCards();
        base.Hide();
    }

    private void DisplayExistingPlayers(ChooseTankBaseInfo info)
    {
        ClearAllPlayerCards();
        //if (IsVisible == false) return;
        MatchPlayerData[] allPlayers = DatabaseManager.GetDB<SOMatchData>().MatchDataArray;
        // foreach (var p in allPlayers)
        // {
        //     Debug.Log($"[DisplayExistingPlayers] -- PlayerID:{p.PlayerId} PlayerName:{p.PlayerName} TeamIndex:{p.TeamIndex} IsLocalPlayer:{p.IsLocalPlayer} TankId:{p.TankId}");
        // }

        // find the local player's team index
        localPlayerTeamIndex = allPlayers.FirstOrDefault(p => p.IsLocalPlayer).TeamIndex;
        MatchPlayerData[] myteamPlayers = allPlayers.Where(p => p.TeamIndex == localPlayerTeamIndex).ToArray();
        Debug.Log($"[DisplayExistingPlayers] local team index: {localPlayerTeamIndex}, players in team: {myteamPlayers.Length}, PlayerIDs: {string.Join(", ", myteamPlayers.Select(p => p.IndexInTeam))}");
        slotIndex = 1;
        
        for (int i = 0; i < myteamPlayers.Length; i++)
        {
            UpdatePlayerCard(new ChooseTankBaseInfo
            {
                IsLocalPlayer = myteamPlayers[i].IsLocalPlayer,
                PlayerIndex = myteamPlayers[i].IndexInTeam,
                PlayerName = myteamPlayers[i].PlayerName,
                TankId = myteamPlayers[i].TankId,
                TeammateIndex = myteamPlayers[i].TeamIndex,
                PlayerId = myteamPlayers[i].PlayerId
            });
        }
    }

    // private IEnumerator DelayedDisplayExistingPlayers()
    // {
    //     while (true)
    //     {
    //         MatchPlayerData[] allPlayers = DatabaseManager.GetDB<SOMatchData>().MatchDataArray;
    //         // nếu tất cả đều Joined = true 
    //         if (allPlayers.All(p => p.IsJoined))
    //         {
    //             break;
    //         }
    //         yield return null;
    //     }

    //     yield return null;
    //     DisplayExistingPlayers(new ChooseTankBaseInfo());
    //     yield return null;
    //     SelectDefaultTank();
    // }
    
    private void SelectDefaultTank()
    {
        // Select the default tank for the local player
        PlayerDocument playerDocument = DatabaseManager.GetDB<PlayerCollection>().GetMine();
        string selectedTankId = playerDocument.selectedTank;

        selectedTankItem = tankItems.FirstOrDefault(item => item.TankId.Equals(selectedTankId));
        if (selectedTankItem != null)
        {
            InventoryHelper.SetSelectedTank(selectedTankId);
            int wrapId = playerDocument.GetWrapId(selectedTankId);
            GameServer.Instance.MyTank.RPC_ChooseTank(selectedTankId, wrapId);
            selectedTankItem.Highlight(true);
        }
        else Debug.LogWarning($"[SelectDefaultTank] Tank ID {selectedTankId} not found in tank items.");
    }

    private void ClearAllPlayerCards()
    {
        // Clear all player cards except the local player card
        for (int i = 0; i < playerCards.Length; i++)
        {
            if (playerCards[i] != null)
            {
                playerCards[i].ClearInfo();
            }
        }
    }

    private void UpdateRule(int teamIndex = -1)
    {
        MatchmakingDocument matchmakingDocument = DatabaseManager.GetDB<MatchmakingCollection>().GetActiveDocument();
        string defenderLabel = GetStringByKey(LocKeys.UI_ChooseTank.UI_ChooseTank_Defender);
        string attackerLabel = GetStringByKey(LocKeys.UI_ChooseTank.UI_ChooseTank_Attacker);
        string protectOutpostLabel = GetStringByKey(LocKeys.UI_ChooseTank.UI_ChooseTank_ProtectOutpost);
        string destroyOutpostLabel = GetStringByKey(LocKeys.UI_ChooseTank.UI_ChooseTank_DestroyOutpost);
        string killEnemiesLabel = GetSmartStringByKey( matchmakingDocument.killsNeededToWin == 1 ? LocKeys.UI_ChooseTank.UI_ChooseTank_KillOne : LocKeys.UI_ChooseTank.UI_ChooseTank_KillEnemies,
        new Dictionary<string, object>
        {
            { "Count", matchmakingDocument.KillsNeededToWin }
        });

        teamText.text = teamIndex == 0 ? defenderLabel : attackerLabel;
        targetText.text = teamIndex == 0 ? protectOutpostLabel : destroyOutpostLabel;
        killText.text = killEnemiesLabel;
        targetIcon.sprite = teamIndex == 0 ? targetSprites[0] : targetSprites[1];
        killIcon.sprite = teamIndex == 0 ? killSprites[0] : killSprites[1];
    }

    private void UpdatePlayerCard(ChooseTankBaseInfo playerData)
    {
        Debug.Log($"[UpdatePlayerCard]  IsLocalPlayer:{playerData.IsLocalPlayer}/ PlayerName:{playerData.PlayerName}/ TeammateIndex:{playerData.TeammateIndex}");
        if (playerData.IsLocalPlayer)
        {
            OnTankSelected(playerData.TankId.ToString());
            UpdateRule(localPlayerTeamIndex);
            playerCards[0].gameObject.SetActive(true);
            playerCards[0].SetInfo(playerData);
        }
        else
        {
            if (slotIndex >= playerCards.Length)
                return;

            Debug.Log($"[UpdatePlayerCard] slotIndex:{slotIndex} TeammateIndex:{playerData.TeammateIndex} localPlayerTeamIndex:{localPlayerTeamIndex}, PlayerIndex:{playerData.PlayerIndex}");
            // Slot 0 is reserved for local player, teammates are 1..4
            playerCards[slotIndex].gameObject.SetActive(true);
            playerCards[slotIndex].SetInfo(playerData);
            slotIndex++;
        }
    }

    private void OnTankSelected(string tankId)
    {
        if (selectedTankItem != null)
        {
            selectedTankItem.Highlight(false);
        }

        selectedTankItem = tankItems.FirstOrDefault(item => item.TankId.Equals(tankId));
        if (selectedTankItem != null)
        {
            selectedTankItem.Highlight(true);
        }
    }

    private void OnUpdateCountdown(ChooseTankTimeEvent @event)
    {
         if (IsVisible == false) return;

        countdownText.text = TimeSpan.FromSeconds(@event.Seconds).ToString(@"ss");
    }

    private void CreateTankItems()
    {
        TankCollection tankCollection = DatabaseManager.GetDB<TankCollection>();
        GameAssetCollection gameAssetCollection = DatabaseManager.GetDB<GameAssetCollection>();
        var allTankDatas = tankCollection.CloneDocumentsByProperty(doc => doc.tankType != TankType.Outpost, true);
        string mySelectedTankId = tankCollection.GetActiveTank().tankId;
        foreach (var item in tankItems)
        {
            Destroy(item.gameObject);
        }

        tankItems = new ChooseTankItem[allTankDatas.Length];
        for (int i = 0; i < allTankDatas.Length; i++)
        {
            var tankDocument = allTankDatas[i];
            var tankItem = Instantiate(chooseTankItemPrefab, tankItemParent);
            tankItem.gameObject.SetActive(true);
            gameAssetCollection.GetAssets(tankDocument.tankId, out Sprite mainIcon, out Sprite previewIcon, out Sprite classIcon);
            tankItem.Init(tankDocument.tankId, tankDocument.tankType, previewIcon, classIcon, ServerState.PICKING);
            tankItem.SetCallback(onSelectTank);
            tankItems[i] = tankItem;

            if (selectedTankItem == null && tankDocument.tankId == mySelectedTankId)
            {
                selectedTankItem = tankItem;
            }
        }

        chooseTankItemPrefab.gameObject.SetActive(false);
    }

    private string GetStringByKey(string key)
    {
        return LocalizationHelper.GetString(nameof(LocKeys.UI_ChooseTank), key);
    }

    private string GetSmartStringByKey(string key, Dictionary<string, object> variables)
    {
        return LocalizationHelper.GetSmartString(nameof(LocKeys.UI_ChooseTank), key, variables);
    }
}

[Serializable]
public struct ChooseTankBaseInfo : INetworkStruct
{
    public int TeammateIndex { get; set; }
    public int PlayerIndex { get; set; }
    public NetworkString<_32> PlayerName { get; set; }
    public NetworkString<_32> TankId { get; set; }
    public int PlayerId { get; set; }
    public bool IsLocalPlayer { get; set; }
}

public struct ChooseTankTimeEvent
{
    public int Seconds { get; set; }
}

public struct PlayerUpdate
{
    public Player Player { get; set; }
}

public struct ChooseTankEvent{
    public ServerState State { get; set; }
}