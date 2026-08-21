using System.Collections;
using UnityEngine;

public class EnemyDrawAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TableManager _tableManager;
    [SerializeField] private GameObject _cardPrefab;

    [Header("Enemy Draw Points")]
    [SerializeField] private Transform _leftDrawPoint;
    [SerializeField] private Transform _centreDrawPoint;
    [SerializeField] private Transform _rightDrawPoint;

    [Header("Animation Settings")]
    [SerializeField] private float _moveDuration = 0.4f;
    [SerializeField] private float _drawLift = 0.25f;
    [SerializeField] private float _timeBetweenCards = 0.1f;

    public IEnumerator DrawCards(TurnSeat enemySeat, DeckManager enemyDeck, int amount)
    {
        if (_tableManager == null || enemyDeck == null)
        {
            yield break;
        }

        Transform drawPoint = GetEnemyDrawPoint(enemySeat);
        Transform deckPoint = _tableManager.DrawDeckSpawnPoint;

        if (drawPoint == null || deckPoint == null)
        {
            yield break;
        }

        for (int i = 0; i < amount; i++)
        {
            // creates temporary card on the DrawDeck
            GameObject flyingCard = Instantiate(_cardPrefab, deckPoint.position, deckPoint.rotation * Quaternion.Euler(0f, 0f, 180f));

            Vector3 startPosition = flyingCard.transform.position;
            Quaternion startRotation = flyingCard.transform.rotation;

            Vector3 endPosition = drawPoint.position;
            Quaternion endRotation = drawPoint.rotation;

            float elapsed = 0f;

            // float card towards the enemy
            while (elapsed < _moveDuration)
            {
                elapsed += Time.deltaTime;

                float percent = Mathf.Clamp01(elapsed / _moveDuration);

                float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

                Vector3 position = Vector3.Lerp(startPosition, endPosition, smoothPercent);

                //arc in air
                float lift = Mathf.Sin(percent * Mathf.PI) * _drawLift;

                position += Vector3.up * lift;

                flyingCard.transform.position = position;

                flyingCard.transform.rotation = Quaternion.Slerp(startRotation, endRotation, smoothPercent);

                yield return null;
            }

            //temporary flying card has reached the enemy
            Destroy(flyingCard);

            //give enemy card
            enemyDeck.DrawAmount(1);

            yield return new WaitForSeconds(_timeBetweenCards);
        }
    }

    private Transform GetEnemyDrawPoint(TurnSeat enemySeat)
    {
        switch (enemySeat)
        {
            case TurnSeat.LeftEnemy:
            return _leftDrawPoint;

            case TurnSeat.CentreEnemy:
            return _centreDrawPoint;

            case TurnSeat.RightEnemy:
            return _rightDrawPoint;
        }

        return null;
    }

    public IEnumerator DrawToFullHand(TurnSeat enemySeat, DeckManager enemyDeck)
    {
        if (enemyDeck == null)
        {
            yield break;
        }

        //enemies should have 5 cards
        int cardsNeeded = 5 - enemyDeck.Hand.Count;

        if (cardsNeeded <= 0)
        {
            yield break;
        }

        yield return StartCoroutine(
            DrawCards(enemySeat, enemyDeck, cardsNeeded)
        );
    }
}