using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vuforiaRainer : MonoBehaviour
{
    [SerializeField] private ParticleSystem rain;
    private SphereCollider _collider;
    
    private void Start()
    {
        _collider = GetComponent<SphereCollider>();
        rain.Stop();
        _collider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            rain.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            rain.Stop();
        }
    }
}