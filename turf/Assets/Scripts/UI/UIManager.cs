using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Singleton that coordinates all in-game UI panels.
// Reacts to EventManager events; does not contain game logic.
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI actionsLabel;
    [SerializeField] private TextMeshProUGUI turnLabel;
    [SerializeField] private EndTurnButton   endTurnButton;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject          gameOverPanel;
    [SerializeField] private TextMeshProUGUI     gameOverLabel;
    [SerializeField] private Button              continueButton;

    private Action _onContinue;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        gameOverPanel?.SetActive(false);
    }

    void OnEnable()
    {
        EventManager.OnNumActionsChanged += HandleActionsChanged;
        EventManager.OnTurnStarted       += HandleTurnStarted;
        EventManager.OnTurnEnded         += HandleTurnEnded;
        EventManager.OnGameOver          += HandleGameOver;
    }

    void OnDisable()
    {
        EventManager.OnNumActionsChanged -= HandleActionsChanged;
        EventManager.OnTurnStarted       -= HandleTurnStarted;
        EventManager.OnTurnEnded         -= HandleTurnEnded;
        EventManager.OnGameOver          -= HandleGameOver;
    }

    // ── Event Handlers ───────────────────────────────────────────────────────

    private void HandleActionsChanged(int remaining)
    {
        if (actionsLabel != null)
            actionsLabel.text = $"Actions: {remaining}";
    }

    private void HandleTurnStarted(BaseMatchProfile player)
    {
        bool isPlayerTurn = player is PlayerMatchData;
        if (turnLabel != null)
            turnLabel.text = isPlayerTurn ? "Your Turn" : "Enemy Turn";
        endTurnButton?.SetInteractable(isPlayerTurn);
    }

    private void HandleTurnEnded(BaseMatchProfile player) { }

    private void HandleGameOver(BaseMatchProfile winner)
    {
        // GameOverState.Enter() calls ShowGameOver with the win/loss flag.
        // We just cache the event here in case we need it for the label.
    }

    // ── Public API ───────────────────────────────────────────────────────────

    // Called by GameOverState with win/loss result and a callback for scene transition.
    public void ShowGameOver(bool playerWon, Action onContinue)
    {
        _onContinue = onContinue;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameOverLabel != null)
            gameOverLabel.text = playerWon ? "Victory!" : "Defeated!";

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => _onContinue?.Invoke());
        }
    }
}
