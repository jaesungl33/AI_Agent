/// <summary>
/// TankSelectedEvent is an event that is triggered when a tank is selected in the game.
/// It contains the index of the selected tank.
/// </summary>
public struct TankSelectedEvent
{
    public int playerIndex;
    public string tankId;

    public TankSelectedEvent(int playerIndex, string tankId)
    {
        this.playerIndex = playerIndex;
        this.tankId = tankId;
    }
}