using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PortableCamera : MonoBehaviour
{
    [SerializeField] private int depth = 16;
    [SerializeField] private Vector2Int textureSize = new Vector2Int(512, 512);
    [SerializeField] private int zoomSpeed = 5;
    [SerializeField] private int minZoom = 5, maxZoom = 150;
    [SerializeField] private Material liveFeedMaterial = default;
    [SerializeField] private MeshRenderer camPlaneRenderer = default;
    [SerializeField] private Transform pivot = default;
    [SerializeField] private float angleSpeed = 5f;
    [SerializeField] private float minAngle = 10f, maxAngle = 170f;
    private Camera _cam;
    public Camera Cam => _cam;
    private RenderTexture _renderTexture;
    private Rigidbody _body;
    private Collider[] _colliders;

    private void Reset()
    {
        if (!_cam)
        {
            _cam = GetComponentInChildren<Camera>();
        }
    }

    private void Awake()
    {
        Reset();
        _body = GetComponent<Rigidbody>();
        _colliders = GetComponentsInChildren<Collider>();
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
        liveFeedMaterial.mainTexture = _renderTexture;
        AdjustZoom(0);
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
            //apply on hud
            GameManager.Instance.UIManager.Feed.texture = texture2D;
            //save as a file
            File.WriteAllBytes("capture.png",texture2D.EncodeToPNG());
        }
    }

    public void Pickup(bool pickingUp)
    {
        if (pickingUp)
        {
            _body.Sleep();
            _body.isKinematic = true;
            foreach (var c in _colliders)
            {
                c.enabled = false;
            }
            //todo reset position + rotation
        }
        else
        {
            _body.WakeUp();
            _body.isKinematic = false;
            foreach (var c in _colliders)
            {
                c.enabled = true;
            }
        }
    }

    public void AdjustZoom(int value)
    {
        _cam.fieldOfView += value * zoomSpeed;
        GameManager.Instance.UIManager.ZoomPercentage.fillAmount = (minZoom + _cam.fieldOfView) / (maxZoom - minZoom);
    }

    public void AdjustRotation(float value)
    {
        pivot.Rotate(pivot.right, value * angleSpeed, Space.World);
    }

    private void OnDisable()
    {
        _renderTexture.DiscardContents();
        _renderTexture.Release();
    }
}