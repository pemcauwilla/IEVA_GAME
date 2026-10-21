using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class PlayerBump : MonoBehaviour
{
    [Header("Impulse Settings")]
    [FormerlySerializedAs("BumpImpulse")]
    [SerializeField] private float bumpImpulse = 8f;

    [FormerlySerializedAs("MinimumImpactSpeed")]
    [SerializeField] private float minimumImpactSpeed = 1f;

    [FormerlySerializedAs("BumpCooldown")]
    [SerializeField] private float bumpCooldown = 0.5f;

    [Header("Dash Launch Settings")]
    [Tooltip("Multiplier applied to the impulse when bumping during or from a Dash")]
    [SerializeField] private float dashBumpMultiplier = 1.8f;

    [Tooltip("Vertical launch force added when dashing to lift the opponent in a parabolic arc")]
    [SerializeField] private float dashUpwardLift = 4.2f;

    [Tooltip("Duration in seconds the dash empowerment lasts to launch on collision")]
    [SerializeField] private float dashEmpowerDuration = 0.4f;

    [Header("Feedback")]
    [FormerlySerializedAs("ImpactCamera")]
    [SerializeField] private SharedCameraController impactCamera;

    public float BumpImpulse { get => bumpImpulse; set => bumpImpulse = value; }
    public float MinimumImpactSpeed { get => minimumImpactSpeed; set => minimumImpactSpeed = value; }
    public float BumpCooldown { get => bumpCooldown; set => bumpCooldown = value; }
    public SharedCameraController ImpactCamera { get => impactCamera; set => impactCamera = value; }

    public event System.Action OnPlayersBumped;
    public event System.Action<Vector3> OnPlayersBumpedAt;

    private Rigidbody body;
    private CubePlayerController playerController;
    private Rigidbody currentTouchedBody;
    private float nextBumpTime;
    private float dashEmpowerUntil;

    #region UnityLifecycle
    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        playerController = GetComponent<CubePlayerController>();
        if (impactCamera == null) impactCamera = FindFirstObjectByType<SharedCameraController>();
    }

    private void OnEnable()
    {
        if (playerController != null)
        {
            playerController.OnPlayerDashed += HandlePlayerDashed;
        }
    }

    private void OnDisable()
    {
        if (playerController != null)
        {
            playerController.OnPlayerDashed -= HandlePlayerDashed;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody otherBody = collision.rigidbody;

        if (otherBody == null || otherBody.isKinematic)
            return;

        if (!otherBody.TryGetComponent<PlayerSurvival>(out _))
            return;

        currentTouchedBody = otherBody;

        bool isDash = Time.time <= dashEmpowerUntil;

        if (!isDash && Time.time < nextBumpTime)
            return;

        if (!isDash && collision.relativeVelocity.magnitude < minimumImpactSpeed)
            return;

        Vector3 impactPosition = collision.contactCount > 0
            ? collision.GetContact(0).point
            : (body.worldCenterOfMass + otherBody.worldCenterOfMass) * 0.5f;

        ApplyBump(otherBody, impactPosition, isDash);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (currentTouchedBody == null && collision.rigidbody != null && collision.rigidbody.TryGetComponent<PlayerSurvival>(out _))
        {
            currentTouchedBody = collision.rigidbody;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.rigidbody == currentTouchedBody)
        {
            currentTouchedBody = null;
        }
    }
    #endregion

    #region Helper Methods
    private void HandlePlayerDashed()
    {
        nextBumpTime = 0f;
        dashEmpowerUntil = Time.time + dashEmpowerDuration;

        if (currentTouchedBody != null)
        {
            Vector3 midPoint = (body.worldCenterOfMass + currentTouchedBody.worldCenterOfMass) * 0.5f;
            ApplyBump(currentTouchedBody, midPoint, isDash: true);
        }
    }

    private void ApplyBump(Rigidbody otherBody, Vector3 impactPosition, bool isDash)
    {
        Vector3 direction = otherBody.worldCenterOfMass - body.worldCenterOfMass;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Vector3 pushDir = direction.normalized;
        float impulse = bumpImpulse;

        if (isDash)
        {
            dashEmpowerUntil = 0f; 
            impulse *= dashBumpMultiplier;

            Vector3 otherVel = otherBody.linearVelocity;
            if (Vector3.Dot(otherVel, pushDir) < 0f)
            {
                otherBody.linearVelocity = new Vector3(0f, otherVel.y, 0f);
            }

            Vector3 launchForce = (pushDir * impulse) + (Vector3.up * dashUpwardLift);
            otherBody.AddForce(launchForce, ForceMode.Impulse);
        }
        else
        {
            otherBody.AddForce(pushDir * impulse, ForceMode.Impulse);
        }

        nextBumpTime = Time.time + bumpCooldown;

        OnPlayersBumped?.Invoke();
        OnPlayersBumpedAt?.Invoke(impactPosition);

        TriggerFeedback();
    }

    private void TriggerFeedback()
    {
        if (impactCamera != null)
        {
            impactCamera.PlayImpactShake();
        }
    }
    #endregion
}
