using UnityEngine;
using UnityEngine.Scripting;

/// <summary>
/// Class that implements player damage and healing behavior
/// </summary>
public class PlayerSurvival : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] public int maxLife = 100;
    public float CurrentLife {get; private set;} // UI need to see the current life but should not be able to set it 
    
    [Header("Game Balance")]
    [SerializeField] private int decayRate = 10;
    [SerializeField] private float decayInterval = 2f;
    [SerializeField] private int healRate = 20;
    [SerializeField] private float healInterval = 2f;

    private Vector3 spawnPosition; // based on the world origin
    public bool isInSafeZone {get; private set;} = false;
    private float healTimer = 0f;
    private float decayTimer = 0f;

    /// <summary>
    ///  Event to be played on player death
    /// </summary>
    public event System.Action<PlayerSurvival> OnPlayerDied;

    private void Awake()
    {
        CurrentLife = maxLife;
        spawnPosition = transform.position;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.TryGetComponent<CaptureZone>(out var _))
        {
            isInSafeZone = true;
            healTimer = 0f;
            decayTimer = 0f;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if(collider.TryGetComponent<CaptureZone>(out var _))
        {
            isInSafeZone = false;
            healTimer = 0f;
            decayTimer = 0f;
        }
    }

    private void Update()
    {
        if (isInSafeZone)
        {
            healTimer += Time.deltaTime;
        }
        else
        {
            decayTimer += Time.deltaTime;
        }

        if(healTimer >= healInterval)
        {
            CurrentLife = Mathf.Min(maxLife, CurrentLife + healRate);
            healTimer = 0f;
        }

        if(decayTimer >= decayInterval)
        {
            TakeDamage(decayRate);
            decayTimer = 0f; 
            
        }
    }

    public void RevivePlayer() {
        CurrentLife = maxLife;
        // reset spawn point
        transform.position = spawnPosition;
        TryGetComponent<Rigidbody>(out var rb);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void TakeDamage(int damage) {
        CurrentLife = Mathf.Max(0, CurrentLife - damage);
        if(CurrentLife <= 0) // floating point numbers can be imprecise   
        {
            OnPlayerDied?.Invoke(this);
        }
    }
}