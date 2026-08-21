using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardShuffleAnimator : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private TableManager _tableManager;
    [SerializeField] private DrawDeckVisual _drawDeckVisual;
    [SerializeField] private Transform _shuffleAreaCentre;

    [Header("Spread Settings")]
    [SerializeField] private float _spreadDistance = 0.3f;
    [SerializeField] private float _spreadDuration = 0.4f;

    [Header("Shuffle Settings")]
    [SerializeField] private int _shufflePasses = 3;
    [SerializeField] private float _shuffleMoveDuration = 0.25f;
    [SerializeField] private float _shuffleWidth = 0.35f;
    [SerializeField] private float _shuffleDepth = 0.2f;
    [SerializeField] private float _shuffleLift = 0.08f;

    [Header("Return To Deck Settings")]
    [SerializeField] private float _returnMoveDuration = 0.35f;
    [SerializeField] private float _timeBetweenCards = 0.08f;
    [SerializeField] private float _returnLift = 0.25f;

    //how far cards can move forwards/backwards
    [SerializeField] private float _depthVariation = 0.08f;

    //maximum random rotation of each card
    [SerializeField] private float _rotationVariation = 12f;

    public IEnumerator SpreadDiscardCards()
    {
        // safety check
        if (_tableManager == null)
        {
            yield break;
        }

        IReadOnlyList<GameObject> discardCards = _tableManager.SharedDiscardCards;

        if (discardCards.Count == 0)
        {
            yield break;
        }

        Transform discardPoint = _tableManager.SharedDiscardPoint;

        if (discardPoint == null)
        {
            yield break;
        }

        List<Vector3> startPositions = new List<Vector3>();
        List<Vector3> targetPositions = new List<Vector3>();

        List<Quaternion> startRotations = new List<Quaternion>();
        List<Quaternion> targetRotations = new List<Quaternion>();

        // work out where every card will move
        for (int i = 0; i < discardCards.Count; i++)
        {
            GameObject card = discardCards[i];

            if (card == null)
            {
                startPositions.Add(Vector3.zero);
                targetPositions.Add(Vector3.zero);

                startRotations.Add(Quaternion.identity);
                targetRotations.Add(Quaternion.identity);

                continue;
            }

            startPositions.Add(card.transform.position);
            startRotations.Add(card.transform.rotation);

            //apread cards left and right
            float sideOffset = (i - (discardCards.Count - 1) / 2f) * _spreadDistance;

            //give every card a random forward/backwards position
            float depthOffset = Random.Range(-_depthVariation, _depthVariation);

            Vector3 targetPosition = discardPoint.position + (discardPoint.right * sideOffset) + (discardPoint.forward * depthOffset);

            // keep them above the table
            targetPosition += discardPoint.up * 0.02f;

            targetPositions.Add(targetPosition);

            //give cards a different angle
            float randomAngle = Random.Range(-_rotationVariation, _rotationVariation);

            Quaternion targetRotation = Quaternion.AngleAxis(randomAngle, discardPoint.up) * discardPoint.rotation;

            targetRotations.Add(targetRotation);
        }

        float elapsed = 0f;

        //animate into their new positions
        while (elapsed < _spreadDuration)
        {
            elapsed += Time.deltaTime;

            float percent = Mathf.Clamp01(elapsed / _spreadDuration);

            float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

            for (int i = 0; i < discardCards.Count; i++)
            {
                GameObject card = discardCards[i];

                if (card == null)
                {
                    continue;
                }

                card.transform.position = Vector3.Lerp(startPositions[i], targetPositions[i], smoothPercent);

                card.transform.rotation = Quaternion.Slerp(startRotations[i], targetRotations[i], smoothPercent);
            }

            yield return null;
        }

        // make sure everything finishes in riught place
        for (int i = 0; i < discardCards.Count; i++)
        {
            GameObject card = discardCards[i];

            if (card == null)
            {
                continue;
            }

            card.transform.position = targetPositions[i];
            card.transform.rotation = targetRotations[i];
        }
    }

    public IEnumerator ShuffleDiscardCards()
    {
        if (_tableManager == null)
        {
            yield break;
        }

        IReadOnlyList<GameObject> discardCards = _tableManager.SharedDiscardCards;

        if (discardCards.Count == 0)
        {
            yield break;
        }

        Transform discardPoint = _tableManager.SharedDiscardPoint;

        if (discardPoint == null)
        {
            yield break;
        }

        if (_shuffleAreaCentre == null)
        {
            yield break;
        }

        // repeat the shuffle movement 
        for (int pass = 0; pass < _shufflePasses; pass++)
        {
            List<Vector3> startPositions = new List<Vector3>();
            List<Vector3> targetPositions = new List<Vector3>();

            List<Quaternion> startRotations = new List<Quaternion>();
            List<Quaternion> targetRotations = new List<Quaternion>();

            for (int i = 0; i < discardCards.Count; i++)
            {
                GameObject card = discardCards[i];

                if (card == null)
                {
                    startPositions.Add(Vector3.zero);
                    targetPositions.Add(Vector3.zero);

                    startRotations.Add(Quaternion.identity);
                    targetRotations.Add(Quaternion.identity);

                    continue;
                }

                startPositions.Add(card.transform.position);
                startRotations.Add(card.transform.rotation);

                // pick new random place around the middle of the table
                float sideOffset = Random.Range(-_shuffleWidth, _shuffleWidth);

                float depthOffset = Random.Range(-_shuffleDepth, _shuffleDepth);

                Vector3 targetPosition = _shuffleAreaCentre.position + (_shuffleAreaCentre.right * sideOffset) + (_shuffleAreaCentre.forward * depthOffset);

                //keep card above the table
                targetPosition += _shuffleAreaCentre.up * 0.02f;

                targetPositions.Add(targetPosition);

                //give each card a different rotation
                float randomAngle = Random.Range(-25f, 25f);

                Quaternion targetRotation = Quaternion.AngleAxis(randomAngle, discardPoint.up) * discardPoint.rotation;

                targetRotations.Add(targetRotation);
            }

            float elapsed = 0f;

            while (elapsed < _shuffleMoveDuration)
            {
                elapsed += Time.deltaTime;

                float percent = Mathf.Clamp01(elapsed / _shuffleMoveDuration);

                float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

                for (int i = 0; i < discardCards.Count; i++)
                {
                    GameObject card = discardCards[i];

                    if (card == null)
                    {
                        continue;
                    }

                    Vector3 position = Vector3.Lerp(startPositions[i], targetPositions[i], smoothPercent);

                    // make the card rise slightly during the movement
                    float lift = Mathf.Sin(percent * Mathf.PI) * _shuffleLift;

                    position += discardPoint.up * lift;

                    card.transform.position = position;

                    card.transform.rotation = Quaternion.Slerp( startRotations[i], targetRotations[i], smoothPercent);
                }

                yield return null;
            }

            // make sure every card finishes 
            for (int i = 0; i < discardCards.Count; i++)
            {
                GameObject card = discardCards[i];

                if (card == null)
                {
                    continue;
                }

                card.transform.position = targetPositions[i];
                card.transform.rotation = targetRotations[i];
            }
        }
    }

    public IEnumerator ReturnCardsToDrawDeck()
    {
        if (_tableManager == null || _drawDeckVisual == null)
        {
            yield break;
        }

        Transform drawSpawnPoint = _tableManager.DrawDeckSpawnPoint;

        if (drawSpawnPoint == null)
        {
            yield break;
        }

        // make our own copy because real discard list is cleared when animation finishes
        List<GameObject> cardsToReturn = new List<GameObject>(_tableManager.SharedDiscardCards);

        for (int i = 0; i < cardsToReturn.Count; i++)
        {
            GameObject card = cardsToReturn[i];

            if (card == null)
            {
                continue;
            }

            // remove it from the discard parent so it can move freely
            card.transform.SetParent(null, true);

            Vector3 startPosition = card.transform.position;
            Quaternion startRotation = card.transform.rotation;

            // updates each time because the visual deck gets taller after every returned card
            Vector3 endPosition = drawSpawnPoint.position;

            //turn the card face down as it returns to the deck
            Quaternion endRotation = drawSpawnPoint.rotation * Quaternion.Euler(0f, 0f, 180f);

            float elapsed = 0f;

            while (elapsed < _returnMoveDuration)
            {
                elapsed += Time.deltaTime;

                float percent = Mathf.Clamp01(elapsed / _returnMoveDuration);

                float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

                Vector3 position = Vector3.Lerp(startPosition, endPosition, smoothPercent);

                // give card curved arc through the air
                float lift = Mathf.Sin(percent * Mathf.PI) * _returnLift;

                position += Vector3.up * lift;

                card.transform.position = position;

                card.transform.rotation = Quaternion.Slerp( startRotation, endRotation, smoothPercent);

                yield return null;
            }

            card.transform.position = endPosition;
            card.transform.rotation = endRotation;

            // add new layer to fake visual DrawDeck
            _drawDeckVisual.AddVisualCard();

            //remove real table card
            Destroy(card);

            //delay makes them land one by one
            yield return new WaitForSeconds(_timeBetweenCards);
        }

        // The real discard cards have now all been converted into visual DrawDeck
        _tableManager.ClearSharedDiscardReferences();
    }

    public IEnumerator PlayReshuffleAnimation()
    {
        // apread pile across table
        yield return StartCoroutine(SpreadDiscardCards());

        yield return new WaitForSeconds(0.2f);

        //mix cards around
        yield return StartCoroutine(ShuffleDiscardCards());

        yield return new WaitForSeconds(0.15f);

        // return them to the DrawDeck one by one
        yield return StartCoroutine(ReturnCardsToDrawDeck());
    }
}