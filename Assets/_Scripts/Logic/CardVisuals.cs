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

    [Header("Status References")]
    [SerializeField] private Image _statusImage;
    [SerializeField] private TMP_Text _statusText;

    /// <summary>
    /// Reads the CardData and updates the 3D text meshes on the prefab
    /// </summary>
    public void Setup(CardData cardData)
    {
        if (cardData == null)
        {
            return;
        }

        // Check if a status card
        if (cardData.IsStatusCard)
        {
            // Hide normal card UI
            if (_rankTextUpper != null)
            {
                _rankTextUpper.gameObject.SetActive(false);
            }

            if (_rankTextLower != null)
            {
                _rankTextLower.gameObject.SetActive(false);
            }

            if (_suitImage != null)
            {
                _suitImage.gameObject.SetActive(false);
            }

            // Show status image
            if (_statusImage != null)
            {
                _statusImage.gameObject.SetActive(true);
                _statusImage.sprite = cardData.StatusSprite;
            }

            // Show the status text
            if (_statusText != null)
            {
                _statusText.gameObject.SetActive(true);

                // On the table, just show the name
                _statusText.text = cardData.StatusName;
            }
        }
        else
        {
            // Turn the normal card UI on
            // Update both the top-left and bottom-right rank numbers
            if (_rankTextUpper != null)
            {
                _rankTextUpper.gameObject.SetActive(true);
                _rankTextUpper.text = cardData.RankDisplayName;
            }

            if (_rankTextLower != null)
            {
                _rankTextLower.gameObject.SetActive(true);
                _rankTextLower.text = cardData.RankDisplayName;
            }

            // Dynamically assign the correct sprite
            if (_suitImage != null)
            {
                _suitImage.gameObject.SetActive(true);
                _suitImage.sprite = cardData.SuitSprite;
            }

            // Hide status text
            if (_statusText != null && _statusImage != null)
            {
                _statusText.gameObject.SetActive(false);
                _statusImage.gameObject.SetActive(false);
            }
        }
    }
}
