using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Handles the cinematic text animations for standoff outcomes (Cheat, Truth, Bluff Successful, Wrong Call).
/// </summary>
public class StandoffResultUI : MonoBehaviour
{
    // Singleton Pattern for easy access
    public static StandoffResultUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _standoffResultText;

    [Header("Animation Settings")]
    [SerializeField] private float _resultHoldTime = 0.8f;
    [SerializeField] private float _resultFadeTime = 0.5f;
    [SerializeField] private float _resultIntroTime = 0.25f;
    [SerializeField] private float _cheatShakeAmount = 18f;
    [SerializeField] private float _bluffRiseDistance = 100f;

    // -----------------------------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (_standoffResultText != null)
        {
            _standoffResultText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Displays a specific animated text banner based on the outcome of a standoff.
    /// </summary>
    public IEnumerator ShowStandoffResult(string message)
    {
        if (_standoffResultText == null) yield break;

        _standoffResultText.text = message;
        _standoffResultText.gameObject.SetActive(true);

        Color textColour = _standoffResultText.color;
        textColour.a = 1f;
        _standoffResultText.color = textColour;

        // Route to the correct visual flair
        switch (message)
        {
            case "CHEAT!":
                yield return StartCoroutine(AnimateCheatResult());
                break;
            case "WRONG CALL!":
                yield return StartCoroutine(AnimateWrongCallResult());
                break;
            case "BLUFF SUCCESSFUL!":
                yield return StartCoroutine(AnimateBluffResult());
                break;
            case "TRUTH!":
                yield return StartCoroutine(AnimateTruthResult());
                break;
            default:
                yield return StartCoroutine(AnimateTruthResult()); // Fallback animation
                break;
        }

        yield return new WaitForSeconds(_resultHoldTime);
        yield return StartCoroutine(FadeStandoffResult());

        _standoffResultText.gameObject.SetActive(false);
    }

    // --- ANIMATION COROUTINES ---

    private IEnumerator AnimateCheatResult()
    {
        RectTransform resultTransform = _standoffResultText.rectTransform;
        Vector2 normalPosition = resultTransform.anchoredPosition;

        // Start massive so it slams into the screen
        resultTransform.localScale = Vector3.one * 2.5f;
        float elapsedTime = 0f;

        while (elapsedTime < _resultIntroTime)
        {
            elapsedTime += Time.deltaTime;
            float percent = Mathf.Clamp01(elapsedTime / _resultIntroTime);

            resultTransform.localScale = Vector3.Lerp(Vector3.one * 2.5f, Vector3.one, percent);

            // Violent screen shake that diminishes over time
            float shakeStrength = 1f - percent;
            float randomX = Random.Range(-_cheatShakeAmount, _cheatShakeAmount);
            float randomY = Random.Range(-_cheatShakeAmount, _cheatShakeAmount);

            resultTransform.anchoredPosition = normalPosition + new Vector2(randomX, randomY) * shakeStrength;
            yield return null;
        }

        resultTransform.localScale = Vector3.one;
        resultTransform.anchoredPosition = normalPosition;
    }

    private IEnumerator AnimateWrongCallResult()
    {
        RectTransform resultTransform = _standoffResultText.rectTransform;
        Vector2 normalPosition = resultTransform.anchoredPosition;

        resultTransform.localScale = Vector3.one * 1.5f;
        float elapsedTime = 0f;

        while (elapsedTime < _resultIntroTime)
        {
            elapsedTime += Time.deltaTime;
            float percent = Mathf.Clamp01(elapsedTime / _resultIntroTime);

            // Horizontal wobble
            float wobble = Mathf.Sin(percent * Mathf.PI * 4f) * 20f;

            resultTransform.anchoredPosition = normalPosition + new Vector2(wobble, 0f);
            resultTransform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, percent);
            yield return null;
        }

        resultTransform.localScale = Vector3.one;
        resultTransform.anchoredPosition = normalPosition;
    }

    private IEnumerator AnimateBluffResult()
    {
        RectTransform resultTransform = _standoffResultText.rectTransform;
        Vector2 normalPosition = resultTransform.anchoredPosition;

        // Start hidden below
        Vector2 startPosition = normalPosition + new Vector2(0f, -_bluffRiseDistance);

        resultTransform.anchoredPosition = startPosition;
        resultTransform.localScale = Vector3.one * 0.7f;
        float elapsedTime = 0f;

        while (elapsedTime < _resultIntroTime)
        {
            elapsedTime += Time.deltaTime;
            float percent = Mathf.Clamp01(elapsedTime / _resultIntroTime);
            float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

            // Smoothly rise and grow
            resultTransform.anchoredPosition = Vector2.Lerp(startPosition, normalPosition, smoothPercent);
            resultTransform.localScale = Vector3.Lerp(Vector3.one * 0.7f, Vector3.one, smoothPercent);
            yield return null;
        }

        resultTransform.anchoredPosition = normalPosition;
        resultTransform.localScale = Vector3.one;
    }

    private IEnumerator AnimateTruthResult()
    {
        RectTransform resultTransform = _standoffResultText.rectTransform;
        resultTransform.localScale = Vector3.one * 0.8f;
        float elapsedTime = 0f;

        while (elapsedTime < _resultIntroTime)
        {
            elapsedTime += Time.deltaTime;
            float percent = Mathf.Clamp01(elapsedTime / _resultIntroTime);
            float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

            // Pop-in
            resultTransform.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, smoothPercent);
            yield return null;
        }

        resultTransform.localScale = Vector3.one;
    }

    private IEnumerator FadeStandoffResult()
    {
        float elapsedTime = 0f;
        Color textColour = _standoffResultText.color;

        while (elapsedTime < _resultFadeTime)
        {
            elapsedTime += Time.deltaTime;
            textColour.a = Mathf.Lerp(1f, 0f, elapsedTime / _resultFadeTime);
            _standoffResultText.color = textColour;
            yield return null;
        }

        textColour.a = 0f;
        _standoffResultText.color = textColour;
    }
}