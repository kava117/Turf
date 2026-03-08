using System.Collections.Generic;
using System.Linq;

// Checks whether all claimable tiles are owned after each turn.
// If so, finds the winner by tile count and transitions to GameOverState.
// Otherwise routes back to the correct next turn.
public class CheckWinState : IGameState
{
    private readonly GameManager        _gm;
    private readonly PlayerMatchData    _player;
    private readonly AIMatchData        _ai;
    private readonly BoardData          _board;

    public CheckWinState(GameManager gm, PlayerMatchData player, AIMatchData ai, BoardData board)
    {
        _gm     = gm;
        _player = player;
        _ai     = ai;
        _board  = board;
    }

    public void Enter()
    {
        int totalClaimable = _board.CountClaimableTiles();
        int totalOwned     = _board.CountOwnedTiles();

        if (totalOwned >= totalClaimable)
        {
            // All tiles claimed — determine winner by majority.
            BaseMatchProfile winner = _player.OwnedTiles.Count >= _ai.OwnedTiles.Count
                ? (BaseMatchProfile)_player
                : _ai;

            EventManager.GameOver(winner);
            _gm.TransitionTo(_gm.GameOverState);
        }
        else
        {
            // Game continues — alternate turns.
            if (_gm.LastTurnWasPlayer)
                _gm.TransitionTo(_gm.EnemyTurnState);
            else
                _gm.TransitionTo(_gm.PlayerTurnState);
        }
    }

    public void Tick()  { }
    public void Exit()  { }
}
