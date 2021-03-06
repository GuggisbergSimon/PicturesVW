using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using PDollarGestureRecognizer;

public class Drawer : MonoBehaviour
{
    [SerializeField] private float drawInterval = 0.5f;
    [SerializeField] private float minDistToDraw = 0.1f;
    [SerializeField] private float minScoreToDraw = 0.9f;
    [SerializeField] private LineRenderer linePrefab = default;
    [SerializeField] private Camera cam = default;
    private LineRenderer _currentLine;
    private bool _isDrawing;
    private bool _isGuessed;

    private List<Gesture> _trainingSet = new List<Gesture>();
    private List<Point> _points = new List<Point>();
    private int _strokeId = -1;
    private List<LineRenderer> _gestureLinesRenderer = new List<LineRenderer>();

    private void Start()
    {
        //Load pre-made gestures
        /*
        TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/10-stylus-MEDIUM/");
        foreach (TextAsset gestureXml in gesturesXml)
            _trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));
        */

        //Load user custom gestures
        string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
        foreach (string filePath in filePaths)
            _trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (_isGuessed)
            {
                _isGuessed = false;
                ClearStrokes();
            }
            _isDrawing = true;
            ++_strokeId;
            _currentLine = Instantiate(linePrefab, transform);
            _gestureLinesRenderer.Add(_currentLine);

            StartCoroutine(Draw());
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            _isDrawing = false;
            _currentLine = null;
            ValidateGesture();
        }
        else if (Input.GetButtonDown("Fire2") && !_isDrawing)
        {
            ClearStrokes();
        }
        else if (Input.GetButtonDown("Fire3"))
        {
            
        }
        else if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            //todo let user choose name of new gesture
            string newGestureName = "test";

            string fileName = $"{Application.persistentDataPath}/{newGestureName}-{DateTime.Now.ToFileTime()}.xml";
            Debug.Log(fileName);
#if !UNITY_WEBPLAYER
            GestureIO.WriteGesture(_points.ToArray(), newGestureName, fileName);
#endif
            _trainingSet.Add(new Gesture(_points.ToArray(), newGestureName));
        }
    }

    private void ValidateGesture()
    {
        if (_points == null) return;
        Gesture candidate = new Gesture(_points.ToArray());
        Result gestureResult = PointCloudRecognizer.Classify(candidate, _trainingSet.ToArray());
            
        if (gestureResult.Score > minScoreToDraw)
        {
            Debug.Log(gestureResult.GestureClass + " " + gestureResult.Score);
            _isGuessed = true;
            if (gestureResult.GestureClass.Equals("NotInfinity"))
            {
                foreach (var line in _gestureLinesRenderer)
                {
                    line.startColor = Color.red;
                    line.endColor = Color.blue;
                }
            }
            else if (gestureResult.GestureClass.Equals("FivePointStar"))
            {
                foreach (var line in _gestureLinesRenderer)
                {
                    line.startColor = Color.yellow;
                    line.endColor = Color.magenta;
                }
            }
        }
        else
        {
            ClearStrokes();
        }
    }

    private void ClearStrokes()
    {
        
        _isDrawing = false;
        _strokeId = -1;
        _points.Clear();
        foreach (LineRenderer lineRenderer in _gestureLinesRenderer)
        {
            Destroy(lineRenderer.gameObject);
        }

        _gestureLinesRenderer.Clear();
    }

    private IEnumerator Draw()
    {
        //doesn't work for first case
        while (_isDrawing)
        {
            Vector3 newPos = cam.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10f);
            if (_currentLine.positionCount == 0 || (_currentLine.positionCount > 0 &&
                                                    Vector3.Distance(
                                                        _currentLine.GetPosition(_currentLine.positionCount - 1),
                                                        newPos) > minDistToDraw))
            {
                _points.Add(new Point(Input.mousePosition.x, -Input.mousePosition.y, _strokeId));
                _currentLine.SetPosition(_currentLine.positionCount++, newPos);
            }

            yield return new WaitForSeconds(drawInterval);
        }
    }
}