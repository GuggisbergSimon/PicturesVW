using System;
using UnityEngine;

public class Sine: MonoBehaviour
{
    [SerializeField] private float amplitude = 1f;
    [SerializeField, Min(0.000001f)] private float period = 1f;
    [SerializeField] private float phaseShift = 0f;

    private Vector3 initPos;

    private void Awake()
    {
        initPos = transform.localPosition;
    }

    private void Update()
    {
        transform.localPosition = initPos + transform.right * (amplitude * Mathf.Sin (Time.time * Mathf.PI * 2 / period + phaseShift));
    }
}