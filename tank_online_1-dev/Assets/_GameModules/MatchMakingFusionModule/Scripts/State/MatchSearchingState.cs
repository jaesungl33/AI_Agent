using System.Collections;
using System.Collections.Generic;
using GDO.Audio;
using UnityEngine;

namespace Fusion.GameSystems
{
    public class MatchSearchingState : MatchState
    {
        public MatchSearchingState(GameStateMachine stateMachine) : base(stateMachine) {}
        public override void Enter()
        {
            base.Enter();
            // Lock find until joined a room
            EventManager.TriggerEvent(MainMenuEvent.Find());
            // Start matchmaking process
            EventManager.TriggerEvent(MatchmakingEvent.Find());
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