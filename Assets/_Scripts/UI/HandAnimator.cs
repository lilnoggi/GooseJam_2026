using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the complex visual animations of the player's hand, such as dealing cards and highlighting.
/// Extracted from PlayerHandManager.
/// </summary>
public class HandAnimator : MonoBehaviour
{
    [Header("Draw Animation Settings")]
    [Tooltip("How far below the screen the card starts before sliding up into the hand.")]
    [SerializeField] private float _drawCardBelowOffset = -500f;
    [Tooltip("The time gap between each card being dealt.")]
    [SerializeField] private float _delayBetweenDraws = 0.08f;
    [Tooltip("How long to wait for the final card to slide into its slot.")]
    [SerializeField] private float _handSettleTime = 0.4f;
    
    [Header("System References")]
    [SerializeField] private RectTransform _handContainer;

    public bool IsDrawingCards { get; private set; }

    // ----------------------------------------------------------------------------------------------- 

    /// <summary>
    /// Animates newly drawn cards sliding into the hand one by one.
    /// Pauses and triggers Insta-Play effects if it detects them.
    /// </summary>
    /// <param name="drawnCards">The visual UI prefabs that need to be animated.</param>
    /// <param name="handManager">Reference back to the manager to trigger actual card logic.</param>
    public IEnumerator AnimateDrawnCardsRoutine(List<PlayerCardView> drawnCards, PlayerHandManager handManager)
    {
        IsDrawingCards = true;
        // Force the hand layout to finish arranging the invisible cards before we slide them in
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_handContainer);

        // Let all cards finish their sliding animation first
        for (int i = 0; i < drawnCards.Count; i++)
        {
            if (drawnCards[i] == null) continue;

            // Move the 3D card from the draw pile towards the player
            yield return StartCoroutine(TableManager.Instance.AnimateCardDrawToPlayer());

            // Swap it for the real 2D card and slide that into the hand
            drawnCards[i].PlayDrawAnimation(_drawCardBelowOffset);

            yield return new WaitForSeconds(_delayBetweenDraws);
        }

        yield return new WaitForSeconds(_handSettleTime);

        // Scan the entire hand for InstaPlay cards
        bool foundInstaPlay = true;
        while (foundInstaPlay)
        {
            foundInstaPlay = false;

            foreach (PlayerCardView cardView in handManager.CardViews)
            {
                if (cardView != null && cardView.CardData.PlayType == CardPlayType.InstaPlay)
                {
                    foundInstaPlay = true;

                    // Pause so the player registers the card
                    yield return new WaitForSeconds(0.5f);

                    // Highlight the card
                    cardView.SetSelected(true);
                    yield return new WaitForSeconds(1.5f);

                    // Tell the manager to discard it and trigger the effect
                    handManager.ExecuteInstaPlay(cardView.CardData);

                    IsDrawingCards = false;

                    // Stop this specific coroutine because ExecuteInstaPlay forces a brand new sequence loop
                    yield break;
                }
            }
        }

        // Tell the Hand Manager that the animation is completely finished
        handManager.CompleteDrawingSequence();
        
        IsDrawingCards = false;
    }

    /// <summary>
    /// Visually highlights a specific card in the player's hand for a set duration.
    /// </summary>
    public IEnumerator HighlightCardRoutine(PlayerCardView visualCard, float duration)
    {
        if (visualCard != null)
        {
            visualCard.SetSelected(true);
            yield return new WaitForSeconds(duration);
            visualCard.SetSelected(false);
        }
    }
}