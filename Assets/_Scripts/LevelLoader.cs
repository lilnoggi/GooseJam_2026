using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A modular wrapper for Unitys SceneManager
/// Designed to be attatched to persistent UI or managers
/// </summary>
public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Set instance to this
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
    /// <summary>
    /// Loads a new unity scene by its name
    /// Target scene must be included in build files
    /// </summary>
    public void LoadNextScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
