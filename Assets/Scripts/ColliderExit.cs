using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class ColliderExit : MonoBehaviour
{
    //[Serializable] public class InteractedEvent : UnityEvent {}

    // Event delegates triggered on click.
    [FormerlySerializedAs("onInteract")] [SerializeField]
    private UnityEvent onInteract = new UnityEvent();

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            onInteract.Invoke();
        }
    }
}