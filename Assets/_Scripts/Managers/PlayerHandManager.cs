using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

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

    [Header("Draw Animation")]
    [SerializeField] private float _drawCardBelowOffset = -500f; //where the 2D card starts before sliding in
    [SerializeField] private float _delayBetweenDraws = 0.08f; //small gap between each drawn card
    [SerializeField] private float _handSettleTime = 0.4f; //give the final card time to reach its slot

    private List<PlayerCardView> _cardViews = new List<PlayerCardView>(); //stores cards that are on screen

    private List<PlayerCardView> _selectedCards = new List<PlayerCardView>(); //stores selected cards

    private bool _isChoosingClaim;

    private bool _isDrawingCards; //stops the player using cards while they are being dealt


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
        _isChoosingClaim = false;
        
        UpdateButtons(); //update buttons when turn changes

        UpdateCardInteraction();
    }

    private void UpdateCardInteraction()
    {
        bool canInteract =_turnManager.IsPlayerTurn && !_isChoosingClaim && !_isDrawingCards;

        for (int i = 0; i < _cardViews.Count; i++)
        {
            _cardViews[i].SetInteractable(canInteract);
        }
    }

    private void RefreshHand()
    {
        //remember how many cards were already being shown
        int previousCardCount = _cardViews.Count;

        //see how many cards are in the hand now
        int newCardCount = _playerDeck.Hand.Count;

        //if the number went up, these are newly drawn cards
        int drawnCardCount = Mathf.Max(0, newCardCount - previousCardCount);

        //store the new card views so we can animate them afterwards
        List<PlayerCardView> newlyDrawnCards = new List<PlayerCardView>();

        ClearHand(); //delete old cards

        _selectedCards.Clear();// delete old selected cards

        for (int i = 0; i < _playerDeck.Hand.Count; i++) //create visual card for every card in player hand
        {
            CardData cardData = _playerDeck.Hand[i];

            PlayerCardView newCard =Instantiate( _cardPrefab,_handContainer);

            newCard.Setup(cardData, CardClicked); //give card data and call method when clicked

            //if this card was just drawn, hide it until the 3D animation reaches the player
            if (drawnCardCount > 0 && i >= previousCardCount)
            {
                newCard.PrepareDrawAnimation();
                newlyDrawnCards.Add(newCard);
            }

            newCard.SetInteractable(_turnManager.IsPlayerTurn && ! _isChoosingClaim); //only allow interaction during player turn and not if they are claiming their cards

            _cardViews.Add(newCard); //store new card
        }

        //only run the dealing animation if new cards were actually drawn
        if (newlyDrawnCards.Count > 0)
        {
            StartCoroutine(AnimateDrawnCards(newlyDrawnCards));
        }

        UpdateButtons();
    }

    private IEnumerator AnimateDrawnCards(List<PlayerCardView> drawnCards)
    {
        _isDrawingCards = true;

        //wait one frame for all the new cards 
        yield return null;

        //force the hand layout to finish arranging the cards... no idea why it wasn't working, I have been stuck on this for close to 2 hours...gyfehwsagh
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_handContainer);

        //don't let the player click anything while cards are being dealt
        UpdateCardInteraction();
        UpdateButtons();

        for (int i = 0; i < drawnCards.Count; i++)
        {
            //first move a 3D card from the pile towards the player
            yield return StartCoroutine(TableManager.Instance.AnimateCardDrawToPlayer());

            //swap it for the real 2D card and slide that into the hand
            drawnCards[i].PlayDrawAnimation(_drawCardBelowOffset);

            //tiny pause before dealing the next card
            yield return new WaitForSeconds(_delayBetweenDraws);
        }

        //give the last card a little time to finish moving
        yield return new WaitForSeconds(_handSettleTime);

        _isDrawingCards = false;

        //player can use their hand again
        UpdateCardInteraction();
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
            cardsToPlay.Add(_selectedCards[i].CardData);
        }

        _isChoosingClaim = true; //player can't interact with cards when making claim

        UpdateCardInteraction();
        UpdateButtons();
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
        // Is the player ALLOWED to play right now?
        bool canUseHand =_turnManager.IsPlayerTurn && !_isChoosingClaim && !_isDrawingCards;

        // If they can use their hand, turn the buttons ON. If not, turn them off
        _playButton.gameObject.SetActive(canUseHand);
        _skipButton.gameObject.SetActive(canUseHand);

        // If the buttons are visible, handle the interactbale states
        if (canUseHand)
        {
            // Play needs at least one selected card.
            _playButton.interactable = canUseHand && _selectedCards.Count > 0; ////player needs atleast 1 card selected in order to play turn
            _skipButton.interactable = canUseHand; //skip button can always be used   
        }
    }
}
