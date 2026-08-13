using System;
using UnityEngine;

[RequireComponent(typeof(Collider))] // Ensure enemy has a physical box
public class EnemyTarget : MonoBehaviour
{
    [SerializeField] private TurnSeat _enemySeat; // Which seat is this specific enemy sat in?
    [SerializeField] private Light _spotLight; // The light object to turn on when hovered

    [Header("System References")]
    [SerializeField] private TurnManager _turnManager;

    private Color _originalColour; // Remember the default spotlight colour
    private bool _isEnemyTurn;

    private void Start()
    {
        // Ensure spotlight is off when game starts
        if (_spotLight != null)
        {
            _spotLight.enabled = false;

            // Save original colour
            _originalColour = _spotLight.color;
        }
    }

    private void OnEnable()
    {
        // Start listening for turn changes when the enemy is active
        if (_turnManager != null)
        {
            _turnManager.OnTurnChanged += HandleTurnChanged;
        }
    }

    private void OnDisable()
    {
        // Always stop listening when disabled
        if (_turnManager != null)
        {
            _turnManager.OnTurnChanged -= HandleTurnChanged;
        }
    }

    private void HandleTurnChanged(TurnSeat currentTurn)
    {
        // Is the current turn matching this enemy's seat?
        _isEnemyTurn = (currentTurn == _enemySeat);

        if (_spotLight != null)
        {
            if (_isEnemyTurn)
            {
                // It is this current enemies turn.
                _spotLight.color = Color.red;
                _spotLight.enabled = true;
            }
            else
            {
                // It is another enemie's or the player turn
                _spotLight.color = _originalColour;
                _spotLight.enabled = false;
            }
        }
    }

    private void OnMouseEnter()
    {
        // Only light up if it is not this enemy's turn and the player is currently in the Claim Phase
        if (!_isEnemyTurn && ClaimMenu.Instance != null && ClaimMenu.Instance.IsActive)
        {
            if (_spotLight != null)
            {
                _spotLight.color = _originalColour;
                _spotLight.enabled = true;
            }
        }
    }

    private void OnMouseExit()
    {
        // Turn off the light
        if (!_isEnemyTurn && _spotLight != null)
        {
            _spotLight.enabled = false;
        }
    }

    private void OnMouseDown()
    {
        // If player clicks enemy while the claim menu is open, submit attack
        if (!_isEnemyTurn && ClaimMenu.Instance != null && ClaimMenu.Instance.IsActive)
        {
            if (_spotLight != null)
            {
                // Turn the light green
                _spotLight.color = Color.green;
            }

            // Pass the current enemy's specific seat to the UI menu
            ClaimMenu.Instance.SubmitClaimWithTarget(_enemySeat);
        }
    }
}
