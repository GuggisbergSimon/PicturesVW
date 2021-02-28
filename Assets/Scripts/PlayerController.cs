//Code mainly taken from catlikecoding

using Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform playerInputSpace = default;

    [SerializeField, Range(0f, 100f)] private float
        maxSpeed = 10f,
        maxSprintSpeed = 20f;

    [SerializeField, Range(0f, 100f)] private float
        maxAcceleration = 10f,
        maxAirAcceleration = 1f,
        maxSprintAcceleration = 20f;

    [SerializeField, Range(0f, 10f)] private float jumpHeight = 2f;
    [SerializeField, Range(0, 5)] private int maxAirJumps = 0;
    [SerializeField, Range(0, 90)] private float maxGroundAngle = 25f, maxStairsAngle = 50f;
    [SerializeField, Range(0f, 100f)] private float maxSnapSpeed = 100f;
    [SerializeField, Min(0f)] private float probeDistance = 1f;
    [SerializeField] private LayerMask probeMask = -1, stairsMask = -1;
    [SerializeField] private Material normalMaterial = default;
    [SerializeField] private float maxDistancePickUp = 2f;

    private Rigidbody _body, _connectedBody, _previousConnectedBody;
    private Vector3 _playerInput;
    private Vector3 _velocity, _connectionVelocity;
    private Vector3 _connectionWorldPosition, _connectionLocalPosition;
    private Vector3 _upAxis, _rightAxis, _forwardAxis;
    private bool _desiredJump;
    private Vector3 _contactNormal, _steepNormal;
    private int _groundContactCount, _steepContactCount;
    private bool _hasCamera = false;
    private Transform _portableCamera;
    private int _jumpPhase;
    private float _minGroundDotProduct, _minStairsDotProduct;
    private int _stepsSinceLastGrounded, _stepsSinceLastJump;
    private MeshRenderer _meshRenderer;

    private bool OnGround => _groundContactCount > 0;
    private bool OnSteep => _steepContactCount > 0;

    public void PreventSnapToGround()
    {
        _stepsSinceLastJump = -1;
    }

    private void OnValidate()
    {
        _minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        _minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    private void Awake()
    {
        _body = GetComponent<Rigidbody>();
        _body.useGravity = false;
        _meshRenderer = GetComponent<MeshRenderer>();
        OnValidate();
    }

    private void Update()
    {
        _playerInput.x = Input.GetAxis("Horizontal");
        _playerInput.y = Input.GetAxis("Vertical");
        _playerInput.z = 0f;
        _playerInput = Vector3.ClampMagnitude(_playerInput, 1f);

        if (playerInputSpace)
        {
            _rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, _upAxis);
            _forwardAxis =
                ProjectDirectionOnPlane(playerInputSpace.forward, _upAxis);
        }
        else
        {
            _rightAxis = ProjectDirectionOnPlane(Vector3.right, _upAxis);
            _forwardAxis = ProjectDirectionOnPlane(Vector3.forward, _upAxis);
        }

        _desiredJump |= Input.GetButtonDown("Jump");

        //turns the player towards where the cinemachine is pointing
        transform.rotation = Quaternion.Euler(0f,
            GameManager.Instance.LevelManager.VCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value,
            0f);

        //Camera Pickup/Drop
        Transform head = transform.GetChild(0);
        head.localRotation = Quaternion.Euler(GameManager.Instance.LevelManager.VCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value, 0f, 0f);
        Ray r = new Ray(head.position, head.forward );
        Debug.DrawLine(r.origin, r.origin + r.direction * maxDistancePickUp, Color.green, 0.2f);
        if (!_hasCamera && Physics.Raycast(r, out RaycastHit hit,
            maxDistancePickUp) && hit.transform.CompareTag("PortableCamera"))
        {
            GameManager.Instance.UIManager.AltPopup.gameObject.SetActive(true);
            GameManager.Instance.UIManager.EqPopup.text = "q et e pour ajuster l'angle de la caméra";
            GameManager.Instance.UIManager.ZoomPercentage.transform.parent.gameObject.SetActive(false);
            if (Input.GetButtonDown("Fire2"))
            {
                _portableCamera = hit.transform;
                _portableCamera.SetParent(transform.GetChild(0));
                _hasCamera = true;
                GameManager.Instance.PCamera.Pickup(true);
            }
            else if (Input.GetButtonDown("Zoom+"))
            {
                GameManager.Instance.PCamera.AdjustRotation(1);
            }
            else if (Input.GetButtonDown("Zoom-"))
            {
                GameManager.Instance.PCamera.AdjustRotation(-1);
            }
        }
        else if (Input.GetButtonDown("Fire2") && _hasCamera)
        {
            GameManager.Instance.PCamera.Pickup(false);
            _portableCamera.parent = null;
            _portableCamera = null;
            _hasCamera = false;
        }
        else if (Input.GetButtonDown("Zoom+"))
        {
            GameManager.Instance.PCamera.AdjustZoom(1);
        }
        else if (Input.GetButtonDown("Zoom-"))
        {
            GameManager.Instance.PCamera.AdjustZoom(-1);
        }
        else
        {
            GameManager.Instance.UIManager.AltPopup.gameObject.SetActive(false);
            GameManager.Instance.UIManager.EqPopup.text = "q et e pour ajuster l'angle le zoom";
            GameManager.Instance.UIManager.ZoomPercentage.transform.parent.gameObject.SetActive(true);
        }

        _meshRenderer.material = normalMaterial;
    }

    private void FixedUpdate()
    {
        Vector3 gravity = CustomGravity.GetGravity(_body.position, out _upAxis);
        UpdateState();

        AdjustVelocity();

        if (_desiredJump)
        {
            _desiredJump = false;
            Jump(gravity);
        }

        if (OnGround && _velocity.sqrMagnitude < 0.01f)
        {
            _velocity +=
                _contactNormal *
                (Vector3.Dot(gravity, _contactNormal) * Time.deltaTime);
        }
        else
        {
            _velocity += gravity * Time.deltaTime;
        }

        _body.velocity = _velocity;
        ClearState();
    }

    private void ClearState()
    {
        _groundContactCount = _steepContactCount = 0;
        _contactNormal = _steepNormal = Vector3.zero;
        _connectionVelocity = Vector3.zero;
        _previousConnectedBody = _connectedBody;
        _connectedBody = null;
    }

    private void UpdateState()
    {
        _stepsSinceLastGrounded += 1;
        _stepsSinceLastJump += 1;
        _velocity = _body.velocity;
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            _stepsSinceLastGrounded = 0;
            if (_stepsSinceLastJump > 1)
            {
                _jumpPhase = 0;
            }

            if (_groundContactCount > 1)
            {
                _contactNormal.Normalize();
            }
        }
        else
        {
            _contactNormal = _upAxis;
        }

        if (_connectedBody)
        {
            if (_connectedBody.isKinematic || _connectedBody.mass >= _body.mass)
            {
                UpdateConnectionState();
            }
        }
    }

    private void UpdateConnectionState()
    {
        if (_connectedBody == _previousConnectedBody)
        {
            Vector3 connectionMovement =
                _connectedBody.transform.TransformPoint(_connectionLocalPosition) -
                _connectionWorldPosition;
            _connectionVelocity = connectionMovement / Time.deltaTime;
        }

        _connectionWorldPosition = _body.position;
        _connectionLocalPosition = _connectedBody.transform.InverseTransformPoint(
            _connectionWorldPosition
        );
    }

    private bool SnapToGround()
    {
        if (_stepsSinceLastGrounded > 1 || _stepsSinceLastJump <= 2 /*|| InWater*/)
        {
            return false;
        }

        float speed = _velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }

        if (!Physics.Raycast(
            _body.position, -_upAxis, out RaycastHit hit,
            probeDistance, probeMask, QueryTriggerInteraction.Ignore
        ))
        {
            return false;
        }

        float upDot = Vector3.Dot(_upAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }

        _groundContactCount = 1;
        _contactNormal = hit.normal;
        float dot = Vector3.Dot(_velocity, hit.normal);
        if (dot > 0f)
        {
            _velocity = (_velocity - hit.normal * dot).normalized * speed;
        }

        _connectedBody = hit.rigidbody;
        return true;
    }

    private bool CheckSteepContacts()
    {
        if (_steepContactCount > 1)
        {
            _steepNormal.Normalize();
            float upDot = Vector3.Dot(_upAxis, _steepNormal);
            if (upDot >= _minGroundDotProduct)
            {
                _steepContactCount = 0;
                _groundContactCount = 1;
                _contactNormal = _steepNormal;
                return true;
            }
        }

        return false;
    }

    private void AdjustVelocity()
    {
        float acceleration, speed;
        Vector3 xAxis, zAxis;
        acceleration = OnGround
            ? Input.GetButton("Fire3") ? maxSprintAcceleration : maxAcceleration
            : maxAirAcceleration;
        speed = OnGround && Input.GetButton("Fire3") ? maxSprintSpeed : maxSpeed;
        xAxis = _rightAxis;
        zAxis = _forwardAxis;

        xAxis = ProjectDirectionOnPlane(xAxis, _contactNormal);
        zAxis = ProjectDirectionOnPlane(zAxis, _contactNormal);

        Vector3 relativeVelocity = _velocity - _connectionVelocity;
        float currentX = Vector3.Dot(relativeVelocity, xAxis);
        float currentZ = Vector3.Dot(relativeVelocity, zAxis);

        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX =
            Mathf.MoveTowards(currentX, _playerInput.x * speed, maxSpeedChange);
        float newZ =
            Mathf.MoveTowards(currentZ, _playerInput.y * speed, maxSpeedChange);

        _velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    private void Jump(Vector3 gravity)
    {
        Vector3 jumpDirection;
        if (OnGround)
        {
            jumpDirection = _contactNormal;
        }
        else if (OnSteep)
        {
            jumpDirection = _steepNormal;
            _jumpPhase = 0;
        }
        else if (maxAirJumps > 0 && _jumpPhase <= maxAirJumps)
        {
            if (_jumpPhase == 0)
            {
                _jumpPhase = 1;
            }

            jumpDirection = _contactNormal;
        }
        else
        {
            return;
        }

        _stepsSinceLastJump = 0;
        _jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);

        jumpDirection = (jumpDirection + _upAxis).normalized;
        float alignedSpeed = Vector3.Dot(_velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }

        _velocity += jumpDirection * jumpSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void EvaluateCollision(Collision collision)
    {
        int layer = collision.gameObject.layer;
        float minDot = GetMinDot(layer);
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(_upAxis, normal);
            if (upDot >= minDot)
            {
                _groundContactCount += 1;
                _contactNormal += normal;
                _connectedBody = collision.rigidbody;
            }
            else
            {
                if (upDot > -0.01f)
                {
                    _steepContactCount += 1;
                    _steepNormal += normal;
                    if (_groundContactCount == 0)
                    {
                        _connectedBody = collision.rigidbody;
                    }
                }
            }
        }
    }

    private Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    private float GetMinDot(int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ? _minGroundDotProduct : _minStairsDotProduct;
    }
}