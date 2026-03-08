using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Drives the MetaScene. Shows run stats and either a perk offer or a plain continue button.
public class MetaSceneController : MonoBehaviour
{
    [Header("Run Stats")]
    [SerializeField] private TextMeshProUGUI livesLabel;
    [SerializeField] private TextMeshProUGUI matchesLabel;
    [SerializeField] private TextMeshProUGUI perksHeldLabel;

    [Header("Perk Offer")]
    [SerializeField] private GameObject  perkOfferPanel;
    [SerializeField] private PerkButton  perkButtonPrefab;
    [SerializeField] private Transform   perkButtonContainer;

    [Header("No-Perk Continue")]
    [SerializeField] private GameObject  continuePanel;
    [SerializeField] private Button      continueButton;

    [Header("Data")]
    [SerializeField] private PerkLibrary perkLibrary;

    void Start()
    {
        PlayerRunData run = SessionData.ActiveRun;
        if (run == null)
        {
            // Safety fallback — shouldn't happen in normal flow.
            SceneManager.LoadScene("MainMenuScene");
            return;
        }

        ShowRunStats(run);

        if (run.IsPerkMatchDue())
            ShowPerkOffer(run);
        else
            ShowContinue();
    }

    void OnEnable()  => EventManager.OnPerkSelected += HandlePerkSelected;
    void OnDisable() => EventManager.OnPerkSelected -= HandlePerkSelected;

    private void ShowRunStats(PlayerRunData run)
    {
        if (livesLabel   != null) livesLabel.text   = $"Lives: {run.LivesRemaining}";
        if (matchesLabel != null) matchesLabel.text = $"Match: {SessionData.MatchNumber}";
        if (perksHeldLabel != null)
            perksHeldLabel.text = $"Perks: {run.ActivePerks.Count} / 5";
    }

    private void ShowPerkOffer(PlayerRunData run)
    {
        perkOfferPanel?.SetActive(true);
        continuePanel?.SetActive(false);

        List<PerkData> offers = perkLibrary.GetRandomOffer(3, run.ActivePerks);
        foreach (PerkData perk in offers)
        {
            PerkButton btn = Instantiate(perkButtonPrefab, perkButtonContainer);
            btn.SetPerk(perk);
        }
    }

    private void ShowContinue()
    {
        perkOfferPanel?.SetActive(false);
        continuePanel?.SetActive(true);

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(LoadNextMatch);
        }
    }

    private void HandlePerkSelected(PerkData perk)
    {
        SessionData.ActiveRun?.AddPerk(perk);
        LoadNextMatch();
    }

    private void LoadNextMatch()
    {
        SceneManager.LoadScene("GameScene");
    }
}
