using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Optional confirm-before-claim button.
// When a tile is selected via click, this panel shows its details.
// Confirming here fires the actual claim through PlayerTurnState.
public class CaptureTileButton : BaseButton
{
    [SerializeField] private TextMeshProUGUI tileInfoLabel;

    private UnityEngine.Vector3Int _pendingCell;
    private bool                   _hasPending;

    public void SetPendingTile(UnityEngine.Vector3Int cell, string tileDescription)
    {
        _pendingCell  = cell;
        _hasPending   = true;
        if (tileInfoLabel != null)
            tileInfoLabel.text = tileDescription;
        gameObject.SetActive(true);
    }

    public void ClearPending()
    {
        _hasPending = false;
        gameObject.SetActive(false);
    }

    protected override void OnClick()
    {
        if (!_hasPending) return;
        GameManager.Instance?.PlayerTurnState?.TryClaimTile(_pendingCell);
        ClearPending();
    }
}
