using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Handles the display and animation of the Turn Banner at the start of each character's turn.
/// </summary>
public class TurnBannerUI : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private TurnController _turnController;

    [Header("UI References")]
    [SerializeField] private GameObject _turnBanner;
    [SerializeField] private TextMeshProUGUI _turnBannerText;
    [SerializeField] private CanvasGroup _turnBannerGroup;

    private Coroutine _bannerFadeRoutine;

    // -----------------------------------------------------------------------------------------------

    private void OnEnable()
    {
        if (_turnController != null)
        {
            _turnController.OnTurnChanged += HandleTurnChanged;
        }
    }

    private void OnDisable()
    {
        if (_turnController != null)
        {
            _turnController.OnTurnChanged -= HandleTurnChanged;
        }
    }

    /// <summary>
    /// Triggered globally whenever the TurnController moves to a new seat.
    /// </summary>
    private void HandleTurnChanged(TurnSeat currentTurn)
    {
        if (_turnBannerText != null)
        {
            _turnBannerText.text = $"{currentTurn}'s Turn!";

            // Stop the current fade if a new turn starts before the old text finishes fading
            if (_bannerFadeRoutine != null)
            {
                StopCoroutine(_bannerFadeRoutine);
            }

            _bannerFadeRoutine = StartCoroutine(FadeBannerRoutine());
        }
    }

    private IEnumerator FadeBannerRoutine()
    {
        // Snap the banner to full visibility
        if (_turnBanner != null) _turnBanner.SetActive(true);
        if (_turnBannerGroup != null) _turnBannerGroup.alpha = 1f;

        // Leave it on screen for 2 seconds
        yield return new WaitForSeconds(2f);

        // Smoothly fade the text out over 1 second
        float fadeDuration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            if (_turnBannerGroup != null)
            {
                _turnBannerGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            }

            yield return null; 
        }

        // Ensure it is completely invisible at the end
        if (_turnBannerGroup != null) _turnBannerGroup.alpha = 0f;
        if (_turnBanner != null) _turnBanner.SetActive(false);
    }
}