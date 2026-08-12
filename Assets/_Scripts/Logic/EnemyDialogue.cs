using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class EnemyDialogue : MonoBehaviour
{
    [Header("Dialogue Data")]
    [SerializeField] private EnemyDialogueData _genericDialogue; // Generic pool of dialogue all enemies share
    [SerializeField] private EnemyDialogueData _uniqueDialogue; // Specific personality dialogue (Leave empty for minions)

    [Header("UI References")]
    [SerializeField] private GameObject _speechBubbleContainer;
    [SerializeField] private TextMeshProUGUI _dialogueText;

    private Coroutine _hideRoutine;

    private void Awake()
    {
        _speechBubbleContainer.SetActive(false);
    }

    /// <summary>
    /// Merges generic and unique dialogue pools, then picks a random line to display
    /// </summary>
    public void Speak(List<string> genericList, List<string> uniqueList, float duration = 3f)
    {
        List<string> combinedPool = new List<string>();

        // Add generic lines if they exists
        if (genericList != null && genericList.Count > 0)
        {
            combinedPool.AddRange(genericList);
        }

        // Add unique boss lines if they exist
        if (uniqueList != null && uniqueList.Count > 0)
        {
            combinedPool.AddRange(uniqueList);
        }

        // Safety check if no dialogue was written
        if (combinedPool.Count == 0)
        {
            return;
        }

        // Pick a random line from the combined pool
        string selectedLine = combinedPool[Random.Range(0, combinedPool.Count)];

        _dialogueText.text = selectedLine;
        _speechBubbleContainer.SetActive(true);

        // Reset timer if already speaking
        if (_hideRoutine != null)
        {
            StopCoroutine(_hideRoutine);
        }

        _hideRoutine = StartCoroutine(HideAfterSeconds(duration));
    }

    private IEnumerator HideAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _speechBubbleContainer.SetActive(false);
    }

    // Helper methods to call specific categories
    public void TriggerTurnStart() => Speak(
        _genericDialogue != null ? _genericDialogue.TurnStartDialogue : null,
        _uniqueDialogue != null ? _uniqueDialogue.TurnStartDialogue : null
    );
    public void TriggerThinking() => Speak(
        _genericDialogue != null ? _genericDialogue.ThinkingDialogue : null,
        _uniqueDialogue != null ? _uniqueDialogue.ThinkingDialogue : null
    );
    public void TriggerTargeted() => Speak(
        _genericDialogue != null ? _genericDialogue.TargetedDialogue : null,
        _uniqueDialogue != null ? _uniqueDialogue.TargetedDialogue : null
    );
    public void TriggerBluffing() => Speak(
            _genericDialogue != null ? _genericDialogue.BluffingDialogue : null,
            _uniqueDialogue != null ? _uniqueDialogue.BluffingDialogue : null
        );
    public void TriggerCallCheat() => Speak(
        _genericDialogue != null ? _genericDialogue.CallCheatDialogue : null,
        _uniqueDialogue != null ? _uniqueDialogue.CallCheatDialogue : null
    );
    public void TriggerCaughtLying() => Speak(
        _genericDialogue != null ? _genericDialogue.CaughtLyingDialogue : null,
        _uniqueDialogue != null ? _uniqueDialogue.CaughtLyingDialogue : null
    );
    public void TriggerSuccessfull() => Speak(
        _genericDialogue != null ? _genericDialogue.SuccessfullDialogue : null,
        _uniqueDialogue != null ? _uniqueDialogue.SuccessfullDialogue : null
    );
}
