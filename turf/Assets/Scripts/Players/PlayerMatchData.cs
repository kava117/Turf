using System.Collections.Generic;
using UnityEngine;

// Human player's in-match state.
// Holds a reference to the current run for perk application.
public class PlayerMatchData : BaseMatchProfile
{
    public PlayerRunData RunData { get; private set; }

    public PlayerMatchData(PlayerRunData runData)
    {
        RunData         = runData;
        DisplayName     = "Player";
        ActionsPerTurn  = 1 + runData.BonusActionsPerTurn;
        ActionsRemaining = ActionsPerTurn;
    }

    // Returns all unclaimed, claimable tiles adjacent to any owned tile.
    // Special tile range extensions are handled by BoardData.GetClaimableTilesFor().
    public override List<Vector3Int> GetClaimableRange(BoardData board)
    {
        return board.GetClaimableTilesFor(this);
    }
}
