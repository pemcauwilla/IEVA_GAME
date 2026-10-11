using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SpherePlayerController : MonoBehaviour
{
    [Header("Touches")]
    public Key ForwardKey = Key.UpArrow;
    public Key BackwardKey = Key.DownArrow;
    public Key LeftKey = Key.LeftArrow;
    public Key RightKey = Key.RightArrow;

    [Header("Roulement")]
    public float RollAcceleration = 20f;
    public float MaxSpeed = 5f;
    public float MaxAngularSpeed = 20f;

    [Header("Super Roll")]
    public Key SuperRollKey = Key.Enter;
    public float SuperRollImpulse = 20f;
    public float SuperRollDuration = 1f;
    public float SuperRollCooldown = 3f;
    public float MassMultiplier = 2f;

    private Vector3 lastMoveDirection = Vector3.forward;
    private bool superRollRequested;
    private bool superRollActive;
    private float nextSuperRollTime;
    private float superRollEndTime;
    private float originalMass;

    private Rigidbody body;
    private Vector3 moveDirection;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.maxAngularVelocity = MaxAngularSpeed;
        originalMass = body.mass;
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        moveDirection = Vector3.zero;

        if (keyboard == null)
            return;

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard[ForwardKey].isPressed) vertical += 1f;
        if (keyboard[BackwardKey].isPressed) vertical -= 1f;
        if (keyboard[LeftKey].isPressed) horizontal -= 1f;
        if (keyboard[RightKey].isPressed) horizontal += 1f;

        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (moveDirection != Vector3.zero)
        {
            lastMoveDirection = moveDirection;
        }

        if (keyboard[SuperRollKey].wasPressedThisFrame &&
            Time.time >= nextSuperRollTime)
        {
            superRollRequested = true;
        }
    }

    void FixedUpdate()
    {
        // Restaure la masse à la fin de la compétence.
        if (superRollActive && Time.time >= superRollEndTime)
        {
            body.mass = originalMass;
            superRollActive = false;
        }

        if (superRollRequested)
        {
            superRollRequested = false;

            if (Time.time >= nextSuperRollTime)
            {
                body.mass = originalMass * MassMultiplier;

                Vector3 rotationAxis =
                    Vector3.Cross(Vector3.up, lastMoveDirection);

                // Augmentation instantanée de la vitesse de rotation.
                body.AddTorque(
                    rotationAxis * SuperRollImpulse,
                    ForceMode.VelocityChange
                );

                superRollActive = true;
                superRollEndTime = Time.time + SuperRollDuration;
                nextSuperRollTime = Time.time + SuperRollCooldown;
            }
        }

        if (moveDirection == Vector3.zero)
            return;

        float speedInDirection =
            Vector3.Dot(body.linearVelocity, moveDirection);

        if (speedInDirection < MaxSpeed)
        {
            // Axe de rotation permettant de rouler vers la direction voulue.
            Vector3 rotationAxis =
                Vector3.Cross(Vector3.up, moveDirection);

            body.AddTorque(
                rotationAxis * RollAcceleration,
                ForceMode.Acceleration
            );
        }
    }
    void OnDisable()
    {
        if (body != null)
        {
            body.mass = originalMass;
        }

        superRollActive = false;
        superRollRequested = false;
    }
}