using System.Collections;
using UnityEngine;
using TMPro;

public class StoryIntroManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private GameObject _actionButton;
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private LevelLoader _levelLoader;

    [Header("Story Settings")]
    [TextArea(3, 5)]
    [SerializeField] private string[] _storyLines;
    [SerializeField] private float _typingSpeed = 0.04f; // Speed in which the text writes

    private int _currentLineIndex = 0;
    private Coroutine _typingCoroutine;

    private void Start()
    {
        // Start the sequence with the first line
        PlayLine(_currentLineIndex);
    }

    private void PlayLine(int index)
    {
        // Clear the text box and hide the button so the player has to read
        _dialogueText.text = "";
        _actionButton.SetActive(false);

        // Check if this is the very last line of the story
        if (index == _storyLines.Length - 1)
        {
            _buttonText.text = "Start";
        }
        else 
        {
            _buttonText.text = "Continue";
        }

        // Trigger typewriter effect
        _typingCoroutine = StartCoroutine(TypeText(_storyLines[index]));
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
        _currentLineIndex++;

        // If there are more lines, play the next one
        if (_currentLineIndex < _storyLines.Length)
        {
            PlayLine(_currentLineIndex);
        }
        else
        {
            // If ran out of lines, trigger the LevelLoader to start the game
            _levelLoader.LoadNextScene("01_Swamp_Fox_Scene");
        }
    }
}
