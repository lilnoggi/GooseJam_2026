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

    //the AI only receives information it could actually know
    //the claimed suit and how many cards are face-down
    public bool DecideToChallenge(CardSuit claimedSuit, int claimedCardCount)
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
            return RunStandardProbability(claimedSuit, claimedCardCount);
        }

        // If a boss, execute their unique logic
        switch (_activeProfile.TypeOfBoss)
        {
            case BossType.FreddyFox:
                return FreddyFoxLogic(claimedSuit, claimedCardCount);

            case BossType.BanditWolf:
                return BanditWolfLogic(claimedSuit, claimedCardCount);

            case BossType.LucySwan:
                return LucySwanLogic(claimedSuit, claimedCardCount);

            default:
                return RunStandardProbability(claimedSuit, claimedCardCount);
        }
    }   

    private bool RunStandardProbability(CardSuit suit, int cardCount)
    {
        // Establish a base 25% chance to challenge any claim
        float baseChance = 0.25f;

        //more cards being claimed makes the claim more susicious
        //1 card = no extra suspicion, 2 cards = +12%, 3 cards = +24%
        float amountFactor = (cardCount - 1) * 0.12f;

        float suitFactor = 0f;//different suits feel more or less threatening to the AI

        switch (suit)
        {
            case CardSuit.Blood:
            suitFactor = 0.18f;
            break;

            case CardSuit.Rot:
            suitFactor = 0.10f;
            break;

            case CardSuit.Bone:
            suitFactor = 0.05f;
            break;

            case CardSuit.Feather:
            suitFactor = 0.03f;
            break;
        }

        float paranoiaFactor = 0f; //paranoi changes how suspicious the enemy is

        if( _stats != null && _activeProfile.MaxParanoia > 0)
        {
            float paranoiaPercent= (float)_stats.CurrentParanoia / _activeProfile.MaxParanoia ;

            //low paranoia makes Ai more trusting
            //high paranoia makes Ai more suspicious
            paranoiaFactor = Mathf.Lerp(-0.10f, 0.20f, paranoiaPercent);
        }

        //put everything together then apply to AI personality
        float finalCheatChance = (baseChance + amountFactor + suitFactor + paranoiaFactor) * _activeProfile.SkepticismMultiplier; 

        //keep the probability as a resenobale number
        finalCheatChance = Mathf.Clamp(finalCheatChance, 0.05f, 0.95f);

        Debug.Log ($"{name} has a {finalCheatChance * 100f:F0}% chance to call CHEAT " +$"on {cardCount} {suit} cards.");

        // Generate a random value between 0.0 and 1.0
        // If the nuymber is lower than the final chance, call cheat
        return Random.value < finalCheatChance;
    }

    /// <summary>
    /// Looks at the physical cards picked and decides strategy
    /// </summary>
    public ClaimData FormulateClaim(List<CardData> trueCards)
    {
        // Placeholder for the rank for the ClaimData constructor so it doesn't break
        CardRank placeholderRank = trueCards[0].Rank;

        // Evaluate wahat the enemy actually just picked
        bool cardsMatch = true;
        CardSuit trueSuit = trueCards[0].Suit;
        for (int i = 1; i < trueCards.Count; i++)
        {
            if (trueCards[i].Suit != trueSuit)
            {
                cardsMatch = false;
            }
        }

        CardSuit claimedSuit = trueSuit;

        // Decide the claim
        if (!cardsMatch)
        {
            // The hand is mixed. FORCE lie
            Debug.Log($"{name} is holding mixed cards. FORCED TO BLUFF!");
            claimedSuit = GetTacticalBluffSuit();
        }
        else
        {
            // Honest set. Does the AI tell the truth or bait the player
            float bluffChance = (_stats != null && _stats.CurrentParanoia <= 0) ? 0.40f : 0.15f;

            if (Random.value < bluffChance)
            {
                Debug.Log($"{name} has a truthful hand, but decided to BLUFF");
                claimedSuit = GetTacticalBluffSuit();
                              
                // Prevent accidental truth
                if (claimedSuit == trueSuit)
                {
                    claimedSuit = (trueSuit == CardSuit.Blood) ? CardSuit.Bone : CardSuit.Blood;
                }
            }
            else
            {
                Debug.Log($"{name} is telling the TRUTH");
                claimedSuit = trueSuit; // Honest play
            }
        }

        return new ClaimData(trueCards, claimedSuit, placeholderRank, TurnSeat.Player);
    }

    // Helper method to assign personality-driven lies
    private CardSuit GetTacticalBluffSuit()
    {
        if (!_activeProfile.IsBoss)
        {
            return (Random.value > 0.5f) ? CardSuit.Blood : CardSuit.Rot;
        }

        switch (_activeProfile.TypeOfBoss)
        {
            case BossType.FreddyFox:
                return CardSuit.Rot;
            
            case BossType.BanditWolf:
                return CardSuit.Blood;
            
            case BossType.LucySwan:
                return (CardSuit)Random.Range(0, 4);
            
            default:
                return CardSuit.Blood;
        }
    }

    private bool FreddyFoxLogic(CardSuit suit, int cardCount)
    {
        //freddy doesnt trust big blood claims
        if (suit == CardSuit.Blood && cardCount >= 2)
        {
            return true; // Almost always challenges this
        }
        
        return RunStandardProbability(suit, cardCount);
    }

    private bool BanditWolfLogic(CardSuit suit, int cardCount)
    {
        // Bandit uses brute force, ignores bone because he just want to attack
        // Almost always accepts Bone claims to keep attacking
        if (suit == CardSuit.Bone)
        {
            return false; // Accept without checking
        }

        return RunStandardProbability(suit, cardCount);
    }

    private bool LucySwanLogic(CardSuit suit, int cardCount)
    {
        // lucy gets more suspicious when multiple cards are played
        float standardChance = 0.20f + ((cardCount - 1) * 0.12f);

        // Add her specific suspicion memory for this suit
        float finalChance = (standardChance + _swanSuspicion[suit]) * _activeProfile.SkepticismMultiplier;

        // Every time a player claims a suit, Lucy gets 15% more suspicious of that specific suit for the rest of the game
        _swanSuspicion[suit] += 0.15f;

        return Random.value < finalChance;
    }

    /// <summary>
    /// Looks at the current hand and decides which specific cards to play
    /// </summary>
    public List<CardData> SelectCardsToPlay(DeckManager enemyDeck)
    {
        IReadOnlyList<CardData> hand = enemyDeck.Hand;
        List<CardData> selectedCards = new List<CardData>();

        // Group the hand by suits to find matching sets
        Dictionary<CardSuit, List<CardData>> sortedHand = new Dictionary<CardSuit, List<CardData>>();
        foreach (CardSuit suit in System.Enum.GetValues(typeof(CardSuit)))
        {
            sortedHand[suit] = new List<CardData>();
        }
        foreach (CardData card in hand)
        {
            sortedHand[card.Suit].Add(card);
        }

        int maxCards = Mathf.Min(3, hand.Count);
        int targetCardCount = (_activeProfile.AggressionMultiplier >= 2.0f && maxCards >= 2) ? Random.Range(2, maxCards + 1) : 1;
        bool foundMatch = false;

        // Tactical Prioritisation based on Paranoia
        CardSuit prioritySuit = CardSuit.Blood; // Default to aggression
        if (_stats != null && _stats.CurrentParanoia >= 60)
        {
            // If highly paranoid/scared prioritise Bone (Shields) or Feather (Dodges)
            prioritySuit = (sortedHand[CardSuit.Bone].Count > 0) ? CardSuit.Bone : CardSuit.Feather;
        }

        // Try to build a hand using the priority suit first
        if (sortedHand[prioritySuit].Count >= targetCardCount)
        {
            selectedCards.AddRange(sortedHand[prioritySuit].GetRange(0, targetCardCount));
            foundMatch = true;
        }
        else
        {
            // Fallback: Look for ANY matching set so the AI can actually tell the truth
            foreach(var suitList in sortedHand.Values)
            {
                if (suitList.Count >= targetCardCount)
                {
                    selectedCards.AddRange(suitList.GetRange(0, targetCardCount));
                    foundMatch = true;
                    break;
                }
            }
        }

        // Forced Bluffs & Desperatin
        if (!foundMatch)
        {
            if (Random.value > 0.4f)
            {
                // Play safe: just play one card honestly
                selectedCards.Add(hand[0]);
            }
            else
            {
                // Desperation: Grab random mismatched cards (GUARANTEES A BLUFF)
                for (int i = 0; i < targetCardCount; i++)
                {
                    selectedCards.Add(hand[i]);
                }
            }
        }

        return selectedCards;
    }
}