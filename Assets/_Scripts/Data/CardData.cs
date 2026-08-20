using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Card_",
    menuName = "Cheating Geese/Cards/Card Data")] //creates an option in Unities create menu so CardData scriptable objects can be created manually
public class CardData : ScriptableObject
{
    [SerializeField] private CardSuit _suit; //the suit this card belongs to
    [SerializeField] private Sprite _suitSprite; // the sprite of the cards suit
    [SerializeField] private CardRank _rank; //the rank of the card

    [Header("Status Cards")]
    [SerializeField] private bool _isStatusCard; // Check this if the card is a status card
    [SerializeField] private StatusType _statusType;
    [SerializeField] private Sprite _statusSprite;
    [SerializeField] private CardPlayType _playType;
    [SerializeField] private string _statusName;
    [TextArea(3, 5)]
    [SerializeField] private string _statusDescription;
    [SerializeField] private StatusEffect _effectLogic;


    //other scripts can read these values but cant change them
    public CardSuit Suit => _suit; 
    public Sprite SuitSprite => _suitSprite;
    public CardRank Rank => _rank;
    public bool IsStatusCard => _isStatusCard;
    public StatusType StatusType => _statusType;
    public Sprite StatusSprite => _statusSprite;
    public CardPlayType PlayType => _playType;
    public string StatusName => _statusName;
    public string StatusDescription => _statusDescription;
    public StatusEffect EffectLogic => _effectLogic;


    //returns A, 2, ,3 ,J ,Q ,K etc.
    public string RankDisplayName =>
        CardRankUtility.GetDisplayName(_rank);


    //Example: A of Blood / K of rot
    public string CardDisplayName =>
        $"{RankDisplayName} of {_suit}";

#if UNITY_EDITOR
    public void EditorSetData(
        CardSuit suit,
        Sprite suitSprite,
        CardRank rank) //only used by the editor generator, this will prevent gamplay scripts from changing card data
    {
        _suit = suit;
        _suitSprite = suitSprite;
        _rank = rank;
    }
#endif
}