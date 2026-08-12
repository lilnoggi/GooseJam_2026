using UnityEngine;
using System.Collections.Generic;
using System.Data;

public class EnemyHandDisplay : MonoBehaviour
{
    [SerializeField] private DeckManager _deckManager; //deck beloging to enemy

    [SerializeField] private Transform _handRoot; //where the cards appear from

    [SerializeField] private GameObject _cardPrefab; //card back prefab

    [SerializeField] private float _cardSpacing = 0.2f; //distance between cards

    [SerializeField] private float _fanAngle = 8f; //how much the cards rotate to make c/fan shape

    private List<GameObject> _cardObjects = new List<GameObject>(); //stores the card objects that are showed


    private void Start()
    {
        _deckManager.HandChanged += UpdateHand; //whenever the enmy hand cahnges update 3d cards
        UpdateHand(); //show hand
    }


    private void UpdateHand()
    {
        ClearHand(); //remove old cards

        int cardCount = _deckManager.HandCount; //how many cards enemy has

        for (int i = 0; i < cardCount; i++) //create a card back from every card enemy has in hand
        {
            GameObject newCard = Instantiate(_cardPrefab, _handRoot);

            float cardPosition = i - ((cardCount - 1) / 2f); //center the cards

            newCard.transform.localPosition =new Vector3( cardPosition * _cardSpacing, 0f, 0f); //move cards left/right

            newCard.transform.localRotation =Quaternion.Euler( 0f, 0f, -cardPosition * _fanAngle); //rotate cards to create c/fan shape


            _cardObjects.Add(newCard); //save card
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
