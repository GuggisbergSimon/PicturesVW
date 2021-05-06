using System;
using System.Collections.Generic;
using UnityEngine;

public class Audio8Cubes : MonoBehaviour
{
    [SerializeField] private MeshRenderer cubePrefab = default;
    [SerializeField] private float startScale = 1f, maxScale = 2f;
    [SerializeField] private float spaceAlignment = 2f;
    [SerializeField] private bool useBuffer;
    private List<MeshRenderer> _cubes = new List<MeshRenderer>();
    
    private void Start()
    {
        for (int i = -4; i < 4; i++)
        {
            MeshRenderer instance = Instantiate(cubePrefab, transform);
            instance.name = "cube" + i;
            instance.transform.localPosition = Vector3.right * i * 2f ;
            _cubes.Add(instance);
        }
    }

    private void Update()
    {
        for (int i = 0; i < _cubes.Count; i++)
        {
            float f = useBuffer ? AudioPeer.audioBandBuffer[i] : AudioPeer.audioBand[i];
                _cubes[i].transform.localScale = new Vector3(_cubes[i].transform.localScale.x,
                    f * maxScale + startScale, _cubes[i].transform.localScale.z);
                _cubes[i].transform.localPosition = new Vector3(_cubes[i].transform.localPosition.x,
                    (f * maxScale + startScale) / 2, _cubes[i].transform.localPosition.z);
                _cubes[i].materials[0].SetColor("_EmissionColor", new Color(f,f,f));
        }
    }
}
