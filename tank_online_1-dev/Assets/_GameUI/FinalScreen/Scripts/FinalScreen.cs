using Fusion.TankOnlineModule;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GDO.Audio;

public class FinalScreen : UIScreenBase
{
    [Header("Team Kill Count")]
    [SerializeField] private TextMeshProUGUI team0KillCountText;
    [SerializeField] private TextMeshProUGUI team1KillCountText;
    
    [Header("Winner/Loser Display")]
    [SerializeField] private GameObject winnerDisplay, loserDisplay, drawDisplay;
    [Header("Winner Display")]
    [SerializeField] private GameObject team0WinDisplay;
    [SerializeField] private GameObject team0LoseDisplay;
    [SerializeField] private GameObject team1WinDisplay;
    [SerializeField] private GameObject team1LoseDisplay;
    
    [Header("MVP Player")]
    [SerializeField] private Image mvpTankIcon;
    [SerializeField] private TankPreviewComp mvpTankPreviewComp;
    [SerializeField] private TextMeshProUGUI mvpPlayerNameText;
    
    [Header("Claim Reward")]
    [SerializeField] private Button claimRewardButton;
    
    [Header("Team 0 Players")]
    [SerializeField] private Transform team0PlayersParent;
    [SerializeField] private List<FinalScreenPlayerCard> team0PlayerCards = new List<FinalScreenPlayerCard>();
    
    [Header("Team 1 Players")]
    [SerializeField] private Transform team1PlayersParent;
    [SerializeField] private List<FinalScreenPlayerCard> team1PlayerCards = new List<FinalScreenPlayerCard>();
    
    private MatchmakingDocument matchmakingDocument;
    private int defenderKill;
    private int attackerKill;
    private int mvpId;
    private int winningTeam;
    private int myPlacementIndex;
    private PackRewardDocument myRewards;

    protected override void Awake()
    {
        base.Awake();
        
        if (claimRewardButton != null)
        {
            claimRewardButton.onClick.AddListener(OnClaimReward);
        }
    }

    public override void Show(int additionalSortingOrder = 0, ScreenParam param = null)
    {
        base.Show();
        InitializeMatchResults();
    }

    private void InitializeMatchResults()
    {
        // Get match data
        matchmakingDocument = DatabaseManager.GetDB<MatchmakingCollection>().GetActiveDocument();
        GetResultsData();
        FindMVP();
        GetRewardsByPlacement();
        DisplayTeamKillCounts();
        DisplayWinnerSide();
        DisplayMvpPlayer();
        DisplayTeamPlayers();
        UpdateElo();
        // Left current session
        EventManager.TriggerEvent(GamePhase.MatchReleasing);
    }

    private void GetResultsData()
    {
        SOMatchData matchDataDoc = DatabaseManager.GetDB<SOMatchData>();
        winningTeam = matchDataDoc.winningTeam;
        defenderKill = matchDataDoc.defenderKill;
        attackerKill = matchDataDoc.attackerKill;
    }

    private void GetRewardsByPlacement()
    {
        SOMatchData matchDataDoc = DatabaseManager.GetDB<SOMatchData>();
        MatchPlayerData[] matchData = matchDataDoc.MatchDataArray;
        // Find all players placement for rewards
        var placements = matchData.OrderByDescending(p => p.Kill)
            .ThenBy(p => p.Death)
            .ToList();
        myPlacementIndex = placements.FindIndex(p => p.IsLocalPlayer);
        var rewardId = matchmakingDocument.Rewards[myPlacementIndex];
        myRewards = DatabaseManager.GetDB<PackRewardCollection>().GetPackRewardByID(rewardId);
    }

    private void FindMVP()
    {
        mvpId = -1; 
        SOMatchData matchDataDoc = DatabaseManager.GetDB<SOMatchData>();
        MatchPlayerData[] matchData = matchDataDoc.MatchDataArray;
        
        var mvpIdx = matchData
            .Select((p, i) => new { p, i })
            .Where(x => x.p.IsJoined && x.p.TeamIndex == winningTeam)
            .OrderByDescending(x => x.p.Kill)
            .ThenBy(x => x.p.Death)
            .Select(x => (int?)x.p.PlayerId)
            .FirstOrDefault();

        if(mvpIdx == null)
        {
            mvpIdx = matchData
            .Select((p, i) => new { p, i })
            .Where(x => x.p.IsJoined)
            .OrderByDescending(x => x.p.Kill)
            .ThenBy(x => x.p.Death)
            .Select(x => (int?)x.p.PlayerId)
            .FirstOrDefault();
        }

        mvpId = mvpIdx ?? -1;
        //Debug.Log($"FinalScreen FindMVP: winningTeam={winningTeam}, mvpIndex={mvpId}, matchData={string.Join(", ", matchData.Select(p => p.PlayerId))}");
    }

    private void UpdateElo()
    {
        Debug.Log("FinalScreen UpdateElo called with winningTeam: " + winningTeam);
        if (winningTeam == -1) return; // no elo change on draw

        // if i am the local player and i am winner, update elo
        MatchPlayerData[] matchData = DatabaseManager.GetDB<SOMatchData>().MatchDataArray;
        MatchPlayerData localPlayerData = matchData.ToList().Find(p => p.IsLocalPlayer);
        
        if (localPlayerData.TeamIndex == winningTeam)
        {
            DatabaseManager.GetDB<PlayerCollection>().UpdatePlayerElo(matchmakingDocument.WinPoint);
        }
        else
        {
            DatabaseManager.GetDB<PlayerCollection>().UpdatePlayerElo(matchmakingDocument.LosePoint);
        }
    }

