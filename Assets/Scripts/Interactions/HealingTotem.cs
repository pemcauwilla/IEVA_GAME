using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HealingTotem : MonoBehaviour
{
    [Header("Healing Settings")]
    [Tooltip("Amount of health restored when a player touches the totem")]
    [SerializeField] private int healAmount = 40;

    [Tooltip("Seconds before the totem recharges and can be used again")]
    [SerializeField] private float cooldownTime = 8.0f;

    [Header("Visual Config (Shared with TotemVisuals)")]
    [Tooltip("Speed of rotation in degrees per second")]
    [SerializeField] private float rotationSpeed = 75f;

    [Tooltip("Angular frequency of vertical floating bob")]
    [SerializeField] private float bobFrequency = 3.2f;

    [Tooltip("Maximum vertical floating oscillation amplitude")]
    [SerializeField] private float bobAmplitude = 0.14f;

    [Tooltip("Scale multiplier applied when on cooldown")]
    [SerializeField] private Vector3 cooldownScaleMultiplier = new Vector3(0.75f, 0.35f, 0.75f);

    [Tooltip("Height multiplier applied to initial Y when on cooldown")]
    [SerializeField] private float cooldownHeightMultiplier = 0.5f;

    public bool IsReady => cooldownTimer >= cooldownTime;
    public float RotationSpeed => rotationSpeed;
    public float BobFrequency => bobFrequency;
    public float BobAmplitude => bobAmplitude;
    public Vector3 CooldownScaleMultiplier => cooldownScaleMultiplier;
    public float CooldownHeightMultiplier => cooldownHeightMultiplier;

    public event Action OnTotemConsumed;
    public event Action OnTotemReadyChanged;

    private float cooldownTimer = 0f;

    #region UnityLifecycle
    private void Awake()
    {
        cooldownTimer = cooldownTime;
    }

    private void Update()
    {
        if (!IsReady)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= cooldownTime)
            {
                OnTotemReadyChanged?.Invoke();
            }
        }
    }

    private void OnTriggerEnter(Collider other) => TryHealPlayer(other);
    private void OnTriggerStay(Collider other) => TryHealPlayer(other);
    #endregion

    #region Helper Methods
    private void TryHealPlayer(Collider other)
    {
        if (!IsReady) return;

        if (other.TryGetComponent<PlayerSurvival>(out var player))
        {
            player.Heal(healAmount);
            cooldownTimer = 0f;
            OnTotemConsumed?.Invoke();
            OnTotemReadyChanged?.Invoke();
        }
    }
    #endregion
}

