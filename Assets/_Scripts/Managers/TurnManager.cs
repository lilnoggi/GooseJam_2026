
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
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

    [SerializeField] private float thinkTime = 1.5f; //amount of time that the other players will think before playing a card

    [Header("Character Stats References")]
    [SerializeField] private CharacterStats _playerStats;
    [SerializeField] private CharacterStats _leftEnemyStats;
    [SerializeField] private CharacterStats _centerEnemyStats;
    [SerializeField] private CharacterStats _rightEnemyStats;

    [Header("UI References")]
    [SerializeField] private PlayerDecisionMenu _playerDecisionMenu;

    [Header("Session Data")]
    [SerializeField] private SessionData _sessionData;

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

        // Start coroutine
        StartCoroutine(PlayerClaimRoutine(claim));
    }

    private IEnumerator PlayerClaimRoutine(ClaimData claim)
    {
        // Discard the true cards from the player's hand so they leave the screen
        playerDeck.DiscardCards(claim.TrueCards);

        yield return StartCoroutine(TableManager.Instance.PlayCardsToTable( claim.TrueCards, TurnSeat.Player));

        // Grab the target's stats & AI
        CharacterStats targetStats = GetStatsForTurn(claim.TargetEnemy);
        EnemyAI targetAI = targetStats.GetComponent<EnemyAI>();

        // Enemy reacts to being targeted
        targetStats.GetComponent<EnemyDialogue>()?.TriggerTargeted();

        // Wait 1.5s while the speech bubble is on screen
        yield return new WaitForSeconds(1.5f);

        // CombatLogic evaluates the claim
        var evaluation = CombatLogic.EvaluateClaim(claim);

        //the AI only knows the suit claimed and how many face-down cards were played, it doesnt get told what the real cards are
        bool isChallenging = targetAI.DecideToChallenge(claim.ClaimedSuit, claim.TrueCards.Count);

        if (isChallenging)
        {
            Debug.Log($"<color=pink>[Standoff] The {claim.TargetEnemy} called CHEAT!</color>");

            // Trigger Calling cheat dialogue
            targetStats.GetComponent<EnemyDialogue>()?.TriggerCallCheat();

            // Trigger standoof logic: Player is the claimer, targetStats is the challenger
            yield return StartCoroutine(ResolveChallenge(claim, _playerStats, targetStats));
        }
        else
        {
            Debug.Log($"<color=purple>[Standoff] The {claim.TargetEnemy} accepted the player's claim.</color>");

            // Trigger cinematic reveal
            yield return StartCoroutine(ResolvePassRoutine(claim, _playerStats, targetStats));
        }

        // End the player's turn
        AdvanceTurn();
    }

    private IEnumerator ResolveChallenge(ClaimData claim, CharacterStats claimer, CharacterStats challenger)
    {
        // Swoop Camera down
        yield return CameraController.Instance.SwoopToTable();

        // Let TableManager handle the reveal
        yield return StartCoroutine(TableManager.Instance.FlipTableCards());

        // Combat Logic evaluates claim
        var evaluation = CombatLogic.EvaluateClaim(claim);

        if (evaluation.isLie)
        {
            //the challenger caught the claim in a lie
            yield return UIManager.Instance.ShowStandoffResult("CHEAT!");

            Debug.Log($"Caught {claimer.name.ToUpper()} IN A LIE!!! {claimer.name} takes penalty.");

            // If caught lying, trigger caught dialogue
            claimer.GetComponent<EnemyDialogue>()?.TriggerCaughtLying();

            // The liar takes a penalty equal to the sum of the cards they physically  placed
            claimer.TakeDamage(evaluation.threatValue);
        }
        else
        {
            //the claim was actually true so the challenger messed up
            yield return UIManager.Instance.ShowStandoffResult("WRONG CALL!");

            Debug.Log($"{claimer.name.ToUpper()} TOLD THE TRUTH!!! {challenger.name} takes critical penalty");

            // Told the truth, trigger successfull dialogue
            claimer.GetComponent<EnemyDialogue>()?.TriggerSuccessfull();

            // Challenger takes double the true damage
            challenger.TakeDamage(evaluation.threatValue * 2);

            // Only apply the card effects if it was a utility / defensive suit
            // Otherwise Blood and Rot would effect the claimer
            if (claim.ClaimedSuit == CardSuit.Bone || claim.ClaimedSuit == CardSuit.Feather)
            {
                CombatLogic.ProcessTurn(claim.TrueCards, claim.ClaimedSuit, claimer, challenger);
            }

            // (Paranoia doesn't drop here because enemy was right to be scared)
        }

        // Dyamic Paranoia Logic
        bool isPlayerClaim = claimer == _playerStats;
        CharacterStats enemyStats = isPlayerClaim ? challenger : claimer;

        // Get CombatLogic to calculate the paranoia shift
        int paranoiaShift = CombatLogic.CalculateParanoiaShift(isPlayerClaim, evaluation.isLie, true, evaluation.threatValue, claim.ClaimedSuit);

        if (paranoiaShift != 0)
        {
            enemyStats.IncreaseParanoia(paranoiaShift);
        }

        // Let damage sink in, then swoop back
        yield return new WaitForSeconds(2.0f);

        // TableManager cleanup cards
        TableManager.Instance.ClearTableCards();
        yield return CameraController.Instance.SwoopToDefault();
    }

    private IEnumerator ResolvePassRoutine(ClaimData claim, CharacterStats claimer, CharacterStats target)
    {
        // Swoop camera down
        yield return CameraController.Instance.SwoopToTable();

        // Let TableManager handle the reveal
        yield return StartCoroutine(TableManager.Instance.FlipTableCards());

        // Combat Logic evaluates claim
        var evaluation = CombatLogic.EvaluateClaim(claim);

        //show whether the accepted claim was honest or a successful bluff
        if (evaluation.isLie)
        {
            yield return UIManager.Instance.ShowStandoffResult("BLUFF SUCCESSFUL!");
        }
        else
        {
            yield return UIManager.Instance.ShowStandoffResult("TRUTH!");
        }

        // Apply base damage
        // Because player passed no one is penalised, play just happens normally
        CombatLogic.ProcessTurn(claim.TrueCards, claim.ClaimedSuit, claimer, target);

        // paranoia 
        bool isPlayerClaim = claimer == _playerStats;
        CharacterStats enemyStats = isPlayerClaim ? target : claimer;

        // Calcualte shift
        int paranoiaShift = CombatLogic.CalculateParanoiaShift(isPlayerClaim, evaluation.isLie, false, evaluation.threatValue, claim.ClaimedSuit);

        if (paranoiaShift != 0)
        {
            enemyStats.IncreaseParanoia(paranoiaShift);
        }

        // Let reveal happen, then swoop back
        yield return new WaitForSeconds(2.0f);

        // Clean up the cards 
        TableManager.Instance.ClearTableCards();
        yield return CameraController.Instance.SwoopToDefault();

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

            // Trigger turn start dialogue
            activeStats.GetComponent<EnemyDialogue>()?.TriggerTurnStart();
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
        // Phase 1: Opening
        // The turn banner is fading and the enemy just said their turn start line
        // Give the player 1.5s to register who's turn it is
        yield return new WaitForSeconds(1.5f);

        // safety check incase turn changed during the delay
        if (currentTurn != enemyTurn) yield break;

        DeckManager enemyDeck = GetDeckForTurn(enemyTurn);
        CharacterStats enemyStats = GetStatsForTurn(enemyTurn);
        EnemyAI activeEnemyAI = enemyStats.GetComponent<EnemyAI>();

        if (enemyDeck.HandCount > 0)
        {
            // Phase 2: Thinking
            // Trigger thinking dialogue
            enemyStats.GetComponent<EnemyDialogue>()?.TriggerThinking();

            // Wait another 1.5s while they "decide" what cards to play
            yield return new WaitForSeconds(thinkTime);

            // Phase 3: Make the claim

            // Get the actual CardData objects before discarding them
            // Get the AI to pick its own cards
            List<CardData> trueCards = activeEnemyAI.SelectCardsToPlay(enemyDeck);
            enemyDeck.DiscardCards(trueCards);

            yield return StartCoroutine (TableManager.Instance.PlayCardsToTable(trueCards, enemyTurn));

            // The enemy uses their AI profile to formulate a claim
            ClaimData enemyClaim = activeEnemyAI.FormulateClaim(trueCards);

            // Phase 4: Standoff
            // Pause the turn and show the massive UI menu to the player
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
                yield return StartCoroutine(ResolveChallenge(enemyClaim, enemyStats, _playerStats));
            }
            else
            {
                Debug.Log("Player passed. Enemy claim accepted.");
                yield return StartCoroutine(ResolvePassRoutine(enemyClaim, enemyStats, _playerStats));
            }
        }

        // Phase 5: End Turn
        // Give the UI one final second to show the damage/status effects before next turn
        yield return new WaitForSeconds(1.0f);

        if (currentTurn == enemyTurn)
        {
            AdvanceTurn();
        }
    }

    private void AdvanceTurn()
    {
        // Check for Victory Condition (All 3 enemies are eliminated)
        if (_leftEnemyStats.IsEliminated && _centerEnemyStats.IsEliminated && _rightEnemyStats.IsEliminated)
        {
            Debug.Log("VICTORY! All enemies have been defeated!");
            // TODO: Trigger Victory UI 
            
            // Tell SessionData the player won
            _sessionData.CompleteCurrentLevel();

            // Load the map scene
            LevelLoader.Instance.LoadNextScene("00c_Map_LevelSelect_Scene");
            return; // Stop the turn loop completely
        }

        // Check for Defeat Condition (Player is eliminated)
        if (_playerStats.IsEliminated)
        {
            Debug.Log("GAME OVER! The Goose has been cooked!");
            // TODO: Trigger Game Over UI

            // Return to map
            LevelLoader.Instance.LoadNextScene("00c_Map_LevelSelect_Scene");
            return; // Stop turn loop completely
        }

        // Find the next living character to take a turn
        int nextTurnIndex = (int)currentTurn;

        // cycles clockwise through 0-3 layout indices

        // Loop up to 4 times to find the next alive seat
        for (int i = 0; i < 4; i++)
        {
            nextTurnIndex = (nextTurnIndex + 1) % 4;
            TurnSeat nextSeat = (TurnSeat)nextTurnIndex;
            CharacterStats nextStats = GetStatsForTurn(nextSeat);

            // If the character in this seat exists and is NOT eliminated, start their turn
            if (nextStats != null && !nextStats.IsEliminated)
            {
                StartTurn(nextSeat);
                return;
            }
        }
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


