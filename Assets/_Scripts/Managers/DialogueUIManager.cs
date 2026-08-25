using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueUIManager : MonoBehaviour
{
    public static DialogueUIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private Image _portraitImage;

    [Header("Settings")]
    [SerializeField] private float _typeSpeed = 0.03f;

    private Coroutine _currentRoutine;

    private void Awake()
    {
        Instance = this;
        _dialoguePanel.SetActive(false);
    }

    public void ShowDialogue(string speakerName, string text, float duration, Sprite portrait)
    {
        if (_currentRoutine != null) StopCoroutine(_currentRoutine);
        _currentRoutine = StartCoroutine(TypewriterRoutine(speakerName, text, duration, portrait));
    }

    private IEnumerator TypewriterRoutine(string speakerName, string text, float duration, Sprite portrait)
    {
        _dialoguePanel.SetActive(true);
        
        // Update name, removing "(Clone)" just in case they are instantiated prefabs
        if (_nameText != null) _nameText.text = speakerName.Replace("(Clone)", ""); 
        
        // Update portrait image
        if (_portraitImage != null && portrait != null)
        {
            _portraitImage.sprite = portrait;
        }

        _dialogueText.text = "";

        foreach (char letter in text.ToCharArray())
        {
            _dialogueText.text += letter;
            if (letter != ' ') AudioManager.Instance.PlaySFX(SFXType.DialogueBlip);
            
            yield return new WaitForSeconds(_typeSpeed);
        }

        yield return new WaitForSeconds(duration);
        _dialoguePanel.SetActive(false);
    }
}