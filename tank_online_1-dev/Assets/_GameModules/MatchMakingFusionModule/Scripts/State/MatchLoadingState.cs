using System.Collections;
using System.Collections.Generic;
using Fusion.TankOnlineModule;
using GDO.Audio;
using UnityEngine;

namespace Fusion.GameSystems
{
    public class MatchLoadingState : MatchState
    {
        public MatchLoadingState(GameStateMachine stateMachine) : base(stateMachine) {}

        public override void Enter()
        {
            base.Enter();

            // Hide all the UI screens
            EventManager.TriggerEvent(new UIEvent(UIIDs.None));

            // Show match loading screen
            EventManager.TriggerEvent(new UIEvent(UIIDs.MatchLoading));

            // Load gameplay scene if not already loaded
            EventManager.TriggerEvent<ELevelManagerState>(ELevelManagerState.GAMEPLAY);

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