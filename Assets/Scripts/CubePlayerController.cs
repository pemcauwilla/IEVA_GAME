using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CubePlayerController : MonoBehaviour
{
    [Header("Touches - positions sur un clavier QWERTY")]
    public Key ForwardKey = Key.W;
    public Key BackwardKey = Key.S;
    public Key LeftKey = Key.A;
    public Key RightKey = Key.D;

    [Header("Déplacement")]
    public float Acceleration = 30f;
    public float MaxSpeed = 5f;

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

        // Évite une accélération plus forte en diagonale.
        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
    }

    void FixedUpdate()
    {
        if (moveDirection == Vector3.zero)
            return;

        // Vitesse actuelle dans la direction demandée.
        float speedInDirection =
            Vector3.Dot(body.linearVelocity, moveDirection);

        // Applique une force sans remplacer la vitesse des collisions.
        if (speedInDirection < MaxSpeed)
        {
            body.AddForce(
                moveDirection * Acceleration,
                ForceMode.Acceleration
            );
        }
    }
}