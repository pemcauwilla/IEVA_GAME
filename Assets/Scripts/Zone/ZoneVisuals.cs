using System;
using UnityEngine;

public class ZoneVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CaptureZone captureZone;
    [SerializeField] private Renderer zoneRenderer;
    [SerializeField] private Light zoneLight;
    [SerializeField] private ArenaThemeController themeController;

    [Header("Glow & Base Settings")]
    [SerializeField] private float baseHdrBoost = 1.95f;
    [SerializeField] private float baseLightIntensity = 5.0f;
    [SerializeField] private float emissionColorAttenuation = 0.45f;

    [Header("Capture Animation Parameters")]
    [SerializeField] private float capturingPulseSpeed = 4f;
    [SerializeField] private float capturingPulseAmplitude = 0.45f;
    [SerializeField] private float capturedPulseSpeed = 2.2f;
    [SerializeField] private float capturedPulseAmplitude = 0.18f;

    [Header("Contested Animation Parameters")]
    [SerializeField] private float contestedClashSpeed = 7f;
    [SerializeField] private float contestedPulseBoost = 1.55f;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private MaterialPropertyBlock propBlock;
    private ArenaPalette currentPalette;

    #region UnityLifecycle
    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();

        if (captureZone == null) captureZone = GetComponent<CaptureZone>();
        if (zoneRenderer == null) zoneRenderer = GetComponent<Renderer>();
        if (zoneLight == null) zoneLight = GetComponentInChildren<Light>();
        if (themeController == null) themeController = FindFirstObjectByType<ArenaThemeController>();
    }

    private void OnEnable()
    {
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
        if (themeController != null)
        {
            themeController.OnPaletteApplied -= HandlePaletteApplied;
        }
    }

    private void Update()
    {
        if (themeController != null && themeController.ActivePaletteIndex < 0) return;
        if (captureZone == null) return;
        UpdateVisuals();
    }
    #endregion

    #region Event Handlers
    private void HandlePaletteApplied(ArenaPalette palette)
    {
        currentPalette = palette;
        UpdateVisuals();
    }
    #endregion

    #region Visual Application
    private void UpdateVisuals()
    {
        Color targetColor = currentPalette.NeutralZoneColor;
        float pulseBoost = 1.0f;

        switch (captureZone.CurrentState)
        {
            case CaptureState.Capturing:
                if (captureZone.CapturingPlayer != null)
                {
                    Color playerCol = GetPlayerColor(captureZone.CapturingPlayer);
                    targetColor = Color.Lerp(currentPalette.NeutralZoneColor, playerCol, captureZone.CaptureProgress);
                    pulseBoost = 1.0f + Mathf.PingPong(Time.time * capturingPulseSpeed, capturingPulseAmplitude);
                }
                break;

            case CaptureState.Captured:
                if (captureZone.CurrentOwner != null)
                {
                    targetColor = GetPlayerColor(captureZone.CurrentOwner);
                    pulseBoost = 1.0f + Mathf.Sin(Time.time * capturedPulseSpeed + transform.position.x) * capturedPulseAmplitude;
                }
                break;

            case CaptureState.Contested:
                float clashLerp = Mathf.PingPong(Time.time * contestedClashSpeed, 1f);
                targetColor = Color.Lerp(currentPalette.Player1Color, currentPalette.Player2Color, clashLerp);
                pulseBoost = contestedPulseBoost;
                break;
        }

        if (zoneRenderer != null)
        {
            Color hdrColor = targetColor * (baseHdrBoost * pulseBoost);
            zoneRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor(BaseColorId, hdrColor);
            propBlock.SetColor(ColorId, hdrColor);
            propBlock.SetColor(EmissionColorId, hdrColor * emissionColorAttenuation);
            zoneRenderer.SetPropertyBlock(propBlock);
        }

        if (zoneLight != null)
        {
            zoneLight.color = targetColor;
            zoneLight.intensity = baseLightIntensity * pulseBoost;
        }
    }

    private Color GetPlayerColor(PlayerIdentity player)
    {
        if (player == null) return currentPalette.NeutralZoneColor;
        return player.Team == PlayerTeam.Team1 ? currentPalette.Player1Color : currentPalette.Player2Color;
    }
    #endregion
}

