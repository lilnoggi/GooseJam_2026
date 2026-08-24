using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private Button _continueButton;
    [SerializeField] private GameObject _guidePanel;

    [Header("Game Data")]
    [SerializeField] private SessionData _sessionData;

    void Start()
    {
        AudioManager.Instance.PlayBGM(BGMType.MainTheme);

        if (_settingsPanel != null)
        {
            _settingsPanel.SetActive(false);
        }

        // Check if there is any saved progress when the menu loads
        if (_continueButton != null)
        {
            // If the saved level is 0, player has not beaten anything yet
            bool hasSaveData = PlayerPrefs.GetInt("SavedLevelIndex", 0) > 0;

            // Make the button not interactable
            _continueButton.interactable = hasSaveData;
        }
    }

    public void OpenSettingsMenu()
    {
        if (_settingsPanel != null)
        {
            _settingsPanel.SetActive(true);
        }

        AudioManager.Instance.PlaySFX(SFXType.Select);
    }

    /// <summary>
    /// Triggered by "New Game" button. Wipes data, then loads intro.
    /// </summary>
    public void StartNewGame()
    {
        StartCoroutine(StartNewGameRoutine());
    }

    private IEnumerator StartNewGameRoutine()
    {
        AudioManager.Instance.PlaySFX(SFXType.Select);

        if (_sessionData != null)
        {
            _sessionData.ResetRun(); // Wipe save data
        }

        yield return new WaitForSeconds(0.5f);

        LevelLoader.Instance.LoadNextScene("00b_IntroCutscene_Scene");
    }

    /// <summary>
    /// Triggered by the "Continue" button. Loads the map with existing data.
    /// </summary>
    public void ContinueGame()
    {
        StartCoroutine(ContinueGameRoutine());
    }

    private IEnumerator ContinueGameRoutine()
    {
        AudioManager.Instance.PlaySFX(SFXType.Select);

        yield return new WaitForSeconds(0.5f);
        
        LevelLoader.Instance.LoadNextScene("00c_Map_LevelSelect_Scene");
    }

    public void OpenGuidePanel()
    {
        if (_guidePanel != null)
        {
            AudioManager.Instance.PlaySFX(SFXType.Select);
            _guidePanel.SetActive(true);
        }
    }

    public void CloseGuidePanel()
    {
        if (_guidePanel != null)
        {
            AudioManager.Instance.PlaySFX(SFXType.Select);
            _guidePanel.SetActive(false);
        }
    }
}
