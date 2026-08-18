using UnityEngine;

[CreateAssetMenu(menuName = "Cheating Geese/Card Effects/Intuition")]
public class IntuitionEffect : StatusEffect
{
    public override void OnPlay(CharacterStats owner, CharacterStats target, DeckManager playerDeck)
    {
        if (target != null)
        {
            // Find the TurnManager 
            TurnController turnController = FindAnyObjectByType<TurnController>();

            if (turnController != null)
            {
                // Ask the TurnManager for the target's specific deck
                DeckManager enemyDeck = turnController.GetDeckForCharacter(target);

                // Open the UI
                IntuitionUI.Instance.ShowEnemyHand(target, enemyDeck);
            }
        }
    }
}
