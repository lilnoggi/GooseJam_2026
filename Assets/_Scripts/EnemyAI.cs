using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private EnemyProfile _activeProfile;

    // Getter for TurnManager
    public EnemyProfile Profile => _activeProfile;

    // This method will be called by TurnManager.cs during Phase 2
    public bool DecideToChallenge(CardSuit claimedSuit, int claimedValue)
    {
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
        // Standard logic goes here, multipled by _activeProfile.SkepticismMultipler

        // TODO: Probability logic
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

    private bool FreddyFoxLogic(CardSuit suit, int value)
    {
        // Heavily weights towards calling bluff on high-damage Blood cards
        if (suit == CardSuit.Blood && value > 10)
        {
            return true; // Almost always challenges this
        }
        
        return RunStandardProbability(suit, value);
    }

    private bool BanditWolfLogic(CardSuit suit, int value)
    {
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
        // TODO: Implement this & add tracking variables later
        return RunStandardProbability(suit, value);
    }
}