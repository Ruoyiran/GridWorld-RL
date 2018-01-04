﻿using UnityEngine;
public abstract class Agent : MonoBehaviour
{
    public abstract void Reset();
    public abstract void Step();
    public abstract void Quit();
}
