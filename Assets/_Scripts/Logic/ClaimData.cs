using System.Collections.Generic;

/// <summary>
/// A data container that holds both the reality of the played cards and the player's lie.
/// </summary>
public class ClaimData
{
    // THE TRUTH (What is actually face down on the table)
    public List<CardData> TrueCards { get; private set; }

    // THE LIE (What the player claimed to put down / what the player is telling the AI)
    public CardSuit ClaimedSuit { get; private set; }
    public CardRank ClaimedRank { get; private set; }

    // The TARGET
    public TurnSeat TargetEnemy { get; private set; }

    // Constructor to easily build 
    public ClaimData(List<CardData> trueCards, CardSuit claimedSuit, CardRank claimedRank, TurnSeat targetEnemy)
    {
        TrueCards = trueCards;
        ClaimedSuit = claimedSuit;
        ClaimedRank = claimedRank;
        TargetEnemy = targetEnemy;
    }
}
