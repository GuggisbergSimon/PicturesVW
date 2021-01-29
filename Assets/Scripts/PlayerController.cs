using Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)] float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] float maxAcceleration = 10f, maxAirAcceleration = 1f;
    [SerializeField, Range(0f, 10f)] float jumpHeight = 2f;
    [SerializeField, Range(0, 5)] int maxAirJumps = 0;
    [SerializeField, Range(0, 90)] float maxGroundAngle = 25f;
    [SerializeField] private float maxDistancePickUp = 1f;

    private Rigidbody _body;
    private Vector3 _velocity, _desiredVelocity;
    private Vector3 _contactNormal;
    private bool _desiredJump;
    private int _groundContactCount;
    private int _jumpPhase;
    private float _minGroundDotProduct;
    private bool _hasCamera = false;
    private Transform _portableCamera;

    private bool OnGround => _groundContactCount > 0;

    private void OnValidate()
    {
        _minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    private void Awake()
    {
        _body = GetComponent<Rigidbody>();
        OnValidate();
    }

    void Update()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        _desiredVelocity =
            new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        _desiredJump |= Input.GetButtonDown("Jump");
        if (Input.GetButtonDown("Fire1") && !_hasCamera && Physics.Raycast(
            GameManager.Instance.VCamera.transform.position,
            transform.forward, out RaycastHit hit,
            maxDistancePickUp) && hit.transform.CompareTag("PortableCamera"))
        {
            //picking the camera
            _portableCamera = hit.transform.parent.transform;
            _portableCamera.SetParent(transform);
            _hasCamera = true;
        }
        else if (Input.GetButtonDown("Fire1") && _hasCamera)
        {
            //dropping the camera
            _portableCamera.parent = null;
            _portableCamera = null;
            _hasCamera = false;
        }

        transform.rotation = Quaternion.Euler(0f,
            GameManager.Instance.VCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value, 0f);
    }

    void FixedUpdate()
    {
        //body.velocity = (Vector3.right * playerInput.x + Vector3.forward * playerInput.z) * speed + Vector3.up * body.velocity.y;

        UpdateState();
        AdjustVelocity();

        if (_desiredJump)
        {
            _desiredJump = false;
            Jump();
        }

        _body.velocity = _velocity;
        ClearState();
    }

    void ClearState()
    {
        _groundContactCount = 0;
        _contactNormal = Vector3.zero;
    }

    void UpdateState()
    {
        _velocity = _body.velocity;
        if (OnGround)
        {
            _jumpPhase = 0;
            if (_groundContactCount > 1)
            {
                _contactNormal.Normalize();
            }
        }
        else
        {
            _contactNormal = Vector3.up;
        }
    }

    void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlane(transform.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(transform.forward).normalized;

        float currentX = Vector3.Dot(_velocity, xAxis);
        float currentZ = Vector3.Dot(_velocity, zAxis);

        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX =
            Mathf.MoveTowards(currentX, _desiredVelocity.x, maxSpeedChange);
        float newZ =
            Mathf.MoveTowards(currentZ, _desiredVelocity.z, maxSpeedChange);

        _velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    void Jump()
    {
        if (OnGround || _jumpPhase < maxAirJumps)
        {
            _jumpPhase += 1;
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            float alignedSpeed = Vector3.Dot(_velocity, _contactNormal);
            if (alignedSpeed > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            }

            _velocity += _contactNormal * jumpSpeed;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= _minGroundDotProduct)
            {
                _groundContactCount += 1;
                _contactNormal += normal;
            }
        }
    }

    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - _contactNormal * Vector3.Dot(vector, _contactNormal);
    }
}