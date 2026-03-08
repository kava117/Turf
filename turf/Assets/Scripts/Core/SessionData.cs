// Scene-to-scene data bus. Set before loading GameScene; read by GameBootstrapper.
// Not saved to disk — lives in memory only.
public static class SessionData
{
    public static PlayerRunData ActiveRun { get; set; }
    public static AIDifficulty  Difficulty { get; set; } = AIDifficulty.Easy;
    public static int           MatchNumber { get; set; } = 1;

    public static void Reset()
    {
        ActiveRun   = null;
        Difficulty  = AIDifficulty.Easy;
        MatchNumber = 1;
    }
}
