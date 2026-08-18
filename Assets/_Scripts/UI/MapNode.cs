using UnityEngine;
using UnityEngine.UI;

public class MapNode : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] private int _nodeIndex;  // 0 for Swamp_01, 1 for Swamp_02
    [SerializeField] private string _sceneToLoad;

    [Header("Visuals")]
    [SerializeField] private Sprite _lockedSprite;
    [SerializeField] private Sprite _unlockedSprite;

    private Image _nodeImage;
    private Button _nodeButton;

    private void Awake()
    {
        _nodeImage = GetComponent<Image>();
        _nodeButton = GetComponent<Button>();

        _nodeButton.onClick.AddListener(OnNodeClicked);
    }

    /// <summary>
    /// Called by MapManager to set the visual state
    /// </summary>
    public void SetState(bool isUnlocked, bool isCurrentLevel)
    {
        if (isUnlocked)
        {
            _nodeImage.sprite = _unlockedSprite;

            // Only let the player press the button if it is the level they are currently on
            _nodeButton.interactable = isCurrentLevel;
        }
        else
        {
            _nodeImage.sprite = _lockedSprite;
            _nodeButton.interactable = false; // Cannot press future levels
        }
    }

    private void OnNodeClicked()
    {
        // TODO: Add SFX here via AudioManager

        // LevelLoader transition to scene
        LevelLoader.Instance.LoadNextScene(_sceneToLoad);
    }
}
