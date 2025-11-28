using Fusion.GameSystems;
using UnityEngine;
using GDO.Audio;

/// <summary>
/// LobbyState represents the lobby state of the game.
/// It handles the display of the lobby UI and load scene and manages the game's time scale.
/// </summary>
public class LobbyState : GameState
{
    public LobbyState(GameStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();

        LoadUI();
        //LoadScene();
        
        // PreLoad gameplay scene
        EventManager.TriggerEvent(LevelEvent.GamePlay());

        // Preload audio
        EventManager.TriggerEvent(new AudioEvent(SoundType.BackgroundMusic, BGM.LOBBY));

        // Set the time scale to normal
        Time.timeScale = 1f;
    }

    private void LoadUI()
    {
        // Hide all the UI screens
        EventManager.TriggerEvent(new UIEvent(UIIDs.None));

        // Load the lobby scene if not already loaded
        //EventManager.Emit(new UIEvent(UIIDs.ChooseTank));
    }

    public override void Exit()
    {
        base.Exit();
    }
}