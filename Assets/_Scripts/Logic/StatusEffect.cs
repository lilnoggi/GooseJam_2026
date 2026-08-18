using UnityEngine;

public abstract class StatusEffect : ScriptableObject
{
    /// <summary>
    /// Called the moment the card is actively played from the hand
    /// </summary>
    public virtual void OnPlay(CharacterStats owner, CharacterStats target, DeckManager playerDeck)
    {
        
    }

    /// <summary>
    /// Called automatically at the start of the player's turn if it's sitting in the hand.
    /// Returns TRUE if the card should be discarded after this effect
    /// </summary>
    public virtual bool OnTurnStart(CharacterStats owner, int turnsHeld)
    {
        return false;
    }

    /// <summary>
    /// Called during a standoff if the player gets caught in a lie while holding this card
    /// </summary>
    public virtual bool OnCaughtLying(CharacterStats owner)
    {
        return false;
    }
}
