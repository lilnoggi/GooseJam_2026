using UnityEngine;

[CreateAssetMenu(menuName = "Cheating Geese/Card Effects/Hollow Promise")]
public class HollowPromiseEffect : StatusEffect
{
    public override bool OnTurnStart(CharacterStats owner, int turnsHeld)
    {
        // If it sits in the hand for 3 full turns without being used
        if (turnsHeld == 3)
        {
            owner.TakeDamage(15);
            return true; // Tell manager to discard it
        }

        return false; // Keep in hand for turns 1 and 2
    }

    public override bool OnCaughtLying(CharacterStats owner)
    {
        // Returning true tells TurnManager to cancel the incoming damage and discard this card
        return true;
    }
}
