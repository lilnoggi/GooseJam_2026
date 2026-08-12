using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerHandManager : MonoBehaviour
{

    [SerializeField] private DeckManager _playerDeck; //players deck

    [SerializeField] private TurnManager _turnManager; //check whoes turn it is

    [SerializeField] private PlayerCardView _cardPrefab;//the card prefab

    [SerializeField] private RectTransform _handContainer;//the panel where the cards spawn

    [SerializeField] private Button _playButton;
    [SerializeField] private Button _skipButton;

    [SerializeField] private int _maximumSelectedCards =3; //the amount of cards that can be selected

    [SerializeField] private ClaimMenu _claimMenu; // Reference to the claim menu

    private List<PlayerCardView> _cardViews = new List<PlayerCardView>(); //stores cards that are on screen

    private List<PlayerCardView> _selectedCards = new List<PlayerCardView>(); //stores selected cards


    private void Start()
    {
        _playerDeck.HandChanged += RefreshHand; //lsiten for player hand changes

        _turnManager.OnTurnChanged += TurnChanged; //update buttons when the turn changes

        //listen for buttons
        _playButton.onClick.AddListener(PlaySelectedCards);
        _skipButton.onClick.AddListener(SkipTurn);

        
        RefreshHand();//show players starting cards
    }



    private void TurnChanged(TurnSeat turn)
    {
        UpdateButtons(); //update buttons when turn changes
    }

    private void RefreshHand()
    {
        ClearHand(); //delete old cards

        _selectedCards.Clear();// delete old selected cards

        for (int i = 0; i < _playerDeck.Hand.Count; i++) //create visual card for every card in player hand
        {
            CardData cardData = _playerDeck.Hand[i];

            PlayerCardView newCard =Instantiate( _cardPrefab,_handContainer);

            newCard.Setup(cardData, CardClicked); //give card data and call method when clicked

            _cardViews.Add(newCard); //store new card
        }

        UpdateButtons();
    }

    private void ClearHand()
    {
        for (int i = 0; i < _cardViews.Count; i++) //delete displayed cards
        {
            Destroy(_cardViews[i].gameObject);
        }

        _cardViews.Clear();
    }


    private void CardClicked(PlayerCardView card)
    {
        if (!_turnManager.IsPlayerTurn)//dont do anything if not polayer turn
        {
            return;
        }

        if (_selectedCards.Contains(card)) //if clciked card again decelect card
        {
            _selectedCards.Remove(card);

            card.SetSelected(false);

            UpdateButtons();

            return;
        }

        if (_selectedCards.Count >= _maximumSelectedCards) //player cant select more than 3 cards
        {
            return;
        }

        _selectedCards.Add(card); //select card

        card.SetSelected(true);

        UpdateButtons();
    }


    private void PlaySelectedCards()
    {
        if (_selectedCards.Count == 0) //player needs atleast 1 card selected in order to play turn
        {
            return;
        }

        List<CardData> cardsToPlay = new List<CardData>(); //list contains CardData for the selected card

        for (int i = 0; i < _selectedCards.Count; i++)
        {
            cardsToPlay.Add(
                _selectedCards[i].CardData);
        }

        // _turnManager.PlayPlayerCards(cardsToPlay); //send the selected cards to the TurnManager script

        // Instead of engind the turn, open the Claim Menu and pass the real cards
        _claimMenu.ShowMenu(cardsToPlay);
    }


    private void SkipTurn()
    {
        _turnManager.SkipPlayerTurn(); //tells the TurnManager script to skip player turn
    }


    private void UpdateButtons()
    {

        bool playerTurn = _turnManager.IsPlayerTurn; //player can use buttons only if their turn

        // Play needs at least one selected card.
        _playButton.interactable = playerTurn && _selectedCards.Count > 0; ////player needs atleast 1 card selected in order to play turn

        _skipButton.interactable = playerTurn; //skip button can always be used
    }
}
