using System.Collections.Generic;
using UnityEngine;

// Produces a BoardData from a BoardSettings. No Unity dependencies beyond Random.
// Does not assign owners or reveal tiles — that happens in GameBootstrapper.
public class BoardGenerator : UnityEngine.MonoBehaviour
{
    public BoardData Generate(BoardSettings settings)
    {
        BoardData board = new BoardData(settings.Width, settings.Height);

        foreach (Vector3Int cell in board.AllCells())
        {
            TileType type = RollTileType(settings.TileWeights);
            board.SetTile(cell, new TileData(type));
        }

        return board;
    }

    private TileType RollTileType(Dictionary<TileType, float> weights)
    {
        float total = 0f;
        foreach (float w in weights.Values) total += w;

        float roll = Random.value * total;
        float cumulative = 0f;

        foreach (var kvp in weights)
        {
            cumulative += kvp.Value;
            if (roll <= cumulative) return kvp.Key;
        }

        return TileType.Plains; // fallback
    }
}
