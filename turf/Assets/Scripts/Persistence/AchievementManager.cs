// Evaluates achievement conditions at the end of each match and grants them.
// Add new achievements here as the design grows — no other files need to change.
public static class AchievementManager
{
    public static void Check(PlayerAccountProfile account, PlayerRunData run)
    {
        // First Win
        if (account.TotalWins == 1)
            account.GrantAchievement("first_win");

        // Veteran — 10 matches played
        if (account.TotalMatchesPlayed >= 10)
            account.GrantAchievement("veteran");

        // Land Baron — 100 tiles captured across all time
        if (account.TotalTilesCaptured >= 100)
            account.GrantAchievement("land_baron");

        // Survivor — completed a run without losing a life
        if (run.IsRunOver() == false && run.LivesRemaining == 3)
            account.GrantAchievement("survivor");
    }
}
