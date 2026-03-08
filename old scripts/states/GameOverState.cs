using Unity.VisualScripting;
using UnityEngine;

public class GameOverState : IGameState
{
    private GameManager gm;

    public GameOverState(GameManager _gm)
    {
        gm = _gm;
    }

    public void Enter()
    {
        
    }

    public void Tick()
    {
        
    }

    public void Exit()
    {
        // add cleanup before leaving player turn
    }
}
