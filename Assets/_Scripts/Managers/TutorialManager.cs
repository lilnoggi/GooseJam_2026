using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _tutorialPanel;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private TextMeshProUGUI _counterText; 
    [SerializeField] private TextMeshProUGUI _continueButtonText;

    [Header("System References")]
    [SerializeField] private TurnController _turnController;
    [SerializeField] private PlayerHandManager _playerHandManager;
    [SerializeField] private DeckManager _playerDeck;
    [SerializeField] private DeckManager _centerEnemyDeck;
    [SerializeField] private CharacterStats _centerEnemyStats;
    [SerializeField] private CharacterStats _playerStats;
    [SerializeField] private ClaimMenu _claimMenu; 
    [SerializeField] private PlayerDecisionMenu _playerDecisionMenu; 

    [Header("Card References")]
    [SerializeField] private CardData _bloodCard;
    [SerializeField] private CardData _boneCard;
    [SerializeField] private CardData _rotCard;
    [SerializeField] private CardData _featherCard;
    [SerializeField] private CardData[] _rubbishCards;
    [SerializeField] private CardData _appleCard;

    private bool _isWaitingForClick;

    private void Start()
    {
        _tutorialPanel.SetActive(false);
        
        _playerDeck.SetForcedCards(new List<CardData> { _bloodCard, _bloodCard, _bloodCard, _bloodCard, _bloodCard });
        _centerEnemyDeck.SetForcedCards(new List<CardData> { _bloodCard, _bloodCard, _bloodCard, _bloodCard, _bloodCard });
        
        // Start the single continuous sequence
        StartCoroutine(TutorialSequenceRoutine());
    }

    private IEnumerator TutorialSequenceRoutine()
    {
        // =========================================================
        // TURN 1: HONEST BLOOD ATTACK
        // =========================================================
        if (_playerDecisionMenu != null) _playerDecisionMenu.HideCheatButtonForTutorial = true;
        _centerEnemyStats.GetComponent<EnemyAI>().ForceHonestPlay = true;
        _centerEnemyStats.GetComponent<EnemyAI>().ForcedCardPlayCount = 3;

        yield return new WaitUntil(() => _turnController.IsPlayerTurn);
        yield return new WaitUntil(() => _playerHandManager.CardViews.Count > 0);
        yield return new WaitUntil(() => !_playerHandManager.IsDrawingCards);

        yield return ShowPopup("Welcome to the table. Your goal is simple: eliminate the enemy before you are cooked.", 1, 2);
        yield return ShowPopup("Blood cards deal direct damage. Select 3 Blood cards and hit 'Play'.", 2, 2);

        _playerHandManager.LockSelectionToSuit(CardSuit.Blood);
        _playerHandManager.SetRequiredCardCount(3);
        _playerHandManager.SetSkipLocked(true);

        yield return new WaitUntil(() => _claimMenu.gameObject.activeInHierarchy);
        _claimMenu.LockSuit(CardSuit.Blood);

        yield return ShowPopup("This is the Claim Panel. Every time you play cards, you must declare what suit you are playing.", 1, 2);
        yield return ShowPopup("Because we are playing honestly, select the 'Blood' suit and hit Confirm! Then select the enemy by clicking it.", 2, 2);

        // Secretly heal the enemy so they NEVER accidentally die during the tutorial
        _centerEnemyStats.Heal(500);
        int startingHealth = _centerEnemyStats.CurrentHealth;
        yield return new WaitUntil(() => _centerEnemyStats.CurrentHealth < startingHealth);
        yield return new WaitForSeconds(2.5f);

        _playerHandManager.UnlockSelection();
        _playerHandManager.SetRequiredCardCount(0);
        _playerHandManager.SetSkipLocked(false);

        // ONLY clear the player's hand so they get a fresh lesson. Leave the enemy's hand alone!
        _playerDeck.DiscardCards(new List<CardData>(_playerDeck.Hand));

        _playerDeck.SetForcedCards(new List<CardData> { _boneCard, _boneCard, _boneCard, _boneCard, _boneCard });
        _centerEnemyDeck.SetForcedCards(new List<CardData> { _bloodCard, _bloodCard, _bloodCard, _bloodCard, _bloodCard });

        yield return ShowPopup("Excellent! They believed you, and your Blood cards dealt direct damage.", 1, 2);
        yield return ShowPopup("Let's see how they retaliate. The turn will now pass to the enemy.", 2, 2);

        // Wait for the Enemy's Turn to finish, then wait for the Player's Turn to start again
        yield return new WaitWhile(() => _turnController.IsPlayerTurn);
        yield return new WaitUntil(() => _turnController.IsPlayerTurn);
        yield return new WaitUntil(() => !_playerHandManager.IsDrawingCards);


        // =========================================================
        // TURN 2: DEFENDING WITH SHIELDS
        // =========================================================
        yield return ShowPopup("Ouch! The enemy attacked you.", 1, 3);
        yield return ShowPopup("Bone cards are your armor. They absorb incoming damage until they shatter.", 2, 3);
        yield return ShowPopup("Select 3 Bone cards, hit Play, and claim them as Bone to build a shield!", 3, 3);

        _playerHandManager.LockSelectionToSuit(CardSuit.Bone);
        _playerHandManager.SetRequiredCardCount(3);
        _playerHandManager.SetSkipLocked(true);

        yield return new WaitUntil(() => _claimMenu.gameObject.activeInHierarchy);
        _claimMenu.LockSuit(CardSuit.Bone);

        yield return ShowPopup("Select the Bone suit to confirm your shield.", 1, 2);
        yield return ShowPopup("Your total shield will equal the sum of the Bone cards you play.", 2, 2);

        yield return new WaitUntil(() => _playerStats.CurrentShield > 0);
        yield return new WaitForSeconds(2.5f);

        _playerHandManager.UnlockSelection();
        _playerDeck.DiscardCards(new List<CardData>(_playerDeck.Hand));

        _playerDeck.SetForcedCards(new List<CardData> { _featherCard, _featherCard, _featherCard, _featherCard, _featherCard });
        _centerEnemyDeck.SetForcedCards(new List<CardData> { _bloodCard, _bloodCard, _bloodCard, _bloodCard, _bloodCard });

        yield return ShowPopup("Great! Your shield is up. It will absorb the enemy's next attack.", 1, 2);
        yield return ShowPopup("Let's see how they retaliate. The turn will now pass to the enemy.", 2, 2);

        yield return new WaitWhile(() => _turnController.IsPlayerTurn);
        yield return new WaitUntil(() => _turnController.IsPlayerTurn);
        yield return new WaitUntil(() => !_playerHandManager.IsDrawingCards);


        // =========================================================
        // TURN 3: DEFENDING WITH FEATHERS
        // =========================================================
        yield return ShowPopup("Wow your shield protected you! Let's go over another way to defend!", 1, 3);
        yield return ShowPopup("Feathers make you evasive, allowing you to completely dodge an incoming attack.", 2, 3);
        yield return ShowPopup("Select a feather card!", 3, 3);

        _playerHandManager.LockSelectionToSuit(CardSuit.Feather);
        _playerHandManager.SetRequiredCardCount(1);
        _playerHandManager.SetSkipLocked(true);

        yield return new WaitUntil(() => _claimMenu.gameObject.activeInHierarchy);
        _claimMenu.LockSuit(CardSuit.Feather);

        yield return ShowPopup("Select the Feather suit to confirm your dodge.", 1, 2);
        yield return ShowPopup("Every Feather card played grants you one Dodge token. One card equals one dodge!", 2, 2);

        yield return new WaitUntil(() => _playerStats.DodgeTokens > 0);
        yield return new WaitForSeconds(2.5f);

        _playerHandManager.UnlockSelection();
        _playerDeck.DiscardCards(new List<CardData>(_playerDeck.Hand));

        _playerDeck.SetForcedCards(new List<CardData> { _rubbishCards[0], _rubbishCards[1], _rubbishCards[2], _rubbishCards[0], _rubbishCards[2] });
        _centerEnemyDeck.SetForcedCards(new List<CardData> { _bloodCard, _bloodCard, _bloodCard, _bloodCard, _bloodCard });

        yield return ShowPopup("Great! You have a dodge!", 1, 2);
        yield return ShowPopup("Let's see how they retaliate!", 2, 2);

        yield return new WaitWhile(() => _turnController.IsPlayerTurn);
        yield return new WaitUntil(() => _turnController.IsPlayerTurn);
        yield return new WaitUntil(() => !_playerHandManager.IsDrawingCards);


        // =========================================================
        // TURN 4: LYING
        // =========================================================
        yield return ShowPopup("Notice how you did not take damage! Power of the feather, am I right?", 1, 3);
        yield return ShowPopup("Sometimes you get a terrible hand. But at this table, you can play whatever you want… if you don't get caught.", 2, 3);
        yield return ShowPopup("Select 3 rubbish cards, but lie and claim they are all Blood!", 3, 3);

        _playerHandManager.UnlockSelection();
        _playerHandManager.SetRequiredCardCount(3);
        _playerHandManager.SetSkipLocked(true);

        yield return new WaitUntil(() => _claimMenu.gameObject.activeInHierarchy);
        _claimMenu.LockSuit(CardSuit.Blood);

        yield return ShowPopup("Select the Blood suit to confirm your lie.", 1, 2);
        yield return ShowPopup("If the enemy buys your bluff, you deal double damage. If they catch you, the damage backfires onto you!", 2, 2);

        _centerEnemyStats.SetParanoia(0);
        _centerEnemyStats.Heal(500); // Heal them so the bluff damage doesn't kill them!
        
        startingHealth = _centerEnemyStats.CurrentHealth;
        yield return new WaitUntil(() => _centerEnemyStats.CurrentHealth < startingHealth);
        yield return new WaitForSeconds(2.5f);

        _playerHandManager.UnlockSelection();
        _playerHandManager.SetRequiredCardCount(0);
        _playerHandManager.SetSkipLocked(false);
        _playerDeck.DiscardCards(new List<CardData>(_playerDeck.Hand));

        _playerDeck.SetForcedCards(new List<CardData> { _rotCard, _rotCard, _rotCard, _rotCard, _rotCard });
        _centerEnemyDeck.SetForcedCards(new List<CardData> { _boneCard, _boneCard, _boneCard, _boneCard, _boneCard });

        yield return ShowPopup("They bought it! Your rubbish cards just dealt double Blood damage.", 1, 2);
        yield return ShowPopup("Let's see how they retaliate!", 2, 2);

        yield return new WaitWhile(() => _turnController.IsPlayerTurn);
        yield return new WaitUntil(() => _turnController.IsPlayerTurn);
        yield return new WaitUntil(() => !_playerHandManager.IsDrawingCards);


        // =========================================================
        // TURN 5: PASSIVE ATTACKING (Poison)
        // =========================================================
        yield return ShowPopup("Rot cards are toxic. They inflict lingering Poison stacks on your target.", 1, 3);
        yield return ShowPopup("Poison bypasses shields, dealing direct damage at the start of the victim's turn before decreasing by one stack.", 2, 3);
        yield return ShowPopup("Rot is perfect for chipping away at heavily shielded enemies. Poison them! Select 3 Rot cards.", 3, 3);

        _playerHandManager.LockSelectionToSuit(CardSuit.Rot);
        _playerHandManager.SetRequiredCardCount(3);
        _playerHandManager.SetSkipLocked(true);
        _centerEnemyStats.SetParanoia(0);

        yield return new WaitUntil(() => _claimMenu.gameObject.activeInHierarchy);
        _claimMenu.LockSuit(CardSuit.Rot);

        yield return new WaitUntil(() => _centerEnemyStats.PoisonStacks > 0);
        yield return new WaitForSeconds(2.5f);

        _playerHandManager.UnlockSelection();
        _playerHandManager.SetRequiredCardCount(0);
        _playerHandManager.SetSkipLocked(false);
        _playerDeck.DiscardCards(new List<CardData>(_playerDeck.Hand));

        // Give the enemy Bone cards so they effortlessly pick 3, but FORCE them to bluff!
        _centerEnemyDeck.DiscardCards(new List<CardData>(_centerEnemyDeck.Hand));
        _playerDeck.SetForcedCards(new List<CardData> { _boneCard, _boneCard, _boneCard, _boneCard, _boneCard });
        _centerEnemyDeck.SetForcedCards(new List<CardData> { _boneCard, _boneCard, _boneCard, _boneCard, _boneCard });

        _centerEnemyStats.GetComponent<EnemyAI>().ForceHonestPlay = false; 
        _centerEnemyStats.GetComponent<EnemyAI>().ForceBluffPlay = true; // MUST LIE!
        if (_playerDecisionMenu != null) _playerDecisionMenu.HideCheatButtonForTutorial = false; 

        yield return ShowPopup("Excellent! The Rot cards successfully applied Poison stacks.", 1, 2);
        yield return ShowPopup("Let's see what happens at the start of their turn.", 2, 2);


        // =========================================================
        // TURN 6: CALLING CHEAT (Enemy Turn)
        // =========================================================
        yield return new WaitUntil(() => _playerDecisionMenu.gameObject.activeInHierarchy);

        _playerDecisionMenu.DisableTimerForTutorial();

        yield return ShowPopup("Did you see their health drop? The poison bypassed their shield and damaged them directly!", 1, 3);
        yield return ShowPopup("Now, look at the enemy's Paranoia meter. The higher it gets, the more desperate they become.", 2, 3);
        yield return ShowPopup("That claim looks highly suspicious. The timer is paused. Hit <color=yellow>CHEAT!</color> to call them out!", 3, 3);

        _centerEnemyStats.Heal(500); 
        startingHealth = _centerEnemyStats.CurrentHealth;

        yield return new WaitUntil(() => _centerEnemyStats.CurrentHealth < startingHealth);
        yield return new WaitForSeconds(2.5f);

        yield return ShowPopup("You caught them! The damage backfired on them.", 1, 2);
        yield return ShowPopup("Let's see what happens when the enemy gets suspicious of you.", 2, 2);

        _playerDeck.SetForcedCards(new List<CardData> { _rubbishCards[0], _rubbishCards[1], _rubbishCards[2], _rubbishCards[0], _rubbishCards[2] });
        _centerEnemyDeck.SetForcedCards(new List<CardData> { _bloodCard, _bloodCard, _bloodCard, _bloodCard, _bloodCard });
        
        // Remove the bluff override so they can play naturally
        _centerEnemyStats.GetComponent<EnemyAI>().ForceBluffPlay = false;

        yield return new WaitWhile(() => _turnController.IsPlayerTurn);
        yield return new WaitUntil(() => _turnController.IsPlayerTurn);
        yield return new WaitUntil(() => !_playerHandManager.IsDrawingCards);


        // =========================================================
        // TURN 7: GETTING CAUGHT
        // =========================================================
        _centerEnemyStats.SetParanoia(100);

        yield return ShowPopup("The enemy's actions change depending on their paranoia meter.", 1, 2);
        yield return ShowPopup("Highly paranoid enemies are more likely to call your bluffs. Let's see what happens if you try to lie now.", 2, 2);

        _playerHandManager.UnlockSelection();
        _playerHandManager.SetRequiredCardCount(3);
        _playerHandManager.SetSkipLocked(true);

        yield return new WaitUntil(() => _claimMenu.gameObject.activeInHierarchy);
        _claimMenu.LockSuit(CardSuit.Blood);

        yield return ShowPopup("Select the Blood suit to confirm your lie.", 1, 1);

        startingHealth = _playerStats.CurrentHealth;
        yield return new WaitUntil(() => _playerStats.CurrentHealth < startingHealth);
        yield return new WaitForSeconds(2.5f);

        _playerHandManager.UnlockSelection();
        _playerHandManager.SetRequiredCardCount(0);
        _playerHandManager.SetSkipLocked(false);
        _playerDeck.DiscardCards(new List<CardData>(_playerDeck.Hand));

        _playerStats.Heal(100);
        _centerEnemyStats.Heal(100);
        _centerEnemyStats.SetParanoia(0);
        _centerEnemyStats.ClearPoison();
        _playerStats.ClearPoison();

        _playerDeck.SetForcedCards(new List<CardData> { _appleCard, _boneCard, _boneCard, _boneCard, _boneCard });
        _centerEnemyDeck.SetForcedCards(new List<CardData> { _bloodCard, _bloodCard, _bloodCard, _bloodCard, _bloodCard });

        _centerEnemyStats.GetComponent<EnemyAI>().ForceHonestPlay = true;
        _centerEnemyStats.GetComponent<EnemyAI>().ForcedCardPlayCount = 3;
        if (_playerDecisionMenu != null) _playerDecisionMenu.HideCheatButtonForTutorial = true;

        yield return ShowPopup("The enemy called your bluff! You took the damage instead.", 1, 2);
        yield return ShowPopup("Always check your target's paranoia before risking a lie.", 2, 2);

        yield return new WaitWhile(() => _turnController.IsPlayerTurn);
        yield return new WaitUntil(() => _turnController.IsPlayerTurn);
        yield return new WaitUntil(() => !_playerHandManager.IsDrawingCards);


        // =========================================================
        // TURN 8: STATUS CARDS
        // =========================================================
        yield return ShowPopup("Occasionally, you will draw a Status Card. These break the normal rules of the game.", 1, 3);
        yield return ShowPopup("Since time is now unpaused, hover your mouse over the Rotten Apple to see its unique effects.", 2, 3);
        yield return ShowPopup("Play 3 Bone cards to end your turn. Watch what the Apple does at the start of your next turn!", 3, 3);

        _playerHandManager.LockSelectionToSuit(CardSuit.Bone);
        _playerHandManager.SetRequiredCardCount(3);
        _playerHandManager.SetSkipLocked(true);

        yield return new WaitUntil(() => _claimMenu.gameObject.activeInHierarchy);
        _claimMenu.LockSuit(CardSuit.Bone);

        yield return new WaitWhile(() => _turnController.IsPlayerTurn);
        yield return new WaitUntil(() => _turnController.IsPlayerTurn);
        yield return new WaitUntil(() => !_playerHandManager.IsDrawingCards);


        // =========================================================
        // TURN 9: FREE PLAY TRANSITION
        // =========================================================
        yield return ShowPopup("Did you see the Apple tick? It just passively dealt 15 damage to you!", 1, 2);
        yield return ShowPopup("You now know everything you need to survive. Defeat the enemy to complete the tutorial!", 2, 2);

        _playerStats.Heal(100);
        _centerEnemyStats.Heal(100);
        _centerEnemyStats.SetParanoia(0);
        _centerEnemyStats.ClearPoison();
        _playerStats.ClearPoison();

        _centerEnemyStats.GetComponent<EnemyAI>().ForceHonestPlay = false;
        _centerEnemyStats.GetComponent<EnemyAI>().ForcedCardPlayCount = 0;
        _centerEnemyStats.GetComponent<EnemyAI>().ForceBluffPlay = false; 
        if (_playerDecisionMenu != null) _playerDecisionMenu.HideCheatButtonForTutorial = false;

        _playerHandManager.UnlockSelection();
        _playerHandManager.SetRequiredCardCount(0);
        _playerHandManager.SetSkipLocked(false);

        // Clear out the forced lists so the deck returns to pulling random cards!
        _playerDeck.SetForcedCards(new List<CardData>());
        _centerEnemyDeck.SetForcedCards(new List<CardData>());

        // REMOVED THE HAND DISCARD LOGIC!
        // The player keeps their Apple and the game naturally draws normal cards to refill the 3 missing slots!
    }

    private IEnumerator ShowPopup(string text, int step, int totalSteps)
    {
        Time.timeScale = 0f; 
        
        _dialogueText.text = text;
        if (_counterText != null) _counterText.text = $"{step}/{totalSteps}";
        
        if (_continueButtonText != null)
        {
            _continueButtonText.text = (step == totalSteps) ? "Play" : "Continue";
        }
        
        _tutorialPanel.SetActive(true);
        
        _isWaitingForClick = true;
        yield return new WaitUntil(() => !_isWaitingForClick);
        
        _tutorialPanel.SetActive(false);
        Time.timeScale = 1f; 
    }

    public void NextPopup()
    {
        AudioManager.Instance.PlaySFX(SFXType.Select);
        _isWaitingForClick = false; 
    }
}