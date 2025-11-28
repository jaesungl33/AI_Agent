[System.Serializable]
public class MatchConditionData
{
	public MatchConditionType conditionType;
	public int value;

	public bool IsWinner()
	{
		return value <= 0;
	}
}