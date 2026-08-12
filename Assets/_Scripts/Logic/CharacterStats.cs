using UnityEngine;
using UnityEngine.InputSystem; // FOR TESTING

public class CharacterStats : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private bool _isPlayer; // Check if this script is on the player
    [Tooltip("0 = Left, 1 = Center, 2 = Right")]
    [SerializeField] private int _enemySeatIndex;

    [Header("Runtime Stats")]
    private int _currentHealth;
    private int _maxHealth;
    private int _currentParanoia;
    private int _maxParanoia;
    private int _currentShield;

    [Header("Runtime Effects")]
    private int _poisonStacks; // Tracks active rot stacks

    public int PoisonStacks => _poisonStacks; // public getter

    // Component references
    private EnemyAI _enemyAI;

    private void Start()
    {
        if (!_isPlayer)
        {
            InitialiseEnemyStats();
        }
        else
        {
            InitialisePlayerStats();
        }
    }

    private void Update()
    {
        // TEMPORARY TEST: Press spacebar to deal 15 damage and add 20 paranoia
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TakeDamage(15);
            IncreaseParanoia(20);
        }
    }

    private void InitialiseEnemyStats()
    {
        // Grab EnemyAI component sitting on this GameObject
        _enemyAI = GetComponent<EnemyAI>();

        if (_enemyAI != null && _enemyAI.Profile != null)
        {
            // Read the starting stats directly from their specific ScriptableObject profile
            _maxHealth = _enemyAI.Profile.MaxHealth;
            _currentParanoia = _enemyAI.Profile.BaseParanoiaLevel;
            _maxParanoia = _enemyAI.Profile.MaxParanoia;

            _currentHealth = _maxHealth;

            // Push all of  the starting data to the UIManager
            UIManager.Instance.UpdateEnemyName(_enemySeatIndex, _enemyAI.Profile.EnemyName);
            UIManager.Instance.UpdateEnemyHealth(_enemySeatIndex, _currentHealth, _maxHealth);
            UIManager.Instance.UpdateEnemyParanoia(_enemySeatIndex, _currentParanoia, _maxParanoia);
        }
    }

    private void InitialisePlayerStats()
    {
        // TODO: Finish this when Player UI is finished
        _maxHealth = 100;
        _currentHealth = _maxHealth;
    }

    /// <summary>
    /// Called when a character successfully resolves a Bone card
    /// </summary>
    public void AddShield(int shieldAmount)
    {
        _currentShield += shieldAmount;
        Debug.Log($"Added {shieldAmount} Bone shield! Current shield: {_currentShield}");

        // TODO: Connect to UIManager later
    }

    public void TakeDamage(int damageAmount)
    {
        // If character has shield, let is absorb the damage first
        if (_currentShield > 0)
        {
            if (_currentShield >= damageAmount)
            {
                // Shield fully absorbs the hit
                _currentShield -= damageAmount;
                damageAmount = 0;
            }
            else
            {
                // Shield breaks, remaining damage carries over
                damageAmount -= _currentShield;
                _currentShield = 0;
            }
        }

        // Any leftover damage hits the health pool
        _currentHealth -= damageAmount;

        // Prevent health from going below 0
        if (_currentHealth < 0)
        {
            _currentHealth = 0;
        }

        if (!_isPlayer)
        {
            UIManager.Instance.UpdateEnemyHealth(_enemySeatIndex, _currentHealth, _maxHealth);
        }
        else
        {
            // Update player health ui
        }
    }

    /// <summary>
    /// Called when a Rot card successfully resolves against a target
    /// </summary>
    public void ApplyPoison(int stacks)
    {
        _poisonStacks += stacks;
        Debug.Log($"{name} was afflicted with {stacks} stacks of poison. Total stacks: {_poisonStacks}");
    }

    /// <summary>
    /// Called at the very start of this character's turn to process poison damage
    /// </summary>
    public void ProcessTurnStartStatusEffects()
    {
        if (_poisonStacks > 0)
        {
            Debug.Log($"[Status Effects] {_poisonStacks} Poison Stacks ticking on {name}");

            // Poison deals direct damage equal to the stack count at the start of the turn
            TakeDamage(_poisonStacks);

            // Decay poison stacks by 1 each turn
            _poisonStacks--;
        }
    }

    /// <summary>
    /// This could be called from EnemyAI OR CombatLogic when an enemy gets paranoid
    /// </summary>
    public void IncreaseParanoia(int amount)
    {
        _currentParanoia += amount;

        // Prevent paranoia from exceeding max limit
        if (_currentParanoia > _maxParanoia)
        {
            _currentParanoia = _maxParanoia;
        }

        if (!_isPlayer)
        {
            UIManager.Instance.UpdateEnemyParanoia(_enemySeatIndex, _currentParanoia, _maxParanoia);
        }
    }
}
