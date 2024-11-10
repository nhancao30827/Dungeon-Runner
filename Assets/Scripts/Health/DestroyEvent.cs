using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEvent : MonoBehaviour
{
    /// <summary>
    /// Event triggered when the object is destroyed.
    /// </summary>
    public event Action<DestroyEvent, DestroyEventArgs> OnDestroy;

    /// <summary>
    /// Calls the destroy event with the specified player death status.
    /// </summary>
    /// <param name="isPlayerDead">Indicates whether the player is dead.</param>
    public void CallDestroyEvent(bool isPlayerDead)
    {
        OnDestroy?.Invoke(this, new DestroyEventArgs { isPlayerDead = isPlayerDead });
    }
}

/// <summary>
/// Arguments for the destroy event.
/// </summary>
public class DestroyEventArgs : EventArgs
{
    /// <summary>
    /// Indicates whether the player is dead.
    /// </summary>
    public bool isPlayerDead;
}
