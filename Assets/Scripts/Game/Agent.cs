using UnityEngine;

public enum AgentAction
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
}

public abstract class Agent : MonoBehaviour
{
    public abstract void Reset();
    public abstract void Step();
    public abstract void Quit();
}
