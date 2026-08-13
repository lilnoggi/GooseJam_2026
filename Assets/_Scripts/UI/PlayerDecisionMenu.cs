using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;

public class PlayerDecisionMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _decisionMenuContainer;
    [SerializeField] private TextMeshProUGUI _claimText;
    [SerializeField] private Button _passButton;
    [SerializeField] private Button _cheatButton;

    [Header("Settings")]
    [SerializeField] private float _decisionTimeLimit = 3.0f; // How many seconds the player has to call cheat
    [SerializeField] private Slider _timerSlider;

    // Action to send the player's choice back to the TurnManager
    private Action<bool> _onDecisionMade;
    private Coroutine _timerRoutine;

    private void Awake()
    {
        // Connect the button's to pass 'true' (Cheat) or 'false'(pass)
        _passButton.onClick.AddListener(() => MakeDecision(false));
        _cheatButton.onClick.AddListener(() => MakeDecision(true));

        _decisionMenuContainer.SetActive(false);
        _claimText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Pops open the menu and waits for the player to click a button
    /// </summary>
    public void ShowMenu(string enemyName, ClaimData claim, Action<bool> callback)
    {
        _claimText.gameObject.SetActive(true);
        _onDecisionMade = callback;

        // Display what the enemy is claiming
        _claimText.text =$"{enemyName} claims they played {claim.TrueCards.Count} cards as the {claim.ClaimedRank} of {claim.ClaimedSuit}";

        _decisionMenuContainer.SetActive(true);

        // Stop any leftover timers just in case, then start countdown
        if (_timerRoutine != null)
        {
            StopCoroutine(_timerRoutine);
        }

        _timerRoutine = StartCoroutine(TimerCountdownRoutine());
    }

    private IEnumerator TimerCountdownRoutine()
    {
        float timeRemaining = _decisionTimeLimit;

        if (_timerSlider != null)
        {
            _timerSlider.maxValue = _decisionTimeLimit;
            _timerSlider.value = _decisionTimeLimit;
        }

        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            // Update visual slider
            if (_timerSlider != null)
            {
                _timerSlider.value = timeRemaining;
            }

            yield return null;
        }

        // Time is up, player did not call cheat
        MakeDecision(false);
    }

    private void MakeDecision(bool calledCheat)
    {
        if (_timerRoutine != null)
        {
            StopCoroutine(_timerRoutine);
        }
        
        _decisionMenuContainer.SetActive(false);

        // Push the result back to the turnmanager
        _onDecisionMade?.Invoke(calledCheat);
    }
}
