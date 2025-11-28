using System.Collections;
using System.Collections.Generic;
using Fusion.TankOnlineModule;
using GDO.Audio;
using UnityEngine;

namespace Fusion.GameSystems
{
    public class MatchCountdownState : MatchState
    {
        public MatchCountdownState(GameStateMachine stateMachine) : base(stateMachine) {}

        public override void Enter()
        {
            base.Enter();
            
            // Hide all the UI screens
            EventManager.TriggerEvent<UIEvent>(new UIEvent(UIIDs.None));

            // Show the gameplay UI
            EventManager.TriggerEvent(new UIEvent(UIIDs.Gameplay));

            EventManager.TriggerEvent(new TurretSpawnerEvent());

            // Start countdown 
            EventManager.TriggerEvent<ELevelManagerState>(ELevelManagerState.COUNTDOWN);

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