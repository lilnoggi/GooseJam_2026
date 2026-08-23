using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _settingsPanel;

    void Start()
    {
        AudioManager.Instance.PlayBGM(BGMType.MainTheme);

        _settingsPanel.SetActive(false);
    }

    public void OpenSettingsMenu()
    {
        if (_settingsPanel != null)
        {
            _settingsPanel.SetActive(true);
        }
    }
}
