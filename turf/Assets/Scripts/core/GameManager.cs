using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PlayerTurnState PlayerTurnState { get; private set; }
    public EnemyTurnState EnemyTurnState { get; private set; }
    public CheckWinState CheckWinState { get; private set; }
    public GameOverState GameOverState { get; private set; }


    private IGameState _currentState;
    public GameState currentState { get; private set; }
    public BaseMatchProfile activePlayer { get; private set; }
    public int actionsRemaining { get; private set; }

    
    void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
        }
        Instance = this;
    }

    


    /* 
    ---------------- GAME LOGIC ------------------
    */
public void InitStates(BaseMatchProfile player, BaseMatchProfile ai)
    {
        PlayerTurnState = new PlayerTurnState(this, player);
        EnemyTurnState = new EnemyTurnState(this, ai);
        CheckWinState = new CheckWinState(this, player, ai);
        GameOverState = new GameOverState(this);
    }

    void Update() => _currentState?.Tick(); // checking for an end turn condition

    public void TransitionTo(GameState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();

        switch (newState)
        {
            case GameState.PlayerTurn:  
                // blah blah write here what it does
                break;
            case GameState.EnemyTurn:    
                StartEnemyTurn();
                // blah blah write here what it does
                break;
            case GameState.CheckWin:    
                CheckWinCondition(); 
                // blah blah write here what it does
                break;
        }
    }

    public enum GameState
    {
        Setup, PlayerTurn, EnemyTurn, CheckWin, GameOver
    }
}
