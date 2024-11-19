using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[DisallowMultipleComponent]
public class IdleEvent : MonoBehaviour
{
    public event Action<IdleEvent> OnIdle;

    /// <summary>
    /// Calls the idle event.
    /// </summary>
    public void CallIdleEvent(){
        OnIdle?.Invoke(this);
    }
}
