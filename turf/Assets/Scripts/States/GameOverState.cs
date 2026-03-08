using UnityEngine.SceneManagement;

// Handles end-of-match cleanup, data flow, and scene transition.
public class GameOverState : IGameState
{
    private readonly GameManager      _gm;
    private readonly PlayerMatchData  _player;
    private readonly AIMatchData      _ai;

    public GameOverState(GameManager gm, PlayerMatchData player)
    {
        _gm     = gm;
        _player = player;
        // AI reference fetched from CheckWinState context — stored via Init.
    }

    // Called by GameManager.Init after all states and players are created.
    private AIMatchData _ai2; // set via SetAI
    public void SetAI(AIMatchData ai) => _ai2 = ai;

    public void Enter()
    {
        // Player wins if they have more tiles. Tie goes to the player.
        bool playerWon = _player.OwnedTiles.Count >= (_ai2?.OwnedTiles.Count ?? 0);

        PlayerRunData        run     = SessionData.ActiveRun;
        PlayerAccountProfile account = SaveManager.Load();

        run.RecordMatchResult(playerWon);
        account.RecordMatchResult(playerWon, _player.OwnedTiles.Count);
        AchievementManager.Check(account, run);
        SaveManager.Save(account);

        SessionData.MatchNumber++;

        UIManager.Instance?.ShowGameOver(playerWon, LoadNextScene);
    }

    public void Tick()  { }
    public void Exit()  { }

    private void LoadNextScene()
    {
        if (SessionData.ActiveRun.IsRunOver())
            SceneManager.LoadScene("RunEndScene");
        else
            SceneManager.LoadScene("MetaScene");
    }
}
