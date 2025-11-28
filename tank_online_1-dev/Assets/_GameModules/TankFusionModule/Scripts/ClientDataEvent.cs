using System.Linq;
using Fusion;

/// <summary>
/// Use this struct to send player-related data during matchmaking and choose tank phase.
/// </summary>
[System.Serializable]
public struct ClientDataEvent : INetworkStruct
{
    public PlayerRef playerRef;
    public int playerIndex;
    public int playerEloPoint;
    public byte matchType;
    public byte matchMode;
    public byte maxPlayers;
    public byte teamIndex;
    public NetworkString<_16> region;
    public NetworkString<_16> fixedEloName;
    public NetworkString<_32> sessionName;
    public NetworkString<_16> playerName;
    public NetworkString<_16> playerSelectedAvatar;
    public NetworkString<_16> playerSelectedTankId;
    public int wrapId;
    public static ClientDataEvent CancelSearching()
    {
        return default;
    }

    public static ClientDataEvent StartSearching(PlayerDocument playerDoc, MatchmakingDocument matchDoc, TankDocument tankDoc)
    {
        return new ClientDataEvent
        {
            playerRef = PlayerRef.None,
            region = "asia",
            fixedEloName = GetEloTier(),
            playerEloPoint = playerDoc.elo,
            matchType = (byte)matchDoc.MatchType,
            matchMode = (byte)matchDoc.MatchMode,
            maxPlayers = matchDoc.MaxPlayers,
            sessionName = new NetworkString<_32>(string.Empty),
            playerName = playerDoc.playerName,
            playerSelectedAvatar = playerDoc.playerAvatar,
            playerSelectedTankId = tankDoc.tankId,
            wrapId = playerDoc.GetWrapId(tankDoc.tankId)
        };
    }

    private static string GetEloTier()
    {
        PlayerDocument playerDoc = DatabaseManager.GetDB<PlayerCollection>().GetMine();
        MatchmakingDocument matchDoc = DatabaseManager.GetDB<MatchmakingCollection>().GetActiveDocument();
        foreach (var tier in matchDoc.PlayerElos.OrderByDescending(x => x.eloPoint))
        {
            if (playerDoc.elo >= tier.eloPoint)
                return tier.eloName;
        }
        return "Unknown";
    }
}