using UnityEngine;
public class BoardGenerator
{
    public BoardData Generate(BoardSettings settings)
    {
        BoardData board = new BoardData(settings.Width, settings.Height);

        foreach (Vector3Int cell in board.AllCells())
        {
            TileType type = RollTileType(settings);
            board.SetTile(cell, new TileData(type));
        }

        return board;
    }

    private TileType RollTileType(BoardSettings settings)
    {
        float roll = Random.value;
        if (roll < settings.PlainChance) return TileType.Plain;
        if (roll < settings.MountainChance) return TileType.Mountain;
        return TileType.Fortress;
    }
}

public class BoardSettings
{
    public int Width = 8;
    public int Height = 8;
    public float PlainChance = 0.70f;
    public float MountainChance = 0.85f;

    // later in the run these values shift to introduce more special tiles
    public static BoardSettings FromRunProgress(PlayerRunProfile profile)
    {
        return new BoardSettings
        {
            PlainChance = Mathf.Max(0.4f, 0.70f - profile.RunMatchesPlayed * 0.05f),
            MountainChance = 0.85f
        };
    }
}