using UnityEngine;

public class PortableCamera : MonoBehaviour
{
    [SerializeField] private int depth = 16;
    [SerializeField] private Vector2 textureSize = new Vector2(512, 512);
    [SerializeField] private Transform target = default;
    private Camera _cam;
    private RenderTexture _renderTexture;


    void Reset()
    {
        if (!_cam) _cam = GetComponentInChildren<Camera>();
        if (!target) target = transform;
    }

    void Awake()
    {
        Reset();
    }

    private void Start()
    {
        if (!_renderTexture)
        {
            _renderTexture = new RenderTexture((int) textureSize.x, (int) textureSize.y, depth);
            _renderTexture.name = "TextureFromCamera_" + _cam.name;
            _renderTexture.Create();
        }

        _cam.targetTexture = _renderTexture;
        target.GetComponent<Renderer>().material.mainTexture = _renderTexture;
    }

    void OnDisable()
    {
        _renderTexture.DiscardContents();
        _renderTexture.Release();
    }
}