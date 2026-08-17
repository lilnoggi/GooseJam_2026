using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _paranoiaSlider;

    [Header("Status Icons")]
    [SerializeField] private Image _statusImage; // Status of the target (shield / health)
    [SerializeField] private Sprite _heartSprite; 
    [SerializeField] private Sprite _shieldSprite;
    [SerializeField] private TextMeshProUGUI _shieldAmountText;

    [Header("Placeholder Statuses")]
    [SerializeField] private GameObject _poisonIcon;
    [SerializeField] private GameObject _dodgeIcon;

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (_healthSlider != null)
        {
            _healthSlider.maxValue = maxHealth;
            _healthSlider.value = currentHealth;
        }
    }

    public void UpdateParanoia(int currentParanoia, int maxParanoia)
    {
        if (_paranoiaSlider != null)
        {
            _paranoiaSlider.maxValue = maxParanoia;
            _paranoiaSlider.value = currentParanoia;
        }
    }

    public void UpdateStatusIcon(bool hasShield, int shieldAmount, bool hasDodge, bool hasPoison)
    {
        // Swap Heart for Shield
        if (_statusImage != null)
        {
            _statusImage.sprite = hasShield ? _shieldSprite : _heartSprite;
        }

        // Toggle Text
        if (_shieldAmountText != null)
        {
            _shieldAmountText.gameObject.SetActive(hasShield);
            _shieldAmountText.text = shieldAmount.ToString();
        }

        // Toggle other statuses
        if (_dodgeIcon != null)
        {
            _dodgeIcon.SetActive(hasDodge);
        }

        if (_poisonIcon != null)
        {
            _poisonIcon.SetActive(hasPoison);
        }
    }
}
