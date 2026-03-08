using System.Collections.Generic;

public enum AIDifficulty { Easy, Medium, Hard }

// AI player's in-match state.
// Treated identically to PlayerMatchData by all game systems.
// No upward reference to run or account data.
public class AIMatchData : BaseMatchProfile
{
    public AIDifficulty Difficulty { get; private set; }

    public AIMatchData(AIDifficulty difficulty)
    {
        Difficulty   = difficulty;
        DisplayName  = "AI";

        ActionsPerTurn = difficulty switch
        {
            AIDifficulty.Easy   => 2,
            AIDifficulty.Medium => 3,
            AIDifficulty.Hard   => 4,
            _                   => 2
        };

        ActionsRemaining = ActionsPerTurn;
    }

    public override List<Vector3Int> GetClaimableRange(BoardData board)
    {
        return board.GetClaimableTilesFor(this);
    }
}
