using System.Collections.Generic;
using UnityEditor.AdaptivePerformance.Editor.Metadata;
using UnityEngine;

public class PlayerRunProfile : MonoBehaviour
{
    public PlayerAccountProfile account { get; private set; }
    public int runMatchesPlayed { get; set; }
    public int runWins { get; set; }
    public int bonusActions { get; set; }
    public List<PerkData> activePerks { get; set; } // perks chosen/in inventory this run

    public PlayerRunProfile(PlayerAccountProfile _account)
    {
        account = _account;
        activePerks = new List<PerkData>();
    }

    public void AddPerk(PerkData perk)
    {
        activePerks.Add(perk);
        //bonusActions += perk.bonusActions;
    }
}
