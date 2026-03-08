using System.Collections.Generic;

// Permanent player data. The only object saved to disk.
// Updated at the end of each match via RecordMatchResult().
[System.Serializable]
public class PlayerAccountProfile
{
    public string       PlayerName          { get; set; } = "Player";
    public int          TotalMatchesPlayed  { get; private set; }
    public int          TotalWins           { get; private set; }
    public int          TotalLosses         { get; private set; }
    public int          TotalTilesCaptured  { get; private set; }
    public List<string> Achievements        { get; private set; } = new List<string>();
    public List<string> UnlockedPerks       { get; private set; } = new List<string>(); // perk IDs unlocked for future runs

    public void RecordMatchResult(bool won, int tilesCaptured)
    {
        TotalMatchesPlayed++;
        TotalTilesCaptured += tilesCaptured;

        if (won) TotalWins++;
        else     TotalLosses++;
    }

    public void GrantAchievement(string achievementId)
    {
        if (!Achievements.Contains(achievementId))
            Achievements.Add(achievementId);
    }

    public void UnlockPerk(string perkId)
    {
        if (!UnlockedPerks.Contains(perkId))
            UnlockedPerks.Add(perkId);
    }
}
