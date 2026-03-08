using System.Collections.Generic;
using UnityEditor.AdaptivePerformance.Editor.Metadata;
using UnityEngine;

public class PlayerAccountProfile : MonoBehaviour
{
    public string PlayerName { get; set; }
    public int TotalMatchesPlayed { get; set; }
    public int TotalWins { get; set; }
    public int TotalTilesCaptured { get; set; }
    public List<string> Achievements { get; set; }
    // public List<PerkData> UnlockedPerks { get; set; } // every perk ever unlocked. maybe an achievement for unlocking them all?

    public void GrantAchievement(string achievement)
    {
        if (!Achievements.Contains(achievement))
        {
            Achievements.Add(achievement);
        }
    }
}
