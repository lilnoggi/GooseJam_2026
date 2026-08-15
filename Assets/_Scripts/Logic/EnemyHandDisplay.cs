using UnityEngine;
using System.Collections.Generic;
using System.Data;

public class EnemyHandDisplay : MonoBehaviour
{
    [SerializeField] private DeckManager _deckManager; //deck beloging to enemy

    [SerializeField] private Transform _handRoot; //where the cards appear from

    [SerializeField] private GameObject _cardPrefab; //card back prefab

    [SerializeField] private float _arcRadius = 0.5f; //how far the cards are from the centre

    [SerializeField] private float _arcAngle = 60f; //how much of the circle the cards use that goes around the enemy

    [SerializeField] private bool _cardsShouldFloat = true; 

    private List<GameObject> _cardObjects = new List<GameObject>(); //stores the card objects that are showed

    private bool _handHasBeenCreated;


    private void Start()
    {
        _deckManager.HandChanged += UpdateHand; //whenever the enmy hand cahnges update 3d cards
        UpdateHand(); //show hand
    }

    private void OnDestroy()
    {
        if (_deckManager != null) //stop listening when destroyed
        {
            _deckManager.HandChanged -= UpdateHand;
        }
    }


    private void UpdateHand()
    {
        int cardCount = _deckManager.HandCount;

        //if the enemy has fewer cards now remove only the extra card objects
        while (_cardObjects.Count > cardCount)
        {
            int lastCardIndex = _cardObjects.Count - 1;

            GameObject cardToRemove = _cardObjects[lastCardIndex];

            _cardObjects.RemoveAt(lastCardIndex);

            Destroy(cardToRemove);
        }

        //if the enemy has drawn new cards, create the missing card 
        while (_cardObjects.Count < cardCount)
        {
            GameObject newCard = Instantiate(_cardPrefab, _handRoot);

            FloatingCards floatingCard = newCard.GetComponent<FloatingCards>();

            if (_cardsShouldFloat && floatingCard == null)
            {
                floatingCard = newCard.AddComponent<FloatingCards>();
            }

            _cardObjects.Add(newCard);
        }

        //the new centred positions
        for (int i = 0; i < cardCount; i++)
        {
            float angleSpacing = _arcAngle / 4f;

            float angle = (i - (cardCount - 1) / 2f) * angleSpacing;

            float angleInRadians = angle * Mathf.Deg2Rad;

            float xPosition = Mathf.Sin(angleInRadians) * _arcRadius;

            float zPosition = Mathf.Cos(angleInRadians) * _arcRadius;

            Vector3 newPosition = new Vector3(xPosition, 0f, zPosition);

            GameObject cardObject = _cardObjects[i];

            //tell FloatingCards where this card should move to
            FloatingCards floatingCard = cardObject.GetComponent<FloatingCards>();

            if (! _handHasBeenCreated)
            {
                // the first time put cards in their correct positions immediately
                if (floatingCard != null)
                {
                    floatingCard.SetStartingPosition(newPosition);
                }

                else
                {
                    cardObject.transform.localPosition = newPosition;
                }
            }

            else
            {
                // move remaining cards into their new positions
                if (floatingCard != null)
                {
                    floatingCard.MoveToPosition(newPosition);
                }

                else
                {
                    cardObject.transform.localPosition = newPosition;
                }
            }
            //keep the card following the curve
            cardObject.transform.localRotation =Quaternion.Euler(0f, angle, 0f);
        }

        if (cardCount > 0)
        {
            _handHasBeenCreated = true;
        }
    }  
}
