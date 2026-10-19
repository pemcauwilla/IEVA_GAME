using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class ArenaThemeController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private MovingZoneController movingZoneController;
    [SerializeField] private PlayerIdentity player1Identity;
    [SerializeField] private PlayerIdentity player2Identity;
    [SerializeField] private CaptureZone[] captureZones;

    [Header("Arena Environment Renderers")]
    [SerializeField] private Renderer groundRenderer;
    [SerializeField] private Renderer[] arenaWallRenderers;
    [SerializeField] private Renderer[] anchorPadRenderers;
    [SerializeField] private Renderer[] gateStripRenderers;
    [SerializeField] private Light directionalLight;

    [Header("Contamination Parameters")]
    [SerializeField] private float transitionSpeed = 3.5f;
    [SerializeField] private float wallHdrBoost = 1.45f;
    [SerializeField] private float gateStripHdrBoost = 2.6f;
    [SerializeField, Range(0.1f, 1f)] private float maxGroundContamination = 0.55f;
    [SerializeField, Range(0.1f, 1f)] private float maxLightContamination = 0.40f;
    [SerializeField] private float emissionColorAttenuation = 0.45f;

    [Header("Palettes")]
    [SerializeField] private ArenaPalette[] palettes;

    public ArenaPalette ActivePalette => activePalette;
    public int ActivePaletteIndex => activePaletteIndex;

    public event Action<ArenaPalette> OnPaletteApplied;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private ArenaPalette activePalette;
    private int activePaletteIndex = -1;
    private MaterialPropertyBlock propBlock;

    private Color currentGroundColor;
    private Color targetGroundColor;
    private Color currentLightColor = Color.white;
    private Color targetLightColor = Color.white;
    private Color[] currentWallColors;
    private Color[] targetWallColors;
    private bool isTransitioning;

    #region UnityLifecycle
    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
        palettes = ArenaPalette.GetDefaultPalettes();
        InitializeBuffers();
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void Start()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.06f, 0.07f, 0.11f, 1f);

        if (directionalLight != null)
        {
            directionalLight.intensity = 0.38f;
        }

        int randomIdx = UnityEngine.Random.Range(0, palettes.Length);
        ApplyPalette(randomIdx);
    }

    private void Update()
    {
        if (isTransitioning)
        {
            StepContaminationLerp();
        }

        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            int nextIdx = (activePaletteIndex + 1) % palettes.Length;
            ApplyPalette(nextIdx);
        }
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }
    #endregion

    #region Public API
    public void ApplyPalette(int index)
    {
        if (palettes == null || palettes.Length == 0) return;

        activePaletteIndex = Mathf.Clamp(index, 0, palettes.Length - 1);
        activePalette = palettes[activePaletteIndex];

        if (gateStripRenderers != null)
        {
            for (int i = 0; i < gateStripRenderers.Length; i++)
            {
                SetRendererColor(gateStripRenderers[i], activePalette.GateStripColor, gateStripHdrBoost);
            }
        }

        if (anchorPadRenderers != null)
        {
            Color padBase = activePalette.VoidGroundColor * 1.55f;
            for (int i = 0; i < anchorPadRenderers.Length; i++)
            {
                SetRendererColor(anchorPadRenderers[i], padBase, 0.85f);
            }
        }

        RecalculateContamination(immediate: true);
        OnPaletteApplied?.Invoke(activePalette);
    }
    #endregion

    #region Event Handlers
    private void SubscribeEvents()
    {
        if (gameManager != null) gameManager.OnScoresUpdated += HandleScoresUpdated;
    }

    private void UnsubscribeEvents()
    {
        if (gameManager != null) gameManager.OnScoresUpdated -= HandleScoresUpdated;
    }

    private void HandleScoresUpdated()
    {
        RecalculateContamination(immediate: false);
    }
    #endregion

    #region Contamination System
    private void RecalculateContamination(bool immediate)
    {
        int p1Score = (gameManager != null && player1Identity != null) ? gameManager.GetScore(player1Identity) : 0;
        int p2Score = (gameManager != null && player2Identity != null) ? gameManager.GetScore(player2Identity) : 0;
        int maxScore = (gameManager != null && gameManager.MaxScore > 0) ? gameManager.MaxScore : 100;

        float dominance = Mathf.Clamp((p1Score - p2Score) / (float)maxScore, -1f, 1f);
        Color dominantColor = dominance >= 0f ? activePalette.Player1Color : activePalette.Player2Color;
        float contaminationAmount = Mathf.Abs(dominance);

        targetGroundColor = Color.Lerp(activePalette.VoidGroundColor, dominantColor, contaminationAmount * maxGroundContamination);
        targetLightColor = Color.Lerp(Color.white, dominantColor, contaminationAmount * maxLightContamination);

        if (arenaWallRenderers != null && targetWallColors != null)
        {
            int totalWalls = arenaWallRenderers.Length;
            int half = totalWalls / 2;
            float p1Ratio = Mathf.Clamp01(p1Score / (float)maxScore);
            float p2Ratio = Mathf.Clamp01(p2Score / (float)maxScore);

            for (int i = 0; i < totalWalls; i++)
            {
                if (i < half)
                {
                    targetWallColors[i] = Color.Lerp(activePalette.VoidWallColor, activePalette.Player1Color * wallHdrBoost, p1Ratio);
                }
                else
                {
                    targetWallColors[i] = Color.Lerp(activePalette.VoidWallColor, activePalette.Player2Color * wallHdrBoost, p2Ratio);
                }

                if (immediate)
                {
                    currentWallColors[i] = targetWallColors[i];
                    SetRendererColor(arenaWallRenderers[i], currentWallColors[i], 1.0f);
                }
            }
        }

        if (immediate)
        {
            currentGroundColor = targetGroundColor;
            currentLightColor = targetLightColor;
            SetRendererColor(groundRenderer, currentGroundColor, 1.0f);
            if (directionalLight != null) directionalLight.color = currentLightColor;
            isTransitioning = false;
        }
        else
        {
            isTransitioning = true;
        }
    }

    private void StepContaminationLerp()
    {
        float step = Time.deltaTime * transitionSpeed;
        bool anyPending = false;

        currentGroundColor = Color.Lerp(currentGroundColor, targetGroundColor, step);
        SetRendererColor(groundRenderer, currentGroundColor, 1.0f);

        currentLightColor = Color.Lerp(currentLightColor, targetLightColor, step);
        if (directionalLight != null) directionalLight.color = currentLightColor;

        if (arenaWallRenderers != null && currentWallColors != null && targetWallColors != null)
        {
            for (int i = 0; i < arenaWallRenderers.Length; i++)
            {
                if (arenaWallRenderers[i] == null) continue;
                currentWallColors[i] = Color.Lerp(currentWallColors[i], targetWallColors[i], step);
                SetRendererColor(arenaWallRenderers[i], currentWallColors[i], 1.0f);

                if (Vector4.Distance(currentWallColors[i], targetWallColors[i]) > 0.01f)
                {
                    anyPending = true;
                }
            }
        }

        if (!anyPending &&
            Vector4.Distance(currentGroundColor, targetGroundColor) < 0.01f &&
            Vector4.Distance(currentLightColor, targetLightColor) < 0.01f)
        {
            isTransitioning = false;
        }
    }

    private void SetRendererColor(Renderer r, Color color, float hdrBoost)
    {
        if (r == null) return;
        Color hdr = new Color(color.r * hdrBoost, color.g * hdrBoost, color.b * hdrBoost, color.a);
        r.GetPropertyBlock(propBlock);
        propBlock.SetColor(BaseColorId, hdr);
        propBlock.SetColor(ColorId, hdr);
        propBlock.SetColor(EmissionColorId, hdr * emissionColorAttenuation);
        r.SetPropertyBlock(propBlock);
    }
    #endregion

    #region Helper Methods
    private void InitializeBuffers()
    {
        if (arenaWallRenderers != null && arenaWallRenderers.Length > 0)
        {
            currentWallColors = new Color[arenaWallRenderers.Length];
            targetWallColors = new Color[arenaWallRenderers.Length];
        }
    }
    #endregion
}
