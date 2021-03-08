using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vuforiaTest : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private SphereCollider _collider;
    
    private void Start()
    {
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        _collider = GetComponent<SphereCollider>();
        _particleSystem.Stop();
        _collider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            var m = _particleSystem.main;
            m.startColor = Color.red;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            var m = _particleSystem.main;
            m.startColor = Color.blue;
        }
    }
}
