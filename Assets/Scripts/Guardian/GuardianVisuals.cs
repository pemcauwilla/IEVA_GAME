using System;
using UnityEngine;

public class GuardianVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GuardianManager guardianManager;
    [SerializeField] private Renderer meshRenderer;
    [SerializeField] private Light pointLight;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private ArenaThemeController themeController;
    [SerializeField] private SharedCameraController sharedCamera;

    [Header("HDR Settings")]
    [SerializeField] private float patrolHdr = 1.7f;
    [SerializeField] private float alertHdr = 1.8f;
    [SerializeField] private float chaseHdr = 2.5f;

    [Header("Light Intensities")]
    [SerializeField] private float patrolLightIntensity = 4.0f;
    [SerializeField] private float alertLightIntensity = 4.5f;
    [SerializeField] private float chaseLightIntensity = 6.5f;
    [SerializeField] private float emissionColorAttenuation = 0.45f;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private MaterialPropertyBlock propBlock;
    private ArenaPalette currentPalette;

    #region UnityLifecycle
    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();

        if (guardianManager == null) guardianManager = GetComponent<GuardianManager>();
        if (meshRenderer == null) meshRenderer = GetComponent<Renderer>();
        if (pointLight == null) pointLight = GetComponentInChildren<Light>();
        if (trail == null) trail = GetComponent<TrailRenderer>();
        if (themeController == null) themeController = FindFirstObjectByType<ArenaThemeController>();
        if (sharedCamera == null) sharedCamera = FindFirstObjectByType<SharedCameraController>();
    }

    private void OnEnable()
    {
        if (guardianManager != null)
        {
            guardianManager.OnGuardianStateChanged += HandleGuardianStateChanged;
            guardianManager.OnGuardianHitPlayer += HandleGuardianHitPlayer;
        }

        if (themeController != null)
        {
            themeController.OnPaletteApplied += HandlePaletteApplied;
            if (themeController.ActivePaletteIndex >= 0)
            {
                HandlePaletteApplied(themeController.ActivePalette);
            }
        }
    }

    private void OnDisable()
    {
        if (guardianManager != null)
        {
            guardianManager.OnGuardianStateChanged -= HandleGuardianStateChanged;
            guardianManager.OnGuardianHitPlayer -= HandleGuardianHitPlayer;
        }

        if (themeController != null)
        {
            themeController.OnPaletteApplied -= HandlePaletteApplied;
        }
    }
    #endregion

    #region Event Handlers

    private void HandlePaletteApplied(ArenaPalette palette)
    {
        currentPalette = palette;
        GuardianState state = (guardianManager != null) ? guardianManager.CurrentState : GuardianState.Patrolling;
        ApplyStateVisuals(state);
    }

    private void HandleGuardianStateChanged(GuardianState state)
    {
        if (themeController != null && themeController.ActivePaletteIndex < 0) return;
        ApplyStateVisuals(state);
    }

    private void HandleGuardianHitPlayer(Vector3 hitPos)
    {
        if (sharedCamera != null)
        {
            sharedCamera.PlayImpactShake();
        }
    }
    #endregion

    #region Visual Application
    private void ApplyStateVisuals(GuardianState state)
    {
        if (themeController != null && themeController.ActivePaletteIndex < 0) return;

        Color targetColor = state switch
        {
            GuardianState.Alerted => currentPalette.GuardianAlertColor,
            GuardianState.Chasing => currentPalette.GuardianChaseColor,
            _ => currentPalette.GuardianPatrolColor
        };

        float hdr = state switch
        {
            GuardianState.Alerted => alertHdr,
            GuardianState.Chasing => chaseHdr,
            _ => patrolHdr
        };

        float lightIntensity = state switch
        {
            GuardianState.Alerted => alertLightIntensity,
            GuardianState.Chasing => chaseLightIntensity,
            _ => patrolLightIntensity
        };

        if (meshRenderer != null)
        {
            Color hdrColor = targetColor * hdr;
            meshRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor(BaseColorId, hdrColor);
            propBlock.SetColor(ColorId, hdrColor);
            propBlock.SetColor(EmissionColorId, hdrColor * emissionColorAttenuation);
            meshRenderer.SetPropertyBlock(propBlock);
        }

        if (pointLight != null)
        {
            pointLight.color = targetColor;
            pointLight.intensity = lightIntensity;
        }

        if (trail != null)
        {
            Color trailColor = targetColor * hdr;
            trail.startColor = trailColor;
            trail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
        }
    }
    #endregion
}

