using UnityEngine;
using UnityEngine.UI;

public class MapNode : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] private int _nodeIndex;  // 0 for Tutorial, 1 for Swamp_01a
    [SerializeField] private string _sceneToLoad;

    [Header("Visuals")]
    [SerializeField] private Sprite _lockedSprite;
    [SerializeField] private Sprite _unlockedSprite;

    private Image _nodeImage;
    private Button _nodeButton;

    // GETTERS
    public int NodeIndex => _nodeIndex;

    private void Awake()
    {
        _nodeImage = GetComponent<Image>();
        _nodeButton = GetComponent<Button>();

        _nodeButton.onClick.AddListener(OnNodeClicked);

        if (_nodeIndex == 0)
        {
            // Is tutorial level
            SetState(true, false);
        }
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
            _nodeButton.interactable = true;
        }
        else
        {
            _nodeImage.sprite = _lockedSprite;
            _nodeButton.interactable = false; // Cannot press future levels
        }
    }

    private void OnNodeClicked()
    {
        AudioManager.Instance.PlaySFX(SFXType.Select);

        // LevelLoader transition to scene
        LevelLoader.Instance.LoadNextScene(_sceneToLoad);
    }
}
