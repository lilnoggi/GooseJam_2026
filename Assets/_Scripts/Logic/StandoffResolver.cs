using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the complex visual and logical sequences of Standoffs (Bluffing, Calling Cheat, Passing).
/// Extracted from TurnController
/// </summary>
public class StandoffResolver : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private TurnController _turnController;
    [SerializeField] private DeckManager _playerDeck;
    [SerializeField] private CharacterStats _playerStats;

    [Header("Cinematic Timings")]
    [Tooltip("How long to wait while the targeted enemy reacts to being claimed against.")]
    [SerializeField] private float _claimReactionTime = 1.5f;
    [Tooltip("How long to let the damage numbers sit on screen before returning to the player.")]
    [SerializeField] private float _damageSinkTime = 2.0f;
    [Tooltip("How long to give the player to read the SAVED BY PROMISE banner.")]
    [SerializeField] private float _hollowPromiseReadTime = 1.0f;
    [Tooltip("How long the Hollow Promise card glows in the hand.")]
    [SerializeField] private float _hollowPromiseGlowTime = 1.5f;

    // ----------------------------------------------------------------------------------------------- 

    /// <summary>
    /// Executes the full cinematic and logical sequence when the Player makes a claim against an Enemy.
    /// Triggers AI decision making to accept or challenge the claim.
    /// </summary>
    /// <param name="claim">The data container holding both the physical cards played and the lie told.</param>
    public IEnumerator PlayerClaimRoutine(ClaimData claim)
    {
        // Physically move cards from the UI hand to the 3D table
        _playerDeck.DiscardCards(claim.TrueCards);
        yield return StartCoroutine(TableManager.Instance.PlayCardsToTable(claim.TrueCards, TurnSeat.Player));

        // Identify who is being attacked and trigger their reaction animations
        CharacterStats targetStats = _turnController.GetStatsForTurn(claim.TargetEnemy);
        EnemyAI targetAI = targetStats.GetComponent<EnemyAI>();

        targetStats.GetComponent<EnemyDialogue>()?.TriggerTargeted();
        yield return new WaitForSeconds(_claimReactionTime);

        // The AI determines if they think the player is lying
        bool isChallenging = targetAI.DecideToChallenge(claim.ClaimedSuit, claim.TrueCards.Count);

        // Branch the sequence based on the AI's decision
        if (isChallenging)
        {
            targetStats.GetComponent<EnemyDialogue>()?.TriggerCallCheat();
            yield return StartCoroutine(ResolveChallenge(claim, _playerStats, targetStats));
        }
        else
        {
            yield return StartCoroutine(ResolvePassRoutine(claim, _playerStats, targetStats));
        }

        // Conclude the sequence and pass play to the next seat
        _turnController.AdvanceTurn();
    }

    /// <summary>
    /// Resolves a standoff where a claim was actively challenged (Cheat called).
    /// Flips the cards, evaluates the truth, applies penalty damage, and handles Hollow Promise saves.
    /// </summary>
    /// <param name="claim">The claim being challenged.</param>
    /// <param name="claimer">The character who placed the cards face-down.</param>
    /// <param name="challenger">The character who called cheat.</param>
    public IEnumerator ResolveChallenge(ClaimData claim, CharacterStats claimer, CharacterStats challenger)
    {
        // Swoop camera to table to reveal the truth
        yield return CameraController.Instance.SwoopToTable();
        yield return StartCoroutine(TableManager.Instance.FlipTableCards());

        var evaluation = CombatLogic.EvaluateClaim(claim);

        // --- SCENARIO A: THE CLAIM WAS A LIE ---
        if (evaluation.isLie)
        {
            bool savedByHollowPromise = false;
            CardData activatedPromise = null;

            // Only check the player's hand for the Hollow Promise shield
            if (claimer == _playerStats)
            {
                foreach (CardData card in _playerDeck.Hand)
                {
                    if (card.IsStatusCard && card.StatusType == StatusType.HollowPromise && card.EffectLogic != null)
                    {
                        savedByHollowPromise = card.EffectLogic.OnCaughtLying(claimer);

                        if (savedByHollowPromise)
                        {
                            activatedPromise = card;
                            break; // Stop searching once a shield is found
                        }
                    }
                }
            }

            // Apply penalty or shield logic
            if (savedByHollowPromise)
            {
                yield return StandoffResultUI.Instance.ShowStandoffResult("SAVED BY HOLLOW PROMISE!");

                // Briefly swoop back to the hand to highlight the specific card saving the player
                PlayerHandManager handManager = FindAnyObjectByType<PlayerHandManager>();
                if (handManager != null && activatedPromise != null)
                {
                    yield return new WaitForSeconds(_hollowPromiseReadTime);
                    yield return CameraController.Instance.SwoopToDefault();
                    yield return StartCoroutine(handManager.HighlightCardInHand(activatedPromise, _hollowPromiseGlowTime));
                }

                // Consume the shield card
                _playerDeck.DiscardCards(new List<CardData> { activatedPromise });
            }
            else
            {
                // Standard punishment for getting caught
                yield return StandoffResultUI.Instance.ShowStandoffResult("CHEAT!");
                claimer.GetComponent<EnemyDialogue>()?.TriggerCaughtLying();
                claimer.TakeDamage(evaluation.threatValue);
            }
        }
        // --- SCENARIO B: THE CLAIM WAS TRUE ---
        else
        {
            yield return StandoffResultUI.Instance.ShowStandoffResult("WRONG CALL!");
            claimer.GetComponent<EnemyDialogue>()?.TriggerSuccessfull();
            
            // Challenger takes double damage for making a false accusation
            challenger.TakeDamage(evaluation.threatValue * 2);

            // True claims also execute their standard suit abilities (Shields/Dodges)
            if (claim.ClaimedSuit == CardSuit.Bone || claim.ClaimedSuit == CardSuit.Feather)
            {
                CombatLogic.ProcessTurn(claim.TrueCards, claim.ClaimedSuit, claimer, challenger);
            }
        }

        // Apply paranoia shifts based on who lied and who called cheat
        bool isPlayerClaim = claimer == _playerStats;
        CharacterStats enemyStats = isPlayerClaim ? challenger : claimer;
        int paranoiaShift = CombatLogic.CalculateParanoiaShift(isPlayerClaim, evaluation.isLie, true, evaluation.threatValue, claim.ClaimedSuit);
        
        if (paranoiaShift != 0)
        {
            enemyStats.IncreaseParanoia(paranoiaShift);
        }

        // Give the UI time to show damage numbers before resetting the camera
        yield return new WaitForSeconds(_damageSinkTime);
        TableManager.Instance.ClearTableCards();
        yield return CameraController.Instance.SwoopToDefault();
    }

    /// <summary>
    /// Resolves a standoff where a claim was accepted without a challenge.
    /// Flips the cards to reveal if it was a successful bluff or a truthful play, then applies standard damage.
    /// </summary>
    /// <param name="claim">The accepted claim.</param>
    /// <param name="claimer">The character who placed the cards face-down.</param>
    /// <param name="target">The character who chose not to challenge.</param>
    public IEnumerator ResolvePassRoutine(ClaimData claim, CharacterStats claimer, CharacterStats target)
    {
        yield return CameraController.Instance.SwoopToTable();
        yield return StartCoroutine(TableManager.Instance.FlipTableCards());

        var evaluation = CombatLogic.EvaluateClaim(claim);

        // Display outcome purely for player feedback (mechanically, both resolve the same way)
        if (evaluation.isLie)
        {
            yield return StandoffResultUI.Instance.ShowStandoffResult("BLUFF SUCCESSFUL!");
        }
        else
        {
            yield return StandoffResultUI.Instance.ShowStandoffResult("TRUTH!");
        }

        // Because the claim was accepted, standard damage and suit effects apply to the target
        CombatLogic.ProcessTurn(claim.TrueCards, claim.ClaimedSuit, claimer, target);

        // Adjust AI paranoia based on whether they fell for a bluff or correctly passed on a truth
        bool isPlayerClaim = claimer == _playerStats;
        CharacterStats enemyStats = isPlayerClaim ? target : claimer;
        int paranoiaShift = CombatLogic.CalculateParanoiaShift(isPlayerClaim, evaluation.isLie, false, evaluation.threatValue, claim.ClaimedSuit);
        
        if (paranoiaShift != 0)
        {
            enemyStats.IncreaseParanoia(paranoiaShift);
        }

        yield return new WaitForSeconds(_damageSinkTime);
        TableManager.Instance.ClearTableCards();
        yield return CameraController.Instance.SwoopToDefault();
    }
}