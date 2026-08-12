
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public enum TurnSeat
{
    Player,
    LeftEnemy,
    CentreEnemy,
    RightEnemy
}
public class TurnManager : MonoBehaviour
{
    [SerializeField] private DeckManager playerDeck; //the players deck
    [SerializeField] private DeckManager leftEnemyDeck; //the player on the lefts deck
    [SerializeField] private DeckManager centreEnemyDeck; //the player on the centir deck
    [SerializeField] private DeckManager rightEnemyDeck; //the player on the right deck

    [SerializeField] private float thinkTime = 0.75f; //amount of time that the other players will think before playing a card

    [Header("Character Stats References")]
    [SerializeField] private CharacterStats _playerStats;
    [SerializeField] private CharacterStats _leftEnemyStats;
    [SerializeField] private CharacterStats _centerEnemyStats;
    [SerializeField] private CharacterStats _rightEnemyStats;

    private TurnSeat currentTurn;
    private bool hasStarted;

    public TurnSeat CurrentTurn => currentTurn;
    public bool IsPlayerTurn => hasStarted && currentTurn == TurnSeat.Player;

    public event Action<TurnSeat> OnTurnChanged;

    private void Start()
    {
        //  basic safety check on startup to avoid null refs
        if (playerDeck == null || leftEnemyDeck == null || centreEnemyDeck == null || rightEnemyDeck == null)
        {
            Debug.LogError("TurnManager is missing one or more DeckManager references!");
            enabled = false;
            return;
        }


        InitDecks();
        StartTurn(TurnSeat.Player); // player goes first always
    }

    public void PlayPlayerCards(IReadOnlyList<CardData> cards)
    {
        if (!IsPlayerTurn) return;

        //validation for card count selection (must be 1 to 3 cards)
        if (cards == null || cards.Count < 1 || cards.Count > 3) return;

        playerDeck.DiscardCards(cards);
        AdvanceTurn();
    }

    public void SkipPlayerTurn()
    {
        if (!IsPlayerTurn) return;
        AdvanceTurn(); // just move to next person

    }

    public void ProcessPlayerClaim(ClaimData claim)
    {
        if (!IsPlayerTurn)
        {
            return;
        }

        // Discard the true cards from the player's hand so they leave the screen
        playerDeck.DiscardCards(claim.TrueCards);

        // Grab the target's stats
        CharacterStats targetStats = GetStatsForTurn(claim.TargetEnemy);

        // TODO: Pass this claim to EnemyAI so they can decide to call "Cheat"
        // For now, accept the claim
        Debug.Log($"Enemy accepted the claim! Processing combat...");

        // End the player's turn
        AdvanceTurn();
    }

    private void InitDecks()
    {
        DeckManager[] decks = { playerDeck, leftEnemyDeck, centreEnemyDeck, rightEnemyDeck };
        
        foreach (var deck in decks)
        {
            deck.InitialiseDeck();
            deck.DrawToFullHand(); // everyone gets 5 cards to start out
        }
    }

    private void StartTurn(TurnSeat turn)
    {
        hasStarted = true;
        currentTurn = turn;

        // Get the active character's stats and process turn-start effects
        CharacterStats activeStats = GetStatsForTurn(turn);
        if (activeStats != null)
        {
            activeStats.ProcessTurnStartStatusEffects();
        }

        // Draw cards for the active deck
        DeckManager activeDeck = GetDeckForTurn(turn);

        activeDeck.DrawToFullHand();

        OnTurnChanged?.Invoke(turn);

        if (turn != TurnSeat.Player)
        {
            StartCoroutine(EnemyTurnRoutine(turn));
        }
    }

    private IEnumerator EnemyTurnRoutine(TurnSeat enemyTurn)
    {
        yield return new WaitForSeconds(thinkTime);

        // safety check incase turn changed during the delay
        if (currentTurn != enemyTurn) yield break;

        DeckManager enemyDeck = GetDeckForTurn(enemyTurn);


        if (enemyDeck.HandCount > 0)
        {
            int maxCards = Mathf.Min(3, enemyDeck.HandCount);
            int cardsToPlay = UnityEngine.Random.Range(1, maxCards + 1);
            
            enemyDeck.DiscardRandomCards(cardsToPlay);
        }

        yield return new WaitForSeconds(thinkTime);

        if (currentTurn == enemyTurn)
        {
            AdvanceTurn();
        }
    }

    private void AdvanceTurn()
    {
        // cycles clockwise through 0-3 layout indices

        int nextTurn = ((int)currentTurn + 1) % 4;
        StartTurn((TurnSeat)nextTurn);
    }

    private DeckManager GetDeckForTurn(TurnSeat turn)
    {
        switch (turn)
        {
            case TurnSeat.Player:      return playerDeck;

            case TurnSeat.LeftEnemy:   return leftEnemyDeck;

            case TurnSeat.CentreEnemy: return centreEnemyDeck;

            case TurnSeat.RightEnemy:  return rightEnemyDeck;

            default:                   return playerDeck;
        }
    }

    /// <summary>
    /// Gets the CharacterStats component associated with the specified turn seat
    /// </summary>
    private CharacterStats GetStatsForTurn(TurnSeat turn)
    {
        switch (turn)
        {
            case TurnSeat.Player:
                return _playerStats;
            case TurnSeat.LeftEnemy:
                return _leftEnemyStats;
            case TurnSeat.CentreEnemy:
                return _centerEnemyStats;
            case TurnSeat.RightEnemy:
                return _rightEnemyStats;
            default:
                return null; // Fallback return value
        }
    }
}


