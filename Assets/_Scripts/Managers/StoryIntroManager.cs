using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class StorySlide
{
    [TextArea(3, 5)]
    public string loreText;
    public Sprite cinematicImage;
}

public class StoryIntroManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private GameObject _actionButton;
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private LevelLoader _levelLoader;

    [Header("Cinematic References")]
    [SerializeField] private Image _cinematicDisplay;
    [SerializeField] private CanvasGroup _imageCanvasGroup;

    [Header("Story Settings")]
    [SerializeField] private StorySlide[] _storySlides;
    [SerializeField] private float _typingSpeed = 0.04f; // Speed in which the text writes
    [SerializeField] private float _fadeDuration = 2f;
    [SerializeField] private float _zoomSpeed = 0.05f;

    private int _currentSlideIndex = 0;
    private Coroutine _typingCoroutine;
    private Coroutine _fadeCoroutine;
    private bool _isZooming = false;

    private void Start()
    {
        // Start the sequence with the first line
        PlaySlide(_currentSlideIndex);
    }

    private void Update()
    {
        // Handles the slow zoom  effect
        if (_isZooming)
        {
            _cinematicDisplay.transform.localScale += Vector3.one * _zoomSpeed * Time.deltaTime;
        }
    }

    private void PlaySlide(int index)
    {
        // Clear the text box and hide the button so the player has to read
        _dialogueText.text = "";
        _actionButton.SetActive(false);

        _buttonText.text = (index == _storySlides.Length - 1) ? "Start" : "Continue";

        // Handle the Image Swap and Fade
        if (_storySlides[index].cinematicImage != null)
        {
            _cinematicDisplay.sprite = _storySlides[index].cinematicImage;

            // Reset scale and alpha for the new image
            _cinematicDisplay.transform.localScale = Vector3.one;
            _isZooming = true;

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

             _fadeCoroutine = StartCoroutine(FadeInImage());
        }

        
        _typingCoroutine = StartCoroutine(TypeText(_storySlides[index].loreText));
    }

    private IEnumerator FadeInImage()
    {
        _imageCanvasGroup.alpha = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            _imageCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / _fadeDuration);
            yield return null;
        }

        _imageCanvasGroup.alpha = 1f;
    }

    private IEnumerator TypeText(string line)
    {
        foreach (char letter in line.ToCharArray())
        {
            _dialogueText.text += letter;
            yield return new WaitForSeconds(_typingSpeed);
        }

        // Once the text finishes typing, reveal the button
        _actionButton.SetActive(true);
    }

    // This goes on the button's OnClick event in Inspector
    public void OnActionButtonClicked()
    {
        _currentSlideIndex++;

        // If there are more lines, play the next one
        if (_currentSlideIndex < _storySlides.Length)
        {
            PlaySlide(_currentSlideIndex);
        }
        else
        {
            _isZooming = false;
            _levelLoader.LoadNextScene("01_Swamp_Fox_Scene");
        }
    }
}
