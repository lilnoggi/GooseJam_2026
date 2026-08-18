using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class IntuitionUI : MonoBehaviour
{
    public static IntuitionUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject _panelRoot;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private RectTransform _cardContainer;

    [Header("Prefabs")]
    [SerializeField] private PlayerCardView _cardPrefab;

    [Header("Settings")]
    [SerializeField] private float _dispayDuration = 3.5f;

    private List<PlayerCardView> _spawnedCards = new List<PlayerCardView>();

    private void Awake()
    {
        // Singleton Setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _panelRoot.SetActive(false);
    }

    public void ShowEnemyHand(CharacterStats targetEnemy, DeckManager enemyDeck)
    {
        // Clear old cards
        foreach (var card in _spawnedCards)
        {
            Destroy(card.gameObject);
        }

        _spawnedCards.Clear();

        _titleText.text = $"{targetEnemy.name.ToUpper()}'S HAND";

        // Spwan the enemy's hidden cards
        foreach (CardData cardData in enemyDeck.Hand)
        {
            PlayerCardView newCard = Instantiate(_cardPrefab, _cardContainer);

            // Setup the card, but pass an empty action so it can't be clicked
            newCard.Setup(cardData, (c) => { });
            newCard.SetInteractable(false);

            _spawnedCards.Add(newCard);
        }

        // Show the panel
        _panelRoot.SetActive(true);

        // Start auo-close timer 
        StartCoroutine(AutoCloseRoutine());
    }

    public IEnumerator AutoCloseRoutine()
    {
        yield return new WaitForSeconds(_dispayDuration);
        _panelRoot.SetActive(false);
    }
}
