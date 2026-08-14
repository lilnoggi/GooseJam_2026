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


    private void Start()
    {
        _deckManager.HandChanged += UpdateHand; //whenever the enmy hand cahnges update 3d cards
        UpdateHand(); //show hand
    }


    private void UpdateHand()
    {
        ClearHand(); //remove the old cards

        int cardCount = _deckManager.HandCount; //how many cards enemy has

        for (int i = 0; i < cardCount; i++)
        {
            GameObject newCard = Instantiate(_cardPrefab, _handRoot);

            float angle = 0f;

            
            if (cardCount > 1) //works out where this current card should sit on the arc
            {
                angle = Mathf.Lerp( - _arcAngle / 2f, _arcAngle / 2f, (float) i / (cardCount - 1));
            }

            
            float angleInRadians = angle * Mathf.Deg2Rad; //converts the angle into sin and cos

            //positions the card around a curved arc
            float xPosition = Mathf.Sin(angleInRadians) * _arcRadius;
            float zPosition = Mathf.Cos(angleInRadians) * _arcRadius;

            newCard.transform.localPosition =new Vector3( xPosition, 0f, zPosition);
           
            newCard.transform.localRotation =Quaternion.Euler( 0f, angle, 0f); //makes each card follow the curve

            if (_cardsShouldFloat)
            {
                newCard.AddComponent<FloatingCards>();
            }

            _cardObjects.Add(newCard);
        }
    }


    private void ClearHand()
    {
        for (int i = 0; i < _cardObjects.Count; i++) //delete cards
        {
            Destroy(_cardObjects[i]);
        }

        _cardObjects.Clear(); //clear list
    }
}
