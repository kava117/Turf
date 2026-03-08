using Unity.VisualScripting;
using UnityEngine;

public class CheckWinState : IGameState
{
    private GameManager _gm;
    private HumanPlayer _player;
    private AIPlayer _ai;
    private int _totalTiles;

    public void Enter()
    {
        int playerTiles = _player.OwnedTiles.Count;
        int aiTiles = _ai.OwnedTiles.Count;
        int majority = Mathf.CeilToInt(_totalTiles / 2f) + 1;

        if (playerTiles >= majority || aiTiles >= majority)
        {
            BasePlayer winner = playerTiles > aiTiles ? _player : (BasePlayer)_ai;
            EventBus.GameOver(winner);
            _gm.TransitionTo(_gm.GameOverState);
        }
        else
        {
            _gm.TransitionTo(_gm.PlayerTurnState); // game continues
        }
    }

    public void Tick() { }
    public void Exit() { }
}