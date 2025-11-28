using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// EventManager is a singleton class that manages game events.
/// It allows for the creation and retrieval of events of specific types.
/// </summary>
public class EventManager : Singleton<EventManager>, IEventManager, IInitializableManager
{
    private readonly Dictionary<Type, object> events = new();
    private readonly object lockObj = new();

    public UnityAction<bool> OnInitialized { get; set; }

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        OnInitialized?.Invoke(true);
    }

    /// <summary>
    /// Get or create an event of type T
    /// </summary>
    private GameEvent<T> GetEvent<T>()
    {
        lock (lockObj)
        {
            var type = typeof(T);
            if (!events.TryGetValue(type, out var existingEvent))
            {
                var newEvent = new GameEvent<T>();
                events[type] = newEvent;
                return newEvent;
            }

            return (GameEvent<T>)existingEvent;
        }
    }

    /// <summary>
    /// Clear all events and dispose of them if they implement IDisposable  
    /// </summary>
    public void ClearEvents()
    {
        lock (lockObj)
        {
            foreach (var eventPair in events)
            {
                if (eventPair.Value is IDisposable disposableEvent)
                {
                    disposableEvent.Dispose();
                }
            }
            events.Clear();
        }
    }

    /// <summary>
    /// Subscribe to an event of type T
    /// </summary>
    /// param name="listener">The listener to subscribe</param>
    /// <typeparam name="T">The type of the event</typeparam>   
    public void Subscribe<T>(Action<T> listener)
    {
        GetEvent<T>().Subscribe(listener);
    }

    /// <summary>
    /// Unsubscribe from an event of type T.
    /// Remember to unsubscribe when no longer needed.
    /// Noted: Do not this for OnDestroy (it will unsubscribe automatically).
    /// </summary>
    /// <param name="listener">The listener to unsubscribe</param>  
    public void Unsubscribe<T>(Action<T> listener)
    {
        GetEvent<T>().Unsubscribe(listener);
    }

    /// <summary>
    /// Invoke an event of type T with the provided event data
    /// </summary>
    /// <param name="eventData">The data to pass to the event listeners</param>
    public void Invoke<T>(T eventData)
    {
        GetEvent<T>().Invoke(eventData);
    }

    protected override void OnDestroy()
    {
        ClearEvents();
    }

    public static void Register<T>(Action<T> listener)
    {
        Debug.Log($"[EventManager] Registering listener for event type: {typeof(T)}");
        Instance?.Subscribe(listener);
    }

    public static void Unregister<T>(Action<T> listener)
    {
        Instance?.Unsubscribe(listener);
    }

    public static void TriggerEvent<T>(T eventData)
    {
        Instance?.Invoke(eventData);
    }

}