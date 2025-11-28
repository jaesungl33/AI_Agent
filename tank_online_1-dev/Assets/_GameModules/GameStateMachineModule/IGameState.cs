namespace Fusion.GameSystems
{
    public interface IGameState
    {
        void Enter();
        void Update();
        void Exit();
    }
}