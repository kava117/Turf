using UnityEngine;

// Owns the state machine and drives the Update loop.
// Does not contain game rules — only transitions between states.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // State instances — created once by Init(), reused each transition.
    public PlayerTurnState PlayerTurnState { get; private set; }
    public EnemyTurnState  EnemyTurnState  { get; private set; }
    public CheckWinState   CheckWinState   { get; private set; }
    public GameOverState   GameOverState   { get; private set; }

    public BaseMatchProfile ActivePlayer { get; private set; }

    // Set by PlayerTurnState/EnemyTurnState so CheckWinState knows where to go next.
    public bool LastTurnWasPlayer { get; set; }

    private IGameState _currentState;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Called by GameBootstrapper after all data and systems are ready.
    public void Init(PlayerMatchData player, AIMatchData ai, BoardData board)
    {
        PlayerTurnState = new PlayerTurnState(this, player, board);
        EnemyTurnState  = new EnemyTurnState(this, ai, new AIController(), board);
        CheckWinState   = new CheckWinState(this, player, ai, board);
        GameOverState   = new GameOverState(this, player);
        GameOverState.SetAI(ai);
    }

    void Update()
    {
        _currentState?.Tick();
    }

    public void TransitionTo(IGameState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }
}
