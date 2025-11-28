using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion.TankOnlineModule;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayScreen : UIScreenBase
{
    [SerializeField] private float _countdownFrom;
    [SerializeField] private AnimationCurve _countdownCurve;
    [SerializeField] private TextMeshProUGUI _countdownUI, _timerText, _attackerKillText, _defenderKillText, _goldText;
    [SerializeField] private TextMeshProUGUI blueTeamDead, redTeamDead;
    [SerializeField] private PlayerStatsUI currentPlayerStats;
    [SerializeField] private PlayerStatsUI[] DefenderPlayersStats;
    [SerializeField] private PlayerStatsUI[] AttackerPlayersStats;
    [SerializeField] private GamePlayButtonUpgrade[] _buttonUpgrades;
    [SerializeField] private List<GamePlayTurretIcon> _turretIcons;
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private GameObject[] resultPanels;
    private int winningTeam = -1;

    public override void Initialize()
    {
        RegisterEvents();
        base.Initialize();
    }

    public override void Show(int additionalSortingOrder = 0, ScreenParam param = null)
    {
        base.Show();
        Reset();
        InitTurrets();
        DisplayUpgradeStats();
        StartCoroutine(ShowUI());
    }

    public override void RegisterEvents()
    {
        EventManager.Register<Fusion.NetworkArray<GameServer.PlayerStats>>(UpdateScores);
        EventManager.Register<GameServer.TimeEvent>(UpdateTimer);
        EventManager.Register<GameServer.GoldEvent>(OnGoldEvent);
        EventManager.Register<GameServer.UpgradeEvent>(OnUpgradeEvent);
        EventManager.Register<GamePhase>(OnGameStateChange);
        // EventManager.Register<CoreGamePlay.GamePlayDataEvent>(UpdateMatchInfo);
        EventManager.Register<GamePlayDataEvent>(OnUpdateAllInfos);
        EventManager.Register<GameOverEvent>(OnServerStateChanged);
    }

    private void UnregisterEvents()
    {
        EventManager.Unregister<Fusion.NetworkArray<GameServer.PlayerStats>>(UpdateScores);
        EventManager.Unregister<GameServer.TimeEvent>(UpdateTimer);
        EventManager.Unregister<GameServer.GoldEvent>(OnGoldEvent);
        EventManager.Unregister<GameServer.UpgradeEvent>(OnUpgradeEvent);
        EventManager.Unregister<GamePhase>(OnGameStateChange);
        EventManager.Unregister<GamePlayDataEvent>(OnUpdateAllInfos);
        EventManager.Unregister<GameOverEvent>(OnServerStateChanged);
    }

    private void OnServerStateChanged(GameOverEvent newState)
    {
        Debug.Log($"[GamePlayScreen] OnServerStateChanged: {newState}");
        StartCoroutine(DisplayWinnerSide());
    }

    private void CalculateMatchResults()
    {
        MatchmakingDocument matchmakingDocument = DatabaseManager.GetDB<MatchmakingCollection>().GetActiveDocument();
        switch (matchmakingDocument.MatchMode)
        {
            case MatchMode.TeamDeathmatch:
                CheckTeamDeathmatchWin();
                break;
            case MatchMode.CaptureBase:
                CheckCaptureBaseWin();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void CheckCaptureBaseWin()
    {
        int defenderKill = 0;
        int attackerKill = 0;
        int outpostDestroyed = 0;
        MatchmakingDocument matchmakingDocument = DatabaseManager.GetDB<MatchmakingCollection>().GetActiveDocument();

        MatchPlayerData[] matchData = DatabaseManager.GetDB<SOMatchData>().MatchDataArray;
        for (int i = 0; i < matchData.Length; i++)
        {
            if (matchData[i].TeamIndex == 0) // defender
            {
                attackerKill += matchData[i].Death;
            }
            else if (matchData[i].TeamIndex == 1) // attacker
            {
                outpostDestroyed += matchData[i].DestroyedTurrets;
                defenderKill += matchData[i].Death;
            }
        }

        winningTeam = -1;

        // win if kill 20 or destroy all turrets
        if (attackerKill >= matchmakingDocument.KillsNeededToWin
            || outpostDestroyed >= matchmakingDocument.TargetsNeededToWin)
        {
            winningTeam = 1;
        }
        else
        {
            winningTeam = 0;
        }
        // save the results to SOMatchData
        SOMatchData sOMatchData = DatabaseManager.GetDB<SOMatchData>();
        sOMatchData.winningTeam = winningTeam;
        sOMatchData.defenderKill = defenderKill;
        sOMatchData.attackerKill = attackerKill;
    }
    
    private void CheckTeamDeathmatchWin()
    {
        int team0Kill = 0;
        int team1Kill = 0;

        MatchPlayerData[] matchData = DatabaseManager.GetDB<SOMatchData>().MatchDataArray;
        for (int i = 0; i < matchData.Length; i++)
        {
            if (matchData[i].TeamIndex == 0)
            {
                team1Kill += matchData[i].Death;
            }
            else if (matchData[i].TeamIndex == 1)
            {
                team0Kill += matchData[i].Death;
            }
        }

        winningTeam = -1;

        if (team0Kill > team1Kill)
        {
            winningTeam = 0;
        }
        else if (team1Kill > team0Kill)
        {
            winningTeam = 1;
        }
        else
        {
            winningTeam = -1; // draw
        }

        // save the results to SOMatchData
        SOMatchData sOMatchData = DatabaseManager.GetDB<SOMatchData>();
        sOMatchData.winningTeam = winningTeam;
        sOMatchData.defenderKill = team0Kill;
        sOMatchData.attackerKill = team1Kill;
    }

    private IEnumerator DisplayWinnerSide()
    {
        //calculate winningTeam
        CalculateMatchResults();
        endGamePanel.SetActive(true);
        MatchPlayerData[] matchData = DatabaseManager.GetDB<SOMatchData>().MatchDataArray;
        MatchPlayerData localPlayerIndex = matchData.ToList().Find(p => p.IsLocalPlayer);
        resultPanels[0].SetActive(false);
        resultPanels[1].SetActive(false);
        resultPanels[2].SetActive(false);
        // Show winner display
        if (winningTeam == localPlayerIndex.TeamIndex)
        {
            if (resultPanels[1] != null) resultPanels[1].SetActive(true); //victory
        }
        else if (winningTeam == -1)
        {
            if (resultPanels[2] != null) resultPanels[2].SetActive(true); //draw
        }
        else
        {
            if (resultPanels[0] != null) resultPanels[0].SetActive(true); //defeat
        }
        yield return new WaitForSeconds(2);
        Debug.Log("[GamePlayScreen] Invoke FINAL state");
        EventManager.TriggerEvent<ServerStateEvent>(new ServerStateEvent{NewState = ServerState.FINAL});
    }

    private void DisplayUpgradeStats()
    {
        foreach (var buttonUpgrade in _buttonUpgrades)
        {
            buttonUpgrade.DefineTankUpgrades();
        }
    }

    private void InitTurrets()
    {
        var turretsAlive = GameServer.Instance.TurretsAlive;
        var collection = DatabaseManager.GetDB<TankCollection>();

        foreach (var turretIcon in _turretIcons)
        {
            turretIcon.gameObject.SetActive(false);
        }
        for (int i = 0; i < turretsAlive.Count; i++)
        {
            while (i >= _turretIcons.Count)
            {
                var newIcon = Instantiate(_turretIcons[0].gameObject, _turretIcons[0].transform.parent)
                    .GetComponent<GamePlayTurretIcon>();
                _turretIcons.Add(newIcon);
            }
            var turretIcon = _turretIcons[i];
            var turret = turretsAlive[i];
            var turretData = collection.GetTankByID(turret);
            if (turretData == null)
            {
                Debug.LogError($"Turret data is null for {turret}");
                continue;
            }
            turretIcon.gameObject.SetActive(true);
            turretIcon.Init(turretData.tankId, turretData.tankName);
        }
    }

    private void UpdateTurrets()
    {
        if (IsVisible == false) return;

        var turretsAlive = GameServer.Instance.TurretsAlive;
        foreach (var turretIcon in _turretIcons)
        {
            if (!turretIcon.gameObject.gameObject.activeSelf) continue;
            turretIcon.UpdateActive(turretsAlive.Contains(turretIcon.TurretId));
        }
    }

    public IEnumerator ShowUI()
    {
        yield return null;
        OnUpdateAllInfos(new GamePlayDataEvent());
    }

    public void OnUpdateAllInfos(GamePlayDataEvent gamePlayDataEvent)
    {
        if (IsVisible == false) return;
        SOMatchData sOMatchData = DatabaseManager.GetDB<SOMatchData>();
        UpdateMatchInfo(sOMatchData);
        UpdateUpgradesUI();
        UpdatePlayersInfo(sOMatchData);
    }


    private void OnGameStateChange(GamePhase stateType)
    {
        if (IsVisible == false) return;

        switch (stateType)
        {
            case GamePhase.MatchPlaying:
                Reset();
                break;

        }
    }

    private void UpdateUpgradesUI()
    {
        if (IsVisible == false) return;
        
        foreach (var buttonUpgrade in _buttonUpgrades)
        {
            buttonUpgrade.Refresh();
        }
    }

    private void OnUpgradeEvent(GameServer.UpgradeEvent @event)
    {
        if (IsVisible == false) return;

        if (GameServer.Instance?.MyTank == null) return;

        GameServer.Instance.MyTank.RPC_AddUpgrade(@event);
        UpdateUpgradesUI();
    }

    private void OnGoldEvent(GameServer.GoldEvent @event)
    {
        if (IsVisible == false) return;

        if (GameServer.Instance?.MyTank == null) return;

        _goldText.text = $"Gold: {GameServer.Instance.MyTank.PlayerData.Gold}";
        UpdateUpgradesUI();
    }

    private void UpdateTimer(GameServer.TimeEvent @event)
    {
        if (IsVisible == false) return;

        //Debug.Log($"UpdateTimer: TimeRemaining={@event.TimeRemaining}, IsStarting={@event.IsStarting}");
        _timerText.rectTransform.localScale = Vector3.one;
        _timerText.text = TimeSpan.FromSeconds(@event.TimeRemaining).ToString(@"mm\:ss");
    }

    public void Reset()
    {
        endGamePanel.SetActive(false);
        _countdownUI.transform.localScale = Vector3.zero;
        _timerText.text = "--:--";
        _attackerKillText.text = "0";
        _defenderKillText.text = "0";
        _goldText.text = "Gold: 0";
        blueTeamDead.text = "0";
        redTeamDead.text = "0";
        //OnUpdateAllInfos(new GamePlayDataEvent());

        // var allPlayers = CoreGamePlay.Instance.AllPlayers.ToList();
        // Debug.Log($"Reset: {allPlayers.Count}");
        // var curPlayer = allPlayers.First(p => p.HasInputAuthority);
        // _currentPlayerStats.Init(curPlayer, true);
        // var teamatePlayers = allPlayers
        //     .Where(p => p.PlayerId != curPlayer.PlayerId && p.NetTeammateIndex == curPlayer.NetTeammateIndex)
        //     .ToArray();
        // Debug.Log($"Reset: {teamatePlayers.Length}");
        // for (int i = 0; i < _teamatePlayersStats.Length; i++)
        // {
        //     if (i < teamatePlayers.Length)
        //     {
        //         _teamatePlayersStats[i].gameObject.SetActive(true);
        //         _teamatePlayersStats[i].Init(teamatePlayers[i], false);
        //     }
        //     else
        //     {
        //         _teamatePlayersStats[i].gameObject.SetActive(false);
        //     }
        // }

        // // hide all buttons to upgrade
        // foreach (var buttonUpgrade in _buttonUpgrades)
        // {
        //     buttonUpgrade.Enable(false);
        // }
    }
    private void UpdatePlayersInfo(SOMatchData gamePlayData)
    {
        if (IsVisible == false) return;
        
        if (gamePlayData == null) return;
        Player myPlayer = GameServer.Instance?.MyTank;
        if (myPlayer == null || myPlayer.PlayerData == null) return;

        var defenderData = gamePlayData.MatchDataArray
            .Where(p => p != null && p.IsJoined && p.TeamIndex == MatchIndexs.Defender && !string.IsNullOrEmpty(p.PlayerName))
            .OrderBy(p => p.IndexInTeam)
            .ToList();

        var attackerData = gamePlayData.MatchDataArray
            .Where(p => p != null && p.IsJoined && p.TeamIndex == MatchIndexs.Attacker && !string.IsNullOrEmpty(p.PlayerName))
            .OrderBy(p => p.IndexInTeam)
            .ToList();

        for (int i = 0; i < DefenderPlayersStats.Length; i++)
        {
            if (i < defenderData.Count)
            {
                bool isMine = defenderData[i].IndexInTeam == myPlayer.PlayerData.IndexInTeam;
                DefenderPlayersStats[i].gameObject.SetActive(true);
                DefenderPlayersStats[i].Init(defenderData[i], isMine);
            }
            else
            {
                DefenderPlayersStats[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < AttackerPlayersStats.Length; i++)
        {
            if (i < attackerData.Count)
            {
                bool isMine = attackerData[i].IndexInTeam == myPlayer.PlayerData.IndexInTeam;
                AttackerPlayersStats[i].gameObject.SetActive(true);
                AttackerPlayersStats[i].Init(attackerData[i], isMine);
            }
            else
            {
                AttackerPlayersStats[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateMatchInfo(SOMatchData gamePlayData)
    {
        if (gamePlayData == null) return;

        // var attackerKillCount = 0;
        // var defenderKillCount = 0;
        // foreach (var player in gamePlayData.MatchDataArray)
        // {
        //     if (player.IsJoined)
        //     {
        //         if (player.TeamIndex == 1)
        //         {
        //             attackerKillCount += player.Kill;
        //         }
        //         else if (player.TeamIndex == 0)
        //         {
        //             defenderKillCount += player.Kill;
        //         }
        //     }
        // }

        UpdateTurrets();

        // _attackerKillText.text = attackerKillCount.ToString();
        // _defenderKillText.text = defenderKillCount.ToString();


        UpdateKill(gamePlayData);
    }

    // Update dead with team
    private void UpdateKill(SOMatchData gamePlayData)
    {
        if (IsVisible == false) return;
        
        var defenderDead = 0;
        var attackerDead = 0;
        foreach (var player in gamePlayData.MatchDataArray)
        {
            if (player.IsJoined)
            {
                if (player.TeamIndex == MatchIndexs.Defender)
                    defenderDead += player.Death;
                else
                    attackerDead += player.Death;
            }
        }
        this.blueTeamDead.text = attackerDead.ToString();
        this.redTeamDead.text = defenderDead.ToString();
    }

    private void UpdateScores(Fusion.NetworkArray<GameServer.PlayerStats> playerStats)
    {
        if (IsVisible == false) return;
        // var attackerKillCount = 0;
        // var defenderKillCount = 0;
        // foreach (var stats in playerStats)
        // {
        //     if (stats.team == 1)
        //     {
        //         attackerKillCount += stats.Kill;
        //     }
        //     else if (stats.team == 0)
        //     {
        //         defenderKillCount += stats.Kill;
        //     }
        // }

        UpdateTurrets();

        // _attackerKillText.text = attackerKillCount.ToString();
        // _defenderKillText.text = defenderKillCount.ToString();

        var _myTeamDead = 0;
        var _enemyDead = 0;
        Player player = GameServer.Instance.MyTank;
        foreach (var stats in playerStats)
        {
            if (stats.team == player.PlayerTeamIndex)
            {
                _myTeamDead += stats.Death;
            }
            else
            {
                _enemyDead += stats.Death;
            }
        }
        blueTeamDead.text = _enemyDead.ToString();
        redTeamDead.text = _myTeamDead.ToString();
    }
}