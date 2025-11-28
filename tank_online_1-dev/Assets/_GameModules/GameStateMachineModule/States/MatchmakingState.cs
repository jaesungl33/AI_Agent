using Fusion.GameSystems;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MatchmakingState represents the matchmaking state of the game.
/// It handles the display of the matchmaking UI and load scene and manages the game's time scale.
/// </summary>
public class MatchmakingState : GameState
{
    public MatchmakingState(GameStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        // Lock find until joined a room
        EventManager.TriggerEvent(MainMenuEvent.Find());
        // Start matchmaking process
        EventManager.TriggerEvent(MatchmakingEvent.Find());
        // Set the time scale to normal
        Time.timeScale = 1f;
    }

    public override void Exit()
    {
        base.Exit();
    }
}