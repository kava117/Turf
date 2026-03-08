using System.Collections.Generic;

public class EnemyTurnState : IGameState
{
    private readonly GameManager   _gm;
    private readonly AIMatchData   _ai;
    private readonly AIController  _controller;
    private readonly BoardData     _board;

    public EnemyTurnState(GameManager gm, AIMatchData ai, AIController controller, BoardData board)
    {
        _gm         = gm;
        _ai         = ai;
        _controller = controller;
        _board      = board;
    }

    public void Enter()
    {
        _gm.LastTurnWasPlayer = false;
        _ai.ResetActions();
        EventManager.TurnStarted(_ai);

        // AI acts synchronously. A coroutine-based delay can be added later for visual pacing.
        _controller.TakeTurn(_ai, _board);

        // Reveal any tiles the AI's new territory exposes (triggers Barbarians too).
        List<Vector3Int> revealed = _board.RefreshVisibility(_ai);
        TileEffectHandler.HandleRevealEffects(revealed, _board);

        _gm.TransitionTo(_gm.CheckWinState);
    }

    public void Tick()  { }
    public void Exit()  => EventManager.TurnEnded(_ai);
}
