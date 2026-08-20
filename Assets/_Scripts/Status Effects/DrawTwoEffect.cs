using UnityEngine;

[CreateAssetMenu(menuName = "Cheating Geese/Card Effects/Draw Two")]
public class DrawTwoEffect : StatusEffect
{
    public override void OnPlay(CharacterStats owner, CharacterStats target, DeckManager playerDeck)
    {
        owner.TakeDamage(5);

        // Draw 2
        playerDeck.DrawAmount(2);
    }
}
