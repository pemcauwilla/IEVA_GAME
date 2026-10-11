using UnityEngine;

public class PlayerSurvival : MonoBehaviour
{

    public float maxCoherence = 100f;
    public float currentCoherence = 100f;
    public float drainRate = 10f; // per second
    public float regenRate = 20f; // per second
    public bool isInSafeZone = false;
    private bool isDead = false;
    private PlayerIdentity playerIdentity;
    public event System.Action<PlayerTeam> OnPlayerDied;
    
    public void Awake()
    {
        playerIdentity = GetComponent<PlayerIdentity>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<CaptureZone>(out _))
        {
            isInSafeZone = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent<CaptureZone>(out _)){
            isInSafeZone = false;
        }
    }

    public void Update()
    {
        if(isInSafeZone)
        {
            currentCoherence = Mathf.Min(maxCoherence, currentCoherence + regenRate * Time.deltaTime);
            return;
        }

        currentCoherence = Mathf.Max(0f, currentCoherence - drainRate * Time.deltaTime);
        if(currentCoherence == 0 && !isDead) {
            OnPlayerDied?.Invoke(playerIdentity.playerTeam);
            isDead = true;
        }
    }

    public void ResetCoherence()
    {
        currentCoherence = maxCoherence;
        isDead = false;
    }
}
