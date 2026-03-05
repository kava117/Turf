using UnityEngine;

public interface IStateBehavior
{
    void Enter(); // call when transitioning into the state
    void Tick(); // call every Update()
    void Exit(); // call when leaving the state
}
