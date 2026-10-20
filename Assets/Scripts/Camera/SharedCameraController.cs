using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SharedCameraController : MonoBehaviour
{
    [Header("Players to Track")]
    public Transform Player1;
    public Transform Player2;

    [Header("Framing")]
    public float Height = 20f;
    public float MinSize = 6f;
    public float MaxSize = 14f;
    public float Padding = 2f;

    [Header("Smoothing")]
    public float FollowSmoothTime = 0.2f;
    public float ZoomSmoothTime = 0.2f;

    [Header("Impact Shake")]
    [Min(0.01f)] public float ShakeDuration = 0.18f;
    [Min(0f)] public float ShakeStrength = 0.15f;

    private Vector3 followPosition;
    private float shakeRemaining;

    private Camera sharedCamera;
    private Vector3 followVelocity;
    private float zoomVelocity;

    void Awake()
    {
        sharedCamera = GetComponent<Camera>();
        sharedCamera.orthographic = true;
    }

    void Start()
    {
        if (Player1 == null || Player2 == null)
        {
            Debug.LogError(
                "SharedCameraController : renseigne les deux joueurs.",
                this
            );

            enabled = false;
            return;
        }

        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        followPosition = GetTargetPosition();
        transform.position = followPosition;
        sharedCamera.orthographicSize = GetRequiredSize();
    }

    void LateUpdate()
    {
        if (Player1 == null || Player2 == null)
            return;

        followPosition = Vector3.SmoothDamp(
            followPosition,
            GetTargetPosition(),
            ref followVelocity,
            FollowSmoothTime
        );

        transform.position = followPosition;

        float requiredSize = GetRequiredSize();

        if (requiredSize > sharedCamera.orthographicSize)
        {
            sharedCamera.orthographicSize = requiredSize;
            zoomVelocity = 0f;
        }
        else
        {
            sharedCamera.orthographicSize = Mathf.SmoothDamp(
                sharedCamera.orthographicSize,
                requiredSize,
                ref zoomVelocity,
                ZoomSmoothTime
            );
        }

        if (shakeRemaining > 0f)
        {
            float fade = Mathf.Clamp01(
                shakeRemaining / Mathf.Max(ShakeDuration, 0.01f)
            );

            Vector2 offset =
                Random.insideUnitCircle * ShakeStrength * fade;

            transform.position = followPosition +
                new Vector3(offset.x, 0f, offset.y);

            shakeRemaining = Mathf.Max(
                0f, shakeRemaining - Time.deltaTime
            );
        }
    }

    [ContextMenu("Test Camera Shake")]
    public void PlayImpactShake()
    {
        if (!Application.isPlaying || !isActiveAndEnabled)
            return;

        shakeRemaining = Mathf.Max(ShakeDuration, 0.01f);
    }

    private void OnDisable()
    {
        shakeRemaining = 0f;
        followVelocity = Vector3.zero;

        if (sharedCamera != null)
            transform.position = followPosition;
    }

    Vector3 GetTargetPosition()
    {
        Vector3 midpoint = (Player1.position + Player2.position) * 0.5f;
        return midpoint + Vector3.up * Height;
    }

    float GetRequiredSize()
    {

        float horizontalExtent = Mathf.Max(
            Mathf.Abs(Player1.position.x - transform.position.x),
            Mathf.Abs(Player2.position.x - transform.position.x)
        );

        float verticalExtent = Mathf.Max(
            Mathf.Abs(Player1.position.z - transform.position.z),
            Mathf.Abs(Player2.position.z - transform.position.z)
        );

        float aspect = Mathf.Max(sharedCamera.aspect, 0.01f);

        float requiredSize = Mathf.Max(
            verticalExtent + Padding,
            (horizontalExtent + Padding) / aspect
        );

        return Mathf.Clamp(requiredSize, MinSize, MaxSize);
    }
}
