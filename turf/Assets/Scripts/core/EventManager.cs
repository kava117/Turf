using System;
using UnityEngine;

public class EventManager
{
    public static event Action<Vector3Int, PlayerPrefs> OnTileCaptured;
    public static event Action<BaseMatchProfile> OnTurnStarted;
    public static event Action<int> OnNumActionsChanged;
    public static event Action<BaseMatchProfile> OnGameOver;

    public static void TileCaptured(Vector3Int cell, PlayerPrefs owner) 
        => OnTileCaptured?.Invoke(cell, owner);

    public static void TurnStarted(BaseMatchProfile player) 
        => OnTurnStarted?.Invoke(player);

    public static void NumActionsChanged(int remaining)
        => OnNumActionsChanged?.Invoke(remaining);
    
    public static void GameOver(BaseMatchProfile winner)
        => OnGameOver?.Invoke(winner);
}
