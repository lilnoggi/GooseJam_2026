using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages the introductory cinematic sequence
/// Handles the typewriter text effect
/// </summary>
public class StoryIntroManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private GameObject _actionButton;
    [SerializeField] private TextMeshProUGUI _buttonText;

    [Header("Story Settings")]
    [TextArea(3, 5)]
    [SerializeField] private string[] _storyLines;
    [SerializeField] private float _typingSpeed = 0.04f; // Speed in which the text writes

    // State trackers
    private int _currentLineIndex = 0;
    private Coroutine _typingCoroutine;

    private void Start()
    {
        AudioManager.Instance.StopBGM();

        // Start the sequence with the first line
        PlayLine(_currentLineIndex);
    }

/// <summary>
/// Prepares and executes the visual and text elements for a specific slide index
/// </summary>
    private void PlayLine(int index)
    {
        // Clear the UI to prevent the player from skipping ahead before reading
        _dialogueText.text = "";
        _actionButton.SetActive(false);

        // Update button text contextually
        if (_buttonText != null)
        {
            _buttonText.text = (index == _storyLines.Length - 1) ? "Play" : "Continue";
        }

        // Stop any active typing just in case, then start the new line
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }
        
        _typingCoroutine = StartCoroutine(TypeText(_storyLines[index]));
    }

    /// <summary>
    /// Appends characters to the dialogue text box one by one to simulate typing
    /// </summary>
    private IEnumerator TypeText(string line)
    {
        foreach (char letter in line.ToCharArray())
        {
            _dialogueText.text += letter;

            if (letter != ' ')
            {
                AudioManager.Instance.PlaySFX(SFXType.DialogueBlip);
            }
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
        AudioManager.Instance.PlaySFX(SFXType.Select);

        _currentLineIndex++;

        // If there are more lines, play the next one
        if (_currentLineIndex < _storyLines.Length)
        {
            PlayLine(_currentLineIndex);
        }
        else
        {
            // Transition to actual game
            LevelLoader.Instance.LoadNextScene("00d_Tutorial_Scene");
        }
    }
}
