using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerSurvival : MonoBehaviour
{
    [Header("Health Config")]
    [Tooltip("Player's max health")]
    [SerializeField] private int maxLife = 100;

    [Header("Entropy Decay (damage outside the zone)")]
    [Tooltip("Damage suffered each tick when outside the zone")]
    [SerializeField] private int decayRate = 10;
    [Tooltip("Interval in seconds between each decay tick")]
    [SerializeField] private float decayInterval = 2f;

    [Header("Zone Recovery (health regen inside zone)")]
    [Tooltip("Health gained each tick when inside a safe zone")]
    [SerializeField] private int healRate = 20;
    [Tooltip("Interval in seconds between each regen tick")]
    [SerializeField] private float healInterval = 2f;

    public float CurrentLife {get; private set;}
    public float CurrentHealth => CurrentLife;
    public int MaxHealth => maxLife;

    public bool isInSafeZone {get; private set;} = false;

    public static readonly List<PlayerSurvival> ActivePlayers = new();

    public event System.Action<PlayerSurvival> OnPlayerDied;
    public event System.Action<float, float> OnHealthChanged;

    private Rigidbody rb;
    private Vector3 spawnPosition;
    private float healTimer = 0f;
    private float decayTimer = 0f;

    #region UnityLifecycle
    private void OnEnable()
    {
        if (!ActivePlayers.Contains(this))
        {
            ActivePlayers.Add(this);
        }
    }

    private void OnDisable()
    {
        ActivePlayers.Remove(this);
    }

    private void Awake()
    {
        if(rb == null) rb = GetComponent<Rigidbody>();
        CurrentLife = maxLife;
        spawnPosition = transform.position;
    }

    private void Start()
    {
        NotifyHealthChanged();
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
            Heal(healRate);
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
            ExitSafeZone();
        }
    }
    #endregion

    #region Public API

    public void RevivePlayer() {
        CurrentLife = maxLife;
        transform.position = spawnPosition;

        isInSafeZone = false;
        healTimer = 0f; decayTimer = 0f;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        NotifyHealthChanged();
    }

    public void TakeDamage(int damage) {
        CurrentLife = Mathf.Max(0, CurrentLife - damage);
        NotifyHealthChanged();
        if(CurrentLife <= 0)
        {
            OnPlayerDied?.Invoke(this);
        }
    }

    public void Heal(int amount)
    {
        CurrentLife = Mathf.Min(maxLife, CurrentLife + amount);
        decayTimer = 0f;
        NotifyHealthChanged();
    }

    public void ExitSafeZone()
    {
        isInSafeZone = false;
        decayTimer = 0f;
    }
    #endregion

    #region Helper Methods
    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(CurrentLife, maxLife);
    }
    #endregion
}
