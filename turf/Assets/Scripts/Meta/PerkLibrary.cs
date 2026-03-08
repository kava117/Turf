using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ScriptableObject — assign all PerkData assets to the AllPerks list in the Inspector.
// Handles random perk offer generation, excluding perks the player already holds.
[CreateAssetMenu(fileName = "PerkLibrary", menuName = "Turf/Perk Library")]
public class PerkLibrary : ScriptableObject
{
    [SerializeField] private List<PerkData> allPerks = new();

    // Returns 'count' random perks not already in the player's active list.
    public List<PerkData> GetRandomOffer(int count, List<PerkData> alreadyHeld)
    {
        List<PerkData> available = allPerks
            .Where(p => !alreadyHeld.Contains(p))
            .ToList();

        // Shuffle.
        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (available[i], available[j]) = (available[j], available[i]);
        }

        return available.Take(count).ToList();
    }
}
