using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameEvent is a generic event class that allows subscribing, unsubscribing, and invoking events with a specific type.
/// </summary>
/// <typeparam name="T"></typeparam>
public class GameEvent<T>: IDisposable
{
    private readonly List<Action<T>> listeners = new();
    private readonly object lockObj = new();

    public void Subscribe(Action<T> listener)
    {
        lock (lockObj)
        {
            if (!listeners.Contains(listener))
                listeners.Add(listener);
        }
    }

    public void Unsubscribe(Action<T> listener)
    {
        lock (lockObj)
        {
            if (listeners.Contains(listener))
                listeners.Remove(listener);
        }
    }

    public void Invoke(T value)
    {
        List<Action<T>> listenersCopy;

        // Copy danh sách để tránh deadlock hoặc exception khi listener unsubscribe trong quá trình invoke
        lock (lockObj)
        {
            listenersCopy = new List<Action<T>>(listeners);
        }

        foreach (var listener in listenersCopy)
        {
            listener.Invoke(value);
        }
    }

    public void Dispose()
    {
        lock (lockObj)
        {
            Debug.Log("Disposing GameEvent: " + typeof(T).Name);
            listeners.Clear();
        }
    }
}