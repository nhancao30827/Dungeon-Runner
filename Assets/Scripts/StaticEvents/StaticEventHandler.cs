using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticEventHandler
{
    // Room changed event
    public static event Action<RoomChangedEventArgs> OnRoomChanged;

    public static void CallRoomChangedEvent(Room room)
    {
        OnRoomChanged?.Invoke(new RoomChangedEventArgs()
        {
            room = room
        });
    }

    public static event Action<EnemyRoomClearedEventArgs> OnEnemyRoomCleared;

    public static void CallEnemyRoomClearedEvent(Room room)
    {
        OnEnemyRoomCleared?.Invoke(new EnemyRoomClearedEventArgs()
        {
            room = room

        });
        Debug.Log($"{room.roomNodeType} cleared");
    }
}

public class RoomChangedEventArgs : EventArgs
{
    public Room room;
}

public class EnemyRoomClearedEventArgs : EventArgs
{
    public Room room;
}
