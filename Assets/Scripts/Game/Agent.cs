using UnityEngine;
public abstract class Agent : MonoBehaviour
{
    public abstract void Reset();
    public abstract void Step();
    public abstract void Exit();

    private byte[] AppendLength(byte[] input)
    {
        byte[] newArray = new byte[input.Length + 4];
        input.CopyTo(newArray, 4);
        System.BitConverter.GetBytes(input.Length).CopyTo(newArray, 0);
        return newArray;
    }
}
