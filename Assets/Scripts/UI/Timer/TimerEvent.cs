using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerEvent : MonoBehaviour
{
    public static event Action<TimerEvent, TimerEventArgs> OnTimerEvent;

    public void CallTimerEvent(GameState gameState)
    {
        OnTimerEvent?.Invoke(this, new TimerEventArgs { gameState = gameState});
    }
}

public class TimerEventArgs : EventArgs
{
    public GameState gameState;
}
