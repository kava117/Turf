using System.Collections.Generic;
using UnityEngine;

// Abstract base for all in-match player representations (human and AI).
// Plain C# — no MonoBehaviour. One instance per player per match.
public abstract class BaseMatchProfile
{
    public string          DisplayName      { get; protected set; }
    public int             ActionsPerTurn   { get; protected set; }
    public int             ActionsRemaining { get; protected set; }
    public List<Vector3Int> OwnedTiles      { get; protected set; } = new List<Vector3Int>();

    // Returns all tiles this player is currently allowed to claim.
    // Implemented differently by PlayerMatchData and AIMatchData (same logic, but kept open for future divergence).
    public abstract List<Vector3Int> GetClaimableRange(BoardData board);

    // All ownership changes go through BoardData.TransferOwnership to keep OwnedTiles in sync.
    public void ClaimTile(Vector3Int cell, BoardData board)
    {
        TileData tile = board.GetTile(cell);
        if (tile == null || !tile.CanBeClaimed()) return;
        board.TransferOwnership(cell, this);
    }

    // Called by BoardData.TransferOwnership when this player loses a tile.
    public void LoseTile(Vector3Int cell)
    {
        OwnedTiles.Remove(cell);
    }

    public void SpendAction()
    {
        ActionsRemaining--;
        EventManager.NumActionsChanged(ActionsRemaining);
    }

    public void ResetActions()
    {
        ActionsRemaining = ActionsPerTurn;
        EventManager.NumActionsChanged(ActionsRemaining);
    }

    // Forfeits all remaining actions (used by EndTurnButton).
    public void ExhaustActions()
    {
        ActionsRemaining = 0;
        EventManager.NumActionsChanged(ActionsRemaining);
    }
}
