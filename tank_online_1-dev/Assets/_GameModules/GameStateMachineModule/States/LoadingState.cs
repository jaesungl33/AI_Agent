using Fusion.GameSystems;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// LoadingState represents the loading state of the game.
/// + Check game version.
/// + Load game configuration.
/// </summary>
public class LoadingState : GameState
{
    public LoadingState(GameStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();

        // Hide all the UI screens
        EventManager.TriggerEvent(new UIEvent(UIIDs.None));

        // Show loading UI
        EventManager.TriggerEvent(new UIEvent(UIIDs.Loading));

        // Set the time scale to normal
        Time.timeScale = 1f;
    }
}