using UnityEngine;

[CreateAssetMenu(menuName = "Cheating Geese/Card Effects/Loaded Dice")]
public class LoadedDiceEffect : StatusEffect
{
    public override void OnPlay(CharacterStats owner, CharacterStats target, DeckManager playerDeck)
    {
        if (target != null)
        {
            target.IncreaseParanoia(100);
        }
    }
}
