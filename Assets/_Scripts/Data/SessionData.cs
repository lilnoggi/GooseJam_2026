using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSessionData", menuName = "Cheating Geese/Session Data")]
public class SessionData : ScriptableObject
{
    [Header("Progression")]
    [SerializeField] private int _currentLevelIndex = 0;

    // GETTERS
    public int CurrentLevelIndex => _currentLevelIndex;

    /// <summary>
    /// Call from title screen when "New Game" is pressed
    /// </summary>
    public void ResetRun()
    {
        _currentLevelIndex = 0;
    }

    /// <summary>
    /// Called when the player wins a combat encounter
    /// </summary>
    public void CompleteCurrentLevel()
    {
        _currentLevelIndex++;
    }
}
