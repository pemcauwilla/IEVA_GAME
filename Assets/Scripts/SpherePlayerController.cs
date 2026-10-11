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

    private Rigidbody body;
    private Vector3 moveDirection;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.maxAngularVelocity = MaxAngularSpeed;
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
    }

    void FixedUpdate()
    {
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
}