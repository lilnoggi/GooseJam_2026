
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("UI References")]
    [SerializeField] private PlayerDecisionMenu _playerDecisionMenu;

    private bool _isWaitingForPlayerDecision;
    private bool _playerCalledCheat;

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
        EnemyAI targetAI = targetStats.GetComponent<EnemyAI>();

        // Convert claimed Enum rank into an integer value for the AI
        int claimedValue = CombatLogic.GetCardValue(claim.ClaimedRank);

        // Ask AI if they think player is lying
        bool isChallenging = targetAI.DecideToChallenge(claim.ClaimedSuit, claimedValue);

        if (isChallenging)
        {
            Debug.Log($"[Standoff] The {claim.TargetEnemy} called CHEAT!");

            // Trigger standoof logic
            ResolveChallenge(claim, targetStats);
        }
        else
        {
            Debug.Log($"[Standoff] The {claim.TargetEnemy} accepted the player's claim.");

            // Did the player successfully lie?
            bool isLie = false;
            foreach (CardData card in claim.TrueCards)
            {
                if (card.Suit != claim.ClaimedSuit || card.Rank != claim.ClaimedRank)
                {
                    isLie = true;
                }
            }

            // If the player got away with a lie, the enemy gets more paranoid
            if (isLie)
            {
                Debug.Log("Player successfully bluffed! Enemy paranoia increases.");
                targetStats.IncreaseParanoia(25);
            }

            // The AI believed the player, apply the cards actually played
            CombatLogic.ProcessTurn(claim.TrueCards, _playerStats, targetStats);
        }

        // End the player's turn
        AdvanceTurn();
    }

    private void ResolveChallenge(ClaimData claim, CharacterStats claimer, CharacterStats challenger)
    {
        // Check if the claimer lied. A lie means ANY card doesn't match the claimed suit or rank
        bool isLie = false;
        foreach (CardData card in claim.TrueCards)
        {
            if (card.Suit != claim.ClaimedSuit || card.Rank != claim.ClaimedRank)
            {
                isLie = true;
                break; // Caught, no need to check the rest of the cards
            }
        }

        if (isLie)
        {
            Debug.Log("{claimer.name.ToUpper()} IN A LIE!!! {claimer.name} takes penalty.");

            // The liar takes thier own claimed damage
            int claimedDamage = claim.TrueCards.Count * CombatLogic.GetCardValue(claim.ClaimedRank);
            claimer.TakeDamage(claimedDamage);

            // Enemy liar paranoia drops because they failed their bluff
            claimer.IncreaseParanoia(-20);
        }
        else
        {
            Debug.Log("{claimer.name.ToUpper()} TOLD THE TRUTH!!! {challenger.name} takes critical penalty");

            // Calculate the true value of the cards
            int trueDamage = 0;
            foreach (CardData card in claim.TrueCards)
            {
                trueDamage += CombatLogic.GetCardValue(card.Rank);
            }

            // Challenger takes double the tru damage
            challenger.TakeDamage(trueDamage * 2);

            // (Paranoia doesn't drop here because enemy was right to be scared)
        }
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
        CharacterStats enemyStats = GetStatsForTurn(enemyTurn);

        if (enemyDeck.HandCount > 0)
        {
            // Enemy chooses their cards
            int maxCards = Mathf.Min(3, enemyDeck.HandCount);
            int cardsToPlayCount = UnityEngine.Random.Range(1, maxCards + 1);

            // Get the actual CardData objects before discarding them
            List<CardData> trueCards = new List<CardData>();
            for (int i = 0; i < cardsToPlayCount; i++)
            {
                trueCards.Add(enemyDeck.Hand[i]);
            }
            
            enemyDeck.DiscardCards(trueCards);

            // The enemy uses their AI profile to formulate a claim
            EnemyAI activeEnemyAI = enemyStats.GetComponent<EnemyAI>();
            ClaimData enemyClaim = activeEnemyAI.FormulateClaim(trueCards);

            // Pause the turn and show the UI to the player
            _isWaitingForPlayerDecision = true;

            _playerDecisionMenu.ShowMenu(enemyTurn.ToString(), enemyClaim, (bool calledCheat) =>
            {
                _playerCalledCheat = calledCheat;
                _isWaitingForPlayerDecision = false; // Unfreeze the Coroutine
            });

            // Coroutine stops here until the player decision has been made
            yield return new WaitUntil(() => !_isWaitingForPlayerDecision);

            // Resolve the Standoff based on the player's button click
            if (_playerCalledCheat)
            {
                Debug.Log("PLAYER CALLED CHEAT ON THE ENEMY!");

                // Reverse the stats as the enemy is the one making a claim
                ResolveChallenge(enemyClaim, enemyStats);
            }
            else
            {
                Debug.Log("Player passed. Enemy claim accepted.");
                CombatLogic.ProcessTurn(enemyClaim.TrueCards, enemyStats, _playerStats);
            }
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


