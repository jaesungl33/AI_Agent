using Fusion.GameSystems;
using UnityEngine;
using GDO.Audio;

/// <summary>
/// LobbyState represents the lobby state of the game.
/// It handles the display of the lobby UI and load scene and manages the game's time scale.
/// </summary>
public class AuthState : GameState
{
    public AuthState(GameStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();

        EventManager.TriggerEvent(new UIEvent(UIIDs.Auth));

        // Set the time scale to normal
        Time.timeScale = 1f;
    }

    public override void Exit()
    {
        base.Exit();
    }
}