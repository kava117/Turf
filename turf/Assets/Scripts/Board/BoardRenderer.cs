using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Translates BoardData into Tilemaps. Purely visual — never queried for game logic.
// Reacts to EventManager events; does not hold any authoritative state.
public class BoardRenderer : MonoBehaviour
{
    [Header("Tilemaps (bottom to top)")]
    [SerializeField] private Tilemap terrainTilemap;
    [SerializeField] private Tilemap ownershipTilemap;
    [SerializeField] private Tilemap highlightTilemap;
    [SerializeField] private Tilemap unitsTilemap; // reserved

    [Header("Terrain Tiles (assign placeholder Tile assets in Inspector)")]
    [SerializeField] private TileBase plainsTile;
    [SerializeField] private TileBase forestTile;
    [SerializeField] private TileBase towerTile;
    [SerializeField] private TileBase caveTile;
    [SerializeField] private TileBase mountainTile;
    [SerializeField] private TileBase wizardTile;
    [SerializeField] private TileBase barbarianTile;
    [SerializeField] private TileBase desolateTile;
    [SerializeField] private TileBase fogTile;         // shown over unrevealed cells

    [Header("Ownership / Highlight Tiles")]
    [SerializeField] private TileBase playerOwnershipTile;
    [SerializeField] private TileBase aiOwnershipTile;
    [SerializeField] private TileBase highlightTile;

    [Header("Player Colors")]
    [SerializeField] private Color playerColor = new Color(0.2f, 0.5f, 1f, 0.5f);
    [SerializeField] private Color aiColor     = new Color(1f,   0.3f, 0.3f, 0.5f);

    private BoardData _board;
    private BaseMatchProfile _humanPlayer;

    void OnEnable()
    {
        EventManager.OnTileCaptured   += HandleTileCaptured;
        EventManager.OnTilesRevealed  += HandleTilesRevealed;
        EventManager.OnHighlightUpdated += HandleHighlightUpdated;
    }

    void OnDisable()
    {
        EventManager.OnTileCaptured   -= HandleTileCaptured;
        EventManager.OnTilesRevealed  -= HandleTilesRevealed;
        EventManager.OnHighlightUpdated -= HandleHighlightUpdated;
    }

    // Called once by GameBootstrapper after board is generated and starting tiles assigned.
    public void Render(BoardData board, BaseMatchProfile humanPlayer)
    {
        _board       = board;
        _humanPlayer = humanPlayer;

        terrainTilemap.ClearAllTiles();
        ownershipTilemap.ClearAllTiles();
        highlightTilemap.ClearAllTiles();

        foreach (Vector3Int cell in board.AllCells())
        {
            TileData tile = board.GetTile(cell);

            // Terrain layer — always set, but covered by fog if not yet revealed.
            terrainTilemap.SetTile(cell, GetTerrainTile(tile.Type));

            // Ownership layer.
            if (tile.Owner != null)
                ownershipTilemap.SetTile(cell, GetOwnershipTile(tile.Owner));

            // Fog — cover unrevealed cells.
            if (!tile.IsRevealed)
                highlightTilemap.SetTile(cell, fogTile);
        }
    }

    // Overload called by GameBootstrapper before human player reference is available.
    public void Render(BoardData board) => Render(board, null);

    // ── Event Handlers ───────────────────────────────────────────────────────

    private void HandleTileCaptured(Vector3Int cell, BaseMatchProfile newOwner)
    {
        if (_board == null) return;
        ownershipTilemap.SetTile(cell, newOwner != null ? GetOwnershipTile(newOwner) : null);
        StartCoroutine(FlashCapture(cell));
    }

    private void HandleTilesRevealed(List<Vector3Int> cells)
    {
        if (_board == null) return;
        foreach (Vector3Int cell in cells)
        {
            // Remove fog tile from the highlight layer for this cell.
            // (Fog is stored in highlightTilemap to avoid overwriting the highlight layer separately.)
            TileBase current = highlightTilemap.GetTile(cell);
            if (current == fogTile)
                highlightTilemap.SetTile(cell, null);
        }
    }

    private void HandleHighlightUpdated(List<Vector3Int> claimableCells)
    {
        if (_board == null) return;
        ClearHighlights();
        foreach (Vector3Int cell in claimableCells)
            highlightTilemap.SetTile(cell, highlightTile);
    }

    // ── Highlight Control ─────────────────────────────────────────────────────

    public void ClearHighlights()
    {
        // Only clear highlight tiles — leave fog tiles intact.
        foreach (Vector3Int cell in _board.AllCells())
        {
            TileBase current = highlightTilemap.GetTile(cell);
            if (current == highlightTile)
                highlightTilemap.SetTile(cell, null);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private TileBase GetTerrainTile(TileType type) => type switch
    {
        TileType.Plains    => plainsTile,
        TileType.Forest    => forestTile,
        TileType.Tower     => towerTile,
        TileType.Cave      => caveTile,
        TileType.Mountain  => mountainTile,
        TileType.Wizard    => wizardTile,
        TileType.Barbarian => barbarianTile,
        TileType.Desolate  => desolateTile,
        _                  => plainsTile
    };

    private TileBase GetOwnershipTile(BaseMatchProfile owner)
    {
        if (owner == _humanPlayer) return playerOwnershipTile;
        return aiOwnershipTile;
    }

    private IEnumerator FlashCapture(Vector3Int cell)
    {
        // Brief highlight flash on capture for visual feedback.
        highlightTilemap.SetTile(cell, highlightTile);
        yield return new WaitForSeconds(0.15f);
        TileBase current = highlightTilemap.GetTile(cell);
        if (current == highlightTile)
            highlightTilemap.SetTile(cell, null);
    }
}
