using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A global static helper class to calculate card logic and apply damage
/// Does NOT inherit from MonoBehaviour. Do not attach to a GameObject pretty pls!
/// </summary>
public static class CombatLogic
{
    /// <summary>
    /// Evaluates a list of played cards and applies the resulting damage to the target
    /// </summary>
    public static void ProcessAttack(List<CardData> playedCards, CharacterStats target)
    {
        // Check to ensure there actually is a target and cards to process
        if (target == null || playedCards == null || playedCards.Count == 0)
        {
            return;
        }

        int totalDamage = 0;

        // Loop through the hand to calculate the total attack value
        foreach (CardData card in playedCards)
        {
            // Only Blood cards deal direct offensice damage
            if (card.Suit == CardSuit.Blood)
            {
                totalDamage += GetCardValue(card.Rank);
            }
        }

        // Apply the final calculated damage to the target
        if (totalDamage > 0)
        {
            Debug.Log($"[CombatLogic] Dealing {totalDamage} Blood damage to the target!");
            target.TakeDamage(totalDamage);
        }
    }

    /// <summary>
    /// Converts the CardRank enum into integer values dictated by the GDD
    /// </summary>
    private static int GetCardValue(CardRank rank)
    {
        switch (rank)
        {
            case CardRank.Two: 
                return 2;

            case CardRank.Three:
                return 3;

            case CardRank.Four:
                return 4;

            case CardRank.Five:
                return 5;

            case CardRank.Six:
                return 6;

            case CardRank.Seven:
                return 7;

            case CardRank.Eight:
                return 8;

            case CardRank.Nine:
                return 9;

            case CardRank.Ten:
                return 10;

            case CardRank.Jack:
                return 11;

            case CardRank.Queen:
                return 12;

            case CardRank.King:
                return 13;

            case CardRank.Ace:
                return 15;       // Ultimate attack value

            default:
                return 0;       // Fallback for Ace or unassigned ranks 
        }
    }
}
