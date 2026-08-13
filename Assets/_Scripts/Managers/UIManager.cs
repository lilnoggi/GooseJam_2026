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

    [Header("Enemy UI")]
    [Tooltip("Index 0 = Left, 1 = Center, 2 = Right")]
    [SerializeField] private Slider[] _enemyHealthBars;
    [SerializeField] private Slider[] _enemyParanoiaBars;
    [SerializeField] private TextMeshProUGUI[] _enemyNames;

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
