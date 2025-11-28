using Fusion;

public struct GamePlayDataEvent : INetworkStruct
{
    public int totalKillTeam1 { get; set; }
    public int totalKillTeam2 { get; set; }
    public int targetToWin { get; set; }
}

public struct GameOverEvent : INetworkStruct
{
    public int winningTeam { get; set; }
}

public struct GamePlayOutpostEvent : INetworkStruct
{
}

public struct GamePlayDeathEvent : INetworkStruct
{
}