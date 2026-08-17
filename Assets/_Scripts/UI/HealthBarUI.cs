using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _paranoiaSlider; // Player health bar leaves this blank

    [Header("Status Icons")]
    [SerializeField] private Image _statusImage; // Status of the target (shield / health)
    [SerializeField] private Sprite _heartSprite; 
    [SerializeField] private Sprite _shieldSprite;
    [SerializeField] private TextMeshProUGUI _shieldAmountText;
        [SerializeField] private GameObject _poisonIcon;

    [Header("Dodge Feathers")]
    [SerializeField] private GameObject _dodgeIcon;
    [SerializeField] private Sprite _dodgeOneFeather;
    [SerializeField] private Sprite _dodgeTwoFeathers;
    [SerializeField] private Sprite _dodgeThreeFeathers;

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

    public void UpdateStatusIcon(bool hasShield, int shieldAmount, int dodgeAmount, bool hasPoison)
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

        // Feather Logic
        if (_dodgeIcon != null)
        {
            _dodgeIcon.SetActive(dodgeAmount > 0);

            if (dodgeAmount > 0)
            {
                Image dodgeImage = _dodgeIcon.GetComponent<Image>();

                if (dodgeImage != null)
                {
                    if (dodgeAmount == 1)
                    {
                        dodgeImage.sprite = _dodgeOneFeather;
                    }
                    else if (dodgeAmount == 2)
                    {
                        dodgeImage.sprite = _dodgeTwoFeathers;
                    }
                    else
                    {
                        dodgeImage.sprite = _dodgeThreeFeathers; // Caps at 3 (can only play 3 cards)
                    }
                }
            }
        }

        // Toggle other statuses
        if (_poisonIcon != null)
        {
            _poisonIcon.SetActive(hasPoison);
        }
    }
}
