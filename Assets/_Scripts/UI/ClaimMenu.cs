using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ClaimMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _claimContainer;
    [SerializeField] private TMP_Dropdown _suitDropdown;
    [SerializeField] private TMP_Dropdown _rankDropdown;
    [SerializeField] private TMP_Dropdown _targetDropdown;
    [SerializeField] private Button _submitClaimButton;

    [Header("System References")]
    [SerializeField] private TurnManager _turnManager;

    private List<CardData> _trueCards;

    private void Awake()
    {
        _submitClaimButton.onClick.AddListener(SubmitClaim);

        // Ensure the menu is hidden when the game starts
        _claimContainer.SetActive(false);
    }

    /// <summary>
    /// Called by PlayerHandManager when the player clicks "Play Cards"
    /// </summary>
    public void ShowMenu(List<CardData> selectedCards)
    {
        _trueCards = selectedCards;
        _claimContainer.SetActive(true);
    }

    private void SubmitClaim()
    {
        // Read the dropdown values (Casting the integer index directly to the Enums)
        CardSuit claimedSuit = (CardSuit)_suitDropdown.value;
        CardRank claimedRank = (CardRank)_rankDropdown.value;

        // Target dropdown options should be 0=Left, 1=Center, 2=Right
        // In TurnSeat enum, Player is 0, Left is 1, so add 1 to the dropdown
        TurnSeat target = (TurnSeat)(_targetDropdown.value + 1);

        // Put this data into the container
        ClaimData newClaim = new ClaimData(_trueCards, claimedSuit, claimedRank, target);

        // Log to check it works
        Debug.Log($"CLAIM MADE! Real Cards: {_trueCards.Count} | Claimed: {claimedRank} of {claimedSuit} | Target: {target}");

        _turnManager.ProcessPlayerClaim(newClaim);

        // Hide the menu and reset for the next turn
        _claimContainer.SetActive(false);
    }
}
