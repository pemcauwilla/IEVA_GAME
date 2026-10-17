using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CubePlayerController : MonoBehaviour
{
    [Header("Movement Keys (QWERTY/WASD Layout)")]
    public Key ForwardKey = Key.W;
    public Key BackwardKey = Key.S;
    public Key LeftKey = Key.A;
    public Key RightKey = Key.D;

    [Header("Movement Settings")]
    public float Acceleration = 30f;
    public float MaxSpeed = 5f;

    [Header("Dash Settings")]
    public Key DashKey = Key.Space;
    public float DashImpulse = 8f;
    public float DashCooldown = 3f;

    public event System.Action OnPlayerDashed;

    private Vector3 lastMoveDirection = Vector3.forward;
    private bool dashRequested;
    private float nextDashTime;

    private Rigidbody body;
    private Vector3 moveDirection;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
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

        if (keyboard[DashKey].wasPressedThisFrame &&
            Time.time >= nextDashTime)
        {
            dashRequested = true;
        }
    }

    void FixedUpdate()
    {
        if (dashRequested)
        {
            dashRequested = false;

            if (Time.time >= nextDashTime)
            {
                body.AddForce(
                    lastMoveDirection * DashImpulse,
                    ForceMode.Impulse
                );

                nextDashTime = Time.time + DashCooldown;
                OnPlayerDashed?.Invoke();
            }
        }
        if (moveDirection == Vector3.zero)
            return;

        float speedInDirection =
            Vector3.Dot(body.linearVelocity, moveDirection);

        if (speedInDirection < MaxSpeed)
        {
            body.AddForce(
                moveDirection * Acceleration,
                ForceMode.Acceleration
            );
        }
    }
}
