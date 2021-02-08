using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private RawImage feed = default;

    public RawImage Feed => feed;
}
