using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cinemachine;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SocialPlatforms;

[RequireComponent(typeof(Rigidbody))]
public class ThirdPersonController : MonoBehaviour
{
    [SerializeField] private float speedMove = 2f;
    [SerializeField] private float speedLook = 2f;
    [SerializeField] private float speedZoom = 2f;
    [SerializeField, Max(90f)] private float angleClampLook = 40f;
    [SerializeField] private Vector2Int textureSize = new Vector2Int(200, 200);
    [Space(10)] [SerializeField] private Transform cameraTarget = default;

    [SerializeField, Tooltip("main virtual camera")]
    private CinemachineVirtualCamera vCam = default;

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

        //Handles firing (only some small vibrations for now)
        if (Input.GetButtonDown("Fire1"))
        {
            _impulseSource.GenerateImpulse(Camera.main.transform.forward);

            StartCoroutine(TakeScreen());
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

        //Adjusts rotation of player, only if moving, to face forward
        if (move != Vector3.zero)
        {
            //note : small jittering in rotation due to directly setting it, by design
            float angle = cameraTarget.localEulerAngles.x;
            _rigidbody.rotation = Quaternion.Euler(0, cameraTarget.rotation.eulerAngles.y, 0);
            cameraTarget.transform.localEulerAngles = new Vector3(angle, 0, 0);
        }

        _rigidbody.velocity += speedMove * Time.deltaTime * move;
    }

