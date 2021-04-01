using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SocialPlatforms;

public class ThirdPersonController : MonoBehaviour
{
    [SerializeField] private float speedMove = 2f;
    [SerializeField] private float speedLook = 2f;
    [SerializeField] private float speedZoom = 2f;
    [SerializeField, Max(90f)] private float angleClampLook = 40f;
    [SerializeField] private Transform cameraTarget = default;
    [SerializeField] private CinemachineVirtualCamera vCam = default;
    private Rigidbody _rigidbody;
    private CinemachineImpulseSource _impulseSource;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void Update()
    {
        //Handles rotation
        AdjustRotation();

        //handles firing (only some small vibrations for now)
        if (Input.GetButtonDown("Fire1"))
        {
            _impulseSource.GenerateImpulse(Camera.main.transform.forward);
        }

        //Handles zoom
        if (Mathf.Abs(Input.GetAxisRaw("Mouse ScrollWheel")) > 0)
        {
            float dist = vCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraDistance;
            if (dist - speedZoom * Input.GetAxisRaw("Mouse ScrollWheel") >= 0)
            {
                vCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraDistance -=
                    speedZoom * Input.GetAxisRaw("Mouse ScrollWheel");
            }
        }
    }

    private void FixedUpdate()
    {
        AdjustPosition();
    }

    private void AdjustRotation()
    {
        cameraTarget.rotation *= Quaternion.AngleAxis(speedLook * Input.GetAxis("Mouse X"), Vector3.up) *
                                 Quaternion.AngleAxis(speedLook * Input.GetAxis("Mouse Y"), Vector3.left);
        Vector3 angles = cameraTarget.localEulerAngles;
        angles.z = 0;
        float angle = angles.x;
        if (angle < 180f && angle > 90f - angleClampLook)
        {
            angles.x = 90f - angleClampLook;
        }
        else if (angle > 180f && angle < 270f + angleClampLook)
        {
            angles.x = 270f + angleClampLook;
        }

        cameraTarget.localEulerAngles = angles;
    }

    private void AdjustPosition()
    {
        Vector3 move = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");

        //adjust rotation of player only if moving
        if (move != Vector3.zero)
        {
            float angle = cameraTarget.localEulerAngles.x;
            _rigidbody.rotation = Quaternion.Euler(0, cameraTarget.rotation.eulerAngles.y, 0);
            cameraTarget.transform.localEulerAngles = new Vector3(angle, 0, 0);
        }

        _rigidbody.velocity += speedMove * Time.deltaTime * move;
    }
}