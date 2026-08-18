using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private bool _isPlayer; // Check if this script is on the player
    [Tooltip("0 = Left, 1 = Center, 2 = Right")]

    [Header("UI Components")]
    [SerializeField] private HealthBarUI _healthBarUI;

    [Header("Runtime Stats")]
    private int _currentHealth;
    private int _maxHealth;
    private int _currentParanoia;
    private int _maxParanoia;
    private int _currentShield;

    [Header("Runtime Effects")]
    private int _poisonStacks; // Tracks active rot stacks
    private int _dodgeTokens; // Guaranteed dodge tokens

    public int PoisonStacks => _poisonStacks; // public getter
    public int DodgeTokens => _dodgeTokens;
    public int CurrentParanoia => _currentParanoia;
    
    public bool IsEliminated { get; private set; } // Public flag to check if this character is out of the game

    // Component references
    private EnemyAI _enemyAI;

    private void Awake()
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

            // Push starting data directly to THIS enemy's health bar ui
            if (_healthBarUI != null)
            {
                _healthBarUI.UpdateHealth(_currentHealth, _maxHealth);
                _healthBarUI.UpdateParanoia(_currentParanoia, _maxParanoia);
            }

            UpdateStatusUI(); 
        }
    }

    private void InitialisePlayerStats()
    {
        _maxHealth = 100;
        _currentHealth = _maxHealth;

        // Push starting health to local UI
        if (_healthBarUI != null)
        {
            _healthBarUI.UpdateHealth(_currentHealth, _maxHealth);
        }
        
        // Ensure all icons start turned off
        UpdateStatusUI(); 
    }

    /// <summary>
    /// Called when a character successfully resolves a Bone card
    /// </summary>
    public void AddShield(int shieldAmount)
    {
        // Don't shield dead characters
        if (IsEliminated)
        {
            return;
        }

        _currentShield += shieldAmount;
        Debug.Log($"Added {shieldAmount} Bone shield! Current shield: {_currentShield}");

        // Connect to UI Manager
        UpdateStatusUI(); 
    }

    public void TakeDamage(int damageAmount)
    {
        // Don't damage characters that are dead
        if (IsEliminated)
        {
            return;
        }

        // Check for a dodge tokens
        if (_dodgeTokens > 0)
        {
            _dodgeTokens--;
            UpdateStatusUI(); 
            return;
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

            // Update UI because shield was broken
                UpdateStatusUI(); 
        }

        // Any leftover damage hits the health pool
        _currentHealth -= damageAmount;

        // Prevent health from going below 0 and trigger elimination
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Die();
        }
        
        // Tell UI exactly WHO took damage
        if (_healthBarUI != null)
        {
            _healthBarUI.UpdateHealth(_currentHealth, _maxHealth);
        }
    }

    /// <summary>
    /// Called when a Rot card successfully resolves against a target
    /// </summary>
    public void ApplyPoison(int stacks)
    {
        // Don't apply to dead characters
        if (IsEliminated)
        {
            return;
        }
        _poisonStacks += stacks;
        Debug.Log($"{name} was afflicted with {stacks} stacks of poison. Total stacks: {_poisonStacks}");

        // Connect to UI Manager
        UpdateStatusUI(); 
    }

    /// <summary>
    /// Called at the very start of this character's turn to process poison damage
    /// </summary>
    public void ProcessTurnStartStatusEffects()
    {
        // Don't process effects for dead characters
        if (IsEliminated)
        {
            return;
        }

        // Shields AND dodge expire at the start of the turn
        _currentShield = 0;
        _dodgeTokens = 0;

        if (_poisonStacks > 0)
        {
            Debug.Log($"[Status Effects] {_poisonStacks} Poison Stacks ticking on {name}");

            // Poison deals direct damage equal to the stack count at the start of the turn
            TakeDamage(_poisonStacks);

            // Decay poison stacks by 1 each turn
            _poisonStacks--;
        }

        // Update UI because Shields, Dodges, and Poison just changed
        UpdateStatusUI(); 
    }

    /// <summary>
    /// Called when a Feather card successfully resolves
    /// </summary>
    public void AddDodgeTokens(int tokenAmount)
    {
        // Not for dead character
        if (IsEliminated)
        {
            return;
        }

        _dodgeTokens += tokenAmount;

        Debug.Log($"{name} gained {tokenAmount} dodge tokens! Total: {_dodgeTokens}");

        // Connect to UI Manager
        UpdateStatusUI(); 
    }

    /// <summary>
    /// This could be called from EnemyAI OR CombatLogic when an enemy gets paranoid
    /// </summary>
    public void IncreaseParanoia(int amount)
    {
        // NO DEAD CHARACTERS!!!
        if (IsEliminated)
        {
            return;
        }

        _currentParanoia += amount;

        // Prevent paranoia from exceeding max limit
        if (_currentParanoia > _maxParanoia)
        {
            _currentParanoia = _maxParanoia;
        }

        // Prevent from dropping below 0
        else if (_currentParanoia < 0)
        {
            _currentParanoia = 0;
        }

        if (!_isPlayer && _healthBarUI != null)
        {
            _healthBarUI.UpdateParanoia(_currentParanoia, _maxParanoia);
        }
    }

    /// <summary>
    /// Restores health up to the maximum limit. Used by Rotten Apple
    /// </summary>
    public void Heal(int healAmount)
    {
        if (IsEliminated)
        {
            return; // Don't heal dead characters
        }

        _currentHealth += healAmount;

        // Prevent overhealing past the max
        if (_currentHealth > _maxHealth)
        {
            _currentHealth = _maxHealth;
        }

        // Update the UI
        if (_healthBarUI != null)
        {
            _healthBarUI.UpdateHealth(_currentHealth, _maxHealth);
        }
    }

    /// <summary>
    /// Permanently reduces the maximum health pool. Used by Gambler's Fallacy
    /// </summary>
    public void ReduceMaxHealth(int amount)
    {
        if (IsEliminated)
        {
            return;
        }

        _maxHealth -= amount;

        // Safety check to ensure max health never drops to 0 or below
        if (_maxHealth < 1)
        {
            _maxHealth = 1;
        }

        // If current health is now higher than the new max, bring it down
        if (_currentHealth > _maxHealth)
        {
            _currentHealth = _maxHealth;
        }

        // Update the UI to reflect smaller health pool
        if (_healthBarUI != null)
        {
            _healthBarUI.UpdateHealth(_currentHealth, _maxHealth);
        }
    }

    /// <summary>
    /// Handles logic for when a character's HP reaches 0
    /// </summary>
    private void Die()
    {
        IsEliminated = true;
        Debug.Log($"{name} HAS BEEN ELIMINATED FROM THE GAME!");

        if (!_isPlayer)
        {
            // Dialogue for final angry line before folding
            GetComponent<EnemyDialogue>()?.TriggerDefeated();

            // TODO: Play animation tossing cards onto table
        }
        else
        {
            // TODO: Trigger game over ui
            Debug.Log("PLAYER HAS DIED! GAME OVER!");
        }
    }

    // Helper method for UIManager to evaluate bools for ANY character
    private void UpdateStatusUI()
    {
        bool hasShield = _currentShield > 0;
        bool hasDodge = _dodgeTokens > 0;
        bool hasPoison = _poisonStacks > 0;

        if (_healthBarUI != null)
        {
            // Call local UI instead of UIManager
            _healthBarUI.UpdateStatusIcon(hasShield, _currentShield, _dodgeTokens, hasPoison);
        }
    }
}
