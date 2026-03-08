using UnityEngine;

public class PlayerMatchProfile : MonoBehaviour
{
    public PlayerRunProfile _profile { get; private set; }

    PlayerRunProfile profile;
    string displayName;
    int actionsPerTurn;
    int actionsRemaining;

    public PlayerMatchProfile(PlayerRunProfile _profile)
    {
        profile = _profile;
        displayName = profile.account.PlayerName;
        actionsPerTurn = 3 + profile.bonusActions;
        actionsRemaining = actionsPerTurn;
    }
}
