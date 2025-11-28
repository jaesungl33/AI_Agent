using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.GameSystems
{
    public abstract class GameState : IGameState
    {
        protected GameStateMachine stateMachine;

        protected GameState(GameStateMachine stateMachine)
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