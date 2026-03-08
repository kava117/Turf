using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Handles all special tile behaviors triggered after a tile is claimed or revealed.
// Static — no state. Called by PlayerTurnState and EnemyTurnState after each claim.
public static class TileEffectHandler
{
    // Call after any tile is claimed. Handles Cave and Wizard special consumption.
    public static void HandleClaimEffect(Vector3Int claimedCell, BoardData board, BaseMatchProfile claimer)
    {
        TileData tile = board.GetTile(claimedCell);
        if (tile == null) return;

        if (tile.Type == TileType.Cave)
            HandleCaveEffect(claimedCell, board, claimer);

        if (tile.Type == TileType.Wizard)
            HandleWizardEffect(claimedCell, board, claimer);
    }

    // If a Cave was reached via teleport (not adjacent to any owned tile), mark
    // both the origin Cave(s) and the destination Cave as used.
    private static void HandleCaveEffect(Vector3Int claimedCell, BoardData board, BaseMatchProfile claimer)
    {
        bool isAdjacentToOwned = board.GetAdjacentCells(claimedCell)
            .Any(c => board.GetTile(c)?.Owner == claimer);

        if (isAdjacentToOwned) return; // claimed normally — no teleport consumed

        // Mark the destination cave as used.
        board.GetTile(claimedCell).CaveUsed = true;

        // Mark all of this player's owned Caves (excluding the destination) as used.
        foreach (Vector3Int ownedCell in claimer.OwnedTiles)
        {
            if (ownedCell == claimedCell) continue;
            TileData t = board.GetTile(ownedCell);
            if (t != null && t.Type == TileType.Cave && !t.CaveUsed)
                t.CaveUsed = true;
        }
    }

    // If a tile was claimed via Wizard's global ability (not reachable by normal range),
    // mark the Wizard tile as used.
    private static void HandleWizardEffect(Vector3Int claimedCell, BoardData board, BaseMatchProfile claimer)
    {
        // If the tile is in the normal range, no special was consumed.
        if (board.GetNormalClaimRange(claimer).Contains(claimedCell)) return;

        // Find and consume the player's unused Wizard tile.
        foreach (Vector3Int ownedCell in claimer.OwnedTiles)
        {
            TileData t = board.GetTile(ownedCell);
            if (t != null && t.Type == TileType.Wizard && !t.WizardUsed)
            {
                t.WizardUsed = true;
                break;
            }
        }
    }

    // Call after RefreshVisibility returns newly revealed cells.
    // Triggers Barbarian charges for any freshly revealed, uncharged Barbarian tiles.
    public static void HandleRevealEffects(List<Vector3Int> newlyRevealedCells, BoardData board)
    {
        foreach (Vector3Int cell in newlyRevealedCells)
        {
            TileData tile = board.GetTile(cell);
            if (tile != null && tile.Type == TileType.Barbarian && !tile.BarbarianCharged)
                TriggerBarbarianCharge(cell, board);
        }
    }

    // Charges down the longer board axis, unclaiming every tile in that row/column.
    private static void TriggerBarbarianCharge(Vector3Int barbarianCell, BoardData board)
    {
        board.GetTile(barbarianCell).BarbarianCharged = true;

        // Pick the longer axis.
        bool chargeHorizontal = board.Width >= board.Height;

        if (chargeHorizontal)
        {
            int y = barbarianCell.y;
            for (int x = 0; x < board.Width; x++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileData tile   = board.GetTile(cell);
                if (tile != null && tile.Owner != null)
                    board.TransferOwnership(cell, null);
            }
        }
        else
        {
            int x = barbarianCell.x;
            for (int y = 0; y < board.Height; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileData tile   = board.GetTile(cell);
                if (tile != null && tile.Owner != null)
                    board.TransferOwnership(cell, null);
            }
        }
    }
}
