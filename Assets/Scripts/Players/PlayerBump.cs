using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerBump : MonoBehaviour
{
    public float BumpImpulse = 6f;
    public float MinimumImpactSpeed = 1f;
    public float BumpCooldown = 0.5f;

    private Rigidbody body;
    private float nextBumpTime;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody otherBody = collision.rigidbody;

        if (otherBody == null || otherBody.isKinematic)
            return;

        // Pour ce premier test, on pousse uniquement le joueur Cube.
        if (otherBody.GetComponent<CubePlayerController>() == null)
            return;

        if (Time.time < nextBumpTime)
            return;

        if (collision.relativeVelocity.magnitude < MinimumImpactSpeed)
            return;

        // Direction horizontale de la Sphère vers le Cube.
        Vector3 direction =
            otherBody.worldCenterOfMass - body.worldCenterOfMass;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        otherBody.AddForce(
            direction.normalized * BumpImpulse,
            ForceMode.Impulse
        );

        nextBumpTime = Time.time + BumpCooldown;
    }
}