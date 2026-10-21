using UnityEngine;

public enum GuardianState
{
    Patrolling,
    Alerted,
    Chasing,
    Stunned
}

[RequireComponent(typeof(GuardianSensor))]
[RequireComponent(typeof(GuardianMotor))]
[RequireComponent(typeof(Rigidbody))]
public class GuardianManager : MonoBehaviour
{
    [Header("Damage Parameters")]
    [Tooltip("Damage taken by the player when taking guardian hits")]
    [SerializeField] private int guardianDamage = 15;
    [Tooltip("Force of the knockback applied when the guardian hits someone")]
    [SerializeField] private float knockbackForce = 8f;

    [Header("Intervals")]
    [Tooltip("Time in seconds the guardian is staying on the alerted State")]
    [SerializeField] private float alertInterval = 0.35f;
    [Tooltip("Time in seconds the guardian is stunned after hitting the player")]
    [SerializeField] private float stunnedInterval = 3.0f;
    [Tooltip("Time in seconds the guardian recalculates the path to be followed when chasing a player")]
    [SerializeField] private float repathInterval = 0.08f;
    [Tooltip("Time in seconds the guardian keeps chasing towards last seen position after losing line of sight")]
    [SerializeField] private float chaseLostInterval = 0.8f;
    [SerializeField] private float recoilDuration = 0.16f;

    public GuardianState CurrentState => currentState;

    public event System.Action<GuardianState> OnGuardianStateChanged;

    public event System.Action<Vector3> OnGuardianHitPlayer;

    private GuardianMotor motor;
    private GuardianSensor sensor;
    private GuardianState currentState;
    private float alertTimer = 0f;
    private float stunTimer = 0f;
    private float repathTimer = 0f;
    private float chaseLostTimer = 0f;
    private Vector3 recoilDirection;

    #region UnityLifecycle

    private void Awake()
    {
        motor = GetComponent<GuardianMotor>();
        sensor = GetComponent<GuardianSensor>();
    }

    private void Start()
    {
        currentState = GuardianState.Patrolling;
        motor.SetChaseMode(false);
        motor.MoveToCurrentWaypoint();
        OnGuardianStateChanged?.Invoke(currentState);
    }

    private void Update()
    {
        EvaluateState();
    }

    #endregion

    private void OnCollisionEnter(Collision collision) => HandleHit(collision);
    private void OnCollisionStay(Collision collision)  => HandleHit(collision);

    #region Helper Methods

    private void EvaluateState()
    {
        GuardianState newState = currentState;
        switch(currentState)
        {
            case GuardianState.Patrolling:
                if (sensor.HasTarget)
                {
                    newState = GuardianState.Alerted;
                }
                else if (motor.HasArrivedAtDestination())
                {
                    motor.AdvanceWaypoint();
                    motor.MoveToCurrentWaypoint();
                }
                break;

            case GuardianState.Alerted:
                if (sensor.CurrentTarget != null) { motor.FaceTarget(sensor.CurrentTarget.position); }

                if(!sensor.HasTarget) { newState = GuardianState.Patrolling; }
                else if(UpdateTimer(ref alertTimer, ref alertInterval)) { newState = GuardianState.Chasing; }
                break;

            case GuardianState.Chasing:
                if(UpdateTimer(ref repathTimer, ref repathInterval))
                {
                    repathTimer = 0f;
                    ChargeTarget();
                }

                if (sensor.HasTarget)
                {
                    chaseLostTimer = 0f;
                }
                else if(UpdateTimer(ref chaseLostTimer, ref chaseLostInterval)) { newState = GuardianState.Patrolling; }
                break;

            case GuardianState.Stunned:
                if (stunTimer < recoilDuration)
                {
                    motor.StepRecoil(recoilDirection);
                }
                if(UpdateTimer(ref stunTimer, ref stunnedInterval)) { newState = sensor.HasTarget ? GuardianState.Chasing : GuardianState.Patrolling; }
                break;
        }

        if(newState != currentState)
        {
            currentState = newState;
            OnStateChanged();
        }
    }

    private void OnStateChanged()
    {
        switch(currentState)
        {
            case GuardianState.Patrolling:
                motor.SetChaseMode(false);
                motor.MoveToCurrentWaypoint();
                break;

            case GuardianState.Alerted:
                alertTimer = 0f;
                motor.Stop();
                break;

            case GuardianState.Chasing:
                chaseLostTimer = 0f;
                motor.SetChaseMode(true);
                ChargeTarget();
                break;

            case GuardianState.Stunned:
                stunTimer = 0f;
                motor.Stop();
                break;
        }
        OnGuardianStateChanged?.Invoke(currentState);
    }

    private void ChargeTarget()
    {
        if (!sensor.HasTarget) return;

        Vector3 dir = (sensor.CurrentTarget.position - transform.position).normalized;
        Vector3 ramTarget = sensor.CurrentTarget.position + dir * 1.5f;
        motor.MoveTo(ramTarget);
    }

    private void HandleHit(Collision collision)
    {
        if (currentState != GuardianState.Chasing) return;

        if (collision.collider.TryGetComponent<PlayerSurvival>(out var player))
        {
            player.TakeDamage(guardianDamage);
            if (collision.collider.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 knockDir = (player.transform.position - transform.position).normalized;
                rb.AddForce(knockDir * knockbackForce, ForceMode.Impulse);
            }
            recoilDirection = (transform.position - player.transform.position).normalized;
            recoilDirection.y = 0f;
            currentState = GuardianState.Stunned;
            OnGuardianHitPlayer?.Invoke(player.transform.position);
            OnStateChanged();
        }
    }

    private bool UpdateTimer(ref float timer, ref float interval)
    {
        timer += Time.deltaTime;
        if(timer >= interval)
        {
            return true;
        }
        return false;
    }

    #endregion
}

