using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    [Header("Screen References")]
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private TextMeshProUGUI _fullScreenCheckboxText;

    [Header("Audio References")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer _audioMixer;

    private Resolution[] _resolutions;
    private List<Resolution> _filteredResolutions;

    private void Start()
    {
        // --- SCREEN SETTINGS ---
        // Get the monitors support resolutions
        _resolutions = Screen.resolutions;
        _filteredResolutions = new List<Resolution>();
        _resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < _resolutions.Length; i++)
        {
            string option = _resolutions[i].width + " x " + _resolutions[i].height;

            // Keep dropdown clean by preventing duplicate sizes with different refresh rates
            if (!options.Contains(option))
            {
                _filteredResolutions.Add(_resolutions[i]);
                options.Add(option);

                // Check if this is the resolution curretnly being used
                if (_resolutions[i].width == Screen.width && _resolutions[i].height == Screen.height)
                {
                    currentResIndex = _filteredResolutions.Count - 1;
                }
            }
        }

        // Populate Dropdwon UI
        _resolutionDropdown.AddOptions(options);
        _resolutionDropdown.value = currentResIndex;
        _resolutionDropdown.RefreshShownValue();

        // Setup the Fullscreen Checkbox UI
        UpdateFullScreenUI();

        // --- AUDIO SETTINGS ---
        // Load saved volume preferences
        float savedMusicVol = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float savedSFXVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        // Update visual sliders
        if (_musicSlider != null)
        {
            _musicSlider.value = savedMusicVol;
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.value = savedSFXVol;
        }

        // Apply the volume to the mixer
        SetMusicVolume(savedMusicVol);
        SetSFXVolume(savedSFXVol);
    }

    // --- SCREEN LOGIC ---

    /// <summary>
    /// Triggered by the Dropdown's OnValueChanged event
    /// </summary>
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = _filteredResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    /// <summary>
    /// Triggered by the FullScreen Checkbox Button's OnClick event
    /// </summary>
    public void ToggleFullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        UpdateFullScreenUI();
    }

    /// <summary>
    /// Visually updates the text inside the checkbox tp be an X or blank
    /// </summary>
    private void UpdateFullScreenUI()
    {
        if (_fullScreenCheckboxText != null)
        {
            _fullScreenCheckboxText.text = Screen.fullScreen ? "X" : " ";
        }
    }

    // --- AUDIO LOGIC ---

    /// <summary>
    /// Triggered by the Music Slider's OnValueChanged event
    /// </summary>
    public void SetMusicVolume(float sliderValue)
    {
        // Convert linear slider value (0.0001 to 1) to logarithmic decibels
        float dbVolume = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
        _audioMixer.SetFloat("MusicVolume", dbVolume);

        // Save the setting
        PlayerPrefs.SetFloat("MusicVolume", sliderValue);
    }

    /// <summary>
    /// Triggered by the SFX Slider's OnValueChanged
    /// </summary>
    public void SetSFXVolume(float sliderValue)
    {
        // Convert to decibel
        float dbVolume = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
        _audioMixer.SetFloat("SFXVolume", dbVolume);

        // Save the setting
        PlayerPrefs.SetFloat("SFXVolume", sliderValue);
    }
}
