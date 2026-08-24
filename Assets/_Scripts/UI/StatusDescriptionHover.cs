using UnityEngine;
using TMPro;

public class StatusDescriptionHover : MonoBehaviour
{
    // Singleton instance
    public static StatusDescriptionHover Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _statusNameText;
    [SerializeField] private TextMeshProUGUI _statusDescriptionText;

    [Header("Positioning")]
    [SerializeField] private float _yOffset = 180f;

    private RectTransform _rectTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _rectTransform = GetComponent<RectTransform>();

        // Ensure panel hidden when game start
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Called by a card when the mouse hovers over it
    /// </summary>
    public void ShowTooltip(string name, string description, Transform cardTransform)
    {
        _statusNameText.text = name;
        _statusDescriptionText.text = description;

        // Pin the tooltip directly above the card using the offset
        _rectTransform.position = cardTransform.position + new Vector3(0, _yOffset, 0);

        // Add UI SFX
        AudioManager.Instance.PlaySFX(SFXType.Hover);

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Called by a card when the mouse leaves
    /// </summary>
    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}
