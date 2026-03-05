using System;
using UnityEngine;

public class EventManager
{
    public static event Action<Vector3Int, PlayerPrefs> OnTileCaptured;
    public static event Action<PlayerPrefs> OnTurnStarted;
    public static event Action<int> OnNumActiosChanged;
    public static event Action<PlayerPrefs> OnGameOver;

    public static void TileCaptured(Vector3Int cell, PlayerPrefs owner) 
        => OnTileCaptured?.Invoke(cell, owner);

    public static void TurnStarted(Player player) 
        => OnTurnStarted?.Invoke(player);

    public static void NumActionsChanged(int remaining)
        => OnNumActionsChanged?.Invoke(remaining);
    
    public static void OnGameOver(Player winner)
        => OnGameOver?.Invoke(winner);
}
