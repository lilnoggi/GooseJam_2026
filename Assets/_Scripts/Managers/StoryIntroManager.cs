using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A data container for cinematic slides
/// Stores the lore text and the corresponding background image
/// </summary>
[System.Serializable]
public class StorySlide
{
    [TextArea(3, 5)]
    public string loreText;
    public Sprite cinematicImage;
}

/// <summary>
/// Manages the introductory cinematic sequence
/// Handles the typewriter text effect, background image fades, and zooming
/// </summary>
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
    [SerializeField] private float _fadeDuration = 2f; // Duration of background image to fully fade in
    [SerializeField] private float _zoomSpeed = 0.05f; // Continous scaling rate applied to background image (Ken Burns effect)

    // State trackers
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
        // Continously scales the image upwards to create slow-pan effect
        if (_isZooming)
        {
            _cinematicDisplay.transform.localScale += Vector3.one * _zoomSpeed * Time.deltaTime;
        }
    }

/// <summary>
/// Prepares and executes the visual and text elements for a specific slide index
/// </summary>
    private void PlaySlide(int index)
    {
        // Clear the UI to prevent the player from skipping ahead before reading
        _dialogueText.text = "";
        _actionButton.SetActive(false);

        // Update button text contextually
        _buttonText.text = (index == _storySlides.Length - 1) ? "Start" : "Continue";

        // Handle the cinematic background if a sprite is assigned to this slide
        if (_storySlides[index].cinematicImage != null)
        {
            _cinematicDisplay.sprite = _storySlides[index].cinematicImage;

            // Reset scale and alpha for the new image
            _cinematicDisplay.transform.localScale = Vector3.one;
            _isZooming = true;

            // Ensure no overlapping fade coroutines if player clicks quickly
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            // Trigger typewriter effect for the current slide's lore
             _fadeCoroutine = StartCoroutine(FadeInImage());
        }

        
        _typingCoroutine = StartCoroutine(TypeText(_storySlides[index].loreText));
    }

/// <summary>
/// Transitions the background image's canvas group alpha from 0 to 1
/// </summary>
    private IEnumerator FadeInImage()
    {
        _imageCanvasGroup.alpha = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            // Ensure alpha never exceeds 1.0
            _imageCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / _fadeDuration);
            
            yield return null;
        }

        _imageCanvasGroup.alpha = 1f;
    }

/// <summary>
/// Appends characters to the dialogue text box one by one to simulate typing
/// </summary>
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

    /// <summary>
    /// Triggered from the Button's OnClick event
    /// Advances the slide index or transitions to the gameplay scene
    /// </summary>
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
            // Lock the zoom state and transition to actual game
            _isZooming = false;
            _levelLoader.LoadNextScene("01a_Swamp_Minions_Scene");
        }
    }
}
