using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private RawImage feed = default;
    [SerializeField] private Image zoomPercentage = default;
    [SerializeField] private RectTransform altPopup = default;
    [SerializeField] private TextMeshProUGUI eqPopup = default;
    
    public RawImage Feed => feed;
    public Image ZoomPercentage => zoomPercentage;
    public RectTransform AltPopup => altPopup;
    public TextMeshProUGUI EqPopup => eqPopup;
}
