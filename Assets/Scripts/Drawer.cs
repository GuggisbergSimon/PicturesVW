using System;
using System.Collections;
using UnityEngine;

public class Drawer : MonoBehaviour
{
    [SerializeField] private float drawInterval = 0.5f;
    [SerializeField] private LineRenderer linePrefab = default;
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
        while (_isDrawing)
        {
            Debug.Log("test"); //it works, but not with currentline ,wtf TODO fix that shit
            //_currentLine.SetPosition(_currentLine.positionCount++, Camera.current.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10f));
            yield return new WaitForSeconds(drawInterval);
        }
    }
}
