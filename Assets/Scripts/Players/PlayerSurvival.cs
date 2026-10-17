using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that implements player damage and healing behavior
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerSurvival : MonoBehaviour
{
    // Class members
    public static readonly List<PlayerSurvival> ActivePlayers = new();  

    [Header("Health Config")]
    [Tooltip("Player's max health")]
    [SerializeField] private int maxLife = 100;
    
    [Header("Entropy Decay (damage outside the zone)")]
    [Tooltip("Damage suffered each tick when outside the zone")]
    [SerializeField] private int decayRate = 10;
    [Tooltip("Interval in seconds between each decay tick")]
    [SerializeField] private float decayInterval = 2f;
    
    [Header("Zone Recovery (health regen inside zone)")]
    [Tooltip("Health gained each ticke when in the zone")]
    [SerializeField] private int healRate = 20;
    [Tooltip("Interval in seconds between each regen tick")]
    [SerializeField] private float healInterval = 2f;

    // Public properties
    /// <summary> Player's current life  </summary>
    public float CurrentLife {get; private set;} // UI need to see the current life but should not be able to set it 
    /// <summary> True if player is inside the capture Zone </summary>
    public bool isInSafeZone {get; private set;} = false;
    /// <summary> Event triggered when player's <see cref="CurrentLife"/> goes down to zero </summary>
    public event System.Action<PlayerSurvival> OnPlayerDied;

    // Private members
    private Rigidbody rb;
    private Vector3 spawnPosition; 
    private float healTimer = 0f;
    private float decayTimer = 0f;

    #region UnityLifecycle
    private void Awake()
    {
        TryGetComponent<Rigidbody>(out rb);
        CurrentLife = maxLife;
        spawnPosition = transform.position;
    }

    private void OnEnable()
    {
        ActivePlayers.Add(this);
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

     private void OnTriggerEnter(Collider collider)
    {
        if(collider.TryGetComponent<CaptureZone>(out var _))
        {
            isInSafeZone = true;
            healTimer = 0f;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if(collider.TryGetComponent<CaptureZone>(out var _))
        {
            isInSafeZone = false;
            decayTimer = 0f;
        }
    }

    private void OnDisable()
    {
        ActivePlayers.Remove(this);
    }
    #endregion
    
    #region Public API
    /// <summary>
    /// Reinitializes player's <see cref="CurrentLife"/> to <see cref="maxLife"/> and set's their position to the respective spawnpoint.
    /// </summary>
    public void RevivePlayer() {
        CurrentLife = maxLife;
        transform.position = spawnPosition;

        isInSafeZone = false;
        healTimer = 0f; decayTimer = 0f;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// Reducts player's <see cref="CurrentLife"/> by <paramref name="damage"/>
    /// </summary>
    /// <param name="damage">Amount of damage to apply</param>
    public void TakeDamage(int damage) {
        CurrentLife = Mathf.Max(0, CurrentLife - damage);
        if(CurrentLife <= 0) // floating point numbers can be imprecise   
        {
            OnPlayerDied?.Invoke(this);
        }
    }
    #endregion
}