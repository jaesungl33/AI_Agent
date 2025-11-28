namespace Fusion.GameSystems
{
    public class GameStateMachine
    {
        private IGameState currentState;

        public void Change(IGameState newState)
        {
            if(currentState == newState) return;
            
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }

        public void Update()
        {
            currentState?.Update();
        }
    }
}