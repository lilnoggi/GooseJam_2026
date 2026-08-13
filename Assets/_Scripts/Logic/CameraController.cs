using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Camera Anchors")]
    [SerializeField] private Transform _defaultAnchor;
    [SerializeField] private Transform _tableTopAnchor;

    [Header("Settings")]
    [SerializeField] private float _swoopSpeed = 2.5f;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator SwoopToTable()
    {
        yield return MoveCamera(_tableTopAnchor);
    }

    public IEnumerator SwoopToDefault()
    {
        yield return MoveCamera(_defaultAnchor);
    }

    private IEnumerator MoveCamera(Transform target)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float transition = 0f;

        while (transition < 1f)
        {
            transition += Time.deltaTime * _swoopSpeed;

            // SmoothStep makes the camera ease-in and ease-out
            float curve = Mathf.SmoothStep(0f, 1f, transition);

            transform.position = Vector3.Lerp(startPos, target.position, curve);
            transform.rotation = Quaternion.Lerp(startRot, target.rotation, curve);

            yield return null;
        }

        // Ensure camera snaps perfectly to the target at the end
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
