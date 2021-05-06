using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Interactable : MonoBehaviour
{
    [Serializable] public class InteractedEvent : UnityEvent {}

    // Event delegates triggered on click.
    [FormerlySerializedAs("onInteract")] [SerializeField]
    private InteractedEvent onInteract = new InteractedEvent();

    private void Update()
    {
        //todo properly call interactable
        //onInteract.Invoke();
    }
}
