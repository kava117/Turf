using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set };

    public GameState CurrentState { get; private set}
    public Player ActivePlayer { get; private set}
    public int ActionsRemaining { get; private set}

    
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
    public void TransitionTo(GameState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GameState.PlayerTurn:  StartPlayerTurn(); break;
            case GameState.EnemyTurn:    StartEnemyTurn(); break;
            case GameState.CheckWin:    CheckWinCondition(); break;
        }
    }

    public enum GameState
    {
        Setup, PlayerTurn, EnemyTurn, CheckWin, GameOver
    }
}
