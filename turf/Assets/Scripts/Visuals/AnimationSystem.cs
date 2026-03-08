using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Subscribes to game events and plays visual effects.
// Fully decoupled from logic — only reacts, never drives state.
public class AnimationSystem : MonoBehaviour
{
    [SerializeField] private Tilemap highlightTilemap;
    [SerializeField] private TileBase captureFlashTile;
    [SerializeField] private float    flashDuration = 0.2f;

    void OnEnable()
    {
        EventManager.OnTileCaptured += HandleTileCaptured;
        EventManager.OnTurnStarted  += HandleTurnStarted;
    }

    void OnDisable()
    {
        EventManager.OnTileCaptured -= HandleTileCaptured;
        EventManager.OnTurnStarted  -= HandleTurnStarted;
    }

    private void HandleTileCaptured(Vector3Int cell, BaseMatchProfile newOwner)
    {
        if (captureFlashTile != null)
            StartCoroutine(FlashTile(cell));
    }

    private void HandleTurnStarted(BaseMatchProfile player)
    {
        // Placeholder: log turn change. Replace with camera pan or UI animation later.
        // Debug.Log($"Turn started: {player.DisplayName}");
    }

    private IEnumerator FlashTile(Vector3Int cell)
    {
        if (highlightTilemap == null || captureFlashTile == null) yield break;

        highlightTilemap.SetTile(cell, captureFlashTile);
        yield return new WaitForSeconds(flashDuration);
        highlightTilemap.SetTile(cell, null);
    }
}
