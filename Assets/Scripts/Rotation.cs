using System;
using UnityEngine;

public class Rotation : MonoBehaviour
{
    [SerializeField] private float speed = 1f;

    private void Update()
    {
        transform.Rotate(transform.up, speed * Time.deltaTime);
    }
}
