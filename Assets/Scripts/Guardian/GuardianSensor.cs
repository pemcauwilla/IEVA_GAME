using UnityEngine;

public class GuardianSensor : MonoBehaviour
{
    [Header("Detection parameters")]
    [Tooltip("The maximum straight line distance the NavMeshAgent will detect a player")]
    [SerializeField] private float detectionRange = 15.0f;
    [Tooltip("The NavMeshAgent field of vision")]
    [SerializeField] private float fov = 90.0f;

    // Public properties
    /// <summary> The Transform component of the Agent's current target </summary>
    public Transform CurrentTarget {get; private set;}
    /// <summary> Property that returns True if <see cref="CurrentTarget"/> is not null </summary>
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
        
        Vector3 origin = myPos + direction.normalized * 0.6f; // raycast should start from 0.6 off the guardian, or else the first hit is the guardian himself
        float distance = direction.magnitude - 0.6f;
  
        if (Physics.Raycast(origin, direction.normalized, out var hits, distance))
        {
            return hits.collider.TryGetComponent<PlayerSurvival>(out _);
        }
        return false;

    }

    #endregion
}
