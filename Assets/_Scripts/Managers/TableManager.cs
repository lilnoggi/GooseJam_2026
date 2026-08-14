using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    // Singleton instance for TurnManager
    public static TableManager Instance { get; private set; }

    [Header("Table Reveal Setup")]
    [SerializeField] private GameObject _tableCardPrefab;
    [SerializeField] private Transform _tableCenterTransform;
    [SerializeField] private float _cardSpacing = 1.2f;
    [SerializeField] private float _flipDuration = 0.5f;

    private List<GameObject> _spawnedTableCards = new List<GameObject>();

    private void Awake()
    {
        // Singleton Setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Spawns cards face-down, waits, flips them face-up, and waits
    /// </summary>
    public IEnumerator SpawnAndFlipTableCards(List<CardData> cardsToReveal)
    {
        // Wipe any old cards
        ClearTableCards();

        int cardCount = cardsToReveal.Count;

        // Spawn cards face down
        for (int i = 0; i < cardCount; i++)
        {
            GameObject newCard = Instantiate(_tableCardPrefab, _tableCenterTransform);

            // Calculate spacing so the group of cards centers perfectly on the table
            float offset = (i - (cardCount - 1) / 2f) * _cardSpacing;
            newCard.transform.localPosition = new Vector3(offset, 0, 0);

            // Rotate exactly 180 degrees on the Z axis so the back of the card faces up
            newCard.transform.localRotation = Quaternion.Euler(0, 0, 180f);

            // TODO: Link up CardVisuals so the card knows what suit/rank to display

            _spawnedTableCards.Add(newCard);
        }

        // Wait half a second 
        yield return new WaitForSeconds(0.5f);

        // Animate the flip
        float elapsed = 0f;

        while (elapsed < _flipDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / _flipDuration;

            // SmoothStep makes the flip start slow, speed up, then slow down
            float smoothPercent = Mathf.SmoothStep(0, 1, percent);
            float currentZRotation = Mathf.Lerp(180f, 0f, smoothPercent);

            foreach (var card in _spawnedTableCards)
            {
                if (card != null)
                {
                    card.transform.localRotation = Quaternion.Euler(0, 0, currentZRotation);
                }
            }

            // Wait for next frame
            yield return null; 
        }

        // Snap to exactly 0 rotation just in case anything was off
        foreach (var card in _spawnedTableCards)
        {
            if (card != null)
            {
                card.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }

        // Let the player look at the flipped cards for a second before the combat logic executes
        yield return new WaitForSeconds(1.0f);
    }

    /// <summary>
    /// Destroys the physical 3D cards curently on the tbale
    /// </summary>
    public void ClearTableCards()
    {
        foreach (var card in _spawnedTableCards)
        {
            if (card != null)
            {
                Destroy(card);
            }
        }

        _spawnedTableCards.Clear();
    }
}
