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
    public static void ProcessTurn(List<CardData> playedCards, CardSuit effectiveSuit, CharacterStats attacker, CharacterStats target)
    {
        // Check to ensure there actually is a target and cards to process
        if (attacker == null || target == null || playedCards == null || playedCards.Count == 0)
        {
            return;
        }

        int totalDamage = 0;
        int totalShield = 0;
        int totalPoison = 0;
        int totalDodge = 0;

        // Loop through the hand to calculate the total values
        foreach (CardData card in playedCards)
        {
            // Check the EFFECTIVE suit (the claim), ignoring the true physical suit
            // Only Blood cards deal direct offensice damage
            if (effectiveSuit == CardSuit.Blood)
            {
                totalDamage += GetCardValue(card.Rank);
            }
            else if (effectiveSuit == CardSuit.Bone)
            {
                totalShield += GetCardValue(card.Rank);
            }
            else if (effectiveSuit == CardSuit.Rot)
            {
                totalPoison += GetCardValue(card.Rank); // Rot applies poison stacks
            }
            else if (effectiveSuit == CardSuit.Feather)
            {
                // Feather dodge chance is Rank x 10
                totalDodge += (GetCardValue(card.Rank) * 10);
            }
        }

        // Apply Dodge chance to the character who played the cards
        if (totalDodge > 0 &&  attacker != null)
        {
            Debug.Log($"<color=cyan>[CombatLogic] Adding {totalDodge}% dodge chance to the attacker</color>");
            attacker.AddDodgeChance(totalDodge);
        }

        // Apply shield to the character who played the cards
        if (totalShield > 0 && attacker != null)
        {
            Debug.Log($"<color=yellow>[CombatLogic] Adding {totalShield} Bone shield to the attcker</color>");
            attacker.AddShield(totalShield);
        }

        // Apply Poison stacks to the target
        if (totalPoison > 0 && target != null)
        {
            Debug.Log($"<color=green>[CombatLogic] Applying {totalPoison} Poison stacks to the target</color>");
            target.ApplyPoison(totalPoison);
        }

        // Apply the final calculated damage to the target
        if (totalDamage > 0 && target != null)
        {
            Debug.Log($"<color=red>[CombatLogic] Dealing {totalDamage} Blood damage to the target!</color>");
            target.TakeDamage(totalDamage);
        }
    }

    /// <summary>
    /// Converts the CardRank enum into integer values dictated by the GDD
    /// </summary>
    public static int GetCardValue(CardRank rank)
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
