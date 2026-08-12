using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class PlayerDecisionMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _decisionMenuContainer;
    [SerializeField] private TextMeshProUGUI _claimText;
    [SerializeField] private Button _passButton;
    [SerializeField] private Button _cheatButton;

    // Action to send the player's choice back to the TurnManager
    private Action<bool> _onDecisionMade;

    private void Awake()
    {
        // Connect the button's to pass 'true' (Cheat) or 'false'(pass)
        _passButton.onClick.AddListener(() => MakeDecision(false));
        _cheatButton.onClick.AddListener(() => MakeDecision(true));

        _decisionMenuContainer.SetActive(false);
    }

    /// <summary>
    /// Pops open the menu and waits for the player to click a button
    /// </summary>
    public void ShowMenu(string enemyName, ClaimData claim, Action<bool> callback)
    {
        _onDecisionMade = callback;

        // Display what the enemy is claiming
        _claimText.text =$"{enemyName} claims they played {claim.TrueCards.Count} cards as the {claim.ClaimedRank} of {claim.ClaimedSuit}";

        _decisionMenuContainer.SetActive(true);
    }

    private void MakeDecision(bool calledCheat)
    {
        _decisionMenuContainer.SetActive(false);

        // Push the result back to the turnmanager
        _onDecisionMade?.Invoke(calledCheat);
    }
}
