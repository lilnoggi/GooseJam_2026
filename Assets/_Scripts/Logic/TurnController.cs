using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Core loop manager. Handles Turn Order, Win/Loss states, and directs actions.
/// </summary>
public class TurnController : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private StandoffResolver _standoffResolver;
    [SerializeField] private SessionData _sessionData;
    [SerializeField] private PlayerDecisionMenu _playerDecisionMenu;

    [Header("Enemy Drawing")]
    [SerializeField] private EnemyDrawAnimator _enemyDrawAnimator;

    [Header("Deck Managers")]
    [SerializeField] private DeckManager playerDeck;
    [SerializeField] private DeckManager leftEnemyDeck;
    [SerializeField] private DeckManager centreEnemyDeck;
    [SerializeField] private DeckManager rightEnemyDeck;

    [Header("Character Stats")]
    [SerializeField] private CharacterStats _playerStats;
    [SerializeField] private CharacterStats _leftEnemyStats;
    [SerializeField] private CharacterStats _centerEnemyStats;
    [SerializeField] private CharacterStats _rightEnemyStats;

    [Header("Cinematic Timings")]
    [Tooltip("How long enemies 'think' before making a claim.")]
    [SerializeField] private float _aiThinkTime = 1.5f;
    [Tooltip("How long to wait at the start of a turn to let players read the banner.")]
    [SerializeField] private float _turnStartDelay = 1.5f;
    [Tooltip("How long to wait at the end of an enemy turn before moving to the next player.")]
    [SerializeField] private float _turnEndDelay = 1.0f;
    [Tooltip("How long to pause and let the player observe an Action Card's effect.")]
    [SerializeField] private float _actionCardObserveTime = 2.0f;

    [Header("Round Reshuffle")]
    [SerializeField] private DiscardShuffleAnimator _discardShuffleAnimator;

    [Header("Player Drawing")]
    [SerializeField] private PlayerHandManager _playerHandManager;

    // State trackers
    private TurnSeat _currentTurn;
    private bool _hasStarted;
    private bool _isWaitingForPlayerDecision;
    private bool _playerCalledCheat;

    public TurnSeat CurrentTurn => _currentTurn;
    public bool IsPlayerTurn => _hasStarted && _currentTurn == TurnSeat.Player;

    public event Action<TurnSeat> OnTurnChanged;

    // ----------------------------------------------------------------------------------------------- 

    private void Start()
    {
        if (playerDeck == null || leftEnemyDeck == null || centreEnemyDeck == null || rightEnemyDeck == null)
        {
            Debug.LogError($"{name} is missing one or more DeckManager references!");
            enabled = false;
            return;
        }

        StartCoroutine(StartGameRoutine()); 
    }

    /// <summary>
    /// Starts the cinematic sequence for playing a face-up Action Status Card (e.g., Loaded Dice, Intuition).
    /// </summary>
    /// <param name="actionCard">The specific action card being played.</param>
    /// <param name="targetSeat">The seat of the enemy being targeted by the action card.</param>
    public void PlayActionCard(CardData actionCard, TurnSeat targetSeat)
    {
        StartCoroutine(PlayActionCardRoutine(actionCard, targetSeat));
    }

    /// <summary>
    /// Instantly ends the player's turn and passes control to the next seat.
    /// </summary>
    public void SkipPlayerTurn()
    {
        if (!IsPlayerTurn) return;
        AdvanceTurn(); 
    }

    /// <summary>
    /// Routes the player's selected bluff/truth claim to the StandoffResolver for cinematic execution.
    /// </summary>
    /// <param name="claim">The formulated claim data from the UI.</param>
    public void ProcessPlayerClaim(ClaimData claim)
    {
        if (!IsPlayerTurn) return;
        
        StartCoroutine(_standoffResolver.PlayerClaimRoutine(claim));
    }

    /// <summary>
    /// Bypasses standard standoff mechanics to instantly trigger an Action card's effect on the table.
    /// </summary>
    private IEnumerator PlayActionCardRoutine(CardData actionCard, TurnSeat targetSeat)
    {
        playerDeck.DiscardCards(new List<CardData> { actionCard });

        // Visually place the spell on the table for all to see
        yield return CameraController.Instance.SwoopToTable();
        yield return StartCoroutine(TableManager.Instance.PlayCardsToTable(new List<CardData> { actionCard }, TurnSeat.Player));
        yield return StartCoroutine(TableManager.Instance.FlipTableCards());

        // Execute the effect directly onto the target
        CharacterStats targetStats = GetStatsForTurn(targetSeat);
        actionCard.EffectLogic.OnPlay(_playerStats, targetStats, playerDeck);

        yield return new WaitForSeconds(_actionCardObserveTime);

        // Cleanup and immediately end turn
        TableManager.Instance.ClearTableCards();
        yield return CameraController.Instance.SwoopToDefault();

        AdvanceTurn();
    }

    /// <summary>
    /// Clears and repopulates all 4 decks at the start of a match.
    /// </summary>
    private void InitDecks()
    {
        // Create all four logical decks
        playerDeck.InitialiseDeck();
        leftEnemyDeck.InitialiseDeck();
        centreEnemyDeck.InitialiseDeck();
        rightEnemyDeck.InitialiseDeck();

        // The player keeps their existing starting-hand system
        playerDeck.DrawToFullHand();

        // Start the physical DrawDeck looking full
        // before we animate the enemies taking their cards
        if (DrawDeckVisual.Instance != null)
        {
            DrawDeckVisual.Instance.RefillVisualDeck();
        }
    }

    /// <summary>
    /// Initialises a character's turn by ticking their status effects and drawing their cards.
    /// </summary>
    /// <param name="turn">The seat index whose turn is beginning.</param>
    private void StartTurn(TurnSeat turn)
    {
        _hasStarted = true;
        _currentTurn = turn;

        // Process status effects (Poison, Shields, Passives)
        CharacterStats activeStats = GetStatsForTurn(turn);
        if (activeStats != null)
        {
            activeStats.ProcessTurnStartStatusEffects();
            activeStats.GetComponent<EnemyDialogue>()?.TriggerTurnStart();
        }

        DeckManager activeDeck = GetDeckForTurn(turn);

        // The player's existing draw system handles their cards.
        // Enemy cards are dealt visually inside EnemyTurnRoutine.
        if (turn == TurnSeat.Player)
        {
            activeDeck.DrawToFullHand();
        }

        OnTurnChanged?.Invoke(turn);

        // If the character died to a start-of-turn effect (like Poison/Apple), skip their turn entirely
        if (activeStats != null && activeStats.IsEliminated)
        {
            AdvanceTurn();
            return;
        }

        // Only trigger the AI routine if it isn't the player's turn
        if (turn != TurnSeat.Player)
        {
            StartCoroutine(EnemyTurnRoutine(turn));
        }
    }

    /// <summary>
    /// The core AI loop. Handles thinking, formulating claims, and waiting for the player's input.
    /// </summary>
    private IEnumerator EnemyTurnRoutine(TurnSeat enemyTurn)
    {
        DeckManager enemyDeck = GetDeckForTurn(enemyTurn);

        // amimate any cards this enemy needs before they take their turn
        if (_enemyDrawAnimator != null)
        {
            yield return StartCoroutine(_enemyDrawAnimator.DrawToFullHand(enemyTurn, enemyDeck));
        }
        else
        {
            // saftey in case animator has not been assigned
            enemyDeck.DrawToFullHand();
        }

        //make sure the turn didnt change during draw animation
        if (_currentTurn != enemyTurn)
        {
            yield break;
        }

        // Small pause before enemy begins thinking
        yield return new WaitForSeconds(_turnStartDelay);

        if (_currentTurn != enemyTurn)
        {
            yield break;
        }

        CharacterStats enemyStats = GetStatsForTurn(enemyTurn);
        EnemyAI activeEnemyAI = enemyStats.GetComponent<EnemyAI>();

        if (enemyDeck.HandCount > 0)
        {
            enemyStats.GetComponent<EnemyDialogue>()?.TriggerThinking();
            yield return new WaitForSeconds(_aiThinkTime);

            // AI selects physical cards to discard from their hand to the table
            List<CardData> trueCards = activeEnemyAI.SelectCardsToPlay(enemyDeck);
            enemyDeck.DiscardCards(trueCards);
            yield return StartCoroutine (TableManager.Instance.PlayCardsToTable(trueCards, enemyTurn));

            // AI decides what lie to tell about the cards they just placed
            ClaimData enemyClaim = activeEnemyAI.FormulateClaim(trueCards);

            // Pause the turn and hand control to the player via UI prompt
            _isWaitingForPlayerDecision = true;
            _playerDecisionMenu.ShowMenu(enemyTurn.ToString(), enemyClaim, (bool calledCheat) =>
            {
                _playerCalledCheat = calledCheat;
                _isWaitingForPlayerDecision = false; // Unpause the Coroutine
            });

            // Stop execution until the player clicks a UI button
            yield return new WaitUntil(() => !_isWaitingForPlayerDecision);

            // Resolve the standoff based on the player's choice
            if (_playerCalledCheat)
            {
                Debug.Log("PLAYER CALLED CHEAT ON THE ENEMY!");
                yield return StartCoroutine(_standoffResolver.ResolveChallenge(enemyClaim, enemyStats, _playerStats));
            }
            else
            {
                Debug.Log("Player passed. Enemy claim accepted.");
                yield return StartCoroutine(_standoffResolver.ResolvePassRoutine(enemyClaim, enemyStats, _playerStats));
            }
        }

        yield return new WaitForSeconds(_turnEndDelay);

        if (_currentTurn == enemyTurn)
        {
            AdvanceTurn();
        }
    }

    /// <summary>
    /// Concludes the current turn, evaluates global Win/Loss states, and iterates to the next living character.
    /// </summary>
    public void AdvanceTurn()
    {
        // Evaluate Victory Condition (All 3 enemies eliminated)
        if (_leftEnemyStats.IsEliminated && _centerEnemyStats.IsEliminated && _rightEnemyStats.IsEliminated)
        {
            Debug.Log("VICTORY! All enemies have been defeated!");
            _sessionData.CompleteCurrentLevel();
            LevelLoader.Instance.LoadNextScene("00c_Map_LevelSelect_Scene");
            return; 
        }

        // Evaluate Defeat Condition (Player eliminated)
        if (_playerStats.IsEliminated)
        {
            Debug.Log("GAME OVER! The Goose has been cooked!");
            LevelLoader.Instance.LoadNextScene("00c_Map_LevelSelect_Scene");
            return; 
        }

        // Cycle clockwise to find the next active seat
        int nextTurnIndex = (int)_currentTurn;
        for (int i = 0; i < 4; i++)
        {
            nextTurnIndex = (nextTurnIndex + 1) % 4;
            TurnSeat nextSeat = (TurnSeat)nextTurnIndex;
            CharacterStats nextStats = GetStatsForTurn(nextSeat);

            if (nextStats != null && !nextStats.IsEliminated)
            {
                // when going back to the player a full round has finished
                if (nextSeat == TurnSeat.Player && _currentTurn != TurnSeat.Player)
                {
                    StartCoroutine(EndRoundRoutine(nextSeat));
                    return;
                }

                StartTurn(nextSeat);
                return;
            }
        }
    }

    private IEnumerator EndRoundRoutine(TurnSeat nextSeat)
    {
        Debug.Log("ROUND COMPLETE - Starting reshuffle");

        //play the visual reshuffle
        if (_discardShuffleAnimator != null)
        {
            yield return StartCoroutine( _discardShuffleAnimator.PlayReshuffleAnimation());
        }

        //reshuffle each characters logical deck
        playerDeck.ShuffleDiscardBackIntoDrawPile();
        leftEnemyDeck.ShuffleDiscardBackIntoDrawPile();
        centreEnemyDeck.ShuffleDiscardBackIntoDrawPile();
        rightEnemyDeck.ShuffleDiscardBackIntoDrawPile();

        //start the players turn after everything is finished
        StartTurn(nextSeat);
    }

    // --------------------------------------------------------
    // ---------------- GETTERS -------------------------------
    // --------------------------------------------------------

    /// <summary>
    /// Gets the DeckManager belonging to a specific seat.
    /// </summary>
    private DeckManager GetDeckForTurn(TurnSeat turn)
    {
        switch (turn)
        {
            case TurnSeat.LeftEnemy: 
            return leftEnemyDeck;

            case TurnSeat.CentreEnemy:
            return centreEnemyDeck;

            case TurnSeat.RightEnemy:
            return rightEnemyDeck;

            default:
            return playerDeck;
        }
    }

    /// <summary>
    /// Gets the CharacterStats component belonging to a specific seat.
    /// </summary>
    public CharacterStats GetStatsForTurn(TurnSeat turn)
    {
        switch (turn)
        {
            case TurnSeat.LeftEnemy:   return _leftEnemyStats;
            case TurnSeat.CentreEnemy: return _centerEnemyStats;
            case TurnSeat.RightEnemy:  return _rightEnemyStats;
            default:                   return _playerStats;
        }
    }

    /// <summary>
    /// Resolves the corresponding DeckManager for a given CharacterStats instance.
    /// Used by Card Effects to locate enemy decks dynamically.
    /// </summary>
    public DeckManager GetDeckForCharacter(CharacterStats stats)
    {
        if (stats == _leftEnemyStats) return leftEnemyDeck;
        if (stats == _centerEnemyStats) return centreEnemyDeck;
        if (stats == _rightEnemyStats) return rightEnemyDeck;
        return playerDeck;
    }

    private IEnumerator StartGameRoutine()
    {
        InitDecks();

        // give players draw animation a frame to begin
        yield return null;

        //wait until all of players starting cards have finished being dealt
        if (_playerHandManager != null)
        {
            yield return new WaitUntil(() => !_playerHandManager.IsDrawingCards);
        }

        // small pause before dealing to first enemy
        yield return new WaitUntil(() => !_playerHandManager.IsDrawingCards);

        if (_enemyDrawAnimator != null)
        {
            // left enemy starting hand
            yield return StartCoroutine(_enemyDrawAnimator.DrawToFullHand (TurnSeat.LeftEnemy, leftEnemyDeck));

            //center enemy starting hand
            yield return StartCoroutine(_enemyDrawAnimator.DrawToFullHand( TurnSeat.CentreEnemy, centreEnemyDeck));

            // right enemy starting hand
            yield return StartCoroutine( _enemyDrawAnimator.DrawToFullHand(TurnSeat.RightEnemy, rightEnemyDeck));
        }
        else
        {
            //saftey in case the animator wasn't assigned
            leftEnemyDeck.DrawToFullHand();
            centreEnemyDeck.DrawToFullHand();
            rightEnemyDeck.DrawToFullHand();
        }

        //gameplay starts after everybody has their cards
        StartTurn(TurnSeat.Player);
    }
}