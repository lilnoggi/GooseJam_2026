using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager Instance { get; private set; }
    [SerializeField] private AudioClip[] _SFX;
    [SerializeField] private AudioClip[] _BGM;
}
