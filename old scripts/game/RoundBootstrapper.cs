using UnityEngine;

public class RoundBootstrapper : MonoBehaviour
{
    [SerializeField] private BoardGenerator boardGenerator;
    [SerializeField] private BoardRenderer boardRenderer;

    void Start()
    {
        // 1. load persistent data
        PlayerAccountProfile account = SaveManager.loadAccount();
        PlayerRunProfile profile = SessionData.currentProfile; // passed from MetaScene

        // 2. create players
        PlayerMatchProfile player = new PlayerMatchProfile(profile);
        AIPlayer ai = new AIPlayer(AIDifficulty.Easy);

        // 3. generate board data
        BoardData boardData = boardGenerator.Generate(GetBoardSettings(profile));

        // 4. assign starting tiles
        boardData.AssignStartingTile(player);
        boardData.AssignStartingTile(ai);

        // 5. render the board
        boardRenderer.Render(boardData);

        // 6. initialize game manager
        GameManager.Instance.Init(player, ai, boardData);

        // 7. start the game
        GameManager.Instance.TransitionTo(GameManager.Instance.PlayerTurnState);
    }
}
