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
    // NOTE: claimedValue now represents the true total mathematical threat of the cards
    public bool DecideToChallenge(CardSuit claimedSuit, int totalThreatValue)
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
            return RunStandardProbability(claimedSuit, totalThreatValue);
        }

        // If a boss, execute their unique logic
        switch (_activeProfile.TypeOfBoss)
        {
            case BossType.FreddyFox:
                return FreddyFoxLogic(claimedSuit, totalThreatValue);

            case BossType.BanditWolf:
                return BanditWolfLogic(claimedSuit, totalThreatValue);

            case BossType.LucySwan:
                return LucySwanLogic(claimedSuit, totalThreatValue);

            default:
                return RunStandardProbability(claimedSuit, totalThreatValue);
        }
    }   

    private bool RunStandardProbability(CardSuit suit, int threatValue)
    {
        // Establish a base 20% chance to challenge any claim
        float baseChance = 0.20f;

        // The higher the total threat value, the more suspicious the enemy gets
        float valueFactor = threatValue * 0.02f;

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

        // Placeholder for the rank for the ClaimData constructor so it doesn't break
        CardRank placeholderRank = trueCards[0].Rank;

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
            }
            else
            {
                // Custom Boss Lies
                switch (_activeProfile.TypeOfBoss)
                {
                    case BossType.FreddyFox:
                        // Freddy lies about playing Traps/Rot to make the player paranoid
                        claimedSuit = CardSuit.Rot;
                        break;
                    
                    case BossType.BanditWolf:
                        // Bandit always lies about massive brute force
                        claimedSuit = CardSuit.Blood;
                        break;

                    case BossType.LucySwan:
                        // Lucy tells unpredictable, weird lies to mess with your head
                        claimedSuit = (CardSuit)Random.Range(0, 4);
                        break;
                }
            }

            // Lie --> Truth Bug Preventer (when the AI tries to lie but accidentally tells the truth)
            bool isActuallyLying = false;
            foreach (CardData card in trueCards)
            {
                if (card.Suit != claimedSuit)
                {
                    isActuallyLying = true;
                    break;
                }
            }

            // If an AI accidentally told the truth, force an actual lie
            if (!isActuallyLying)
            {
                Debug.Log($"{name} accidentally told the truth! Forcing a real lie...");
                
                // Swap the suit to guarantee it is a lie
                claimedSuit = (claimedSuit == CardSuit.Blood) ? CardSuit.Bone : CardSuit.Blood;
            }
        }

        // Always target the player
        return new ClaimData(trueCards, claimedSuit, placeholderRank, TurnSeat.Player);
    }

    private bool FreddyFoxLogic(CardSuit suit, int threatValue)
    {
        // Freddy is cunning and untrusting.
        // Heavily weights towards calling bluff on high-damage Blood cards
        if (suit == CardSuit.Blood && threatValue > 10)
        {
            return true; // Almost always challenges this
        }
        
        return RunStandardProbability(suit, threatValue);
    }

    private bool BanditWolfLogic(CardSuit suit, int threatValue)
    {
        // Bandit uses brute force, ignores bone because he just want to attack
        // Almost always accepts Bone claims to keep attacking
        if (suit == CardSuit.Bone)
        {
            return false; // Accept without checking
        }

        return RunStandardProbability(suit, threatValue);
    }

    private bool LucySwanLogic(CardSuit suit, int threatValue)
    {
        // Dynamic AI. Remembers past bluffs and adapts her probability per suit
        float standardChance = 0.20f + (threatValue * 0.02f);

        // Add her specific suspicion memory for this suit
        float finalChance = (standardChance + _swanSuspicion[suit]) * _activeProfile.SkepticismMultiplier;

        // Every time a player claims a suit, Lucy gets 15% more suspicious of that specific suit for the rest of the game
        _swanSuspicion[suit] += 0.15f;

        return Random.value < finalChance;
    }

    /// <summary>
    /// Looks at the current hand and decides how many cards to play based on aggression
    /// </summary>
    public List<CardData> SelectCardsToPlay(DeckManager enemyDeck)
    {
        float aggression = _activeProfile.AggressionMultiplier;
        int maxCards = Mathf.Min(3, enemyDeck.HandCount);
        int cardsToPlayCount;

        // Apply aggression logic
        if (aggression >= 2.0f && maxCards >= 2)
        {
            cardsToPlayCount = Random.Range(2, maxCards + 1); // Highly aggressive
        }
        else
        {
            cardsToPlayCount = Random.Range(1, maxCards + 1); // Standard
        }

        List<CardData> selectedCards = new List<CardData>();
        for (int i = 0; i < cardsToPlayCount; i++)
        {
            selectedCards.Add(enemyDeck.Hand[i]);
        }

        return selectedCards;
    }
}