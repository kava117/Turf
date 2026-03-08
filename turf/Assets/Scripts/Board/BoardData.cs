using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Source of truth for all tile state. Tilemaps are purely visual and never queried for logic.
public class BoardData
{
    public int Width  { get; private set; }
    public int Height { get; private set; }

    private readonly Dictionary<Vector3Int, TileData> _tiles = new();
    private List<BaseMatchProfile> _players = new();

    // All cells ever revealed by any player. Used for Barbarian trigger checks.
    private readonly HashSet<Vector3Int> _globalRevealed = new();

    public BoardData(int width, int height)
    {
        Width  = width;
        Height = height;
    }

    // ── Tile Access ─────────────────────────────────────────────────────────

    public void SetTile(Vector3Int cell, TileData data) => _tiles[cell] = data;

    public TileData GetTile(Vector3Int cell)
        => _tiles.TryGetValue(cell, out TileData t) ? t : null;

    public IEnumerable<Vector3Int> AllCells()
    {
        for (int x = 0; x < Width;  x++)
        for (int y = 0; y < Height; y++)
            yield return new Vector3Int(x, y, 0);
    }

    public List<Vector3Int> GetAdjacentCells(Vector3Int cell)
    {
        return new List<Vector3Int>
        {
            cell + Vector3Int.up,
            cell + Vector3Int.down,
            cell + Vector3Int.left,
            cell + Vector3Int.right
        }.Where(c => _tiles.ContainsKey(c)).ToList();
    }

    // ── Ownership ────────────────────────────────────────────────────────────

    // Central ownership transfer. Keeps OwnedTiles lists in sync and fires the event.
    public void TransferOwnership(Vector3Int cell, BaseMatchProfile newOwner)
    {
        TileData tile = GetTile(cell);
        if (tile == null) return;

        tile.Owner?.LoseTile(cell);
        tile.SetOwner(newOwner);
        newOwner?.OwnedTiles.Add(cell);

        EventManager.TileCaptured(cell, newOwner);
    }

    // Finds two starting tiles as far apart as possible and assigns one to each player.
    public void AssignStartingTiles(List<BaseMatchProfile> players)
    {
        _players = players;

        // Use opposite corners of the board for up to 4 players.
        Vector3Int[] corners =
        {
            new(0,         0,          0),
            new(Width - 1, Height - 1, 0),
            new(Width - 1, 0,          0),
            new(0,         Height - 1, 0),
        };

        for (int i = 0; i < players.Count; i++)
        {
            Vector3Int start = FindNearestClaimableCell(corners[i % corners.Length]);
            TransferOwnership(start, players[i]);
        }
    }

    private Vector3Int FindNearestClaimableCell(Vector3Int target)
    {
        Vector3Int best = default;
        float bestDist  = float.MaxValue;

        foreach (Vector3Int cell in AllCells())
        {
            TileData t = GetTile(cell);
            if (t == null || !t.CanBeClaimed() || t.Owner != null) continue;

            float d = Vector3Int.Distance(cell, target);
            if (d < bestDist) { bestDist = d; best = cell; }
        }
        return best;
    }

    // ── Fog of War / Visibility ──────────────────────────────────────────────

    // Computes all cells currently visible to a player based on their owned tiles.
    public HashSet<Vector3Int> GetVisibleTilesFor(BaseMatchProfile player)
    {
        var visible = new HashSet<Vector3Int>();

        foreach (Vector3Int ownedCell in player.OwnedTiles)
        {
            TileData tile = GetTile(ownedCell);
            if (tile == null) continue;

            visible.Add(ownedCell);
            AddVisibilityFromTile(ownedCell, tile, visible);
        }
        return visible;
    }

    private void AddVisibilityFromTile(Vector3Int origin, TileData tile, HashSet<Vector3Int> result)
    {
        switch (tile.Type)
        {
            case TileType.Forest:
            case TileType.Barbarian:
            case TileType.Wizard:
                AddCardinalRange(origin, 1, result);
                break;

            case TileType.Plains:
                AddCardinalRange(origin, 2, result);
                AddDiagonals(origin, result);
                break;

            case TileType.Tower:
                // Full diamond: all cells within Manhattan distance 3.
                AddDiamondFull(origin, 3, result);
                break;

            case TileType.Cave:
                AddCardinalRange(origin, 1, result);
                // Caves can also see all other caves on the map.
                foreach (Vector3Int cell in AllCells())
                {
                    TileData t = GetTile(cell);
                    if (t != null && t.Type == TileType.Cave)
                        result.Add(cell);
                }
                break;
        }
    }

    // Reveals all tiles visible to a player; returns cells newly revealed this call.
    public List<Vector3Int> RefreshVisibility(BaseMatchProfile player)
    {
        HashSet<Vector3Int> nowVisible = GetVisibleTilesFor(player);
        var newlyRevealed = new List<Vector3Int>();

        foreach (Vector3Int cell in nowVisible)
        {
            if (_globalRevealed.Contains(cell)) continue;
            _globalRevealed.Add(cell);
            GetTile(cell)?.Reveal();
            newlyRevealed.Add(cell);
        }

        if (newlyRevealed.Count > 0)
            EventManager.TilesRevealed(newlyRevealed);

        return newlyRevealed;
    }

    // ── Claim Range ──────────────────────────────────────────────────────────

