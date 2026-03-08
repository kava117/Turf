using UnityEngine;

// ScriptableObject — create one .asset file per perk via the Unity Asset menu.
// Perks are applied to PlayerMatchData at match start via GameBootstrapper.
[CreateAssetMenu(fileName = "NewPerk", menuName = "Turf/Perk")]
public class PerkData : ScriptableObject
{
    [field: SerializeField] public string DisplayName        { get; private set; }
    [field: SerializeField] public string Description        { get; private set; }
    [field: SerializeField] public string PerkId             { get; private set; } // unique key for save data

    [Header("Stat Modifiers")]
    [field: SerializeField] public int BonusActionsPerTurn   { get; private set; } = 0;
    [field: SerializeField] public int BonusStartingTiles    { get; private set; } = 0;
    // Add more modifiers here as the design expands.
}
