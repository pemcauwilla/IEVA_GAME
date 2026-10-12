using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum GuardianState
{
    Patrolling,
    Waiting,
    Alerted,
    Chasing,
    Stunned
}

public class GuardianAI : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    private int currentWaypoint = 0;
    
    [Header("State variables")]
    [SerializeField] private float waitingTime = 3f;
    private float waitedTime = 0f; 
    [SerializeField] private float alertDuration = 0.5f;
    private float alertTimer = 0f;
    
    [Header("Chase variables")]
    [SerializeField] private float visionRange = 10f;
    [SerializeField] private float stunDuration = 2f;
    private float stunTimer = 0f;
    [SerializeField] private float chaseSpeed = 6;  
    private float patrolSpeed;
    
    [Header("Combat")]
    [SerializeField] private float guardianDamage = 25f;
    [SerializeField] private float pushStrength = 12f;
    
    private Transform currentTarget;
    private GuardianState currentState = GuardianState.Patrolling;
    private NavMeshAgent navAgent;

    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        patrolSpeed = navAgent != null ? navAgent.speed : 0f;
    }

    void Start()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        navAgent.SetDestination(waypoints[currentWaypoint].position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (currentState != GuardianState.Chasing) return;
  
        if (collision.gameObject.TryGetComponent<PlayerSurvival>(out var survival))
        {
            ApplyHit(survival);
            navAgent.isStopped = true;
            currentState = GuardianState.Stunned;
        }
    }

    void Update()
    {
        if (currentState == GuardianState.Patrolling || currentState == GuardianState.Waiting)
        {
            if (TryFindVisiblePlayer(out currentTarget))
            {
                currentState = GuardianState.Alerted;
                navAgent.isStopped = true; 
                // OnAlertSound?.Invoke();
                return;
            }
        }

        switch (currentState)
        {
            case GuardianState.Patrolling: HandlePatrolling(); break;
            case GuardianState.Waiting: HandleWaiting(); break;
            case GuardianState.Alerted: HandleAlert(); break;
            case GuardianState.Chasing: HandleChasing(); break;
            case GuardianState.Stunned: HandleStun(); break;
        }
    }

    void HandlePatrolling()
    {
        if(!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            currentState = GuardianState.Waiting;
            waitedTime = 0f;
        }
    }

    void HandleWaiting()
    {
        waitedTime += Time.deltaTime;

        if(waitedTime >= waitingTime)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            navAgent.SetDestination(waypoints[currentWaypoint].position);
            currentState = GuardianState.Patrolling;
        }
    }

    void HandleAlert()
    {
        alertTimer += Time.deltaTime;
        Vector3 lookPos = new Vector3(currentTarget.position.x, transform.position.y, currentTarget.position.z); // should not rotate in Y axis
        transform.LookAt(lookPos);
        if(alertTimer >= alertDuration)
        {
            navAgent.isStopped = false;
            navAgent.speed = chaseSpeed;
            currentState = GuardianState.Chasing;
            alertTimer = 0f;
        }
    }

    void HandleChasing()
    {
        if(currentTarget == null)
        {
            ReturnToPatrol();
            return;
        }

        // if player escaped
        float distance = Vector3.Distance(transform.position, currentTarget.position);
        bool isSafe = currentTarget.TryGetComponent<PlayerSurvival>(out var survival) && survival.isInSafeZone;
        bool isTooFar = distance >= visionRange * 1.5f;
        if(isSafe || isTooFar) 
        {
            ReturnToPatrol();
            return;
        } 

        navAgent.SetDestination(currentTarget.position);
    }

    void HandleStun()
    {
        stunTimer += Time.deltaTime;
        if(stunTimer >= stunDuration)
        {
            navAgent.isStopped = false;
            ReturnToPatrol();
            stunTimer = 0f;
        }
    }

    private void ReturnToPatrol()
    {
        navAgent.speed = patrolSpeed;
        navAgent.SetDestination(waypoints[currentWaypoint].position);
        currentState = GuardianState.Patrolling;
        currentTarget = null;
    }

    bool TryFindVisiblePlayer(out Transform targetPlayer)
    {
        targetPlayer = null;
  
        Collider[] hits = Physics.OverlapSphere(transform.position, visionRange);
  
        foreach (var hit in hits)
        {
            // is it a player?
            if (hit.TryGetComponent<PlayerSurvival>(out var survival))
            {
                // if in capture zone, do nothing
                if (survival.isInSafeZone) continue;
  
                // raycast on eye height to see if player is behind obstacle
                Vector3 eyePos = transform.position + Vector3.up * 0.5f; // eye height
                Vector3 dirToPlayer = (hit.bounds.center - eyePos).normalized; // bounds.center always return central position, not possible "feet pivot"
  
                if (Physics.Raycast(eyePos, dirToPlayer, out RaycastHit rayHit, visionRange)) // visionRange = max raycast distance
                {
                    // if hit is a player and not an obstacle
                    if (rayHit.transform == hit.transform)
                    {
                        targetPlayer = hit.transform;
                        return true;
                    }
                }
            }
        }
        return false; 
    }

    void ApplyHit(PlayerSurvival survival)
    {
        survival.currentCoherence = Mathf.Max(0f, survival.currentCoherence - guardianDamage);

        survival.TryGetComponent<Rigidbody>(out var rb);
        
        Vector3 pushDir = (survival.transform.position - transform.position).normalized;
        pushDir.y = 0.2f;
        rb?.AddForce(pushDir.normalized * pushStrength, ForceMode.Impulse);
    }
}
