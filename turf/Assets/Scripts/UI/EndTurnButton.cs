// Ends the player's turn early (forfeits remaining actions).
// Only interactable during PlayerTurnState — UIManager controls this.
public class EndTurnButton : BaseButton
{
    protected override void OnClick()
    {
        // PlayerTurnState is accessed via GameManager so this button
        // has no direct dependency on the state itself.
        if (GameManager.Instance?.PlayerTurnState != null)
            GameManager.Instance.PlayerTurnState.EndTurn();
    }
}
