using UnityEngine;
using System;
using Unity.VisualScripting;

/// <summary>
/// Enum for SFX (Sound Effects)
/// </summary>
public enum SFXType {
    CardPlace,
    CardShuffle,
    CardFlip,
    Damage,
    ShieldBlock,
    WingFlap,
    Select,
    Back,
    Error,
    Hover,
    CardDealing,
    CardSelect,
    Blood,
    Rot,
    DialogueBlip,
    VictoryJingle,
    TurnBell,
}

/// <summary>
/// Enum for BGM (Background Music)
/// </summary>
public enum BGMType
{
    MainTheme,
    PlayingTheme
}

/// <summary>
/// Struct to link SFX enum to actual audio
/// </summary>
[System.Serializable]
public struct SFXMapping
{
    public SFXType SoundType;
    public AudioClip AudioFile;
}

/// <summary>
/// Struct to link BGM to actual audio
/// </summary>
[System.Serializable]
public struct BGMMapping
{
    public BGMType TrackType;
    public AudioClip AudioFile;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Mixer Tracks (Audio Soruces)")]
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _bgmSource;

    [Header("Audio Libraries")]
    [SerializeField] private SFXMapping[] _sfxLibrary;
    [SerializeField] private BGMMapping[] _bgmLibrary;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Play sounds by name.
    /// </summary>
    public void PlaySFX(SFXType sfxToPlay)
    {
        foreach (SFXMapping mapping in _sfxLibrary)
        {
            if (mapping.SoundType == sfxToPlay)
            {
                // Only randomise pitch for sounds that do not overlap
                if (sfxToPlay != SFXType.CardDealing)
                {
                    _sfxSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                }
                else
                {
                    // Lock pitch to prevent phase cancellation
                    _sfxSource.pitch = 1.0f;
                }
                
                _sfxSource.PlayOneShot(mapping.AudioFile);
                return;
            }       
        }
    }
}