    private void DisplayTeamKillCounts()
    {
        if (team0KillCountText != null)
            team0KillCountText.text = defenderKill.ToString();

        if (team1KillCountText != null)
            team1KillCountText.text = attackerKill.ToString();
    }

    private void DisplayWinnerSide()
    {
        MatchPlayerData[] matchData = DatabaseManager.GetDB<SOMatchData>().MatchDataArray;
        MatchPlayerData localPlayerIndex = matchData.ToList().Find(p => p.IsLocalPlayer);
        // Reset all displays
        if (team0WinDisplay != null) team0WinDisplay.SetActive(false);
        if (team0LoseDisplay != null) team0LoseDisplay.SetActive(false);
        if (team1WinDisplay != null) team1WinDisplay.SetActive(false);
        if (team1LoseDisplay != null) team1LoseDisplay.SetActive(false);

        if (winnerDisplay != null) winnerDisplay.SetActive(false);
        if (loserDisplay != null) loserDisplay.SetActive(false);
        if (drawDisplay != null) drawDisplay.SetActive(false);
        
        // Show winner display
        if (winningTeam == localPlayerIndex.TeamIndex)
        {
            if (winnerDisplay != null) winnerDisplay.SetActive(true);
        }
        else if (winningTeam == -1)
        {
            if (drawDisplay != null) drawDisplay.SetActive(true);
        }
        else
        {
            if (loserDisplay != null) loserDisplay.SetActive(true);
        }
    }

    private void DisplayMvpPlayer()
    {
        if (mvpId == -1) return;

        MatchPlayerData[] matchData = DatabaseManager.GetDB<SOMatchData>().MatchDataArray;
        GameAssetCollection gameAssetCollection = DatabaseManager.GetDB<GameAssetCollection>();
        MatchPlayerData mvpPlayer = matchData.ToList().Find(p => p.PlayerId == mvpId);
        // Display MVP tank icon
        if (mvpTankIcon != null)
        {
            var tankIconSprite = gameAssetCollection.GetMainIcon(mvpPlayer.TankId);
            mvpTankIcon.sprite = tankIconSprite;
        }
        if(mvpTankPreviewComp != null)
        {
            mvpTankPreviewComp.ShowTankPreview(mvpPlayer.TankId);
            mvpTankPreviewComp.ChangeWrap(mvpPlayer.WrapId,mvpPlayer.TankId);
        }
        
        // Display MVP player name
        if (mvpPlayerNameText != null)
        {
            mvpPlayerNameText.text = mvpPlayer.PlayerName;
        }
    }

    private void DisplayTeamPlayers()
    {
        MatchPlayerData[] matchData = DatabaseManager.GetDB<SOMatchData>().MatchDataArray;
        // Setup team 0 player cards
        var team0Players = matchData.Where(p => p.TeamIndex == 0).ToList();
        SetupTeamPlayerCards(team0Players, team0PlayerCards, team0PlayersParent);
        
        // Setup team 1 player cards
        var team1Players = matchData.Where(p => p.TeamIndex == 1).ToList();
        SetupTeamPlayerCards(team1Players, team1PlayerCards, team1PlayersParent);
    }

    private void SetupTeamPlayerCards(List<MatchPlayerData> teamPlayers, List<FinalScreenPlayerCard> cardsList, Transform parent)
    {
        GameAssetCollection gameAssetCollection = DatabaseManager.GetDB<GameAssetCollection>();
        Debug.Log($"FinalScreen SetupTeamPlayerCards: teamPlayers.Count={teamPlayers.Count}, cardsList.Count={cardsList.Count}");
        // Set player info for active players
        for (int i = 0; i < cardsList.Count; i++)
        {
            if (i < teamPlayers.Count)
            {
                Debug.Log($"FinalScreen SetupTeamPlayerCards: Setting up card for player {teamPlayers[i].PlayerName}");
                // Player slot - show and set info
                var player = teamPlayers[i];
                bool isMvp = mvpId != -1 && player.PlayerId == mvpId;
                cardsList[i].SetPlayerInfo(player, gameAssetCollection.GetMainIcon(player.TankId), isMvp);
                cardsList[i].gameObject.SetActive(true);
            }
            else
            {
                Debug.Log($"FinalScreen SetupTeamPlayerCards: Hiding card at index {i}");
                // Empty slot - hide the card
                cardsList[i].gameObject.SetActive(false);
            }
        }
    }

    private void ClearPlayerCards()
    {
        // Just hide all cards instead of destroying them
        foreach (var card in team0PlayerCards)
        {
            if (card != null)
                card.gameObject.SetActive(false);
        }
        
        foreach (var card in team1PlayerCards)
        {
            if (card != null)
                card.gameObject.SetActive(false);
        }
    }

    private void OnClaimReward()
    {
        EventManager.TriggerEvent(new ShowPopupEvent(PopupIDs.Rewards, new RewardPopupParam
        {
            packReward = myRewards,
            onClose = () =>
            {
                EventManager.TriggerEvent(GamePhase.MatchIdling);
                mvpTankPreviewComp?.HideTankPreview();
            }
        }));
    }

    protected void OnDestroy()
    {
        if (claimRewardButton != null)
        {
            claimRewardButton.onClick.RemoveListener(OnClaimReward);
            mvpTankPreviewComp?.HideTankPreview();
        }
    }
}