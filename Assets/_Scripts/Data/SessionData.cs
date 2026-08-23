using UnityEngine;

[CreateAssetMenu(fileName = "NewSessionData", menuName = "Cheating Geese/Session Data")]
public class SessionData : ScriptableObject
{
    [Header("Progression")]
    [SerializeField] private int _currentLevelIndex = 0;

    // GETTERS
    public int CurrentLevelIndex => _currentLevelIndex;

    /// <summary>
    /// Automatically called when ScriptableObject is loaded into memory
    /// </summary>
    private void OnEnable()
    {
        // Load the saved level index, default to 0 if no save file exosts
        _currentLevelIndex = PlayerPrefs.GetInt("SavedLevelIndex", 0);
    }

    /// <summary>
    /// Call from title screen when "New Game" is pressed
    /// </summary>
    public void ResetRun()
    {
        _currentLevelIndex = 0;

        // Save the reset state to hard drive
        PlayerPrefs.SetInt("SavedLevelIndex", _currentLevelIndex);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Called when the player wins a combat encounter
    /// </summary>
    public void CompleteCurrentLevel()
    {
        _currentLevelIndex++;

        // Save the new progress to hard drive
        PlayerPrefs.SetInt("SavedLevelIndex", _currentLevelIndex);
        PlayerPrefs.Save();
    }
}
