using UnityEngine;
using UnityEngine.EventSystems;

[SingletonMode(true)]
public class EventSystemInstance : Singleton<EventSystemInstance>
{
    public static EventSystem Current => Instance == null ? null : Instance.eventSystem;

    [CacheComponent]
    [SerializeField]
    private EventSystem eventSystem;
}
