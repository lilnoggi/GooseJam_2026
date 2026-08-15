using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CardVisuals : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text _rankTextUpper;
    [SerializeField] private TMP_Text _rankTextLower;

    [Header("Image References")]
    [SerializeField] private Image _suitImage;

    /// <summary>
    /// Reads the CardData and updates the 3D text meshes on the prefab
    /// </summary>
    public void Setup(CardData cardData)
    {
        if (cardData == null)
        {
            return;
        }

        // Update both the top-left and bottom-right rank numbers
        if (_rankTextUpper != null)
        {
            _rankTextUpper.text = cardData.RankDisplayName;
        }

        if (_rankTextLower != null)
        {
            _rankTextLower.text = cardData.RankDisplayName;
        }

        // Dynamically assign the correct sprite
        if (_suitImage != null)
        {
            _suitImage.sprite = cardData.SuitSprite;
        }
    }
}
