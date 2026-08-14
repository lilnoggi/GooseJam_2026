using UnityEngine;

public class FloatingCards : MonoBehaviour
{
    [SerializeField] private float _floatHeight = 0.01f; //distance that the cards will float up and down
    [SerializeField] private float _floatSpeed = 0.8f ; //speed the cards move at

    private Vector3 _startPosition;

    private float _randomOffset; //allows cards to float at different times

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _startPosition = transform.localPosition;

        _randomOffset = Random.Range( 0f, 6f); //each card start at a random point in floating anim

        _floatSpeed = Random.Range( 0.7f, 1.3f); //give each card different speed
    }

    // Update is called once per frame
    void Update()
    {
        float newHeight = Mathf.Sin (Time.time * _floatSpeed + _randomOffset ) * _floatHeight;

        transform.localPosition = _startPosition + new Vector3( 0f, newHeight, 0f);
    }
}
