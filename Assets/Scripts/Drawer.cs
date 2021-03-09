using System;
using UnityEngine;

public class Drawer : MonoBehaviour
{
    [SerializeField] private Color color = Color.white;
    private LineRenderer _lineRenderer;
    private bool _isDrawing;
    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            _isDrawing = true;
            _lineRenderer.positionCount = 0;
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            _isDrawing = false;
        }
    }

    private void FixedUpdate()
    {
        if (_isDrawing)
        {
            _lineRenderer.positionCount++;
            _lineRenderer.SetPosition(_lineRenderer.positionCount-1, Input.mousePosition);
        }
    }
}
