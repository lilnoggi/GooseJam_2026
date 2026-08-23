using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {
        AudioManager.Instance.PlayBGM(BGMType.MainTheme);
    }
}
