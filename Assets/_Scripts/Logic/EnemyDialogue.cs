using UnityEngine;
using System.Collections.Generic;

public class EnemyDialogue : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private Sprite _portraitIcon;

    [Header("Dialogue Data")]
    [SerializeField] private EnemyDialogueData _genericDialogue; // Generic pool of dialogue all enemies share
    [SerializeField] private EnemyDialogueData _uniqueDialogue; // Specific personality dialogue (Leave empty for minions)

    public void Speak(List<string> genericList, List<string> uniqueList, float duration = 3f)
    {
        List<string> combinedPool = new List<string>();

        if (genericList != null && genericList.Count > 0) combinedPool.AddRange(genericList);
        if (uniqueList != null && uniqueList.Count > 0) combinedPool.AddRange(uniqueList);

        if (combinedPool.Count == 0) return;

        string selectedLine = combinedPool[Random.Range(0, combinedPool.Count)];

        // Forward the line to the new global UI instead of a floating bubble!
        if (DialogueUIManager.Instance != null)
        {
            DialogueUIManager.Instance.ShowDialogue(gameObject.name, selectedLine, duration, _portraitIcon);
        }
    }

    // Helper methods remain exactly the same!
    public void TriggerTurnStart() => Speak(_genericDialogue?.TurnStartDialogue, _uniqueDialogue?.TurnStartDialogue);
    public void TriggerThinking() => Speak(_genericDialogue?.ThinkingDialogue, _uniqueDialogue?.ThinkingDialogue);
    public void TriggerTargeted() => Speak(_genericDialogue?.TargetedDialogue, _uniqueDialogue?.TargetedDialogue);
    public void TriggerBluffing() => Speak(_genericDialogue?.BluffingDialogue, _uniqueDialogue?.BluffingDialogue);
    public void TriggerCallCheat() => Speak(_genericDialogue?.CallCheatDialogue, _uniqueDialogue?.CallCheatDialogue);
    public void TriggerCaughtLying() => Speak(_genericDialogue?.CaughtLyingDialogue, _uniqueDialogue?.CaughtLyingDialogue);
    public void TriggerSuccessfull() => Speak(_genericDialogue?.SuccessfullDialogue, _uniqueDialogue?.SuccessfullDialogue);
    public void TriggerDefeated() => Speak(_genericDialogue?.DefeatedDialogue, _uniqueDialogue?.DefeatedDialogue);
}