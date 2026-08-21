using System.Collections.Generic;
using UnityEngine;

public class DrawDeckVisual : MonoBehaviour
{
    public static DrawDeckVisual Instance { get; private set; }

    public Transform DrawSpawnPoint => _drawSpawnPoint;

    [Header("Stack References")]
    [SerializeField] private GameObject _visualCardPrefab;
    [SerializeField] private Transform _visualStackRoot;
    [SerializeField] private Transform _drawSpawnPoint;

    [Header("Stack Settings")]
    [SerializeField] private int _maximumVisualCards = 24;

    //small gap between cards
    [SerializeField] private float _cardHeight = 0.003f;

    private List<GameObject> _visualCards = new List<GameObject>();
    

    private void Awake()
    {
        Instance = this;
        CreateFullStack();
    }


    private void CreateFullStack()
    {
        //clear anything previously created
        foreach (GameObject card in _visualCards)
        {
            if (card != null)
            {
                Destroy(card);
            }
        }

        _visualCards.Clear();

        //create fake cards
        for (int i = 0; i < _maximumVisualCards; i++)
        {
            GameObject newCard = Instantiate(_visualCardPrefab, _visualStackRoot);

            //each card sits above the previous card
            newCard.transform.localPosition = new Vector3(0f, 0f, -i * _cardHeight);

            newCard.transform.localRotation = Quaternion.identity;

            _visualCards.Add(newCard);
        }

        UpdateDrawSpawnPoint();
    }

    public void RemoveVisualCard()
    {
        //dont remove if the visual deck is empty
        if (_visualCards.Count == 0)
        {
            return;
        }

        int topCardIndex = _visualCards.Count - 1;

        GameObject topCard = _visualCards[topCardIndex];

        _visualCards.RemoveAt(topCardIndex);

        if (topCard != null)
        {
            Destroy(topCard);
        }

        UpdateDrawSpawnPoint();
    }

    public void AddVisualCard()
    {
        // dont grow past the maximum visual size
        if (_visualCards.Count >= _maximumVisualCards)
        {
            return;
        }

        int newCardIndex = _visualCards.Count;

        GameObject newCard = Instantiate( _visualCardPrefab, _visualStackRoot);

        newCard.transform.localPosition = new Vector3(0f, 0f, -newCardIndex * _cardHeight);

        newCard.transform.localRotation = Quaternion.identity;

        _visualCards.Add(newCard);

        UpdateDrawSpawnPoint();
    }

    public void RefillVisualDeck()
    {
        CreateFullStack();
    }

    private void UpdateDrawSpawnPoint()
    {
        if (_drawSpawnPoint == null || _visualStackRoot == null)
        {
            return;
        }

        //work out how tall the current stack is
        float stackHeight = _visualCards.Count * _cardHeight;

        // Keep the draw point just above the top card
        _drawSpawnPoint.position = _visualStackRoot.position - (_visualStackRoot.forward * stackHeight);
    }
}