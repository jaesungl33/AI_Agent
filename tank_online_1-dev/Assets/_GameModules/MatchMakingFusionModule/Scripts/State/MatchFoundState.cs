using System.Collections;
using System.Collections.Generic;
using GDO.Audio;
using UnityEngine;

namespace Fusion.GameSystems
{
    public class MatchFoundState : MatchState
    {
        public MatchFoundState(GameStateMachine stateMachine) : base(stateMachine) {}

        public override void Enter()
        {
            base.Enter();

            // Load lobby scene if not already loaded
            // EventManager.Emit<ELevelManagerState>(ELevelManagerState.LOBBY);

            // Preload audio
            AudioHelper.PlayBGM(BGM.LOBBY);

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