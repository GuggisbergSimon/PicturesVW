 using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

using PDollarGestureRecognizer;

public class Demo : MonoBehaviour {

	[SerializeField] private LineRenderer gestureOnScreenPrefab = default;
	private List<Gesture> _trainingSet = new List<Gesture>();
	private List<Point> _points = new List<Point>();
	private int _strokeId = -1;
	private Vector3 _virtualKeyPosition = Vector2.zero;
	private Rect _drawArea;
	private RuntimePlatform _platform;
	private int _vertexCount = 0;
	private List<LineRenderer> _gestureLinesRenderer = new List<LineRenderer>();
	private LineRenderer _currentGestureLineRenderer;
	private Camera _cam;

	//GUI
	private string _message;
	private bool _recognized;
	private string _newGestureName = "";

	private void Start ()
	{

		_cam = GetComponent<Camera>();
		_platform = Application.platform;
		_drawArea = new Rect(0, 0, Screen.width - Screen.width / 3, Screen.height);

		//Load pre-made gestures
		TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/10-stylus-MEDIUM/");
		foreach (TextAsset gestureXml in gesturesXml)
			_trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));

		//Load user custom gestures
		string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
		foreach (string filePath in filePaths)
			_trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));
	}

	private void Update () {

		if (_platform == RuntimePlatform.Android || _platform == RuntimePlatform.IPhonePlayer) {
			if (Input.touchCount > 0) {
				_virtualKeyPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
			}
		} else {
			if (Input.GetMouseButton(0)) {
				_virtualKeyPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
			}
		}

		if (_drawArea.Contains(_virtualKeyPosition)) {

			if (Input.GetMouseButtonDown(0)) {

				if (_recognized) {

					_recognized = false;
					_strokeId = -1;

					_points.Clear();

					foreach (LineRenderer lineRenderer in _gestureLinesRenderer) {

						lineRenderer.positionCount = 0;
						Destroy(lineRenderer.gameObject);
					}

					_gestureLinesRenderer.Clear();
				}

				++_strokeId;
				
				_currentGestureLineRenderer = Instantiate(gestureOnScreenPrefab, transform.position, transform.rotation);
				
				_gestureLinesRenderer.Add(_currentGestureLineRenderer);
				
				_vertexCount = 0;
			}
			
			if (Input.GetMouseButton(0)) {
				_points.Add(new Point(_virtualKeyPosition.x, -_virtualKeyPosition.y, _strokeId));
				_vertexCount = ++_currentGestureLineRenderer.positionCount;
				_currentGestureLineRenderer.SetPosition(_vertexCount - 1, _cam.ScreenToWorldPoint(new Vector3(_virtualKeyPosition.x, _virtualKeyPosition.y, 10)));
			}
		}
	}

	private void OnGUI() {

		GUI.Box(_drawArea, "Draw Area");

		GUI.Label(new Rect(10, Screen.height - 40, 500, 50), _message);

		if (GUI.Button(new Rect(Screen.width - 100, 10, 100, 30), "Recognize")) {

			_recognized = true;

			Gesture candidate = new Gesture(_points.ToArray());
			Result gestureResult = PointCloudRecognizer.Classify(candidate, _trainingSet.ToArray());
			
			_message = gestureResult.GestureClass + " " + gestureResult.Score;
		}

		GUI.Label(new Rect(Screen.width - 200, 150, 70, 30), "Add as: ");
		_newGestureName = GUI.TextField(new Rect(Screen.width - 150, 150, 100, 30), _newGestureName);

		if (GUI.Button(new Rect(Screen.width - 50, 150, 50, 30), "Add") && _points.Count > 0 && _newGestureName != "") {

			string fileName = String.Format("{0}/{1}-{2}.xml", Application.persistentDataPath, _newGestureName, DateTime.Now.ToFileTime());

			#if !UNITY_WEBPLAYER
				GestureIO.WriteGesture(_points.ToArray(), _newGestureName, fileName);
			#endif

			_trainingSet.Add(new Gesture(_points.ToArray(), _newGestureName));

			_newGestureName = "";
		}
	}
}
