using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

/// <summary>
/// Defines the specific bosses for the AI to override standard enemies
/// </summary>
public enum BossType
{
    None,
    FreddyFox,
    BanditWolf,
    LucySwan
}

[CreateAssetMenu(fileName = "NewEnemyProfile", menuName = "Cheating Geese/Enemy Profile")]
public class EnemyProfile : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string _enemyName;
    [SerializeField] private bool _isBoss;
    [SerializeField] private BossType _bossType = BossType.None;

    [Header("Combat Stats")]
    [Tooltip("Minions 75 HP | Bosses: 150-200 HP")]
    [SerializeField] private int _maxHealth = 75;

    [Tooltip("The starting integer for the meter (0-100)")]
    [SerializeField] private int _baseParanoiaLevel = 0;
    [SerializeField] private int _maxParanoia = 100;

    [Header("Deck & AI")]
    [Tooltip("Leave empty if this enemy uses the standard deck")]
    [SerializeField] private CardDatabase _customBossDeck;

    [Tooltip("Values between 0.0 & 1.0 to weigh how likely they are to call Cheat")]
    [SerializeField] private float _skepticismMultiplier = 1.0f;
    [SerializeField] private float _aggressionMultiplier = 1.0f;

    // Public Getters 
    public string EnemyName => _enemyName;
    public bool IsBoss => _isBoss;
    public BossType TypeOfBoss => _bossType;
    public int MaxHealth => _maxHealth;
    public int BaseParanoiaLevel => _baseParanoiaLevel;
    public int MaxParanoia => _maxParanoia;
    public CardDatabase CustomBossDeck => _customBossDeck;
    public float SkepticismMultiplier => _skepticismMultiplier;
    public float AggressionMultiplier => _aggressionMultiplier;
}
