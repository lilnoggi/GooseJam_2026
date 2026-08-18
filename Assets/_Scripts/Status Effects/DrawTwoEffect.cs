using UnityEngine;

[CreateAssetMenu(menuName = "Cheating Geese/Card Effects/Draw Two")]
public class DrawTwoEffect : StatusEffect
{
    public override void OnPlay(CharacterStats owner, CharacterStats target, DeckManager playerDeck)
    {
        owner.TakeDamage(5);

        // Draw 1 to replace ITSELF + 2 extra = 7 cards
        playerDeck.DrawAmount(3);
    }
}
