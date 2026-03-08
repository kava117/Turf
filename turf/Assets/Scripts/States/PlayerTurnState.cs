using System.Collections.Generic;
using UnityEngine;

public class PlayerTurnState : IGameState
{
    private readonly GameManager      _gm;
    private readonly PlayerMatchData  _player;
    private readonly BoardData        _board;
    private          BoardInput       _input;
    private          List<Vector3Int> _claimableRange = new();

    public PlayerTurnState(GameManager gm, PlayerMatchData player, BoardData board)
    {
        _gm     = gm;
        _player = player;
        _board  = board;
    }

    // Called by GameBootstrapper after BoardInput is created.
    public void SetBoardInput(BoardInput input) => _input = input;

    public void Enter()
    {
        _gm.LastTurnWasPlayer = true;
        _player.ResetActions();
        EventManager.TurnStarted(_player);

        RefreshClaimableRange();
        _input?.SetActive(true);
    }

    public void Tick()
    {
        if (_player.ActionsRemaining <= 0)
            _gm.TransitionTo(_gm.CheckWinState);
    }

    public void Exit()
    {
        _input?.SetActive(false);
        EventManager.HighlightUpdated(new List<Vector3Int>()); // clear highlights
        EventManager.TurnEnded(_player);
    }

    // Called by BoardInput when the player clicks a cell.
    public void TryClaimTile(Vector3Int cell)
    {
        if (!_claimableRange.Contains(cell)) return;

        _player.ClaimTile(cell, _board);
        TileEffectHandler.HandleClaimEffect(cell, _board, _player);
        RefreshVisibility();
        _player.SpendAction();
        RefreshClaimableRange();
    }

    // Force-end the player's turn (called by EndTurnButton).
    public void EndTurn() => _player.ExhaustActions();

    private void RefreshClaimableRange()
    {
        _claimableRange = _player.GetClaimableRange(_board);
        EventManager.HighlightUpdated(_claimableRange);
    }

    private void RefreshVisibility()
    {
        List<Vector3Int> newlyRevealed = _board.RefreshVisibility(_player);
        TileEffectHandler.HandleRevealEffects(newlyRevealed, _board);
    }
}
