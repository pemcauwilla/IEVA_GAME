using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardianMotor : MonoBehaviour
{
    [Header("Movement Config")]
    [Tooltip("Agent's normal walking speed")]
    [SerializeField] private float patrolSpeed = 3.5f;
    [Tooltip("Agent's movement speed when chasing a player")]
    [SerializeField] private float chaseSpeed = 7.5f;
    [Tooltip("Agent's turning speed")]
    [SerializeField] private float angularSpeed = 720f;
    [Tooltip("Minimum distance so the agent can change its target")]
    [SerializeField] private float closeDistance = 0.3f;

    [Header("Waypoints")]
    [Tooltip("The location of the points the agent chases around")]
    [SerializeField] private Transform[] waypoints;

    private NavMeshAgent navAgent;
    private int currentWaypointIdx = 0;

    #region UnityLifecycle

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = patrolSpeed;
        navAgent.angularSpeed = angularSpeed;
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
    }
    public void Resume() => navAgent.isStopped = false;

    public void MoveToCurrentWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        navAgent.SetDestination(waypoints[currentWaypointIdx].position);
        Resume();
    }

    public void AdvanceWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        currentWaypointIdx = (currentWaypointIdx + 1) % waypoints.Length;
    }

    public bool HasArrivedAtDestination() => !navAgent.pathPending && (!navAgent.hasPath || navAgent.remainingDistance <= closeDistance);

    public void SetChaseMode(bool chasing)
    {
        navAgent.speed = chasing ? chaseSpeed : patrolSpeed;
        navAgent.autoBraking = false;
        navAgent.stoppingDistance = 0f;
    }

    public void FaceTarget(Vector3 targetPosition) => transform.LookAt(new Vector3(targetPosition.x, transform.position.y, targetPosition.z));

    #endregion
}

