using UnityEngine;
using System;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
   [SerializeField] private CardDatabase _cardDatabase;//contains all the possible cards used to build the deck
   [SerializeField] private CardDatabase _statusDatabase; // Only assigned to the player

   [SerializeField] private int _handSize = 5; //how many cards the character will hold

   private readonly List <CardData> _drawPile = new ();//cards waiting to be drawn
   private readonly List <CardData> _hand = new (); //cards currently being held
   private readonly List<CardData> _discardPile = new (); //cards that have already been played

   public IReadOnlyList<CardData> Hand => _hand; // other scripts can read the hand but cant edit the list

   public int HandCount => _hand.Count;
   public int DrawPileCount => _drawPile.Count;
   public int DiscardPileCount => _discardPile.Count;



   public event Action HandChanged; //fired whenever the contents of the hand change, the UI and enemy card visuals will be able to listen

   public void InitialiseDeck()
    {
        //clear any runtime data
        _drawPile.Clear();
        _hand.Clear();
        _discardPile.Clear();

        //prevent errors in database if forgotton in inspector
        if (_cardDatabase == null)
        {
            Debug.LogError($"{name} has no CardDatabase assigned -_-");
            return;
        }

        _drawPile.AddRange(_cardDatabase.Cards); //copy all 52 card refrences into the draw pile

        // If a status database is assigned (ONLY TO THE PLAYYYYEEEEEEEEEERRRRRRRRR) addd those!
        if (_statusDatabase != null && _statusDatabase.Cards.Count > 0)
        {
            _drawPile.AddRange(_statusDatabase.Cards);
        }

        Shuffle(_drawPile); //remember SHUFFLEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE //shuffel them

    }

    public void DrawToFullHand()//draw untill hand is full
    {
        bool handChanged = false;

        while (_hand.Count < _handSize)//continue drawing until reached max handsize
        {
            if ( _drawPile.Count == 0)//if run out of drrwa cards recycle the discard pile
            {
                ReshuffleDiscardPile();
            }

            if (_drawPile.Count == 0) //if both draw and discard piles are empty there is nothing left to draw
            {
                break;
            }

            int cardIndex = _drawPile.Count -1; //take the last card from the shuffeled list

            CardData card = _drawPile [cardIndex];

            _drawPile.RemoveAt(cardIndex); //remove it from the deck

            _hand.Add(card); //add it to the hand

            handChanged = true;
        }


        if (handChanged) //tell anything displaying this hand to update
        {
            HandChanged?.Invoke();
        }

    }

    /// <summary>
    /// Forces the deck to draw a specific amount of cards, ignoring the hand size limit
    /// Used by Draw 2 status card
    /// </summary>
    public void DrawAmount(int amount)
    {
        bool handChanged = false;

        for (int i = 0; i < amount; i++)
        {
            // Recycle the discard pile if ran out of cards mid-draw
            if (_drawPile.Count == 0)
            {
                ReshuffleDiscardPile();
            }

            // If it is STILL empty, stop drawing
            if (_drawPile.Count == 0)
            {
                break;
            }

            // Get the top card
            int cardIndex = _drawPile.Count - 1;
            CardData card = _drawPile[cardIndex];

            // Move it from the deck to the hand
            _drawPile.RemoveAt(cardIndex);
            _hand.Add(card);

            handChanged = true;
        }

        // Refresh visuals if successfully drew anything
        if (handChanged)
        {
            HandChanged?.Invoke();
        }
    }


    public void DiscardCards(IReadOnlyList<CardData> cards)//moves specific cards from hand into discard pile
    {
        bool handChanged = false;

        for (int i = 0; i < cards.Count; i++)
        {
            CardData card = cards [i];

            if (_hand.Remove(card))//only discard if it exists inside of the hand
            {
                _discardPile.Add(card);
                handChanged = true;
            }

        }

        if (handChanged) //refresh visuals
        {
            HandChanged?.Invoke();
        }

    }



    public void DiscardRandomCards(int amount) //Temporary method for testing prototype enemies, will be replaced later with AI
    {
        amount = Mathf.Clamp(amount,0, _hand.Count);//saftey to stop trying to discard more cards than holding

        bool handChanged = false;
        
        for(int i= 0; i < amount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, _hand.Count); //pick random card from hand

            CardData card = _hand[randomIndex];

            _hand.RemoveAt(randomIndex);//move card to discard

            _discardPile.Add(card);
            handChanged= true;
        }

        if (handChanged)
        {
            HandChanged?.Invoke();
        }
    }


    private void ReshuffleDiscardPile()
    {
        if (_discardPile.Count == 0)
        {
            return;
        }

        _drawPile.AddRange(_discardPile);
        _discardPile.Clear();
        Shuffle(_discardPile);
    }



    public void Shuffle(List<CardData> cards) //Fisher Yates card shuffle hehehe
    {
        for (int i = cards.Count -1; i>0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            CardData temporaryCard = cards[i];

            cards[i] = cards [randomIndex];

            cards[randomIndex] = temporaryCard;
        }
    }

}
