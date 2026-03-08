using System.Collections.Generic;

// Tracks state for a single run. Lives in memory only — never saved to disk.
// Discarded if the run is lost; bubbled up to PlayerAccountProfile on run end.
public class PlayerRunData
{
    public int           LivesRemaining { get; private set; } = 3;
    public int           MatchesPlayed  { get; private set; } = 0;
    public int           RunWins        { get; private set; } = 0;
    public int           BonusActionsPerTurn { get; private set; } = 0;
    public List<PerkData> ActivePerks   { get; private set; } = new List<PerkData>();

    public void RecordMatchResult(bool won)
    {
        MatchesPlayed++;
        if (won) RunWins++;
        else     LoseLife();
    }

    public void LoseLife()
    {
        LivesRemaining--;
    }

    public bool IsRunOver() => LivesRemaining <= 0;

    // Returns true if the player should be offered a perk after this match.
    public bool IsPerkMatchDue() => MatchesPlayed > 0 && MatchesPlayed % 3 == 0;

    public void AddPerk(PerkData perk)
    {
        if (ActivePerks.Count >= 5) return; // cap at 5 perks
        ActivePerks.Add(perk);
        BonusActionsPerTurn += perk.BonusActionsPerTurn;
    }
}
