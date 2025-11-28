using System.Collections;
using System.Collections.Generic;
using Fusion.TankOnlineModule;
using GDO.Audio;
using UnityEngine;

namespace Fusion.GameSystems
{
    public class MatchPlayingState : MatchState
    {
        public MatchPlayingState(GameStateMachine stateMachine) : base(stateMachine) {}

        public override void Enter()
        {
            base.Enter();

            // change play game state
            EventManager.TriggerEvent<ServerStateEvent>(new ServerStateEvent{NewState = ServerState.LEVEL});

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