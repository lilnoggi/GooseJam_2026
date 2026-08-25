using UnityEngine;

[CreateAssetMenu(menuName = "Cheating Geese/Card Effects/Rotten Apple")]
public class RottenAppleEffect : StatusEffect
{
    public override bool OnTurnStart(CharacterStats owner, int turnsHeld)
    {
        if (turnsHeld < 3)
        {
            owner.TakeDamage(15);
            return false;
        }
        else
        {
            // Health the player after 3 successful turns
            owner.Heal(75);
            return true;
        }
    }
}
