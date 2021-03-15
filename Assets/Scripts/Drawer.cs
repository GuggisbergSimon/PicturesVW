using System;
using System.Collections;
using UnityEngine;

public class Drawer : MonoBehaviour
{
    [SerializeField] private float drawInterval = 0.5f;
    [SerializeField] private float minDistToDraw = 0.1f;
    [SerializeField] private LineRenderer linePrefab = default;
    [SerializeField] private Camera cam = default;
    private LineRenderer _currentLine;
    private bool _isDrawing;

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            _currentLine = Instantiate(linePrefab, transform);
            _isDrawing = true;
            StartCoroutine(Draw());
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            _isDrawing = false;
            _currentLine = null;
        }
        else if (Input.GetButtonDown("Fire2") && !_isDrawing)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }

    private IEnumerator Draw()
    {
        //doesn't work for the initial case
        while (_isDrawing)
        {
            Vector3 newPos = cam.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10f);
            if (_currentLine.positionCount == 0 || Vector3.Distance(_currentLine.GetPosition(_currentLine.positionCount - 1), newPos) > minDistToDraw)
            {
                _currentLine.SetPosition(_currentLine.positionCount++, newPos);
            }
            yield return new WaitForSeconds(drawInterval);
        }
    }
}
