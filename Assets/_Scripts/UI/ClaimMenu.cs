using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ClaimMenu : MonoBehaviour
{
    // Singleton so the 3D enemies can easily find this menu!!!
    public static ClaimMenu Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject _claimContainer;
    [SerializeField] private TMP_Dropdown _suitDropdown;
    [SerializeField] private TMP_Dropdown _rankDropdown;

    [Header("System References")]
    [SerializeField] private TurnManager _turnManager;

    private List<CardData> _trueCards;

    // Public getter so enemies know if they should light up or ignore the mouse
    public bool IsActive => _claimContainer.activeInHierarchy;

    private void Awake()
    {
        Instance = this;

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

    // Now called directly by the 3D enemy when clicked
    public void SubmitClaimWithTarget(TurnSeat targetEnemy)
    {
        // Read the dropdown values (Casting the integer index directly to the Enums)
        CardSuit claimedSuit = (CardSuit)_suitDropdown.value;
        CardRank claimedRank = (CardRank)_rankDropdown.value;

        // Put this data into the container using the seat passed by the clicked enemy
        ClaimData newClaim = new ClaimData(_trueCards, claimedSuit, claimedRank, targetEnemy);

        _turnManager.ProcessPlayerClaim(newClaim);

        // Hide the menu and reset for the next turn
        _claimContainer.SetActive(false);
    }
}
