// Invoke (an) event handler(s) when an object enters this gameobject's collider

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ColliderEventTrigger : MonoBehaviour
{
    public EventTrigger.TriggerEvent onEnter;

    [Tooltip("Only trigger the event if the colliding object has one of these tags. Leave empty to trigger with all objets")]
    public List<string> tagFilter;

    void OnTriggerEnter(Collider other) {
        if (tagFilter.Count == 0 || tagFilter.Contains(other.tag)) {
            BaseEventData eventData = new BaseEventData(EventSystem.current);
            onEnter.Invoke(eventData);
        }
    }
}
