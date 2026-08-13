using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class ClaimMenu : MonoBehaviour
{
    // Singleton so the 3D enemies can easily find this menu!!!
    public static ClaimMenu Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject _claimContainer;
    [SerializeField] private TextMeshProUGUI _cardDisplayText;
    [SerializeField] private TextMeshProUGUI _instructionText;

    [Header("System References")]
    [SerializeField] private TurnManager _turnManager;

    private List<CardData> _trueCards;
    private CardSuit _selectedSuit;
    private CardRank _selectedRank;

    // Track what step of the lie the player is currently on
    private enum ClaimPhase { Inactive, SuitPhase, RankPhase, TargetPhase }
    private ClaimPhase _currentPhase = ClaimPhase.Inactive;

    // Store Enums as arrays to easily cycle through
    private Array _suits = Enum.GetValues(typeof(CardSuit));
    private Array _ranks = Enum.GetValues(typeof(CardRank));
    private int _currentIndex = 0;

    // Public getter | 3D spotlight will only turn on during the final target phase
    public bool IsActive => _currentPhase == ClaimPhase.TargetPhase;

    private void Awake()
    {
        Instance = this;

        // Ensure the menu is hidden when the game starts
        _claimContainer.SetActive(false);
        if (_instructionText != null)
        {
            _instructionText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called by PlayerHandManager when the player clicks "Play Cards"
    /// </summary>
    public void ShowMenu(List<CardData> selectedCards)
    {
        _trueCards = selectedCards;
        _currentPhase = ClaimPhase.SuitPhase;
        _currentIndex = 0;

        _claimContainer.SetActive(true);
        if (_instructionText != null)
        {
            _instructionText.gameObject.SetActive(true);

            UpdateDisplay();
        }
    }

    // Attatched to a --> button on the UI
    public void CycleRight()
    {
        if (_currentPhase == ClaimPhase.SuitPhase)
        {
            _currentIndex = (_currentIndex + 1) % _suits.Length;
        }
        else if (_currentPhase == ClaimPhase.RankPhase)
        {
            _currentIndex = (_currentIndex + 1) % _ranks.Length;
        }

        UpdateDisplay();
    }

    // Attatched to a <-- button on the UI
    public void CycleLeft()
    {
        if (_currentPhase == ClaimPhase.SuitPhase)
        {
            _currentIndex--;
            if (_currentIndex < 0)
            {
               _currentIndex = _suits.Length - 1; 
            }
        }
        else if (_currentPhase == ClaimPhase.RankPhase)
        {
            _currentIndex--;
            if (_currentIndex < 0)
            {
                _currentIndex = _ranks.Length - 1;
            }
        }

        UpdateDisplay();
    }

    // Attatched to a "Confirm" button
    public void ConfirmSelection()
    {
        if (_currentPhase == ClaimPhase.SuitPhase)
        {
            // Save the suit and move to ranks
            _selectedSuit = (CardSuit)_suits.GetValue(_currentIndex);
            _currentPhase = ClaimPhase.RankPhase;
            _currentIndex = 0; // Reset index to start of ranks array

            UpdateDisplay();
        }
        else if (_currentPhase == ClaimPhase.RankPhase)
        {
            // Save the rank
            _selectedRank = (CardRank)_ranks.GetValue(_currentIndex);

            // Hide the blank card UI and activate Target Phase
            _claimContainer.SetActive(false);
            _currentPhase = ClaimPhase.TargetPhase;

            if (_instructionText != null)
            {
                _instructionText.text = "SELECT YOUR TARGET!";
            }
        }
    }

    // Now called directly by the 3D enemy when clicked
    public void SubmitClaimWithTarget(TurnSeat targetEnemy)
    {
        // Check to ensure player canno tclick an enemy early
        if (_currentPhase != ClaimPhase.TargetPhase)
        {
            return;
        }

        // Build the final claim and send it to the turnmanager
        ClaimData newClaim = new ClaimData(_trueCards, _selectedSuit, _selectedRank, targetEnemy);

        _turnManager.ProcessPlayerClaim(newClaim);

        // Hide the menu and reset for the next turn
        _currentPhase = ClaimPhase.Inactive;
        if (_instructionText != null)
        {
            _instructionText.gameObject.SetActive(false);
        }
    }

    private void UpdateDisplay()
    {
        if (_currentPhase == ClaimPhase.SuitPhase)
        {
            if (_instructionText != null)
            {
                _instructionText.text = "CHOOSE A SUIT";
                _cardDisplayText.text = _suits.GetValue(_currentIndex).ToString();
            }
        }
        else if (_currentPhase == ClaimPhase.RankPhase)
            {
                if (_instructionText != null)
                {
                    _instructionText.text = "CHOOSE A VALUE";
                    _cardDisplayText.text = _ranks.GetValue(_currentIndex).ToString();
                }
            }
    }
}
