using System.Collections.Generic;
using UnityEngine;

// Attached to a GameObject in GameScene.
// Initializes all systems in the correct order on scene load.
public class GameBootstrapper : MonoBehaviour
{
    [SerializeField] private BoardRenderer boardRenderer;
    [SerializeField] private BoardGenerator boardGenerator;
    [SerializeField] private BoardInput     boardInput;

    void Start()
    {
        // 1. Load persistent account data from disk.
        PlayerAccountProfile account = SaveManager.Load();

        // 2. Read run data passed from MetaScene (or create a fresh run for a new game).
        PlayerRunData run = SessionData.ActiveRun ?? new PlayerRunData();
        SessionData.ActiveRun = run;

        // 3. Create match profiles.
        PlayerMatchData player = new PlayerMatchData(run);
        AIMatchData     ai     = new AIMatchData(SessionData.Difficulty);

        // 4. Generate board.
        BoardSettings settings = BoardSettings.ForMatch(SessionData.MatchNumber);
        BoardData board = boardGenerator.Generate(settings);

        // 5. Assign starting tiles — placed as far apart as possible.
        var players = new List<BaseMatchProfile> { player, ai };
        board.AssignStartingTiles(players);

        // 5b. Apply BonusStartingTiles from perks (extra owned tiles at match start).
        foreach (PerkData perk in run.ActivePerks)
            for (int i = 0; i < perk.BonusStartingTiles; i++)
                AssignBonusStartingTile(player, board);

        // 6. Initial visibility reveal for the player's starting tile.
        board.RefreshVisibility(player);

        // 7. Render the board, passing the human player reference for ownership coloring.
        boardRenderer.Render(board, player);

        // 8. Initialize GameManager with all players and the board.
        GameManager.Instance.Init(player, ai, board);

        // 9. Wire BoardInput to PlayerTurnState.
        if (boardInput != null)
            boardInput.Init(GameManager.Instance.PlayerTurnState);

        // 10. Wire BoardInput reference into PlayerTurnState so it can enable/disable it.
        GameManager.Instance.PlayerTurnState.SetBoardInput(boardInput);

        // 11. Start the match.
        GameManager.Instance.TransitionTo(GameManager.Instance.PlayerTurnState);
    }

    // Assigns an extra starting tile adjacent to any existing player-owned tile.
    private void AssignBonusStartingTile(PlayerMatchData player, BoardData board)
    {
        foreach (Vector3Int owned in player.OwnedTiles)
        {
            foreach (Vector3Int adjacent in board.GetAdjacentCells(owned))
            {
                TileData t = board.GetTile(adjacent);
                if (t != null && t.CanBeClaimed() && t.IsUnclaimed)
                {
                    board.TransferOwnership(adjacent, player);
                    return;
                }
            }
        }
    }
}
