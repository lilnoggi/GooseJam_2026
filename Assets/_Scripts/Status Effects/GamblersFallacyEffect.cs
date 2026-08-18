using UnityEngine;

[CreateAssetMenu(menuName = "Cheating Geese/Card Effects/Gambler's Fallacy")]
public class GamblersFallacyEffect : StatusEffect
{
    public override void OnPlay(CharacterStats owner, CharacterStats target, DeckManager playerDeck)
    {
        // Discard whatever is currently in the hand
        playerDeck.DiscardRandomCards(playerDeck.HandCount);

        // Draw a fresh hand of 5
        playerDeck.DrawToFullHand();

        // Reduce player max health
        owner.ReduceMaxHealth(10);
    }
}
