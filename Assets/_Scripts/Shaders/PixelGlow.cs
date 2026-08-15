using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class PixelGlow : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Vector2 _panSpeed = new Vector2(0.5f, 0.5f);

    private RawImage _rawImage;
    private float _timeX;
    private float _timeY;

    private void Awake()
    {
        _rawImage = GetComponent<RawImage>();
    }

    private void Update()
    {
        // Move time forward continously
        _timeX += Time.deltaTime * _panSpeed.x;
        _timeY += Time.deltaTime * _panSpeed.y;

        // RawImage pan the texture direclty without a mterials
        _rawImage.uvRect = new Rect(_timeX, _timeY, 1f, 1f);
    }
}
