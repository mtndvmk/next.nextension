using System;
using UnityEngine;

public class PoolIdentity : MonoBehaviour
{
    [field: SerializeField] public int Id { get; private set; }

    private void OnValidate()
    {
        if (Id == 0)
        {
            refreshId();
        }
    }

    [ContextMenu("Refresh Id")]
    public void refreshId()
    {
        Id = GetHashCode();
    }

    public event Action onRespawnedEvent;
    public event Action onSentToPoolEvent;

    internal void invokeRespawnedEvent()
    {
        onRespawnedEvent?.Invoke();
    }
    internal void invokeSentToPoolEvent()
    {
        onSentToPoolEvent?.Invoke();
    }

    public void removeRespawnedEvents()
    {
        onRespawnedEvent = null;
    }
    public void removeSentToPoolEvents()
    {
        onSentToPoolEvent = null;
    }
    public void removeAllEvents()
    {
        removeRespawnedEvents();
        removeSentToPoolEvents();
    }
}
