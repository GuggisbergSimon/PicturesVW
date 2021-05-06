using System;
using System.Collections.Generic;
using UnityEngine;

public class Audio512Cubes : MonoBehaviour
{
    [SerializeField] private GameObject cubePrefab = default;
    [SerializeField] private float startScale = 1f, maxScale = 2f;
    [SerializeField] private float radius = 20f;
    private List<GameObject> _cubes = new List<GameObject>();

    private void Start()
    {
        for (int i = 0; i < 512; i++)
        {
            GameObject instance = Instantiate(cubePrefab, transform);
            instance.name = "cube" + i;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, 360 / 512f * i, transform.eulerAngles.z);
            instance.transform.localEulerAngles = -transform.eulerAngles;
            instance.transform.position = Vector3.forward * radius;
            _cubes.Add(instance);
        }
    }

    private void Update()
    {
        for (int i = 0; i < _cubes.Count; i++)
        {
                _cubes[i].transform.localScale = new Vector3(_cubes[i].transform.localScale.x,
                    AudioPeer.samples[i] * maxScale + startScale,
                    _cubes[i].transform.localScale.z);
        }
    }
}