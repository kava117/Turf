// Contract for all game states.
// States are plain C# — no MonoBehaviour.
// GameManager calls these in order: Enter → Tick (each Update) → Exit.
public interface IGameState
{
    void Enter();
    void Tick();
    void Exit();
}
