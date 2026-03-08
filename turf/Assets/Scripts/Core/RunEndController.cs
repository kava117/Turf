using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Drives RunEndScene. Shows the run's final result and returns to main menu.
public class RunEndController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultLabel;
    [SerializeField] private TextMeshProUGUI statsLabel;
    [SerializeField] private Button          mainMenuButton;

    void Start()
    {
        PlayerRunData run = SessionData.ActiveRun;

        if (resultLabel != null)
            resultLabel.text = run != null && run.RunWins > 0 ? "Run Complete!" : "Run Failed";

        if (statsLabel != null && run != null)
            statsLabel.text = $"Matches Won: {run.RunWins}\nMatches Played: {run.MatchesPlayed}";

        mainMenuButton?.onClick.AddListener(() =>
        {
            SessionData.Reset();
            SceneManager.LoadScene("MainMenuScene");
        });
    }
}
