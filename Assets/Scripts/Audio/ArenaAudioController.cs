using UnityEngine;

public class ArenaAudioController : MonoBehaviour
{
    [Header("Gameplay Event Sources")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private MovingZoneController movingZoneController;
    [SerializeField] private CubePlayerController player1Controller;
    [SerializeField] private CubePlayerController player2Controller;
    [SerializeField] private PlayerBump player1Bump;
    [SerializeField] private PlayerBump player2Bump;
    [SerializeField] private CaptureZone[] captureZones;
    [SerializeField] private GuardianManager[] guardians;
    [SerializeField] private HealingTotem[] healingTotems;
    [SerializeField] private DynamicGate[] dynamicGates;

    [Header("Volume Settings")]
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.55f;
    [Range(0f, 1f)] [SerializeField] private float droneVolume = 0.14f;

    [Header("Audio Sources")]
    [Tooltip("Channel for one-shot sound effects")]
    [SerializeField] private AudioSource sfxSource;
    [Tooltip("Channel for continuous ambient drone music")]
    [SerializeField] private AudioSource droneSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip ambientDroneClip;
    [SerializeField] private AudioClip dashClip;
    [SerializeField] private AudioClip bumpClip;
    [SerializeField] private AudioClip zoneCaptureClip;
    [SerializeField] private AudioClip zoneRelocateClip;
    [SerializeField] private AudioClip scoreTickClip;
    [SerializeField] private AudioClip guardianAlertClip;
    [SerializeField] private AudioClip guardianHitClip;
    [SerializeField] private AudioClip totemHealClip;
    [SerializeField] private AudioClip gatePopClip;
    [SerializeField] private AudioClip victoryClip;

    #region UnityLifecycle
    private void Awake()
    {
        EnsureAudioSources();
        LoadAudioClips();
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void Start()
    {
        if (droneSource != null && ambientDroneClip != null)
        {
            droneSource.clip = ambientDroneClip;
            droneSource.volume = droneVolume;
            droneSource.Play();
        }
    }

    private void Update()
    {

        if (droneSource != null)
        {
            float targetVol = Time.timeScale > 0.01f ? droneVolume : droneVolume * 0.45f;
            droneSource.volume = Mathf.MoveTowards(droneSource.volume, targetVol, Time.unscaledDeltaTime * 0.5f);
        }
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }
    #endregion

    #region Event Subscriptions & Handlers
    private void SubscribeEvents()
    {
        if (gameManager != null)
        {
            gameManager.OnScoresUpdated += HandleScoresUpdated;
            gameManager.OnGameWon += HandleGameWon;
        }

        if (movingZoneController != null)
        {
            movingZoneController.OnZonesRelocated += HandleZonesRelocated;
        }

        if (player1Controller != null) player1Controller.OnPlayerDashed += HandlePlayerDashed;
        if (player2Controller != null) player2Controller.OnPlayerDashed += HandlePlayerDashed;

        if (player1Bump != null) player1Bump.OnPlayersBumped += HandlePlayersBumped;
        if (player2Bump != null) player2Bump.OnPlayersBumped += HandlePlayersBumped;

        if (captureZones != null)
        {
            for (int i = 0; i < captureZones.Length; i++)
            {
                if (captureZones[i] != null)
                    captureZones[i].OnZoneCaptured += HandleZoneCaptured;
            }
        }

        if (guardians != null)
        {
            for (int i = 0; i < guardians.Length; i++)
            {
                if (guardians[i] != null)
                {
                    guardians[i].OnGuardianStateChanged += HandleGuardianStateChanged;
                    guardians[i].OnGuardianHitPlayer += HandleGuardianHitPlayer;
                }
            }
        }

        if (healingTotems != null)
        {
            for (int i = 0; i < healingTotems.Length; i++)
            {
                if (healingTotems[i] != null)
                    healingTotems[i].OnTotemConsumed += HandleTotemConsumed;
            }
        }

        if (dynamicGates != null)
        {
            for (int i = 0; i < dynamicGates.Length; i++)
            {
                if (dynamicGates[i] != null)
                    dynamicGates[i].OnGateStateChanged += HandleGateStateChanged;
            }
        }
    }

    private void UnsubscribeEvents()
    {
        if (gameManager != null)
        {
            gameManager.OnScoresUpdated -= HandleScoresUpdated;
            gameManager.OnGameWon -= HandleGameWon;
        }

        if (movingZoneController != null)
        {
            movingZoneController.OnZonesRelocated -= HandleZonesRelocated;
        }

        if (player1Controller != null) player1Controller.OnPlayerDashed -= HandlePlayerDashed;
        if (player2Controller != null) player2Controller.OnPlayerDashed -= HandlePlayerDashed;

        if (player1Bump != null) player1Bump.OnPlayersBumped -= HandlePlayersBumped;
        if (player2Bump != null) player2Bump.OnPlayersBumped -= HandlePlayersBumped;

        if (captureZones != null)
        {
            for (int i = 0; i < captureZones.Length; i++)
            {
                if (captureZones[i] != null)
                    captureZones[i].OnZoneCaptured -= HandleZoneCaptured;
            }
        }

        if (guardians != null)
        {
            for (int i = 0; i < guardians.Length; i++)
            {
                if (guardians[i] != null)
                {
                    guardians[i].OnGuardianStateChanged -= HandleGuardianStateChanged;
                    guardians[i].OnGuardianHitPlayer -= HandleGuardianHitPlayer;
                }
            }
        }

        if (healingTotems != null)
        {
            for (int i = 0; i < healingTotems.Length; i++)
            {
                if (healingTotems[i] != null)
                    healingTotems[i].OnTotemConsumed -= HandleTotemConsumed;
            }
        }

        if (dynamicGates != null)
        {
            for (int i = 0; i < dynamicGates.Length; i++)
            {
                if (dynamicGates[i] != null)
                    dynamicGates[i].OnGateStateChanged -= HandleGateStateChanged;
            }
        }
    }

    private void HandlePlayerDashed() => PlaySfx(dashClip, 0.65f);
    private void HandlePlayersBumped() => PlaySfx(bumpClip, 0.85f);
    private void HandleZoneCaptured(CaptureZone zone, PlayerIdentity owner) => PlaySfx(zoneCaptureClip, 0.90f);
    private void HandleZonesRelocated() => PlaySfx(zoneRelocateClip, 0.75f);
    private void HandleScoresUpdated() => PlaySfx(scoreTickClip, 0.30f);
    private void HandleGuardianHitPlayer(Vector3 pos) => PlaySfx(guardianHitClip, 0.95f);
    private void HandleTotemConsumed() => PlaySfx(totemHealClip, 0.85f);

    private void HandleGuardianStateChanged(GuardianState state)
    {
        if (state == GuardianState.Alerted)
        {
            PlaySfx(guardianAlertClip, 0.75f);
        }
    }

    private void HandleGateStateChanged(DynamicGate gate, bool isOpen)
    {
        if (!isOpen)
        {
            PlaySfx(gatePopClip, 0.70f);
        }
    }

    private void HandleGameWon(PlayerIdentity winner)
    {
        PlaySfx(victoryClip, 1.0f);
    }
    #endregion

    #region Audio Initialization & Fallbacks
    private void EnsureAudioSources()
    {
        if (sfxSource == null || droneSource == null)
        {
            AudioSource[] sources = GetComponents<AudioSource>();
            if (sfxSource == null && sources.Length > 0) sfxSource = sources[0];
            if (droneSource == null && sources.Length > 1) droneSource = sources[1];
        }

        if (sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
        }

        if (droneSource != null)
        {
            droneSource.playOnAwake = false;
            droneSource.loop = true;
            droneSource.spatialBlend = 0f;
            droneSource.volume = droneVolume;
        }
    }

    private void PlaySfx(AudioClip clip, float scale)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume * scale);
        }
    }

    private void LoadAudioClips()
    {
        if (ambientDroneClip == null) ambientDroneClip = Resources.Load<AudioClip>("Audio/ambient_drone");
        if (dashClip == null) dashClip = Resources.Load<AudioClip>("Audio/dash");
        if (bumpClip == null) bumpClip = Resources.Load<AudioClip>("Audio/bump");
        if (zoneCaptureClip == null) zoneCaptureClip = Resources.Load<AudioClip>("Audio/zone_capture");
        if (zoneRelocateClip == null) zoneRelocateClip = Resources.Load<AudioClip>("Audio/zone_relocate");
        if (scoreTickClip == null) scoreTickClip = Resources.Load<AudioClip>("Audio/score_tick");
        if (guardianAlertClip == null) guardianAlertClip = Resources.Load<AudioClip>("Audio/guardian_alert");
        if (guardianHitClip == null) guardianHitClip = Resources.Load<AudioClip>("Audio/guardian_hit");
        if (totemHealClip == null) totemHealClip = Resources.Load<AudioClip>("Audio/totem_heal");
        if (gatePopClip == null) gatePopClip = Resources.Load<AudioClip>("Audio/gate_pop");
        if (victoryClip == null) victoryClip = Resources.Load<AudioClip>("Audio/victory");
    }
    #endregion
}

