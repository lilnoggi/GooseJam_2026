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

    [Header("Card Play Animation")]
    [SerializeField] private Transform _playerPlayAnchor;
    [SerializeField] private Transform _leftEnemyPlayAnchor;
    [SerializeField] private Transform _centreEnemyPlayAnchor;
    [SerializeField] private Transform _rightEnemyPlayAnchor;

    [Header("Player Draw Animation")]
    [SerializeField] private Transform _drawPileAnchor;
    [SerializeField] private Transform _playerDrawArrivalAnchor;
    [SerializeField] private float _drawMoveDuration = 0.45f;
    [SerializeField] private float _drawArcHeight = 0.25f;

    [SerializeField] private float _cardPlayDuration = 0.5f;
    [SerializeField] private float _cardArcHeight = 0.3f;
    [SerializeField] private float _cardStaggerDelay = 0.08f;
    [SerializeField] private float _cardTravelTilt = 12f;

    [SerializeField] private Light _revealSpotlight;
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

        // Ensure spotlight is off when scene starts
        if (_revealSpotlight != null)
        {
            _revealSpotlight.enabled = false;
        }
    }



    public IEnumerator PlayCardsToTable(List<CardData> cards, TurnSeat fromSeat)
    {
        ClearTableCards();

        Transform startAnchor = GetPlayAnchor(fromSeat);

        if (startAnchor == null)
        {
            Debug.LogWarning($"No card play anchor assigned for {fromSeat}");
            yield break;
        }

        int cardCount = cards.Count;

        List<Vector3> startPositions = new List<Vector3>();
        List<Vector3> endPositions = new List<Vector3>();
        List<Quaternion> startRotations = new List<Quaternion>();
        List<Quaternion> endRotations = new List<Quaternion>();

        for (int i = 0; i < cardCount; i++)
        {
            GameObject newCard = Instantiate(_tableCardPrefab, _tableCenterTransform);

            newCard.transform.localScale = _tableCardPrefab.transform.localScale;

            // Spread the cards slightly where they start
            float startOffset = (i - (cardCount - 1) / 2f) * (_cardSpacing * 0.25f);

            Vector3 startPosition = startAnchor.position + startAnchor.right * startOffset;

            // Work out the final centred position
            float endOffset = (i - (cardCount - 1) / 2f) * _cardSpacing;

            Vector3 endPosition = _tableCenterTransform.TransformPoint(new Vector3(endOffset, 0f, 0f));

            newCard.transform.position = startPosition;

            float tiltDirection = i % 2 == 0 ? -_cardTravelTilt : _cardTravelTilt;

            Quaternion startRotation = Quaternion.Euler(0f, tiltDirection, 180f);
            Quaternion endRotation = Quaternion.Euler(0f, 0f, 180f);

            newCard.transform.localRotation = startRotation;

            CardVisuals visuals = newCard.GetComponent<CardVisuals>();

            if (visuals != null)
            {
                visuals.Setup(cards[i]);
            }

            _spawnedTableCards.Add(newCard);

            startPositions.Add(startPosition);
            endPositions.Add(endPosition);
            startRotations.Add(startRotation);
            endRotations.Add(endRotation);
        }

        float elapsed = 0f;

        float totalDuration = _cardPlayDuration + ( _cardStaggerDelay * ( _spawnedTableCards.Count - 1));

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;

            for (int i = 0; i < _spawnedTableCards.Count; i++)
            {
                // each card will starts slightly after the previous one
                float cardElapsed = elapsed - (i * _cardStaggerDelay);

                if (cardElapsed < 0f)
                {
                    continue;
                }

                float percent = Mathf.Clamp01(cardElapsed / _cardPlayDuration);

                float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

                // normal movement from player/enemy towards table
                Vector3 position = Vector3.Lerp(startPositions[i],endPositions[i],smoothPercent);

                // add a curved arc during the cards movement
                float arcAmount =Mathf.Sin(percent * Mathf.PI) * _cardArcHeight;

                position += Vector3.up * arcAmount;

                _spawnedTableCards[i].transform.position = position;

                //slowly straighten the card as it lands
                _spawnedTableCards[i].transform.localRotation =Quaternion.Slerp(startRotations[i],endRotations[i],smoothPercent);
            }

            yield return null;
        }

        // Make sure every card finishes perfectly in place
        for (int i = 0; i < _spawnedTableCards.Count; i++)
        {
            _spawnedTableCards[i].transform.position = endPositions[i];
            _spawnedTableCards[i].transform.localRotation = endRotations[i];
        }
    }

    private Transform GetPlayAnchor(TurnSeat seat)
    {
        switch (seat)
        {
            case TurnSeat.Player:
                return _playerPlayAnchor;

            case TurnSeat.LeftEnemy:
                return _leftEnemyPlayAnchor;

            case TurnSeat.CentreEnemy:
                return _centreEnemyPlayAnchor;

            case TurnSeat.RightEnemy:
                return _rightEnemyPlayAnchor;

            default:
                return null;
        }
    }

    /// <summary>
    /// Spawns cards face-down, waits, flips them face-up, and waits
    /// </summary>
    public IEnumerator FlipTableCards()
    {

        if (_spawnedTableCards.Count == 0)
        {
            yield break;
        }

        // Turn on the spotlight
        if (_revealSpotlight != null)
        {
            _revealSpotlight.enabled = true;
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

    public IEnumerator AnimateCardDrawToPlayer()
    {
        if (_drawPileAnchor == null || _playerDrawArrivalAnchor == null) //make sure both points are assigned
        {
            yield break;
        }

        GameObject drawCard = Instantiate(_tableCardPrefab); //create card for transition

        //where the card starts and ends
        Vector3 startPosition = _drawPileAnchor.position;
        Vector3 endPosition = _playerDrawArrivalAnchor.position;

        //copy rotation sothat they are upsidedown
        Quaternion startRotation = _drawPileAnchor.rotation;
        Quaternion endRotation = _playerDrawArrivalAnchor.rotation;

        //put card at draw pile
        drawCard.transform.position = startPosition;
        drawCard.transform.rotation = startRotation;
        drawCard.transform.localScale = _tableCardPrefab.transform.localScale;

        float elapsed = 0f;

        while (elapsed < _drawMoveDuration)//move card to player
        {
            elapsed += Time.deltaTime;

            float percent = Mathf.Clamp01(elapsed / _drawMoveDuration);

            //make movement smooth and not linear
            float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

            Vector3 position =Vector3.Lerp( startPosition, endPosition, smoothPercent);//card from draw pile to playwr

            float arc = Mathf.Sin(percent * Mathf.PI) * _drawArcHeight; //move in arch

            position += Vector3.up * arc;

            drawCard.transform.position = position;

            drawCard.transform.rotation =Quaternion.Slerp(startRotation,endRotation,smoothPercent); //rotate card to player side

            yield return null;
        }

        Destroy(drawCard); //destroys 3D card
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

        // Turn off the spotlight
        if (_revealSpotlight != null)
        {
            _revealSpotlight.enabled = false;
        }
    }
}
