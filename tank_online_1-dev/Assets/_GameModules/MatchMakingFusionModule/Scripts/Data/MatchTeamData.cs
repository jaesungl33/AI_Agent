using System.Linq;
using Fusion.TankOnlineModule;

[System.Serializable]
public struct MatchTeamData
{
	private int teamIndex;
	private string teamName;
	private MatchTeamType teamType;
	private MatchConditionData[] winConditions;
	private Player[] players;
	private MatchPlayerData[] matchPlayerDatas;
	public int TeamIndex => teamIndex;
	public string TeamName => teamName;
	public MatchTeamType TeamType => teamType;
	public MatchConditionData[] WinConditions => winConditions;
	public int Kill => matchPlayerDatas.Sum(p => p.Kill);
	public int Death => matchPlayerDatas.Sum(p => p.Death);

	public bool WinConditionMet()
	{
		foreach (var condition in winConditions)
		{
			return condition.IsWinner();
		}
		return false;
	}
}