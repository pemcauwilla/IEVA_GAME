using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CubePlayerController : MonoBehaviour
{
    [Header("Primary Movement Keys")]
    public Key ForwardKey = Key.W;
    public Key BackwardKey = Key.S;
    public Key LeftKey = Key.A;
    public Key RightKey = Key.D;

    [Header("Alternative Movement Keys (e.g. AZERTY / Layout Fallbacks)")]
    public Key AltForwardKey = Key.Z;
    public Key AltBackwardKey = Key.None;
    public Key AltLeftKey = Key.Q;
    public Key AltRightKey = Key.None;

    [Header("Movement Settings")]
    public float Acceleration = 30f;
    public float MaxSpeed = 5f;

    [Header("Dash Settings")]
    public Key DashKey = Key.Space;
    public Key AltDashKey = Key.None;
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
        EnsureAltBindingDefaults();
    }

    private void EnsureAltBindingDefaults()
    {
        if (ForwardKey == Key.W && AltForwardKey == Key.None) AltForwardKey = Key.Z;
        if (LeftKey == Key.A && AltLeftKey == Key.None) AltLeftKey = Key.Q;
        if (DashKey == Key.RightShift && AltDashKey == Key.None) AltDashKey = Key.Enter;
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        moveDirection = Vector3.zero;

        if (keyboard == null)
            return;

        float horizontal = 0f;
        float vertical = 0f;

        if (IsActionPressed(keyboard, ForwardKey, AltForwardKey)) vertical += 1f;
        if (IsActionPressed(keyboard, BackwardKey, AltBackwardKey)) vertical -= 1f;
        if (IsActionPressed(keyboard, LeftKey, AltLeftKey)) horizontal -= 1f;
        if (IsActionPressed(keyboard, RightKey, AltRightKey)) horizontal += 1f;

        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (moveDirection != Vector3.zero)
        {
            lastMoveDirection = moveDirection;
        }

        if (WasActionPressed(keyboard, DashKey, AltDashKey) && Time.time >= nextDashTime)
        {
            dashRequested = true;
        }
    }

    private bool IsActionPressed(Keyboard kb, Key primary, Key alt)
    {
        return (primary != Key.None && kb[primary].isPressed) ||
               (alt != Key.None && kb[alt].isPressed);
    }

    private bool WasActionPressed(Keyboard kb, Key primary, Key alt)
    {
        return (primary != Key.None && kb[primary].wasPressedThisFrame) ||
               (alt != Key.None && kb[alt].wasPressedThisFrame);
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
