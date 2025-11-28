using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.GameSystems
{
    public abstract class MatchState : IGameState
    {
        protected GameStateMachine stateMachine;

        protected MatchState(GameStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }
        
        public virtual void Enter()
        {
            Debug.Log($"{GetType().Name}: Entered");
        }
        public virtual void Exit()
        { 
            Debug.Log($"{GetType().Name}: Exited");
        }
        public virtual void Update() { }
    }
}