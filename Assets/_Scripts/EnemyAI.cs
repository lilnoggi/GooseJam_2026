using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private EnemyProfile _activeProfile;

    private CharacterStats _stats;

    // Lucy memory: Tracks how suspicious she is of each suit
    private Dictionary<CardSuit, float> _swanSuspicion = new Dictionary<CardSuit, float>()
    {
        { CardSuit.Blood, 0f }, { CardSuit.Bone, 0f }, { CardSuit.Rot, 0f }, { CardSuit.Feather, 0f }
    };

    // Getter for TurnManager
    public EnemyProfile Profile => _activeProfile;

    private void Awake()
    {
        // Get the stats component sitting on this current enemy GameObject
        _stats = GetComponent<CharacterStats>();
    }

    // This method will be called by TurnManager.cs during Phase 2
    public bool DecideToChallenge(CardSuit claimedSuit, int claimedValue)
    {
        // Check Paranoia Overrides first
        if (_stats != null)
        {
            if (_stats.CurrentParanoia >= _activeProfile.MaxParanoia)
            {
                Debug.Log($"{name} is HYPER-VIGILANT! Auto-Challenging!");
                return true;
            }

            if (_stats.CurrentParanoia <= 0)
            {
                Debug.Log($"{name} is OVERCONFIDENT! Auto-Accepting");
                return false;
            }
        }

        // If no overrides, execute standard logic
        // If a standard minion, execute standard logic
        if (!_activeProfile.IsBoss)
        {
            return RunStandardProbability(claimedSuit, claimedValue);
        }

        // If a boss, execute their unique logic
        switch (_activeProfile.TypeOfBoss)
        {
            case BossType.FreddyFox:
                return FreddyFoxLogic(claimedSuit, claimedValue);

            case BossType.BanditWolf:
                return BanditWolfLogic(claimedSuit, claimedValue);

            case BossType.LucySwan:
                return LucySwanLogic(claimedSuit, claimedValue);

            default:
                return RunStandardProbability(claimedSuit, claimedValue);
        }
    }   

    private bool RunStandardProbability(CardSuit suit, int value)
    {
        // Establish a base 20% chance to challenge any claim
        float baseChance = 0.20f;

        // The higher the claimed card value, the more suspicious the enemy gets
        float valueFactor = value * 0.02f;

        // Add together & multiply by the specific enemy's skepticism modifier
        float finalCheatChance = (baseChance + valueFactor) * _activeProfile.SkepticismMultiplier;

        // Generate a random value between 0.0 and 1.0
        // If the nuymber is lower than the final chance, call cheat
        return Random.value < finalCheatChance;
    }

    /// <summary>
    /// Looks at the true cards and decides whether to tell the truth or bluff
    /// </summary>
    public ClaimData FormulateClaim(List<CardData> trueCards)
    {
        // Default to the truth
        CardSuit claimedSuit = trueCards[0].Suit;
        CardRank claimedRank = trueCards[0].Rank;

        // Base 25% chance to lie (adjust in profiles)
        float bluffChance = 0.25f;

        // If the enemy has 0 paranoia, they become overconfident and are much more likely to bluff
        if (_stats != null && _stats.CurrentParanoia <= 0)
        {
            bluffChance = 0.50f;
        }

        // Random value to see if they lie
        if (Random.value < bluffChance)
        {
            Debug.Log($"{name} decideed to BLUFF");

            if (!_activeProfile.IsBoss)
            {
            // STANDARD MINION LIE
            // THE LIE: Claim a random scary suit (Blood or Rot)
            claimedSuit = (Random.value > 0.5f) ? CardSuit.Blood : CardSuit.Rot;

            // THE LIE: Claim a random high value (Jack - Ace)
            CardRank[] highRanks = { CardRank.Jack, CardRank.Queen, CardRank.King, CardRank.Ace };
            claimedRank = highRanks[Random.Range(0, highRanks.Length)];
            }
            else
            {
                // Custom Boss Lies
                switch (_activeProfile.TypeOfBoss)
                {
                    case BossType.FreddyFox:
                        // Freddy lies about playing Traps/Rot to make the player paranoid
                        claimedSuit = CardSuit.Rot;
                        claimedRank = CardRank.Ace;
                        break;
                    
                    case BossType.BanditWolf:
                        // Bandit always lies about massive brute force
                        claimedSuit = CardSuit.Blood;
                        claimedRank = CardRank.Ace;
                        break;

                    case BossType.LucySwan:
                        // Lucy tells unpredictable, weird lies to mess with your head
                        claimedSuit = (CardSuit)Random.Range(0, 4);
                        claimedRank = CardRank.Ace;
                        break;
                }
            }
        }

        // Always target the player
        return new ClaimData(trueCards, claimedSuit, claimedRank, TurnSeat.Player);
    }

    private bool FreddyFoxLogic(CardSuit suit, int value)
    {
        // Freddy is cunning and untrusting.

        // Heavily weights towards calling bluff on high-damage Blood cards
        if (suit == CardSuit.Blood && value > 10)
        {
            return true; // Almost always challenges this
        }
        
        return RunStandardProbability(suit, value);
    }

    private bool BanditWolfLogic(CardSuit suit, int value)
    {
        // Bandit uses brute force, ignores bone because he just want to attack

        // Almost always accepts Bone claims to keep attacking
        if (suit == CardSuit.Bone)
        {
            return false; // Accept without checking
        }

        return RunStandardProbability(suit, value);
    }

    private bool LucySwanLogic(CardSuit suit, int value)
    {
        // Dynamic AI. Remembers past bluffs and adapts her probability per suit
        float standardChance = 0.20f + (value * 0.02f);

        // Add her specific suspicion memory for this suit
        float finalChance = (standardChance + _swanSuspicion[suit]) * _activeProfile.SkepticismMultiplier;

        // Every time a player claims a suit, Lucy gets 15% more suspicious of that specific suit for the rest of the game
        _swanSuspicion[suit] += 0.15f;

        return Random.value < finalChance;
    }
}