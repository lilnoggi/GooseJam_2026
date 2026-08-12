using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueData", menuName = "Cheating Geese/Enemy Dialogue Data")]
public class EnemyDialogueData : ScriptableObject
{
    [Header("Turn Phase Reactions")]
    public List<string> TurnStartDialogue;
    public List<string> ThinkingDialogue;

    [Header("Standoff Reactions")]
    public List<string> BluffingDialogue;
    public List<string> CallCheatDialogue;
    public List<string> CaughtLyingDialogue;
    public List<string> SuccessfullDialogue;
}