    // Returns all valid tiles this player can claim on their current turn.
    public List<Vector3Int> GetClaimableTilesFor(BaseMatchProfile player)
    {
        var reachable = new HashSet<Vector3Int>();

        foreach (Vector3Int ownedCell in player.OwnedTiles)
        {
            TileData tile = GetTile(ownedCell);
            if (tile == null) continue;
            AddClaimRangeFromTile(ownedCell, tile, player, reachable);
        }

        // Filter: must exist, be claimable, and not already belong to this player.
        reachable.RemoveWhere(cell =>
        {
            TileData t = GetTile(cell);
            return t == null || !t.CanBeClaimed() || t.Owner == player;
        });

        return reachable.ToList();
    }

    private void AddClaimRangeFromTile(
        Vector3Int origin, TileData tile, BaseMatchProfile player, HashSet<Vector3Int> result)
    {
        switch (tile.Type)
        {
            case TileType.Forest:
            case TileType.Barbarian:
                AddCardinalRange(origin, 1, result);
                break;

            case TileType.Plains:
                AddCardinalRange(origin, 2, result);
                AddDiagonals(origin, result);
                break;

            case TileType.Tower:
                // Only the perimeter of the distance-3 diamond (not inner tiles).
                AddDiamondPerimeter(origin, 3, result);
                break;

            case TileType.Cave:
                AddCardinalRange(origin, 1, result);
                if (!tile.CaveUsed)
                {
                    // Can also teleport to any unclaimed Cave on the map.
                    foreach (Vector3Int cell in AllCells())
                    {
                        TileData t = GetTile(cell);
                        if (t != null && t.Type == TileType.Cave && t.IsUnclaimed)
                            result.Add(cell);
                    }
                }
                break;

            case TileType.Wizard:
                AddCardinalRange(origin, 1, result);
                if (!tile.WizardUsed)
                {
                    // Can claim any unclaimed claimable tile anywhere.
                    foreach (Vector3Int cell in AllCells())
                    {
                        TileData t = GetTile(cell);
                        if (t != null && t.CanBeClaimed() && t.IsUnclaimed)
                            result.Add(cell);
                    }
                }
                break;
        }
    }

    // Returns claimable range WITHOUT wizard/cave specials.
    // Used by TileEffectHandler to detect if a special ability was consumed.
    public HashSet<Vector3Int> GetNormalClaimRange(BaseMatchProfile player)
    {
        var reachable = new HashSet<Vector3Int>();
        foreach (Vector3Int ownedCell in player.OwnedTiles)
        {
            TileData tile = GetTile(ownedCell);
            if (tile == null) continue;
            // Use standard adjacency only — no wizard/cave special logic.
            switch (tile.Type)
            {
                case TileType.Forest:
                case TileType.Cave:
                case TileType.Barbarian:
                case TileType.Wizard:
                    AddCardinalRange(ownedCell, 1, reachable);
                    break;
                case TileType.Plains:
                    AddCardinalRange(ownedCell, 2, reachable);
                    AddDiagonals(ownedCell, reachable);
                    break;
                case TileType.Tower:
                    AddDiamondPerimeter(ownedCell, 3, reachable);
                    break;
            }
        }
        reachable.RemoveWhere(cell =>
        {
            TileData t = GetTile(cell);
            return t == null || !t.CanBeClaimed() || t.Owner == player;
        });
        return reachable;
    }

    // ── Geometry Helpers ─────────────────────────────────────────────────────

    private void AddCardinalRange(Vector3Int origin, int maxDist, HashSet<Vector3Int> result)
    {
        Vector3Int[] dirs = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        foreach (Vector3Int dir in dirs)
            for (int i = 1; i <= maxDist; i++)
            {
                Vector3Int cell = origin + dir * i;
                if (_tiles.ContainsKey(cell)) result.Add(cell);
            }
    }

    private void AddDiagonals(Vector3Int origin, HashSet<Vector3Int> result)
    {
        Vector3Int[] diags =
        {
            new(1,  1, 0), new(1,  -1, 0),
            new(-1, 1, 0), new(-1, -1, 0)
        };
        foreach (Vector3Int d in diags)
        {
            Vector3Int cell = origin + d;
            if (_tiles.ContainsKey(cell)) result.Add(cell);
        }
    }

    // All cells within Manhattan distance <= dist.
    private void AddDiamondFull(Vector3Int origin, int dist, HashSet<Vector3Int> result)
    {
        for (int dx = -dist; dx <= dist; dx++)
        for (int dy = -(dist - Mathf.Abs(dx)); dy <= dist - Mathf.Abs(dx); dy++)
        {
            Vector3Int cell = origin + new Vector3Int(dx, dy, 0);
            if (_tiles.ContainsKey(cell)) result.Add(cell);
        }
    }

    // Only cells at Manhattan distance exactly == dist (the perimeter ring).
    private void AddDiamondPerimeter(Vector3Int origin, int dist, HashSet<Vector3Int> result)
    {
        for (int dx = -dist; dx <= dist; dx++)
        {
            int absDx = Mathf.Abs(dx);
            int dy1   =  (dist - absDx);
            int dy2   = -(dist - absDx);

            Vector3Int c1 = origin + new Vector3Int(dx, dy1, 0);
            Vector3Int c2 = origin + new Vector3Int(dx, dy2, 0);

            if (_tiles.ContainsKey(c1)) result.Add(c1);
            if (dy1 != dy2 && _tiles.ContainsKey(c2)) result.Add(c2);
        }
    }

    // ── Query Helpers ─────────────────────────────────────────────────────────

    public int CountClaimableTiles()
        => AllCells().Count(c => GetTile(c)?.CanBeClaimed() == true);

    public int CountOwnedTiles()
        => AllCells().Count(c => GetTile(c)?.Owner != null);

    public List<BaseMatchProfile> GetAllPlayers() => _players;
}
