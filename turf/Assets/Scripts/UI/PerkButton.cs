using TMPro;

// Displayed in MetaScene. Shows one perk offer; fires OnPerkSelected when chosen.
public class PerkButton : BaseButton
{
    [SerializeField] private TextMeshProUGUI descriptionLabel;

    private PerkData _perkData;

    public void SetPerk(PerkData perk)
    {
        _perkData = perk;
        if (label != null)           label.text           = perk.DisplayName;
        if (descriptionLabel != null) descriptionLabel.text = perk.Description;
    }

    protected override void OnClick()
    {
        if (_perkData == null) return;
        EventManager.PerkSelected(_perkData);
    }
}
