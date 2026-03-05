using UnityEngine;

public class PlayerMatchProfile : MonoBehaviour
{
    public PlayerRunProfile _profile { get; private set; }

    public PlayerMatchProfile(PlayerRunProfile _profile)
    {
        profile = _profile;
        displayName = profile.Account.PlayerName;
        actionsPerTurn = 3 + profile.BonusActions;
        actionsRemaining = actionsPerTurn;
    }
}
