using System;
using System.Linq;
using Fusion;
using Fusion.TankOnlineModule;

/// <summary>
/// Use this struct to send player-related data during matchmaking and choose tank phase.
/// </summary>
[System.Serializable]
public struct ServerDataEvent : INetworkStruct
{
    public PlayerRef playerRef;
    public byte matchType;
    public byte matchMode;
    public byte maxPlayers;
    public byte sceneIndex;
    public NetworkString<_32> region;
    public NetworkString<_32> fixedEloName;
    public NetworkString<_32> sessionName;

    public static ServerDataEvent StartServer(MatchmakingDocument matchDoc)
    {
        return new ServerDataEvent
        {
            playerRef = PlayerRef.None,
            region = matchDoc.fixedRegion,
            fixedEloName = matchDoc.fixedEloName,
            matchType = (byte)matchDoc.matchType,
            matchMode = (byte)matchDoc.matchMode,
            sceneIndex = (byte)matchDoc.mapSceneIndex,
            maxPlayers = matchDoc.maxPlayers,
            sessionName = GetSessionName(matchDoc)
        };
    }
    
    private static string GetSessionName(MatchmakingDocument matchDoc)
    {
        var matchType = Enum.GetName(typeof(MatchType), matchDoc.matchType);
        string roomName = $"{matchDoc.fixedRegion}_{matchType}_{matchDoc.fixedEloName}_max{matchDoc.maxPlayers}_room{matchDoc.maxRoom}";
        return roomName.ToLower();
    }
}