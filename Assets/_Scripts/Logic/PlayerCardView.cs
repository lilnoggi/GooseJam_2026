using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class PlayerCardView : MonoBehaviour
{
    
    [SerializeField] private Button _button; //the button on the card

    [SerializeField] private RectTransform _visualRoot;//The part of the card that will move upwards when it is selected

    //the text for rank and suite
    [SerializeField] private TMP_Text _rankText;
    [SerializeField] private TMP_Text _suitText;

    [SerializeField] private float _selectedHeight = 35f; //the height that the card will move upwards when it is selected

    public CardData CardData {get; private set;}//the card that the UI is showing

    private Action<PlayerCardView> _onCardClicked; //lets the card talk to manager to say that it was clicked

    public void Setup(CardData cardData, Action<PlayerCardView> onCardClicked)//method called when the card is created
    {
        CardData = cardData;
        _onCardClicked = onCardClicked;
        _rankText.text = cardData.RankDisplayName; //show rank
        _suitText.text = cardData.Suit.ToString().ToUpper();//show suit
        _button.onClick.RemoveAllListeners(); //cant trigger click event twice
        _button.onClick.AddListener(CardClicked); //runs method when button pressed
        SetSelected(false); //cards start in normal position

    }


    private void CardClicked()
    {
        _onCardClicked?.Invoke(this);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            //Moves the cars upwards
            _visualRoot.anchoredPosition = new Vector2(0f, _selectedHeight);
        }
        else
        {
            //put card back into its normal position
            _visualRoot.anchoredPosition = Vector2.zero;
        }
    }


}
