using System.Collections;
using System.Collections.Generic;
using GDO.Audio;
using UnityEngine;

namespace Fusion.GameSystems
{
    public class MatchPickingState : MatchState
    {
        public MatchPickingState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            
            // Hide all the UI screens
            EventManager.TriggerEvent<UIEvent>(new UIEvent(UIIDs.None));

            // Load the lobby scene if not already loaded
            EventManager.TriggerEvent<UIEvent>(new UIEvent(UIIDs.ChooseTank));

            // allow picking tank
            EventManager.TriggerEvent<ChooseTankEvent>(new ChooseTankEvent() { State = ServerState.PICKING });

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