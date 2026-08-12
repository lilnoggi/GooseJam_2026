using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.VisualScripting;

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

    // TODO: Add Health Bars, Paranoia Meters here later

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

            // TODO: Add animation trigger later to make text fade
        }

        Debug.Log($"[UIManager] UI updated for: {currentTurn}");
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
}
