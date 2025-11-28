using System.Collections;
using System.Collections.Generic;
using GDO.Audio;
using UnityEngine;

namespace Fusion.GameSystems
{
    public class MatchCancelingState : MatchState
    {
        public MatchCancelingState(GameStateMachine stateMachine) : base(stateMachine) {}

        public override void Enter()
        {
            base.Enter();
            // Hide all the UI screens
            EventManager.TriggerEvent(new UIEvent(UIIDs.None));

            // Show the gameplay UI
            EventManager.TriggerEvent(new UIEvent(UIIDs.Gameplay));

            // Preload audio
            EventManager.TriggerEvent(new AudioEvent(SoundType.BackgroundMusic, BGM.GAMEPLAY));

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
}