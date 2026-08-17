using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    // Singleton Pattern
    public static UIManager Instance { get; private set; }

    [Header("System References")]
    [SerializeField] private TurnManager _turnManager;

    [Header("Turn Callouts")]
    [SerializeField] private TextMeshProUGUI _turnBannerText; // A text element in the center of the screen

    [Header("Standoff Result")]
    [SerializeField] private TextMeshProUGUI _standoffResultText; //TRUTH / CHEAT message
    [SerializeField] private float _resultHoldTime = 0.8f; //how long it stays fully visible
    [SerializeField] private float _resultFadeTime = 0.5f; //how long it takes to fade away
    [SerializeField] private float _resultIntroTime = 0.25f; //how long the result takes to appear
    [SerializeField] private float _cheatShakeAmount = 18f; //how violently CHEAT shakes
    [SerializeField] private float _bluffRiseDistance = 100f; //how far BLUFF SUCCESSFUL rises from

    [Header("Enemy UI")]
    [Tooltip("Index 0 = Left, 1 = Center, 2 = Right")]
    [SerializeField] private Slider[] _enemyHealthBars;
    [SerializeField] private Slider[] _enemyParanoiaBars;
    [SerializeField] private TextMeshProUGUI[] _enemyNames;
    [SerializeField] private GameObject[] _enemyShieldIcons;
    [SerializeField] private GameObject[] _enemyDodgeIcons;
    [SerializeField] private GameObject[] _enemyPoisonIcons;

    [Header("Player UI")]
    [SerializeField] private Slider _playerHealthBar; // The slider of the player health bar
    [SerializeField] private Sprite _playerHeartIcon; // The heart Sprite
    [SerializeField] private Sprite _playerShieldIcon; // The Shield Sprite
    [SerializeField] private GameObject _playerStatusIcon; // The actual status icon gameObject
    [SerializeField] private TextMeshProUGUI _playerShieldText; // Shows the amount of shield the player currently has
    [SerializeField] private GameObject _playerPoisonIcon; // Placeholder
    [SerializeField] private GameObject _playerDodgeIcon; // Placeholder
    // fade coroutine tracker
    private Coroutine _bannerFadeRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        //hide the standoff result until we need it
        if (_standoffResultText != null)
        {
            _standoffResultText.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // Safely subscribe to the event when this script turns on
        if (_turnManager != null)
        {
            _turnManager.OnTurnChanged += HandleTurnChanged;
        }
    }

    private void OnDisable()
    {
        // ALWAYS unsubscribe from events when this script turns off
        if (_turnManager != null)
        {
            _turnManager.OnTurnChanged -= HandleTurnChanged;
        }
    }

    /// <summary>
    /// Triggered whenever TurnManager.StartTurn() executes
    /// </summary>
    private void HandleTurnChanged(TurnSeat currentTurn)
    {
        // Check if the text element is assigned
        if (_turnBannerText != null)
        {
            _turnBannerText.text = $"{currentTurn}'s Turn!";

            // Stop the current fade if a new turn starts before the old text finishes
            if (_bannerFadeRoutine != null)
            {
                StopCoroutine(_bannerFadeRoutine);
            }

            // Start the new fade sequence
            _bannerFadeRoutine = StartCoroutine(FadeBannerRoutine());
        }
    }

    /// <summary>
    /// Call this from CharacterStats.cs whenever an enemy takes damage.
    /// </summary>
    public void UpdateEnemyHealth(int enemySeatIndex, int currentHealth, int maxHealth)
    {
        // Check to ensure the slider exists
        if (enemySeatIndex >= 0 && enemySeatIndex < _enemyHealthBars.Length)
        {
            _enemyHealthBars[enemySeatIndex].maxValue = maxHealth;
            _enemyHealthBars[enemySeatIndex].value = currentHealth;
        }
    }

    /// <summary>
    /// This is called when the scene loads to display the boss names
    /// </summary>
    public void UpdateEnemyName(int enemySeatIndex, string name)
    {
        // Check to ensure the text element exists in array
        if (enemySeatIndex >= 0 && enemySeatIndex < _enemyNames.Length)
        {
            if (_enemyNames[enemySeatIndex] != null)
            {
                _enemyNames[enemySeatIndex].text = name;
            }
        }
    }

    // Call this in CharacterStats.cs when an enemy gets more paranoid
    public void UpdateEnemyParanoia(int enemySeatIndex, int currentParanoia, int maxParanoia)
    {
        // Check to ensure the slider exists in the array
        if (enemySeatIndex >= 0 && enemySeatIndex < _enemyParanoiaBars.Length)
        {
            _enemyParanoiaBars[enemySeatIndex].maxValue = maxParanoia;
            _enemyParanoiaBars[enemySeatIndex].value = currentParanoia;
        }
    }

    /// <summary>
    /// Toggles the status icons for a specific enemy on the HUD
    /// </summary>
    public void UpdateEnemyStatusIcons(int enemySeatIndex, bool hasShield, bool hasDodge, bool hasPoison)
    {
        // Check to make sure the seat index is valid and the arrays are set
        if (enemySeatIndex >= 0 && enemySeatIndex < _enemyShieldIcons.Length)
        {
            if (_enemyShieldIcons[enemySeatIndex] != null)
            {
                _enemyShieldIcons[enemySeatIndex].SetActive(hasShield);
            }

            if (_enemyDodgeIcons[enemySeatIndex] != null)
            {
                _enemyDodgeIcons[enemySeatIndex].SetActive(hasDodge);
            }

            if (_enemyPoisonIcons[enemySeatIndex] != null)
            {
                _enemyPoisonIcons[enemySeatIndex].SetActive(hasPoison);
            }
        }
    }

    public void UpdatePlayerHealth(int currentHealth, int maxHealth)
    {
        if (_playerHealthBar != null)
        {
            _playerHealthBar.maxValue = maxHealth;
            _playerHealthBar.value = currentHealth;
        }
    }

    public void UpdatePlayerStatusIcon(bool hasShield, int shieldAmount, bool hasDodge, bool hasPoison)
    {
        // Make sure status icon is assigned
        if (_playerStatusIcon != null)
        {
            // Get the Image component attatched to it
            Image statusImage = _playerStatusIcon.GetComponent<Image>();

            if (statusImage != null)
            {
                // Swap the sprite depending on if the player has shield
                // If hasShield is true, use ShieldIcon, otherwise use HeartIcon
                statusImage.sprite = hasShield ? _playerShieldIcon : _playerHeartIcon;
            }
        }

        // Toggle shield text ON if the player has shield, OFF if they don't
        if (_playerShieldText != null)
        {
            _playerShieldText.gameObject.SetActive(hasShield);

            // Set the text to display the current shield amount
            _playerShieldText.text = shieldAmount.ToString();
        }

        if (_playerDodgeIcon != null)
        {
            _playerDodgeIcon.SetActive(hasDodge);
        }

        if (_playerPoisonIcon != null)
        {
            _playerPoisonIcon.SetActive(hasPoison);
        }
    }


    public IEnumerator ShowStandoffResult(string message)
    {
        //safety check in case the text wasn't assigned
        if (_standoffResultText == null)
        {
            yield break;
        }

        _standoffResultText.text = message;
        _standoffResultText.gameObject.SetActive(true);

        //make sure the text starts fully visible
        Color textColour = _standoffResultText.color;
        textColour.a = 1f;
        _standoffResultText.color = textColour;

        //pick a different intro animation depending on what happened
        switch (message)
        {
            case "CHEAT!":
            yield return StartCoroutine(AnimateCheatResult());
            break;

            case "WRONG CALL!":
            yield return StartCoroutine(AnimateWrongCallResult());
            break;

            case "BLUFF SUCCESSFUL!":
            yield return StartCoroutine(AnimateBluffResult());
            break;

            case "TRUTH!":
            yield return StartCoroutine(AnimateTruthResult());
            break;
        }

        //leave the result on screen for a moment
        yield return new WaitForSeconds(_resultHoldTime);

        //fade it away afterwards
        yield return StartCoroutine(FadeStandoffResult());

        _standoffResultText.gameObject.SetActive(false);
    }

    private IEnumerator AnimateCheatResult()
    {
        RectTransform resultTransform = _standoffResultText.rectTransform;

        Vector2 normalPosition = resultTransform.anchoredPosition;

        //start big so it feels like the word slams into the screen
        resultTransform.localScale = Vector3.one * 2.5f;

        float elapsedTime = 0f;

        while (elapsedTime < _resultIntroTime)
        {
            elapsedTime += Time.deltaTime;

            float percent = Mathf.Clamp01(elapsedTime / _resultIntroTime);

            //quickly shrink down to its normal size
            resultTransform.localScale = Vector3.Lerp(Vector3.one * 2.5f, Vector3.one, percent);

            //shake it around while it lands
            float randomX = Random.Range(-_cheatShakeAmount, _cheatShakeAmount);
            float randomY = Random.Range(-_cheatShakeAmount, _cheatShakeAmount);

            //shake gets weaker towards the end
            float shakeStrength = 1f - percent;

            resultTransform.anchoredPosition = normalPosition + new Vector2(randomX, randomY) * shakeStrength;

            yield return null;
        }

        //put everything back exactly where it should be
        resultTransform.localScale = Vector3.one;
        resultTransform.anchoredPosition = normalPosition;
    }

    private IEnumerator AnimateWrongCallResult()
    {
        RectTransform resultTransform = _standoffResultText.rectTransform;

        Vector2 normalPosition = resultTransform.anchoredPosition;

        //start slightly bigger
        resultTransform.localScale = Vector3.one * 1.5f;

        float elapsedTime = 0f;

        while (elapsedTime < _resultIntroTime)
        {
            elapsedTime += Time.deltaTime;

            float percent = Mathf.Clamp01(elapsedTime / _resultIntroTime);

            //wobble left and right
            float wobble = Mathf.Sin(percent * Mathf.PI * 4f) * 20f;

            resultTransform.anchoredPosition = normalPosition + new Vector2(wobble, 0f);

            resultTransform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, percent);

            yield return null;
        }

        resultTransform.localScale = Vector3.one;
        resultTransform.anchoredPosition = normalPosition;
    }

    private IEnumerator AnimateBluffResult()
    {
        RectTransform resultTransform = _standoffResultText.rectTransform;

        Vector2 normalPosition = resultTransform.anchoredPosition;

        //start underneath the normal position
        Vector2 startPosition = normalPosition + new Vector2(0f, -_bluffRiseDistance);

        resultTransform.anchoredPosition = startPosition;
        resultTransform.localScale = Vector3.one * 0.7f;

        float elapsedTime = 0f;

        while (elapsedTime < _resultIntroTime)
        {
            elapsedTime += Time.deltaTime;

            float percent = Mathf.Clamp01(elapsedTime / _resultIntroTime);
            float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

            //rise into place
            resultTransform.anchoredPosition = Vector2.Lerp(startPosition, normalPosition, smoothPercent);

            //grow slightly while rising
            resultTransform.localScale = Vector3.Lerp( Vector3.one * 0.7f, Vector3.one, smoothPercent);

            yield return null;
        }

        resultTransform.anchoredPosition = normalPosition;
        resultTransform.localScale = Vector3.one;
    }

    private IEnumerator AnimateTruthResult()
    {
        RectTransform resultTransform = _standoffResultText.rectTransform;

        //start slightly smaller
        resultTransform.localScale = Vector3.one * 0.8f;

        float elapsedTime = 0f;

        while (elapsedTime < _resultIntroTime)
        {
            elapsedTime += Time.deltaTime;

            float percent = Mathf.Clamp01(elapsedTime / _resultIntroTime);
            float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

            resultTransform.localScale =
                Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, smoothPercent);

            yield return null;
        }

        resultTransform.localScale = Vector3.one;
    }

    private IEnumerator FadeStandoffResult()
    {
        float elapsedTime = 0f;

        Color textColour = _standoffResultText.color;

        //slowly make the text transparent
        while (elapsedTime < _resultFadeTime)
        {
            elapsedTime += Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / _resultFadeTime);

            textColour.a = alpha;
            _standoffResultText.color = textColour;

            yield return null;
        }

        //make sure it finishes completely invisible
        textColour.a = 0f;
        _standoffResultText.color = textColour;
    }

    private IEnumerator FadeBannerRoutine()
    {
        // Reset the text colour so it is 100% visible
        Color originalColour = _turnBannerText.color;
        originalColour.a = 1f;
        _turnBannerText.color = originalColour;

        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Smoothly fade the text out over 1 second
        float fadeDuration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            // Calculate the exact alpha value between 1 an 0
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

            Color newColour = _turnBannerText.color;
            newColour.a = alpha;
            _turnBannerText.color = newColour;

            yield return null; // Wait for next frame before looping again
        }

        // Ensure it is completely invisible at the very end
        Color finalColour = _turnBannerText.color;
        finalColour.a = 0f;
        _turnBannerText.color = finalColour;
    }
}
