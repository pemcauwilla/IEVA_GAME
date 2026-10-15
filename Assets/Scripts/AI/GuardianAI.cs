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
    [Header("Guardian Config")]
    [SerializeField] private float patrolSpeed = 3.0f;
    [SerializeField] private float chaseSpeed = 6.0f;
    [SerializeField] private float angularSpeed = 720f;
    [SerializeField] private int guardianDamage = 20;
    [SerializeField] private float knockbackForce = 3.0f;
    [SerializeField] private float closeDistance = 0.3f;
    [SerializeField] private float detectionRange = 15.0f;
    [SerializeField] private float detectionAngle = 70.0f;
    
    [Header("Animation Config")]
    [SerializeField] private float waitingInterval = 2.0f;
    private float waitingAngle = 45;
    private float lastAgentAngle = 0f;
    [SerializeField] private float stunnedInterval = 3.0f;
    [SerializeField] private float alertInterval = 1.5f;

    [Header("Waypoints")]
    [SerializeField] private Transform[] waypoints;

    private Transform targetPlayer;
    private NavMeshAgent navAgent;
    private int currentWaypointIdx = 0;
    private GuardianState currentState = GuardianState.Patrolling;    
    private float chaseInterval = 0.2f;
    private float waitTimer = 0f;
    private float alertTimer = 0f;
    private float stunTimer = 0f;
    private float chaseTimer = 0f;

    private void Awake()
    {
        TryGetComponent<NavMeshAgent>(out navAgent);
        if(navAgent == null || waypoints == null) return;
        navAgent.speed = patrolSpeed;
        navAgent.angularSpeed = angularSpeed;
        navAgent.SetDestination(waypoints[currentWaypointIdx].position);
    }

    private void Update()
    {
        // if the agent has arrived at the waypoint
        DetectPlayer();
        GuardianState nextState = UpdateState();
        if(nextState != currentState)
        {
            currentState = nextState;
            OnStateChange();
        }
        OnStateUpdate();
    }

    private void OnCollisionEnter(Collision other)
    {   
        if(other.collider.TryGetComponent<PlayerSurvival>(out var p))
        {
            if(currentState == GuardianState.Chasing) 
            {
                currentState = GuardianState.Stunned; // will only be stunned if chasing, else continue what it was doing
                OnStateChange();
            }
            ApplyHit(p);
        }
    }

    private GuardianState UpdateState()
    {
        switch(currentState)
        {
            case GuardianState.Patrolling: 
                if(targetPlayer != null) return GuardianState.Alerted;

                if(!navAgent.pathPending && navAgent.remainingDistance <= closeDistance)
                {
                    return GuardianState.Waiting;
                }
                break;

            case GuardianState.Waiting:
                if(targetPlayer != null) return GuardianState.Alerted;

                if(waitTimer >= waitingInterval)
                {
                    return GuardianState.Patrolling;
                }
                break;

            case GuardianState.Alerted:
                if(alertTimer >= alertInterval)
                {
                    return GuardianState.Chasing;
                }
                break;

            case GuardianState.Chasing:    
                if(targetPlayer == null)
                {
                    return GuardianState.Patrolling;
                }
                break;

            case GuardianState.Stunned:
                if(stunTimer >= stunnedInterval)
                {
                    if(targetPlayer != null) return GuardianState.Chasing;
                    else return GuardianState.Patrolling;
                }
                break;
        }
        return currentState;
    }

    private void OnStateChange()
    {
        switch(currentState)
        {
            case GuardianState.Patrolling:
                navAgent.isStopped = false;
                navAgent.autoBraking = true;
                navAgent.speed = patrolSpeed;
                navAgent.SetDestination(waypoints[currentWaypointIdx].position);
                break;

            case GuardianState.Waiting:
                waitTimer = 0f;
                lastAgentAngle = transform.eulerAngles.y;
                navAgent.isStopped = true;
                currentWaypointIdx = (currentWaypointIdx + 1) % waypoints.Length;
                navAgent.SetDestination(waypoints[currentWaypointIdx].position);
                break;

            case GuardianState.Alerted:
                alertTimer = 0f;
                lastAgentAngle = transform.eulerAngles.y;
                navAgent.isStopped = true;
                break;

            case GuardianState.Chasing:
                navAgent.autoBraking = false;
                navAgent.isStopped = false;
                navAgent.speed = chaseSpeed;
                if(targetPlayer != null) navAgent.SetDestination(targetPlayer.position);
                break;    

            case GuardianState.Stunned:
                stunTimer = 0f;
                navAgent.isStopped = true;
                break;
        }
    }

    private void OnStateUpdate()
    {
        switch (currentState)
        {
            case GuardianState.Waiting: 
                waitTimer += Time.deltaTime;
                navAgent.transform.rotation = ProcessAnimationRotation(waitTimer, waitingInterval, 2, waitingAngle, lastAgentAngle); 
                break;
            case GuardianState.Alerted: 
                alertTimer += Time.deltaTime; 
                if(targetPlayer != null) navAgent.transform.LookAt(new Vector3(targetPlayer.position.x, transform.position.y, targetPlayer.position.z));
                break;
            case GuardianState.Stunned: stunTimer += Time.deltaTime; break;
            case GuardianState.Chasing: 
                chaseTimer = chaseTimer > chaseInterval ? 0f : chaseTimer + Time.deltaTime; 
                if(targetPlayer != null && chaseTimer >= chaseInterval) navAgent.SetDestination(targetPlayer.position); 
                break;
        }
    }

    private Quaternion ProcessAnimationRotation(float timer, float interval, float freq, float angleRange, float baseAngle)
    {
        float newRotationAngle = baseAngle + (Mathf.Sin(timer/interval * Mathf.PI * freq) * angleRange);
        return Quaternion.Euler(0f, newRotationAngle, 0f);
    }

    private void DetectPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange); // use this so it's not needed to store the player list to iterate through
        foreach(var hitCollider in hitColliders)
        {
            if(hitCollider.TryGetComponent<PlayerSurvival>(out var player))
            {
                Vector3 targetDir = player.transform.position - transform.position;
                bool isPlayerInFov = Vector3.Angle(targetDir, transform.forward) <= detectionAngle;
                
                if(Physics.Raycast(transform.position, targetDir, out var hit, detectionRange) && isPlayerInFov && !player.isInSafeZone) // if player's in fov the raycast can be directed at him 
                {
                    if (hit.collider.TryGetComponent<PlayerSurvival>(out _))
                    {
                        targetPlayer = player.transform;         
                        return;              
                    }
                }
            }
        }
        targetPlayer = null; 
    }

    private void ApplyHit(PlayerSurvival player)
    {
        player.TakeDamage(guardianDamage);
        if(player.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 normDistance = (player.transform.position - transform.position).normalized;
            rb.AddForce(normDistance * knockbackForce, ForceMode.Impulse); // impulse for knockback
        }
    }

}