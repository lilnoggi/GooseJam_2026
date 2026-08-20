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

    [Header("Status Card Visuals")]
    [SerializeField] private Image _statusImage;
    [SerializeField] private TMP_Text _statusDescriptionText;

    [SerializeField] private float _hoverHight =25f; //height when hovered
    [SerializeField] private float _selectedHeight = 35f; //the height that the card will move upwards when it is selected

    [SerializeField] private float _moveSpeed = 12f; //speed of card movement
    [SerializeField] private float _drawMoveSpeed = 4f; //speed the new card slides into the hand

    public CardData CardData {get; private set;}//the card that the UI is showing

    private Action<PlayerCardView> _onCardClicked; //lets the card talk to manager to say that it was clicked

    private bool _isSelected; //is card selceted

    private bool _isHovered; //is card hovered

    private bool _canInteract = true;

    private bool _isDrawingIntoHand; 

    private Vector2 _targetPosition; //where the card moves to

    public void Setup(CardData cardData, Action<PlayerCardView> onCardClicked)//method called when the card is created
    {
        CardData = cardData;
        _onCardClicked = onCardClicked;

        // Check if it is a stuats card
        if (cardData.IsStatusCard)
        {
            // Hide standard suit image and numbers
            if (_suitImage != null && _rankTextLower != null && _rankTextUpper != null)
            {
                _suitImage.gameObject.SetActive(false);
                _rankTextLower.gameObject.SetActive(false);
                _rankTextUpper.gameObject.SetActive(false);
            }

            // Turn on status card image
            if (_statusImage != null)
            {
                _statusImage.gameObject.SetActive(true);
                _statusImage.sprite = cardData.StatusSprite;
            }

            // Show description text 
            if (_statusDescriptionText != null)
            {
                _statusDescriptionText.gameObject.SetActive(true);

                // Make the text bold and clear for prototypeing
                _statusDescriptionText.text = $"<b>{cardData.StatusName}</b>\n\n{cardData.StatusDescription}";
            }
        }
        else
        {
            // It is a normal card
            if (_statusDescriptionText != null && _statusImage != null)
            {
                _statusDescriptionText.gameObject.SetActive(false);
                _statusImage.gameObject.SetActive(false);
            }
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
        // Safety check: If the visual root was destroyed mid-animation, stop running update
        if (_visualRoot == null)
        {
            return;
        }
           
        float currentSpeed = _isDrawingIntoHand ? _drawMoveSpeed : _moveSpeed; //use a slower speed when a newly drawn card is moving into the hand

        _visualRoot.anchoredPosition = Vector2.Lerp(_visualRoot.anchoredPosition, _targetPosition, Time.deltaTime * currentSpeed);

        //once the card is basically in place stop the draw animation
        if (_isDrawingIntoHand && Vector2.Distance( _visualRoot.anchoredPosition, _targetPosition) < 0.5f)
        {
            _visualRoot.anchoredPosition = _targetPosition;
            _isDrawingIntoHand = false;
        }
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

    public void PrepareDrawAnimation()
    {
        //hide the 2D card while the 3D card is travelling towards the player
        _visualRoot.gameObject.SetActive(false);
    }

    public void PlayDrawAnimation(float verticalOffset)
    {
        //the 3D card has reached the player so show the 2D version
        _visualRoot.gameObject.SetActive(true);

        //start it off to the below its normal hand position
        _visualRoot.anchoredPosition = new Vector2( 0f, verticalOffset);

        //its normal resting position is the centre of its card slot
        _targetPosition = Vector2.zero;

        _isDrawingIntoHand = true;
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
