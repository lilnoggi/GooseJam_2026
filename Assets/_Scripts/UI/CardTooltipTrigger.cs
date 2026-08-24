using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Settings")]
    [SerializeField] private float _hoverDelay = 1.0f;

    private PlayerCardView _cardView;
    private Coroutine _hoverCoroutine;

    private void Awake()
    {
        _cardView = GetComponent<PlayerCardView>();
    }

    /// <summary>
    /// Triggered the exact frame the mouse enters the card's physical space
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_cardView != null && _cardView.CardData != null)
        {
            // Only trigger tooltip if this is a status card
            if (_cardView.CardData.IsStatusCard)
            {
                // Start timer
                _hoverCoroutine = StartCoroutine(HoverTimerRoutine());
            }
        }
    }

    /// <summary>
    /// Triggered the exact frame the mouse leaves the card's area
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        // Cancel the timer if the mouse leaves early
        if (_hoverCoroutine != null)
        {
            StopCoroutine(_hoverCoroutine);
            _hoverCoroutine = null;
        }

        if (StatusDescriptionHover.Instance != null)
        {
            StatusDescriptionHover.Instance.HideTooltip();
        }
    }

    /// <summary>
    /// Waits for the delay, then passes this card's physical location to the tooltip
    /// </summary>
    /// <returns></returns>
    private IEnumerator HoverTimerRoutine()
    {
        yield return new WaitForSeconds(_hoverDelay);

        string cardName = _cardView.CardData.StatusName;
        string cardDescription = _cardView.CardData.StatusDescription;

        // Pass transform so the tooltip knows where to pin to
        StatusDescriptionHover.Instance.ShowTooltip(cardName, cardDescription, transform);
    }
}
