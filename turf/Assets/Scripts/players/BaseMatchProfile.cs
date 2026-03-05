using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BaseMatchProfile : MonoBehaviour
{
    public static BaseMatchProfile Instance { get; private set; }

    public string DisplayName { get; protected set; }
    public int ActionsPerTurn { get; protected set; }
    public int ActionsRemaining { get; protected set; }
    public List<Vector3Int> OwnedTiles { get; protected set; }

    public BaseMatchProfile()
    {
        OwnedTiles = new List<Vector3Int>();
    }

    public void SpendAction()
    {
        ActionsRemaining--;
        //EventBus.ActionsChanged(ActionsRemaining, this);
    }

    public void ResetActions()
    {
        ActionsRemaining = ActionsPerTurn;
    }
}
