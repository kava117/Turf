using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// AI decision logic. Completely separate from AIMatchData.
// Receives AIMatchData + BoardData; no other dependencies.
public class AIController
{
    public void TakeTurn(AIMatchData ai, BoardData board)
    {
        while (ai.ActionsRemaining > 0)
        {
            List<Vector3Int> claimable = ai.GetClaimableRange(board);
            if (claimable.Count == 0) break;

            Vector3Int chosen = ai.Difficulty switch
            {
                AIDifficulty.Easy   => GreedyMove(ai, board, claimable),
                AIDifficulty.Medium => GreedyDefensiveMove(ai, board, claimable),
                AIDifficulty.Hard   => LookaheadMove(ai, board, claimable),
                _                   => claimable[0]
            };

            ai.ClaimTile(chosen, board);
            TileEffectHandler.HandleClaimEffect(chosen, board, ai);
            ai.SpendAction();
        }
    }

    // ── Easy: pick the tile that opens the most new frontier tiles ───────────

    private Vector3Int GreedyMove(AIMatchData ai, BoardData board, List<Vector3Int> claimable)
    {
        return claimable.OrderByDescending(c => FrontierScore(c, ai, board)).First();
    }

    // ── Medium: greedy + penalise tiles that expand the player's reach ───────

    private Vector3Int GreedyDefensiveMove(AIMatchData ai, BoardData board, List<Vector3Int> claimable)
    {
        // Find the human player by checking tile owners.
        BaseMatchProfile humanPlayer = GetHumanPlayer(ai, board);

        return claimable.OrderByDescending(c =>
        {
            float score = FrontierScore(c, ai, board);

            // Bonus if this tile is in the player's current claimable range (blocking).
            if (humanPlayer != null)
            {
                List<Vector3Int> playerRange = humanPlayer.GetClaimableRange(board);
                if (playerRange.Contains(c)) score += 2f;
            }

            return score;
        }).First();
    }

    // ── Hard: 2-step lookahead, picks first move of the best 2-move sequence ─

    private Vector3Int LookaheadMove(AIMatchData ai, BoardData board, List<Vector3Int> claimable)
    {
        Vector3Int bestFirst  = claimable[0];
        float      bestScore  = float.MinValue;

        foreach (Vector3Int first in claimable)
        {
            // Simulate claiming 'first'.
            float immediateScore = FrontierScore(first, ai, board);

            // Score the best follow-up tile after claiming 'first'.
            // We approximate by checking what the claimable range would look like
            // if 'first' were owned (without actually mutating board state).
            float followScore = SimulateBestFollowup(first, ai, board);

            float total = immediateScore + followScore * 0.8f; // discount lookahead
            if (total > bestScore) { bestScore = total; bestFirst = first; }
        }

        return bestFirst;
    }

    // ── Scoring Helpers ───────────────────────────────────────────────────────

    // Score = number of unclaimed claimable tiles adjacent to the candidate cell.
    private float FrontierScore(Vector3Int candidate, AIMatchData ai, BoardData board)
    {
        return board.GetAdjacentCells(candidate)
            .Count(c =>
            {
                TileData t = board.GetTile(c);
                return t != null && t.CanBeClaimed() && t.IsUnclaimed;
            });
    }

    // Approximates the best single follow-up move if 'first' were claimed.
    // Does NOT mutate board state — uses adjacency geometry only.
    private float SimulateBestFollowup(Vector3Int first, AIMatchData ai, BoardData board)
    {
        // Tiles reachable from 'first' (simple adjacency) that are currently unclaimed.
        List<Vector3Int> followups = board.GetAdjacentCells(first)
            .Where(c =>
            {
                TileData t = board.GetTile(c);
                return t != null && t.CanBeClaimed() && t.IsUnclaimed && c != first;
            }).ToList();

        if (followups.Count == 0) return 0f;

        return followups.Max(c => FrontierScore(c, ai, board));
    }

    private BaseMatchProfile GetHumanPlayer(AIMatchData ai, BoardData board)
    {
        return board.GetAllPlayers().FirstOrDefault(p => p != ai);
    }
}
