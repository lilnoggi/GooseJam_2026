using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _guidePanel;

    [Header("System References")]
    [SerializeField] private ApplicationController _appController;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference _pauseAction;

    private bool _isPaused = false;

    // --- INPUT SYSTEM BINDING ---
    private void OnEnable()
    {
        if (_pauseAction != null)
        {
            _pauseAction.action.Enable();
            _pauseAction.action.performed += TogglePause;
        }
    }

    private void OnDisable()
    {
        if (_pauseAction != null)
        {
            _pauseAction.action.performed -= TogglePause;
            _pauseAction.action.Disable();
        }
    }

    private void Start()
    {
        // Ensure the game starts unpaused and all menus are hidden
        _isPaused = false;
        Time.timeScale = 1f;

        if (_pausePanel != null) _pausePanel.SetActive(false);
        if (_settingsPanel != null) _settingsPanel.SetActive(false);
        if (_guidePanel != null) _guidePanel.SetActive(false);
    }

    // --- PAUSE LOGIC ---

    /// <summary>
    /// Triggered by the Input System when the pause button is pressed.
    /// </summary>
    private void TogglePause(InputAction.CallbackContext context)
    {
        if (_isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        _isPaused = true;
        Time.timeScale = 0f; // Freezes all game logic and animations
        
        if (_pausePanel != null) _pausePanel.SetActive(true);
        
        AudioManager.Instance.PlaySFX(SFXType.Select);
    }

    public void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f; // Unfreezes the game
        
        // Hide all panels so nothing gets left behind on screen
        if (_pausePanel != null) _pausePanel.SetActive(false);
        if (_settingsPanel != null) _settingsPanel.SetActive(false);
        if (_guidePanel != null) _guidePanel.SetActive(false);

        AudioManager.Instance.PlaySFX(SFXType.Back);
    }

    // --- SUB-MENU NAVIGATION ---
    public void OpenSettings()
    {
        if (_settingsPanel != null)
        {
            // Settings panel overlays the Pause menu
            _settingsPanel.SetActive(true);
            AudioManager.Instance.PlaySFX(SFXType.Select);
        }
    }

    public void OpenGuide()
    {
        if (_guidePanel != null)
        {
            // Guide panel overlays the Pause menu
            _guidePanel.SetActive(true);
            AudioManager.Instance.PlaySFX(SFXType.Select);
        }
    }

    public void CloseGuide()
    {
        if (_guidePanel != null)
        {
            _guidePanel.SetActive(false);
            AudioManager.Instance.PlaySFX(SFXType.Back);
        }
    }

    // --- QUIT LOGIC ---
    public void QuitToDesktop()
    {
        AudioManager.Instance.PlaySFX(SFXType.Select);
        
        // Always reset time scale before quitting or loading a new scene
        Time.timeScale = 1f; 

        if (_appController != null)
        {
            _appController.QuitGame();
        }
    }
}
