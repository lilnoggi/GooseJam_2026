using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndGameManagerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private Image _gooseImage;
    [SerializeField] private Animator _gooseAnimator;

    [Header("Assets")]
    [SerializeField] private Sprite _victorySprite;
    [SerializeField] private Sprite _failSprite;

    [Header("Fade Settings")]
    [SerializeField] private float _fadeDuration = 1.0f;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        // Ensure starts completely invisible and turned off
        _canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public IEnumerator ShowEndScreenRoutine(bool isVictory, float holdDuration)
    {
        gameObject.SetActive(true);

        // SETUP THE VISUALS
        if (isVictory)
        {
            _titleText.text = "Victory";
            
            // Turn OFF the animator to show the static crown sprite
            if (_gooseAnimator != null) _gooseAnimator.enabled = false;
            if (_gooseImage != null && _victorySprite != null) _gooseImage.sprite = _victorySprite;
        }
        else
        {
            _titleText.text = "Game Over";
            
            // Turn the animator back ON so the goose cooks!
            if (_gooseAnimator != null) _gooseAnimator.enabled = true;
            if (_gooseImage != null && _failSprite != null) _gooseImage.sprite = _failSprite;
        }

        // FADE IN
        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / _fadeDuration);
            yield return null;
        }
        _canvasGroup.alpha = 1f;

        // HOLD ON SCREEN
        yield return new WaitForSeconds(holdDuration);

        // FADE OUT
        elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / _fadeDuration);
            yield return null;
        }
        _canvasGroup.alpha = 0f;
        
        gameObject.SetActive(false);
    }
}
