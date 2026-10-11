using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SharedCameraController : MonoBehaviour
{
    [Header("Joueurs à suivre")]
    public Transform Player1;
    public Transform Player2;

    [Header("Cadrage")]
    public float Height = 20f;
    public float MinSize = 6f;
    public float Padding = 2f;

    [Header("Fluidité")]
    public float FollowSmoothTime = 0.2f;
    public float ZoomSmoothTime = 0.2f;

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

        // Cadrage immédiat au début de la partie.
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        transform.position = GetTargetPosition();
        sharedCamera.orthographicSize = GetRequiredSize();
    }

    void LateUpdate()
    {
        if (Player1 == null || Player2 == null)
            return;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            GetTargetPosition(),
            ref followVelocity,
            FollowSmoothTime
        );

        float requiredSize = GetRequiredSize();

        // Élargit immédiatement le cadre pour ne pas perdre un joueur.
        // Le rapprochement reste progressif.
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
    }

    Vector3 GetTargetPosition()
    {
        Vector3 midpoint = (Player1.position + Player2.position) * 0.5f;
        return midpoint + Vector3.up * Height;
    }

    float GetRequiredSize()
    {
        // Mesure les écarts depuis le centre réel de la caméra.
        // Cela tient compte du retard du suivi progressif.
        float horizontalExtent = Mathf.Max(
            Mathf.Abs(Player1.position.x - transform.position.x),
            Mathf.Abs(Player2.position.x - transform.position.x)
        );

        float verticalExtent = Mathf.Max(
            Mathf.Abs(Player1.position.z - transform.position.z),
            Mathf.Abs(Player2.position.z - transform.position.z)
        );

        float aspect = Mathf.Max(sharedCamera.aspect, 0.01f);

        return Mathf.Max(
            MinSize,
            verticalExtent + Padding,
            (horizontalExtent + Padding) / aspect
        );
    }
}