using Unity.VisualScripting;
using UnityEngine;

public class PlayerTurnState : IGameState
{
    private GameManager gm;

    public PlayerTurnState(GameManager _gm)
    {
        gm = _gm;
    }

    public void Enter()
    {
        gm.ResetActions();
        EventBus.TurnStarted(gm.ActivePlayer);
    }

    public void Tick()
    {
        // check for input using whatever class i end up making
        if (gm.ActionsRemaining <= 0)
        {
            gm.TransitionTo(gm.EnemyTurnState);
        }
    }

    public void Exit()
    {
        // add cleanup before leaving player turn
    }
}
