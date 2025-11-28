
using UnityEngine;
using System;

[System.Serializable]
public class MatchmakingDocument
{
    public const string mainIdFieldName = nameof(matchID); // Tên field chính để lấy ID tài liệu remote config 
    // ====== FIELDS (Unity/JSON dùng) ======
    public string matchID;
    public string matchName;
    public string matchDescription;
    public bool isActive;
    public bool isSelected;
    public string fixedRegion; // Vùng cố định để vào phòng
    public string fixedEloName; // Tên elo cố định để vào phòng
    public int maxRoom;
    public PlayerElos[] playerElos;
    public MatchType matchType;
    public MatchMode matchMode;
    public byte maxPlayers;
    public int killsNeededToWin;
    public int targetsNeededToWin;

    [Min(0)]  public int maxWaitPlayersTime;
    [Min(5)]  public int selectTankTime;
    [Min(3)]  public int countdownTime;
    [Min(5)]  public int targetReadingTime;

    [Min(0)]  public int matchDuration;
    [Min(0)]  public int lobbySceneIndex;
    [Min(0)]  public int mapSceneIndex;
    public int goldStarting;
    public int goldPerSecond;
    public int goldPerKill;
    public int goldDestroyOutpost;
    public int goldDefendOutpost;
    public int doubleKillBonus;
    public int tripleKillBonus;
    public int quadraKillBonus;
    public int pentaKillBonus;
    // Hai field này bạn có trong class gốc, thêm luôn để đầy đủ:
    public int goldKillStreakBonus;
    public int goldShutDownBonus;
    public int winPoint;
    public int losePoint;
    public int respawnInSeconds;
    public string[] rewards;

    public string MatchName { get => matchName; }
    public string MatchDescription { get => matchDescription; }
    public bool IsActive { get => isActive; set => isActive = value; }
    public bool IsSelected { get => isSelected; set => isSelected = value; }
    public string FixedRegion { get => fixedRegion; }
    public string FixedEloName { get => fixedEloName; }
    public PlayerElos[] PlayerElos { get => playerElos; }
    public MatchType MatchType { get => matchType; }
    public MatchMode MatchMode          { get => matchMode; }
    public byte MaxPlayers              { get => maxPlayers; }
    public int KillsNeededToWin         { get => killsNeededToWin; }
    public int TargetsNeededToWin       { get => targetsNeededToWin; }
    public int PickingTime { get => selectTankTime; }
    public int CountdownTime { get => countdownTime; }
    public int TargetReadingTime { get => targetReadingTime; }
    public int MaxRoom { get => maxRoom; }
    public int LobbySceneIndex          { get => lobbySceneIndex; }
    public int MapSceneIndex            { get => mapSceneIndex; }
    public int MatchDuration            { get => matchDuration; }
    public int GoldPerSecond            { get => goldPerSecond; }
    public int GoldStarting             { get => goldStarting; }
    public int GoldPerKill              { get => goldPerKill; }
    public int GoldDestroyOutpost       { get => goldDestroyOutpost; }
    public int GoldDefendOutpost        { get => goldDefendOutpost; }
    public int DoubleKillBonus          { get => doubleKillBonus; }
    public int TripleKillBonus          { get => tripleKillBonus; }
    public int QuadraKillBonus          { get => quadraKillBonus; }
    public int PentaKillBonus           { get => pentaKillBonus; }
    public int WinPoint                 { get => winPoint; }
    public int LosePoint                { get => losePoint; }
    public int RespawnInSeconds         { get => respawnInSeconds; }
    public string[] Rewards             { get => rewards; }

}
