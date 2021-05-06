using UnityEngine;

public class ScreenShotter : MonoBehaviour
{
    [SerializeField] private bool updateEachFrame = false;
    [SerializeField] private bool isMirror = false;
    private MeshRenderer _renderer;
    private Camera _cam;
    private RenderTexture _renderTexture;

    private void Reset()
    {
        if (!_cam)
        {
            _cam = Camera.main;
        }
    }

    private void Awake()
    {
        Reset();
        _renderer = GetComponent<MeshRenderer>();
        _cam = GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        if (isMirror)
        {
            int depth = 16;
            Vector2 textureSize = new Vector2(512, 512);
            if (!_renderTexture)
            {
                _renderTexture = new RenderTexture((int) textureSize.x, (int) textureSize.y, depth);
                _renderTexture.Create();
            }

            _cam.targetTexture = _renderTexture;
            _renderer.material.mainTexture = _renderTexture;
        }
    }

    private void Update()
    {
        if (Application.isPlaying && (Input.GetButtonDown("Fire1") && !isMirror || updateEachFrame))
        {
            _renderer.material.mainTexture = ScreenCapture.CaptureScreenshotAsTexture();
        }
    }

    private void OnDisable()
    {
        if (_renderTexture)
        {
            _renderTexture.DiscardContents();
            _renderTexture.Release();
        }
    }
}