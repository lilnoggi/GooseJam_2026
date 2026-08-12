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
    private int _dodgeChance; // Tracks the percentage chance (0-100) to dodge the next attack

    public int PoisonStacks => _poisonStacks; // public getter
    public int DodgeChance => _dodgeChance;
    public int CurrentParanoia => _currentParanoia;

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
        // TEMPORARY TEST
        if (Keyboard.current == null)
        {
            return;
        }

        // Standard test (15 damage, 20 paranoia)
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TakeDamage(15);
            IncreaseParanoia(20);
        }

        // Key '1': Test Bone Shield (adds 25 shield points)
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            AddShield(25);
        }

        // Key '2' Test Rot
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            ApplyPoison(10);
        }

        // Key '3' Test feather
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            AddDodgeChance(100);
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
        // Check for a dodge before anything else
        if (_dodgeChance > 0)
        {
            // Random number between 0 and 99
            int roll = Random.Range(0, 100);

            if (roll < _dodgeChance)
            {
                Debug.Log($"{name} DODGED the attack. (Rolled {roll} vs {_dodgeChance}% chance)");
                _dodgeChance = 0;
                return;
            }
            else
            {
                Debug.Log($"{name} failed to dodge. (Rolled {roll} vs {_dodgeChance}% chance)");
                _dodgeChance = 0;
            }
        }
        // If character has shield, let is absorb the damage 
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
    /// Called when a Feather card successfully resolves
    /// </summary>
    public void AddDodgeChance(int chance)
    {
        _dodgeChance += chance;

        // Cap the dodge chance at 100%
        if (_dodgeChance > 100)
        {
            _dodgeChance = 100;
        }

        Debug.Log($"{name} gained {_dodgeChance}% dodge chance for the next attack");
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
