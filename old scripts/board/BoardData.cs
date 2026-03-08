using System.Collections.Generic;
using UnityEngine;

public class BoardData
{
    private Dictionary<Vector3Int, TileData> tiles;
    public int width { get; private set; }
    public int height { get; private set; }

    public BoardData(int _width, int _height)
    {
        width = _width;
        height = _height;
        _tiles = new Dictionary<Vector3Int, TileData>();
    }

    public void SetTile(Vector3Int _cell, TileData _data)
    {
        tiles[_cell] = _data;
    }

    public TileData GetTile(Vector3Int _cell)
    {
        return tiles.TryGetValue(_cell, out TileData data) ? data : null;
    }

    public IEnumerable<Vector3Int> AllCells()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                yield return new Vector3Int(x, y, 0);
    }

    public List<Vector3Int> GetAdjacentCells(Vector3Int _cell)
    {
        return new List<Vector3Int> {
            _cell + Vector3Int.up,
            _cell + Vector3Int.down,
            _cell + Vector3Int.left,
            _cell + Vector3Int.right
        }.Where(c => _tiles.ContainsKey(c)).ToList();
    }

    public void AssignStartingTile(BaseMatchProfile player)
    {
        // find a plain tile far from other starting tiles and assign it
        Vector3Int cell = FindValidStartingCell(player);
        tiles[cell].SetOwner(player);
    }
}