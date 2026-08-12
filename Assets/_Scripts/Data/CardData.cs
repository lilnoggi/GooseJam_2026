using UnityEngine;

[CreateAssetMenu(
    fileName = "Card_",
    menuName = "Cheating Geese/Cards/Card Data")] //creates an option in Unities create menu so CardData scriptable objects can be created manually
public class CardData : ScriptableObject
{
    [SerializeField] private CardSuit _suit; //the suit this card belongs to
    [SerializeField] private CardRank _rank; //the rank of the card


    //other scripts can read these values but cant change them
    public CardSuit Suit => _suit; 
    public CardRank Rank => _rank;


    //returns A, 2, ,3 ,J ,Q ,K etc.
    public string RankDisplayName =>
        CardRankUtility.GetDisplayName(_rank);


    //Example: A of Blood / K of rot
    public string CardDisplayName =>
        $"{RankDisplayName} of {_suit}";

#if UNITY_EDITOR
    public void EditorSetData(
        CardSuit suit,
        CardRank rank) //only used by the editor generator, this will prevent gamplay scripts from changing card data
    {
        _suit = suit;
        _rank = rank;
    }
#endif
}