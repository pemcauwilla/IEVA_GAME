using UnityEngine;
using UnityEngine.Scripting;

/// <summary>
/// Class that implements player damage and healing behavior
/// </summary>
public class PlayerSurvival : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] public float maxLife = 100f;
    public float CurrentLife {get; private set;} // UI need to see the current life but should not be able to set it 
    
    [Header("Game Balance")]
    [SerializeField] private float decayRate = 10f;
    [SerializeField] private float decayInterval = 2f;
    [SerializeField] private float healRate = 20f;
    [SerializeField] private float healInterval = 2f;

    private bool isOnZone = false;
    private float healTimer = 0f;
    private float decayTimer = 0f;
    private PlayerIdentity pId; // Store this in class so TryGetComponent doesn't get called on each death

    /// <summary>
    ///  Event to be played on player death
    /// </summary>
    public event System.Action<PlayerIdentity> OnPlayerDied;

    private void Awake()
    {
        CurrentLife = maxLife;
        TryGetComponent<PlayerIdentity>(out pId);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.TryGetComponent<CaptureZone>(out var _))
        {
            isOnZone = true;
            healTimer = 0f;
            decayTimer = 0f;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if(collider.TryGetComponent<CaptureZone>(out var _))
        {
            isOnZone = false;
            healTimer = 0f;
            decayTimer = 0f;
        }
    }

    private void Update()
    {
        if (isOnZone)
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
            CurrentLife = Mathf.Max(0f, CurrentLife - decayRate);
            decayTimer = 0f; 
            if(CurrentLife <= 0) // floating point numbers can be imprecise   
            {
                if(pId != null) OnPlayerDied?.Invoke(pId);
            }
        }
    }

}