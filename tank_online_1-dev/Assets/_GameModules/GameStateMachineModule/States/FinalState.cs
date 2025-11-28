using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion.GameSystems;
using GDO.Audio;

public class FinalState : GameState
{
    public FinalState(GameStateMachine stateMachine) : base(stateMachine) {}

    public override void Enter()
    {
        base.Enter();

        // Hide all the UI screens
        EventManager.TriggerEvent(new UIEvent(UIIDs.None));

        // Show the final UI
        EventManager.TriggerEvent(new UIEvent(UIIDs.Final));

        EventManager.TriggerEvent(new AudioEvent(SoundType.BackgroundMusic, BGM.VICTORY));

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
