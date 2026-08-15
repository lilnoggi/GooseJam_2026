using UnityEngine;
using UnityEngine.UIElements;

public class FloatingCards : MonoBehaviour
{
    [SerializeField] private float _floatHeight = 0.01f; //distance that the cards will float up and down
    [SerializeField] private float _floatSpeed = 0.8f ; //speed the cards move at

    [SerializeField] private float _moveSpeed = 0.5f; //speed the cards move to in the hand position

    private Vector3 _basePosition;
    private Vector3 _targetPosition;

    private float _randomOffset; //allows cards to float at different times

    private bool _hasBeenInitialised;


    // Update is called once per frame
    private void Update()
    {
        if (!_hasBeenInitialised)
        {
            return;
        }
        
        _basePosition = Vector3.MoveTowards( _basePosition, _targetPosition, _moveSpeed * Time.deltaTime);// move the card towards new hand position

        
        float newHeight = Mathf.Sin(Time.time * _floatSpeed + _randomOffset) * _floatHeight;// floating animation

        transform.localPosition = _basePosition + new Vector3(0f, newHeight, 0f);
    }

    public void SetStartingPosition(Vector3 position) //EnemyHandDisplay calls this method when the hand rearranges
    {
        _basePosition = position;
        _targetPosition = position;

        transform.localPosition = position;

        _randomOffset = Random.Range(0f, 6f);
        _floatSpeed = Random.Range(0.7f, 1.3f);

        _hasBeenInitialised = true;
    }


    public void MoveToPosition(Vector3 newPosition) //hand needs to rearange
    {
        // If this is a newly created card, place it correctly first... PLZ stop getting stuck in the body :'(
        if (!_hasBeenInitialised)
        {
            SetStartingPosition(newPosition);
            return;
        }

        _targetPosition = newPosition;
    }
}
