using UnityEngine;

public abstract class Observer : MonoBehaviour
{
    public abstract void Reset();
    public abstract void Step();
    public abstract void Quit();
}
