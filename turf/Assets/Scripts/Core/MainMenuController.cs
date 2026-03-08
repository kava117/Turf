using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Drives MainMenuScene. Handles new run, continue (load account), and quit.
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button newRunButton;
    [SerializeField] private Button continueButton; // visible only if a save exists
    [SerializeField] private Button quitButton;

    void Start()
    {
        if (newRunButton  != null) newRunButton.onClick.AddListener(OnNewRun);
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(SaveManager.SaveExists());
            continueButton.onClick.AddListener(OnContinue);
        }
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
    }

    private void OnNewRun()
    {
        SessionData.Reset();
        SessionData.ActiveRun = new PlayerRunData();
        SceneManager.LoadScene("GameSelectionScene");
    }

    private void OnContinue()
    {
        // Accounts are persistent; runs are not saved.
        // "Continue" means: load account data and start a fresh run.
        SessionData.Reset();
        SessionData.ActiveRun = new PlayerRunData();
        SceneManager.LoadScene("GameSelectionScene");
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
