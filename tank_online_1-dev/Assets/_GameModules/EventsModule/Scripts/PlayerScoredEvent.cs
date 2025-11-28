/// <summary>
/// PlayerScoredEvent is an event that is triggered when a player scores in the game.
/// It contains the score value associated with the event.
/// </summary>
public struct PlayerScoredEvent
{
    public int score;

    public PlayerScoredEvent(int s)
    {
        score = s;
    }
}