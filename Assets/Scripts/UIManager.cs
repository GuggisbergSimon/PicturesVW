using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private RectTransform feedPanel = default;
    [SerializeField] private RawImage feedPrefab = default;
    [SerializeField] private Image zoomPercentage = default;
    [SerializeField] private RectTransform altPopup = default;
    [SerializeField] private TextMeshProUGUI eqPopup1 = default;
    [SerializeField] private TextMeshProUGUI eqPopup2 = default;
    
    public Image ZoomPercentage => zoomPercentage;
    public RectTransform AltPopup => altPopup;
    public TextMeshProUGUI EqPopup1 => eqPopup1;
    public TextMeshProUGUI EqPopup2 => eqPopup2;

    public void CreatePicture(Texture2D tex)
    {
        RawImage img = Instantiate(feedPrefab, feedPanel) as RawImage;
        img.texture = tex;
        img.transform.SetAsFirstSibling();
    }
}
