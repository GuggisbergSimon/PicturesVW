using System;
using UnityEngine;

public class PortableCamera : MonoBehaviour
{
    [SerializeField] private int depth = 16;
    [SerializeField] private Vector2Int textureSize = new Vector2Int(512, 512);
    private Camera _cam;
    private RenderTexture _renderTexture;


    void Reset()
    {
        if (!_cam)
        {
            _cam = GetComponentInChildren<Camera>();
        }
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
        //for live feed
        //GameManager.Instance.UIManager.Feed.texture = _cam.targetTexture;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            RenderTexture.active = _renderTexture;
            Texture2D texture2D = new Texture2D(textureSize.x, textureSize.y);
            texture2D.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
            texture2D.Apply();
            GameManager.Instance.UIManager.Feed.texture = texture2D;
        }
    }

    void OnDisable()
    {
        _renderTexture.DiscardContents();
        _renderTexture.Release();
    }
}