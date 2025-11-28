using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion.GameSystems;
using GDO.Audio;

public class GamePlayState : GameState
{
    public GamePlayState(GameStateMachine stateMachine) : base(stateMachine) {}

    public override void Enter()
    {
        base.Enter();
        // Hide all the UI screens
        EventManager.TriggerEvent<UIEvent>(new UIEvent(UIIDs.None));

        // Show the gameplay UI
        EventManager.TriggerEvent<UIEvent>(new UIEvent(UIIDs.Gameplay));

        // Preload audio
        AudioHelper.PlayBGM(BGM.GAMEPLAY);

        // Set the time scale to normal
        Time.timeScale = 1f;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        
    }
}
