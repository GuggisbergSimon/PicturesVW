using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private RawImage feed = default;
    [SerializeField] private Image zoomPercentage = default;

    public RawImage Feed => feed;

    public Image ZoomPercentage => zoomPercentage;
}
