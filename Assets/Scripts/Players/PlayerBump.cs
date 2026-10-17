using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class PlayerBump : MonoBehaviour
{
    [Header("Impulse Settings")]
    [FormerlySerializedAs("BumpImpulse")]
    [SerializeField] private float bumpImpulse = 6f;

    [FormerlySerializedAs("MinimumImpactSpeed")]
    [SerializeField] private float minimumImpactSpeed = 1f;

    [FormerlySerializedAs("BumpCooldown")]
    [SerializeField] private float bumpCooldown = 0.5f;

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
    private float nextBumpTime;

    #region UnityLifecycle
    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        if (impactCamera == null) impactCamera = FindFirstObjectByType<SharedCameraController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody otherBody = collision.rigidbody;

        if (otherBody == null || otherBody.isKinematic)
            return;

        if (!otherBody.TryGetComponent<PlayerSurvival>(out _))
            return;

        if (Time.time < nextBumpTime)
            return;

        if (collision.relativeVelocity.magnitude < minimumImpactSpeed)
            return;

        Vector3 direction = otherBody.worldCenterOfMass - body.worldCenterOfMass;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        otherBody.AddForce(direction.normalized * bumpImpulse, ForceMode.Impulse);
        nextBumpTime = Time.time + bumpCooldown;

        Vector3 impactPosition = collision.contactCount > 0
            ? collision.GetContact(0).point
            : (body.worldCenterOfMass + otherBody.worldCenterOfMass) * 0.5f;

        OnPlayersBumped?.Invoke();
        OnPlayersBumpedAt?.Invoke(impactPosition);

        TriggerFeedback();
    }
    #endregion

    #region Helper Methods
    private void TriggerFeedback()
    {
        if (impactCamera != null)
        {
            impactCamera.PlayImpactShake();
        }
    }
    #endregion
}
