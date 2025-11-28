[System.Serializable]
public struct MatchData
{
	private int mapId;
	private MatchType matchType;
	private MatchMode matchMode;
	private MatchTeamData[] teams;
	private int maxPlayers;
	private int matchDuration; // Time of match in seconds
	private MatchmakingDocument document;

	public int MapId { get => mapId; set => mapId = value; }
	public MatchType MatchType { get => matchType; set => matchType = value; }
	public MatchMode MatchMode { get => matchMode; set => matchMode = value; }
	public MatchTeamData[] Teams { get => teams; set => teams = value; }
	public int MaxPlayers { get => maxPlayers; set => maxPlayers = value; }
	public int MatchDuration { get => matchDuration; set => matchDuration = value; }
	public MatchmakingDocument Document
	{
		get => document; set => document = value;
	}
	public MatchData(int mapId, MatchType matchType, MatchMode matchMode, MatchTeamData[] teams, int maxPlayers, int matchDuration, MatchmakingDocument matchmakingDocument)
	{
		this.mapId = mapId;
		this.matchType = matchType;
		this.matchMode = matchMode;
		this.teams = teams;
		this.maxPlayers = maxPlayers;
		this.matchDuration = matchDuration;
		this.document = matchmakingDocument;
	}
}