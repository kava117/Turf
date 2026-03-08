using UnityEngine;
using UnityEngine.Tilemaps;

// Translates mouse input into tile-level game requests.
// Knows nothing about game rules — only converts world position to grid cell
// and delegates to PlayerTurnState.
public class BoardInput : MonoBehaviour
{
    [SerializeField] private Tilemap referenceTilemap; // any tilemap on the grid, used for WorldToCell

    private PlayerTurnState _playerTurnState;
    private bool _active;

    public void Init(PlayerTurnState playerTurnState)
    {
        _playerTurnState = playerTurnState;
    }

    public void SetActive(bool active)
    {
        _active = active;
    }

    void Update()
    {
        if (!_active || _playerTurnState == null) return;
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;

        Vector3Int cell = referenceTilemap.WorldToCell(worldPos);
        _playerTurnState.TryClaimTile(cell);
    }
}
