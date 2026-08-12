using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    // Singleton Pattern
    public static UIManager Instance { get; private set; }

    [Header("System References")]
    [SerializeField] private TurnManager _turnManager;

    [Header("Turn Callouts")]
    [SerializeField] private TextMeshProUGUI _turnBannerText; // A text element in the center of the screen

    [Header("Enemy UI")]
    [SerializeField] private Slider[] _enemyHealthBars;

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
}
