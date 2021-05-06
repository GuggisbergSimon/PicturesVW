using UnityEngine;

public class Magic : MonoBehaviour
{
    public enum ColorMode
    {
        Cycle,
        Loop,
        Random
    }

    [SerializeField] private MeshRenderer[] objectsToColorized = default;
    [SerializeField] private Material[] materialsToCycleThrough = default;
    [SerializeField] private ColorMode colorMode = ColorMode.Cycle;
    [SerializeField] private float teleportWeight = 10f;
    [SerializeField] private Vector3[] teleports = default;
    [SerializeField] private float colorizeWeight = 70f;
    private int _currentMat;

    private void Start()
    {
        ChangeMat();
    }

    public void DoMagic(Transform t)
    {
        float r = Random.Range(0f, teleportWeight + colorizeWeight);
        if (r < teleportWeight)
        {
            Teleport(t);
        }
        else if (r >= teleportWeight)
        {
            ChangeColor();
        }
    }

    private void Teleport(Transform t)
    {
        t.transform.position += teleports[Random.Range(0, teleports.Length)];
    }

    private void ChangeColor()
    {
        switch (colorMode)
        {
            case ColorMode.Cycle:
                _currentMat = (_currentMat + 1) % materialsToCycleThrough.Length;
                break;
            case ColorMode.Loop:
                ++_currentMat;
                int range = materialsToCycleThrough.Length - 1;
                _currentMat = Mathf.Abs((_currentMat + range) % (range * 2) - range);
                break;
            case ColorMode.Random:
                _currentMat = Random.Range(0, materialsToCycleThrough.Length);
                break;
        }

        ChangeMat();
    }

    private void ChangeMat()
    {
        foreach (var obj in objectsToColorized)
        {
            obj.material = materialsToCycleThrough[_currentMat];
        }
    }
}