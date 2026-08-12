using UnityEngine;

public enum CardSuit //card suites used by Cheating Geese
{
    Bone,
    Blood,
    Rot,
    Feather
}

public enum CardRank // every value avaliable in the deck
{
    Ace,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King
}

public static class CardRankUtility //helper class to convert the enum names into the text that will appear on card (FOR NOW)
{
    public static string GetDisplayName(CardRank rank)
    {
        switch (rank)
        {
        case CardRank.Ace:
        return "A";

        case CardRank.Two:
        return "2";

        case CardRank.Three:
        return "3";

        case CardRank.Four:
        return "4";

        case CardRank.Five:
        return "5";

        case CardRank.Six:
        return "6";

        case CardRank.Seven:
        return "7";

        case CardRank.Eight:
        return "8";

        case CardRank.Nine:
        return "9";

        case CardRank.Ten:
        return "10";

        case CardRank.Jack:
        return "J";

        case CardRank.Queen:
        return "Q";

        case CardRank.King:
        return "K";

        default:
        return "?";

        }
    }
}

