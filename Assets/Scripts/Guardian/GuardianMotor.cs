using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardianMotor : MonoBehaviour
{
    [Header("Movement Config")]
    [Tooltip("Agent's normal walking speed")]
    [SerializeField] private float patrolSpeed = 3.5f;
    [Tooltip("Agent's walking speed when purchasing a player")]
    [SerializeField] private float chaseSpeed = 9.0f;
    [Tooltip("Agent's turning speed")]
    [SerializeField] private float angularSpeed = 720f;
    [Tooltip("Minimum distance so the agent can change its target")]
    [SerializeField] private float closeDistance = 0.3f;

    [Header("Waypoints")]
    [Tooltip("The location of the points the agent chases around")]
    [SerializeField] private Transform[] waypoints;

    [Header("Head Wobble Animation")]
    [Tooltip("The maximum rotation the agent executes when doing the waiting animation")]
    [SerializeField] private float waitingAngle = 45f;
    [Tooltip("Time in seconds to complete the entire waiting animation")]
    [SerializeField] private float waitingAnimationTime = 2f;

    // Public members
    /// <summary> Returns true if waiting animation is still in course. /// </summary>
    public bool IsInAnimation {get; private set;} = false;

    // Private members
    private NavMeshAgent navAgent;
    private int currentWaypointIdx = 0;
    private float waitAnimationTimer = 0f;
    private float baseAngle = 0f;

    #region UnityLifecycle

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = patrolSpeed;
        navAgent.angularSpeed = angularSpeed;
    }

    private void Update()
    {
        if (IsInAnimation)
        {
            waitAnimationTimer += Time.deltaTime;
            AnimateScan();

            if(waitAnimationTimer >= waitingAnimationTime)
            {
                IsInAnimation = false;
            }
        }
    }

    #endregion

    #region Public API

    public void MoveTo(Vector3 position)
    {
        navAgent.SetDestination(position);
        Resume();    
    }

    public void Stop() 
    {
        navAgent.isStopped = true;
        navAgent.velocity = Vector3.zero;
        navAgent.ResetPath();
    }
    public void Resume() => navAgent.isStopped = false;

    public void MoveToCurrentWaypoint()
    {
        navAgent.SetDestination(waypoints[currentWaypointIdx].position);
        Resume();
    }

    public void AdvanceWaypoint() => currentWaypointIdx = (currentWaypointIdx + 1) % waypoints.Length;

    public bool HasArrivedAtDestination() => navAgent.remainingDistance <= closeDistance && !navAgent.pathPending;

    public void SetChaseMode(bool chasing)
    {
        navAgent.speed = chasing ? chaseSpeed : patrolSpeed;
        navAgent.autoBraking = !chasing; 
        navAgent.stoppingDistance = 0f;  
    }
  
    public void FaceTarget(Vector3 targetPosition) => transform.LookAt(new Vector3(targetPosition.x, targetPosition.y, targetPosition.z));

    public void StartScan()
    {
        baseAngle = transform.rotation.eulerAngles.y;
        waitAnimationTimer = 0f;
        IsInAnimation = true;
    }

    public void AnimateScan()
    {
        float animationProgress = waitAnimationTimer / waitingAnimationTime;
        var addAngle = Mathf.Sin(2 * Mathf.PI * animationProgress) * waitingAngle;
        float resultAngle = baseAngle + addAngle;

        transform.rotation = Quaternion.Euler(0f, resultAngle, 0f);
    }   

    #endregion
}
