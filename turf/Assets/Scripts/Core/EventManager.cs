using System;
using UnityEngine;

// Static event bus. No GameObject required.
// MonoBehaviours subscribe in OnEnable / unsubscribe in OnDisable.
// Plain C# classes subscribe in constructor / unsubscribe in destructor.
public static class EventManager
{
    // Fired when a tile's owner changes.
    public static event Action<Vector3Int, BaseMatchProfile> OnTileCaptured;

    // Fired at the start of any player's turn.
    public static event Action<BaseMatchProfile> OnTurnStarted;

    // Fired at the end of any player's turn.
    public static event Action<BaseMatchProfile> OnTurnEnded;

    // Fired whenever a player's remaining actions change.
    public static event Action<int> OnNumActionsChanged;

    // Fired when a winner is determined.
    public static event Action<BaseMatchProfile> OnGameOver;

    // Fired when the player selects a perk in MetaScene.
    public static event Action<PerkData> OnPerkSelected;

    public static void TileCaptured(Vector3Int cell, BaseMatchProfile newOwner)
        => OnTileCaptured?.Invoke(cell, newOwner);

    public static void TurnStarted(BaseMatchProfile player)
        => OnTurnStarted?.Invoke(player);

    public static void TurnEnded(BaseMatchProfile player)
        => OnTurnEnded?.Invoke(player);

    public static void NumActionsChanged(int remaining)
        => OnNumActionsChanged?.Invoke(remaining);

    public static void GameOver(BaseMatchProfile winner)
        => OnGameOver?.Invoke(winner);

    public static void PerkSelected(PerkData perk)
        => OnPerkSelected?.Invoke(perk);

    // Fired when tiles become newly visible (fog of war lifted).
    public static event Action<List<Vector3Int>> OnTilesRevealed;
    public static void TilesRevealed(List<Vector3Int> cells)
        => OnTilesRevealed?.Invoke(cells);

    // Fired when the set of highlight tiles changes.
    public static event Action<List<Vector3Int>> OnHighlightUpdated;
    public static void HighlightUpdated(List<Vector3Int> cells)
        => OnHighlightUpdated?.Invoke(cells);
}
