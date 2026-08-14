using UnityEngine;
using TMPro;

public class CardVisuals : MonoBehaviour
{
    [SerializeField] private TMP_Text _rankText;
    [SerializeField] private TMP_Text _suitText;

    /// <summary>
    /// Reads the CardData and updates the 3D text meshes on the prefab
    /// </summary>
    public void Setup(CardData cardData)
    {
        if (cardData == null)
        {
            return;
        }

        if (_rankText != null)
        {
            _rankText.text = cardData.RankDisplayName;
        }

        if (_suitText != null)
        {
            _suitText.text = cardData.Suit.ToString().ToUpper();
        }
    }
}
