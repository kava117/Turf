using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class TileData
{
    public TileType Type { get; private set; }
    public BasePlayer Owner { get; private set; }
    public int CaptureCost { get; private set; }
    public bool IsNeutral => Owner == null;

    public TileData(TileType type)
    {
        Type = type;
        CaptureCost = GetBaseCost(type);
    }

    public void SetOwner(BasePlayer player)
    {
        Owner = player;
    }

    private int GetBaseCost(TileType type)
    {
        switch (type)
        {
            case TileType.Plain: return 1;
            case TileType.Mountain: return 2;
            case TileType.Fortress: return 3;
            default: return 1;
        }
    }
}

public enum TileType { Plain, Mountain, Fortress }