    private IEnumerator TakeScreen()
    {
        Debug.Log("Begin...");
        yield return null;

        //use either a screenshot (rn a little bugged) or a .png located in Assets/Resources/base.png
        Texture2D texture2D = ScreenCapture.CaptureScreenshotAsTexture();
        //Texture2D texture2D = Resources.Load<Texture2D>("base");
        
        File.WriteAllBytes("0_start.png", texture2D.EncodeToPNG());
        //grayscale
        float[][] grayTex = new float[texture2D.width][];
        for (int index = 0; index < texture2D.width; index++)
        {
            grayTex[index] = new float[texture2D.height];
        }

        for (int i = 0; i < texture2D.width; i++)
        {
            for (int j = 0; j < texture2D.height; j++)
            {
                Color p = texture2D.GetPixel(i, j);
                float gray = 0.21f * p.r + 0.72f * p.g + 0.07f * p.b;
                grayTex[i][j] = gray;
                texture2D.SetPixel(i, j, new Color(gray, gray, gray));
            }
        }

        File.WriteAllBytes("1_gray.png", texture2D.EncodeToPNG());

        //gaussian blur
        int[,] gaussianMatrix = new int[,]
        {
            {2, 4, 5, 4, 2},
            {4, 9, 12, 9, 4},
            {5, 12, 15, 12, 5},
            {4, 9, 12, 9, 4},
            {2, 4, 5, 4, 2}
        };

        float[][] gaussianTex = new float[texture2D.width][];
        for (int index = 0; index < texture2D.width; index++)
        {
            gaussianTex[index] = new float[texture2D.height];
        }
        for (int i = 0; i < texture2D.width; i++)
        {
            for (int j = 0; j < texture2D.height; j++)
            {
                float a = 0f;
                for (int i_ = 0; i_ < gaussianMatrix.GetLength(0); i_++)
                {
                    for (int j_ = 0; j_ < gaussianMatrix.GetLength(1); j_++)
                    {
                        int x = i + i_ - gaussianMatrix.GetLength(0) / 2;
                        int y = j + j_ - gaussianMatrix.GetLength(1) / 2;
                        if (x < 0 || x >= texture2D.width || y < 0 || y >= texture2D.height)
                        {
                            break;
                        }

                        a += gaussianMatrix[i_, j_] * grayTex[x][y] / 159;
                    }
                }

                gaussianTex[i][j] = a;
                texture2D.SetPixel(i, j, new Color(a, a, a));
            }
        }

        File.WriteAllBytes("2_gaussian.png", texture2D.EncodeToPNG());
        int[,] sobelXMatrix =
        {
            {1, 0, -1},
            {2, 0, -2},
            {1, 0, -1}
        };
        int[,] sobelYMatrix =
        {
            {1, 2, 1},
            {0, 0, -2},
            {-1, -2, -1}
        };
        Vector2[][] gTex = new Vector2[texture2D.width][];
        for (int index = 0; index < texture2D.width; index++)
        {
            gTex[index] = new Vector2[texture2D.height];
        }
        for (int i = 0; i < texture2D.width; i++)
        {
            for (int j = 0; j < texture2D.height; j++)
            {
                float gX = 0f;
                float gY = 0f;
                for (int i_ = 0; i_ < sobelXMatrix.GetLength(0); i_++)
                {
                    for (int j_ = 0; j_ < sobelXMatrix.GetLength(1); j_++)
                    {
                        int x = i + i_ - sobelXMatrix.GetLength(0) / 2;
                        int y = j + j_ - sobelXMatrix.GetLength(1) / 2;
                        if (x < 0 || x >= texture2D.width || y < 0 || y >= texture2D.height)
                        {
                            break;
                        }
                        
                        gX += sobelXMatrix[i_, j_] * gaussianTex[x][y];
                        gY += sobelYMatrix[i_, j_] * gaussianTex[x][y];
                    }
                }

                gTex[i][j] = new Vector2(gX, gY);

                float g = Mathf.Sqrt(gX * gX + gY * gY);
                texture2D.SetPixel(i,j, new Color(g, g, g));
            }
        }
        File.WriteAllBytes("3_g.png", texture2D.EncodeToPNG());

        for (int i = 0; i < texture2D.width; i++)
        {
            for (int j = 0; j < texture2D.height; j++)
            {
                Vector2 g = gTex[i][j];
                float tetha = (Mathf.Rad2Deg * Mathf.Atan2(g.y, g.x) + 180f) % 180f;
                float gradient = Mathf.Sqrt(g.x * g.x + g.y * g.y);
                if (tetha >= 0f && tetha <= 22.5f || tetha > 157.5f && tetha <= 180f)
                {
                    //if gradientMagnitude is greater than E AND W
                    if (i > 0 && i < texture2D.width - 1)
                    {
                        Vector2 g1 = gTex[i - 1][j];
                        Vector2 g2 = gTex[i + 1][j];
                        if (gradient > Mathf.Sqrt(g1.x * g1.x + g1.y * g1.y) && gradient > Mathf.Sqrt(g2.x * g2.x + g2.y * g2.y))
                        {
                            texture2D.SetPixel(i,j, new Color(gradient, gradient, gradient));
                            continue;
                        }
                    }
                    texture2D.SetPixel(i,j, new Color(0, 0, 0));
                }
                else if (tetha > 22.5f && tetha <= 67.5f)
                {
                    //if gradientMagnitude is greater than NW AND SE
                    if (i > 0 && j < texture2D.height - 1 && j > 0 && i < texture2D.width - 1)
                    {
                        Vector2 g1 = gTex[i - 1][j - 1];
                        Vector2 g2 = gTex[i + 1][j + 1];
                        if (gradient > Mathf.Sqrt(g1.x * g1.x + g1.y * g1.y) && gradient > Mathf.Sqrt(g2.x * g2.x + g2.y * g2.y))
                        {
                            texture2D.SetPixel(i,j, new Color(gradient, gradient, gradient));
                            continue;
                        }
                    }
                    texture2D.SetPixel(i,j, new Color(0, 0, 0));
                }
                else if (tetha > 67.5 && tetha <= 112.5f)
                {
                    //if gradientMagnitude is greater than N AND S
                    if (j > 0 && j < texture2D.height - 1)
                    {
                        Vector2 g1 = gTex[i][j - 1];
                        Vector2 g2 = gTex[i][j + 1];
                        if (gradient > Mathf.Sqrt(g1.x * g1.x + g1.y * g1.y) && gradient > Mathf.Sqrt(g2.x * g2.x + g2.y * g2.y))
                        {
                            texture2D.SetPixel(i,j, new Color(gradient, gradient, gradient));
                            continue;
                        }
                    }
                    texture2D.SetPixel(i,j, new Color(0, 0, 0));
                }
                else if (tetha > 112.5 && tetha <= 157.5f)
                {
                    //if gradientMagnitude is greater than NE AND SW
                    if (i > 0 && j < texture2D.height - 1 && j > 0 && i < texture2D.width - 1)
                    {
                        Vector2 g1 = gTex[i - 1][j + 1];
                        Vector2 g2 = gTex[i + 1][j - 1];
                        if (gradient > Mathf.Sqrt(g1.x * g1.x + g1.y * g1.y) && gradient > Mathf.Sqrt(g2.x * g2.x + g2.y * g2.y))
                        {
                            texture2D.SetPixel(i,j, new Color(gradient, gradient, gradient));
                            continue;
                        }
                    }
                    texture2D.SetPixel(i,j, new Color(0, 0, 0));
                }
                else
                {
                    Debug.Log("uncaught angle on : " + i + " : " + j + " " + tetha);
                }
            }
        }
        
        File.WriteAllBytes("4_edge.png", texture2D.EncodeToPNG());

        Debug.Log("Finished !");
    }
}