using Unity.VisualScripting;
using UnityEngine;

public class EnemyTurnState : IGameState
{
    private GameManager gm;

    public EnemyTurnState (GameManager _gm)
    {
        gm = _gm;
    }

    public void Enter()
    {
    }

    public void Tick()
    {
        // check for input using whatever class i end up making
    }

    public void Exit()
    {
        // add cleanup before leaving player turn
    }
}
