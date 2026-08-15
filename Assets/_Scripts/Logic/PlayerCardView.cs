using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class PlayerCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    
    [SerializeField] private Button _button; //the button on the card

    [SerializeField] private RectTransform _visualRoot;//The part of the card that will move upwards when it is selected

    [SerializeField] private GameObject _selectedGlowOutline; //border of card

    [Header("Card Visuals")]
    [SerializeField] private TMP_Text _rankTextUpper;
    [SerializeField] private TMP_Text _rankTextLower;
    [SerializeField] private Image _suitImage;

    [SerializeField] private float _hoverHight =25f; //height when hovered
    [SerializeField] private float _selectedHeight = 35f; //the height that the card will move upwards when it is selected

    [SerializeField] private float _moveSpeed = 12f; //speed of card movement

    public CardData CardData {get; private set;}//the card that the UI is showing

    private Action<PlayerCardView> _onCardClicked; //lets the card talk to manager to say that it was clicked

    private bool _isSelected; //is card selceted

    private bool _isHovered; //is card hovered

    private bool _canInteract = true;

    private Vector2 _targetPosition; //where the card moves to

    public void Setup(CardData cardData, Action<PlayerCardView> onCardClicked)//method called when the card is created
    {
        CardData = cardData;
        _onCardClicked = onCardClicked;

        // Apply Data
        if (_rankTextUpper != null)
        {
            _rankTextUpper.text = cardData.RankDisplayName;
        }

        if (_rankTextLower != null)
        {
            _rankTextLower.text = cardData.RankDisplayName;
        }

        if (_suitImage != null)
        {
            _suitImage.sprite = cardData.SuitSprite;
        }

        _button.onClick.RemoveAllListeners(); //cant trigger click event twice
        _button.onClick.AddListener(CardClicked); //runs method when button pressed

        _isSelected = false;
        _isHovered = false;

        _selectedGlowOutline.SetActive(false); // Turn off the glow object

        UpdateCardVisual();

    }

    private void Update()
    {
        _visualRoot.anchoredPosition = Vector2.Lerp( _visualRoot.anchoredPosition, _targetPosition , Time.deltaTime * _moveSpeed );
    }


    private void CardClicked()
    {
        if (!_canInteract) //do nothing if not turn
        {
            return;
        }
        _onCardClicked?.Invoke(this);
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateCardVisual();
    }

    public void SetInteractable(bool canInteract)
    {
        _canInteract = canInteract;

        //Disable the button during enemy turns.
        _button.interactable = canInteract; //disable button during enemy turns

        if (!canInteract) //when not turn remove hover
        {
            _isHovered = false;
        }

        UpdateCardVisual();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
       if(!_canInteract)
        {
            return;
        } 

        _isHovered = true;

        UpdateCardVisual();
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;

        UpdateCardVisual();
    }



    private void UpdateCardVisual()
    {
        if(_isSelected ) //selected cards go up and get tint
        {
            _targetPosition = new Vector2 (0f, _selectedHeight );

            _selectedGlowOutline.SetActive(true); // Turn on the glow object
        }

        else if (_isHovered && _canInteract) //hovered card move slightly and normal colour
        {
            _targetPosition = new Vector2 (0f, _hoverHight);

            _selectedGlowOutline.SetActive(false); // Turn off the glow object

        }

        else
        {
            _targetPosition = Vector2.zero;

            _selectedGlowOutline.SetActive(false); // Turn off the glow object
        }

    }

}
