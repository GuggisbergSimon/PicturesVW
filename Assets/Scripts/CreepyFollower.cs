using Cinemachine;
using UnityEngine;

public class CreepyFollower : MonoBehaviour
{
    [SerializeField] private GameObject anchor = default;
    [SerializeField] private LayerMask layerProbe = -1;
    [SerializeField] private float distToFollow = 2f;
    [SerializeField] private float angleIncertitude = 20f;
    [SerializeField] private CinemachineVirtualCamera vCam = default;
    [SerializeField] private GameObject[] shapes = default;
    private int _currentShape;
    private Renderer _currentRenderer;
    private bool _isDetected;
    private Cinemachine3rdPersonFollow _vCamAim;
    private float _fovAdjusted;
    private void Start()
    {
        _vCamAim = vCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        _fovAdjusted = Mathf.Rad2Deg * Mathf.Asin(
            2 * Mathf.Sin(Mathf.Deg2Rad * vCam.m_Lens.FieldOfView) *
            Mathf.Sqrt(Mathf.Pow(Screen.width / 2f, 2) + Mathf.Pow(Screen.height / 2f, 2)) / Screen.width);
        _currentShape = 0;
        _currentRenderer = shapes[_currentShape].GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        //when the creep can sees the player, freezing
        bool isWallBetweenCreepVCam = Physics.Raycast(anchor.transform.position,
            vCam.transform.position - anchor.transform.position,
            (vCam.transform.position - anchor.transform.position).magnitude, layerProbe);
        Vector3 anchorAdjusted = anchor.transform.position + _vCamAim.ShoulderOffset + Vector3.up * _vCamAim.VerticalArmLength;
        if (!_isDetected && !isWallBetweenCreepVCam && InFOVOfVCam(anchorAdjusted))
        {
            Debug.Log("detected");
            _isDetected = true;
        }
        else if (!_isDetected && (int) Time.time % 2 == 0)
        {
            Vector3 nextPos = vCam.Follow.parent.position - vCam.Follow.parent.forward * distToFollow -
                              Vector3.up * 0.5f;
            if (!InFOVOfVCam(nextPos))
            transform.position = vCam.Follow.parent.position - vCam.Follow.parent.forward * distToFollow -
                                 Vector3.up * 0.5f;
        }

        //when the creep no longer sees the player, switching form
        else if (_isDetected && (isWallBetweenCreepVCam || !InFOVOfVCam(anchorAdjusted)))
        {
            Debug.Log("changing shape");
            shapes[_currentShape].SetActive(false);
            _currentShape = (_currentShape + 1) % shapes.Length;
            shapes[_currentShape].SetActive(true);
            _currentRenderer = shapes[_currentShape].GetComponent<Renderer>();
            _isDetected = false;
        }
    }

    //Debug.Break();
    private bool InFOVOfVCam(Vector3 point)
    {
        //alternate way to get the same result, less precise
        //return _currentRenderer.isVisible;
        //vCam.Follow.transform.up
        float angleVCamCreep =
            Vector3.Angle(vCam.transform.position - point, vCam.transform.position - vCam.Follow.position);
        return angleVCamCreep < _fovAdjusted;
    }
}