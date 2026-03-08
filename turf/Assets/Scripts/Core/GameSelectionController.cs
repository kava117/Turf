using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Drives GameSelectionScene. Sets difficulty and game mode, then loads GameScene.
// Endless and Challenge modes are stubbed — only Campaign is implemented.
public class GameSelectionController : MonoBehaviour
{
    [Header("Difficulty")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button hardButton;

    [Header("Mode (Challenge/Endless stubbed)")]
    [SerializeField] private Button campaignButton;

    [Header("Start")]
    [SerializeField] private Button        startButton;
    [SerializeField] private TextMeshProUGUI selectedLabel;

    private AIDifficulty _selectedDifficulty = AIDifficulty.Easy;

    void Start()
    {
        easyButton?.onClick.AddListener(()   => SelectDifficulty(AIDifficulty.Easy));
        mediumButton?.onClick.AddListener(() => SelectDifficulty(AIDifficulty.Medium));
        hardButton?.onClick.AddListener(()   => SelectDifficulty(AIDifficulty.Hard));
        startButton?.onClick.AddListener(StartGame);

        UpdateLabel();
    }

    private void SelectDifficulty(AIDifficulty difficulty)
    {
        _selectedDifficulty = difficulty;
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (selectedLabel != null)
            selectedLabel.text = $"Difficulty: {_selectedDifficulty}";
    }

    private void StartGame()
    {
        SessionData.Difficulty = _selectedDifficulty;
        SceneManager.LoadScene("GameScene");
    }
}
