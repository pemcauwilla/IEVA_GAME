using UnityEngine;

public class GuardianSensor : MonoBehaviour
{
    [Header("Detection parameters")]
    [Tooltip("The maximum straight line distance the NavMeshAgent will detect a player")]
    [SerializeField] private float detectionRange = 15.0f;
    [Tooltip("The NavMeshAgent field of vision")]
    [SerializeField] private float fov = 90.0f;

    public Transform CurrentTarget {get; private set;}

    public bool HasTarget => CurrentTarget != null;

    #region UnityLifecycle

    private void Update()
    {
        Transform targetFound = null;
        foreach(PlayerSurvival p in PlayerSurvival.ActivePlayers)
        {
            if(p.isInSafeZone) continue;

            if(!IsInRange(p) || !IsInFov(p) || !IsNotOccluded(p)) continue;

            targetFound = p.transform;
            break;
        }
        CurrentTarget = targetFound;
    }

    #endregion

    #region Helper Methods

    private bool IsInRange(PlayerSurvival player) => Vector3.Distance(player.transform.position, transform.position) <= detectionRange;
    private bool IsInFov(PlayerSurvival player)
    {
        Vector3 direction = player.transform.position - transform.position;
        return Vector3.Angle(direction, transform.forward) <= fov / 2;
    }

    private bool IsNotOccluded(PlayerSurvival player)
    {
        Vector3 targetPos = player.transform.position;
        Vector3 myPos = transform.position;
        Vector3 direction = targetPos - myPos;
        float dist = direction.magnitude;

        if (Physics.Raycast(myPos, direction.normalized, out var hits, dist, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            return hits.collider.TryGetComponent<PlayerSurvival>(out _) || hits.collider.GetComponentInParent<PlayerSurvival>() != null;
        }
        return true;
    }

    #endregion
}

