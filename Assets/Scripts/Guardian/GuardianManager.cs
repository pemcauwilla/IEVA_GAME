using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Enum that represents all the possible guardian states
/// </summary>
public enum GuardianState
{
    Patrolling,
    Waiting,
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
    [SerializeField] private float alertInterval = 0.4f;
    [Tooltip("Time in seconds the guardian is stunned after hitting the player")]
    [SerializeField] private float stunnedInterval = 3.0f;
    [Tooltip("Time in seconds the guardian recalculates the path to be followed when chasing a player")]
    [SerializeField] private float repathInterval = 0.2f;
    
    // Private members
    private GuardianMotor motor;
    private GuardianSensor sensor;
    private GuardianState currentState; 
    private float alertTimer = 0f;
    private float stunTimer = 0f;
    private float repathTimer = 0f;

    #region UnityLifecycle

    private void Awake()
    {
        motor = GetComponent<GuardianMotor>();
        sensor = GetComponent<GuardianSensor>();
    }

    private void Update()
    {
        EvaluateState();
        switch (currentState)
        {
            case GuardianState.Chasing:
                if(UpdateTimer(ref repathTimer, ref repathInterval))
                {
                    repathTimer = 0f;
                    if(sensor.HasTarget) motor.MoveTo(sensor.CurrentTarget.position);   
                }
                break;
            case GuardianState.Alerted: {
                UpdateTimer(ref alertTimer, ref alertInterval); 
                motor.FaceTarget(sensor.CurrentTarget.position);
                break;
            }
            case GuardianState.Stunned: UpdateTimer(ref stunTimer, ref stunnedInterval); break;
        }
    }

    #endregion

    private void OnCollisionEnter(Collision collision) => HandleHit(collision);
    private void OnCollisionStay(Collision collision)  => HandleHit(collision); // In case player was already in collision with guardian
  
    #region Helper Methods

    private void EvaluateState()
    {
        GuardianState newState = currentState;
        switch(currentState)
        {
            case GuardianState.Patrolling:
                if(sensor.HasTarget) newState = GuardianState.Alerted;
                else if(motor.HasArrivedAtDestination()) newState = GuardianState.Waiting;
                break;

            case GuardianState.Waiting:
                if(sensor.HasTarget) newState = GuardianState.Alerted;
                else if(!motor.IsInAnimation) newState = GuardianState.Patrolling; 
                break;

            case GuardianState.Alerted:
                if(!sensor.HasTarget) newState = GuardianState.Patrolling;
                if(alertTimer >= alertInterval) newState = GuardianState.Chasing;
                break;

            case GuardianState.Chasing:
                if(!sensor.HasTarget) newState = GuardianState.Patrolling;
                break;

            case GuardianState.Stunned:
                if(stunTimer >= stunnedInterval) newState = sensor.HasTarget ? GuardianState.Chasing : GuardianState.Patrolling;
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

            case GuardianState.Waiting:
                motor.Stop();
                motor.StartScan();
                motor.AdvanceWaypoint();
                break;

            case GuardianState.Alerted:
                alertTimer = 0f;
                motor.Stop();
                break;

            case GuardianState.Chasing:
                motor.SetChaseMode(true);
                break;

            case GuardianState.Stunned:
                stunTimer = 0f;
                motor.Stop();
                break;
        }
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
            currentState = GuardianState.Stunned;
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
