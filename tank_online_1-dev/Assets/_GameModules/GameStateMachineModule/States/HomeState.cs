using Fusion.GameSystems;
using UnityEngine;
using UnityEngine.SceneManagement;
using GDO.Audio;
using Fusion.TankOnlineModule;

/// <summary>
/// MainMenuState represents the main menu state of the game.
/// It handles the display of the main menu UI and load scene and manages the game's time scale.
/// </summary>
public class HomeState : GameState
{
    public HomeState(GameStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        // Hide all the UI screens
        EventManager.TriggerEvent(new UIEvent(UIIDs.None));

        if (SceneManager.GetActiveScene().buildIndex != SceneIDs.MainMenu)
        {
            // load scene & show the UI
            AsyncOperation op = SceneManager.LoadSceneAsync(SceneIDs.MainMenu);
            op.completed += (asyncOp) =>
            {
                // Cancel matchmaking if in progress
                EventManager.TriggerEvent(MatchmakingEvent.Exit());
                EventManager.TriggerEvent(new UIEvent(UIIDs.Home));
            };
        }
        else
        {
            // Cancel matchmaking if in progress
            EventManager.TriggerEvent(MatchmakingEvent.Exit());
            // only show the UI
            EventManager.TriggerEvent(new UIEvent(UIIDs.Home));
        }

        // Preload audio
        EventManager.TriggerEvent(new AudioEvent(SoundType.BackgroundMusic, BGM.HOME));

        // Set the time scale to normal
        Time.timeScale = 1f;
    }

    public override void Exit()
    {
        base.Exit();
    }
}