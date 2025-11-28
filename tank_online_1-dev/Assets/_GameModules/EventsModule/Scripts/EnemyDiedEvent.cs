/// <summary>
/// EnemyDiedEvent is an event that is triggered when an enemy dies in the game.
/// It contains the ID of the enemy that died.
/// </summary>
public struct EnemyDiedEvent
{
    public int enemyId;

    public EnemyDiedEvent(int id)
    {
        enemyId = id;
    }
}