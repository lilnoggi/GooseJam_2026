using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages the player's physical hand UI, card selection rules, and passive status card timers.
/// Offloads heavy visual coroutines to the HandAnimator script.
/// </summary>
public class PlayerHandManager : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private DeckManager _playerDeck;
    [SerializeField] private TurnController _turnController;
    [SerializeField] private HandAnimator _handAnimator;
    [SerializeField] private ClaimMenu _claimMenu;
    
    [Header("Character References")]
    [SerializeField] private CharacterStats _playerStats;

    [Header("UI References")]
    [SerializeField] private RectTransform _handContainer;
    [SerializeField] private PlayerCardView _cardPrefab;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _skipButton;
    
    [Header("Selection Rules")]
    [Tooltip("The maximum amonut of normal cards a player can select to form a single claim.")]
    [SerializeField] private int _maximumSelectedCards = 3;

    private List<PlayerCardView> _cardViews = new List<PlayerCardView>();
    private List<PlayerCardView> _selectedCards = new List<PlayerCardView>();

    // Memory cache to track ohw many turns a specific passive card has survived in the player's hand
    private Dictionary<CardData, int> _passiveCardTimers = new Dictionary<CardData, int>();

    // State trackers to prevent the player from interacting with cards during animations or enemy turns
    private bool _isChoosingClaim;
    private bool _isDrawingCards;
    private bool _isProcessingPassives;

    private Coroutine _drawCoroutine;

    /// <summary>
    /// Exposed a read-only list so the HandAminator acan scan the UI for InstaPlay cards
    /// </summary>
    public IReadOnlyList<PlayerCardView> CardViews => _cardViews;

    // ----------------------------------------------------------------------------------------------- 

    private void Start()
    {
        // Subscribe to system events
        _playerDeck.HandChanged += RefreshHand;
        _turnController.OnTurnChanged += TurnChanged;

        _playButton.onClick.AddListener(PlaySelectedCards);
        _skipButton.onClick.AddListener(SkipTurn);
        
        RefreshHand();
    }

    /// <summary>
    /// Triggered globally whenever the turn seat changes
    /// Initiates passive card effects if it is the player's turn.
    /// </summary>
    private void TurnChanged(TurnSeat turn)
    {
        _isChoosingClaim = false;

        if (turn == TurnSeat.Player)
        {
            StartCoroutine(ProcessStartOfTurnPassivesRoutine());
        }
        else
        {
            // If it is an enemy turn, instantly lock the hand controls
            UpdateButtons();
            UpdateCardInteraction();
        }
    }

    /// <summary>
    /// Evaluates all animation and turn states to determine if the cards should be clickable.
    /// </summary>
    private void UpdateCardInteraction()
    {
        bool canInteract = _turnController.IsPlayerTurn && !_isChoosingClaim && !_isDrawingCards && !_isProcessingPassives;

        foreach (var cardView in _cardViews)
        {
            cardView.SetInteractable(canInteract);
        }
    }

    /// <summary>
    /// Rebuilds the physical UI prefabs based on the new data state of the DeckManager.
    /// </summary>
    private void RefreshHand()
    {
        // Calculate exactly how many new cards arrived so only the new ones are animated
        int previousCardCount = _cardViews.Count;
        int newCardCount = _playerDeck.Hand.Count;
        int drawnCardCount = Mathf.Max(0, newCardCount - previousCardCount);

        List<PlayerCardView> newlyDrawnCards = new List<PlayerCardView>();

        ClearHand();
        _selectedCards.Clear();

        // Instantiate cards based on the deck data
        for (int i = 0; i < _playerDeck.Hand.Count; i++)
        {
            CardData cardData = _playerDeck.Hand[i];
            PlayerCardView newCard = Instantiate(_cardPrefab, _handContainer);

            newCard.Setup(cardData, CardClicked);

            // If this specific card is newly drawn, prepare it for the sliding animation
            if (drawnCardCount > 0 && i >= previousCardCount)
            {
                newCard.PrepareDrawAnimation();
                newlyDrawnCards.Add(newCard);
            }

            // Lock interaction if an enemy turn
            newCard.SetInteractable(_turnController.IsPlayerTurn && !_isChoosingClaim);
            _cardViews.Add(newCard);
        }

        // Delegate the complex sliding animations to the HandAnimator script
        if (newlyDrawnCards.Count > 0 && _handAnimator != null)
        {
            // Stop any overlapping drawing sequences before starting a fresh one
            if (_drawCoroutine != null) StopCoroutine(_drawCoroutine);

            _isDrawingCards = true;
            UpdateCardInteraction();
            UpdateButtons();

            _drawCoroutine = StartCoroutine(_handAnimator.AnimateDrawnCardsRoutine(newlyDrawnCards, this));
        }
        else
        {
            // Failsafe in case a card wipe results in 0 drawn cards
            CompleteDrawingSequence();
        }

        CleanUpTimers();
        UpdateButtons();
    }

    /// <summary>
    /// Callback fired by the HandAnimator when all drawing animations and InstaPlay effects conclude.
    /// </summary>
    public void CompleteDrawingSequence()
    {
        _isDrawingCards = false;
        UpdateCardInteraction();
        UpdateButtons();
    }

    /// <summary>
    /// Destroys all physical UI cards before a refresh
    /// </summary>
    private void ClearHand()
    {
        foreach (var card in _cardViews) Destroy(card.gameObject);
        _cardViews.Clear();
    }

    /// <summary>
    /// Enforces selection rules (e.g., max hand limits).
    /// </summary>
    private void CardClicked(PlayerCardView card)
    {
        if (!_turnController.IsPlayerTurn) return;

        // RULE 1: Passives act automatically; the player cannot actively bluff with them
        if (card.CardData.IsStatusCard && card.CardData.PlayType == CardPlayType.Passive) return; 

        // Handle Deselection
        if (_selectedCards.Contains(card))
        {
            _selectedCards.Remove(card);
            card.SetSelected(false);
            UpdateButtons();
            return;
        }

        // RULE 2: Action/InstaPlay cards are too powerful to combine with other bluffs
        if (card.CardData.PlayType == CardPlayType.Action || card.CardData.PlayType == CardPlayType.InstaPlay)
        {
            foreach (var selected in _selectedCards) selected.SetSelected(false);
            _selectedCards.Clear();
        }
        else if (_selectedCards.Count > 0 &&
                (_selectedCards[0].CardData.PlayType == CardPlayType.Action || 
                 _selectedCards[0].CardData.PlayType == CardPlayType.InstaPlay))
        {
            return; // Reject normal cards if an action is already queued
        }

        // Enforce max hand size
        if (_selectedCards.Count >= _maximumSelectedCards) return;

        _selectedCards.Add(card);
        card.SetSelected(true);
        UpdateButtons();
    }

    /// <summary>
    /// Routes the selected cards to the correct execution phase (InstaPlay vs Action Target vs Claim Menu)
    /// </summary>
    private void PlaySelectedCards()
    {
        if (_selectedCards.Count == 0) return;

        List<CardData> cardsToPlay = new List<CardData>();
        foreach (var c in _selectedCards) cardsToPlay.Add(c.CardData);

        _isChoosingClaim = true;
        UpdateCardInteraction();
        UpdateButtons();

        CardPlayType type = cardsToPlay[0].PlayType;

        if (type == CardPlayType.InstaPlay)
        {
            // InstaPlay cards bypass the standoff entirely
            cardsToPlay[0].EffectLogic.OnPlay(_playerStats, null, _playerDeck);
            _playerDeck.DiscardCards(cardsToPlay);
            _turnController.AdvanceTurn();
        }
        else if (type == CardPlayType.Action)
        {
            // Action card bypass suit selection and go straight to target selection
            _claimMenu.ShowActionTargetMenu(cardsToPlay[0]);
        }
        else
        {
            // Normal cards open the standard bluffing menu
            _claimMenu.ShowMenu(cardsToPlay);
        }
    }

    private void SkipTurn()
    {
        _turnController.SkipPlayerTurn();
    }

    /// <summary>
    /// Manages the visibility and interactability of the Play and Skip buttons based on system locks
    /// </summary>
    private void UpdateButtons()
    {
        bool canUseHand = _turnController.IsPlayerTurn && !_isChoosingClaim && !_isDrawingCards && !_isProcessingPassives;

        _playButton.gameObject.SetActive(canUseHand);
        _skipButton.gameObject.SetActive(canUseHand);

        if (canUseHand)
        {
            _playButton.interactable = _selectedCards.Count > 0; 
            _skipButton.interactable = true;   
        }
    }

    /// <summary>
    /// Iterates through the player's held passive cards and resolves their timers at the start of the turn.
    /// </summary>
    private IEnumerator ProcessStartOfTurnPassivesRoutine()
    {
        _isProcessingPassives = true;
        UpdateButtons();
        UpdateCardInteraction();

        // Lock execution until dealing animations finish
        yield return new WaitUntil(() => !_isDrawingCards);
        yield return new WaitForSeconds(0.5f);

        // Iterate backwards to allow safe removal during the loop
        for (int i = _playerDeck.Hand.Count - 1; i >= 0; i--)
        {
            CardData cardData = _playerDeck.Hand[i];

            if (cardData.IsStatusCard && cardData.PlayType == CardPlayType.Passive && cardData.EffectLogic != null)
            {
                PlayerCardView visualCard = _cardViews.Find(v => v.CardData == cardData);

                if (visualCard != null)
                {
                    visualCard.SetSelected(true);
                    yield return new WaitForSeconds(1.5f);

                    // Initialise or increment timer
                    if (!_passiveCardTimers.ContainsKey(cardData)) _passiveCardTimers[cardData] = 1;
                    else _passiveCardTimers[cardData]++;

                    int turnsHeld = _passiveCardTimers[cardData];

                    // The specific card's ScriptableObject dictates if it should be destroyed
                    bool shouldDiscard = cardData.EffectLogic.OnTurnStart(_playerStats, turnsHeld);

                    yield return new WaitForSeconds(1.0f);
                    visualCard.SetSelected(false);

                    if (shouldDiscard)
                    {
                        _passiveCardTimers.Remove(cardData);
                        _playerDeck.DiscardCards(new List<CardData> { cardData });
                        yield return new WaitForSeconds(0.5f);
                    }
                }
            }
        }

        _isProcessingPassives = false;
        UpdateButtons();
        UpdateCardInteraction();
    }

    /// <summary>
    /// Memory management: Clears the timer tracking data for passives that have left the player's hand.
    /// </summary>
    private void CleanUpTimers()
    {
        // Extract keys to a seperate list to modify the dictoinary during iteration
        List<CardData> trackedCards = new List<CardData>(_passiveCardTimers.Keys);
        
        foreach (var card in trackedCards)
        {
            // Verify if the deck's true hand still contains this specific card
            bool isStillInHand = false;
            for (int i = 0; i < _playerDeck.Hand.Count; i++)
            {
                if (!_playerDeck.Hand[i] == (card))
                {
                    isStillInHand = true;
                    break;
                }   
            }

            // If the player bluffed it away or it dissolvedd, clear its memory cache
            if (!isStillInHand)
            {
                _passiveCardTimers.Remove(card);
            }
        }
    }
    
    /// <summary>
    /// Invoked by the HandAnimator to forcefully resolve an InstaPlay card drawn mid-sequence.
    /// </summary>
    public void ExecuteInstaPlay(CardData cardData)
    {
        _playerDeck.DiscardCards(new List<CardData> { cardData });
        cardData.EffectLogic.OnPlay(_playerStats, null, _playerDeck);
    }

    /// <summary>
    /// Routes the glow animation request (e.g., from Hollow Promise) to the HandAnimator script.
    /// </summary>
    public IEnumerator HighlightCardInHand(CardData cardData, float duration)
    {
        PlayerCardView visualCard = _cardViews.Find(v => v.CardData == cardData);
        if (visualCard != null && _handAnimator != null)
        {
            yield return StartCoroutine(_handAnimator.HighlightCardRoutine(visualCard, duration));
        }
    }
}