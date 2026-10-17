using UnityEngine;

public class TotemVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealingTotem totem;
    [SerializeField] private Renderer totemRenderer;
    [SerializeField] private Light totemLight;
    [SerializeField] private ArenaThemeController themeController;

    [Header("Visual Parameters")]
    [SerializeField] private float readyHdr = 1.55f;
    [SerializeField] private float unreadyHdr = 0.8f;
    [SerializeField] private Color unreadyColor = new Color(0.18f, 0.20f, 0.24f);
    [SerializeField] private float emissionColorAttenuation = 0.45f;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private MaterialPropertyBlock propBlock;
    private ArenaPalette currentPalette;
    private Vector3 initialPosition;
    private Vector3 initialScale;

    #region UnityLifecycle
    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();

        initialPosition = transform.position;
        initialScale = transform.localScale;

        if (totem == null) totem = GetComponent<HealingTotem>();
        if (totemRenderer == null) totemRenderer = GetComponent<Renderer>();
        if (totemLight == null) totemLight = GetComponentInChildren<Light>();
        if (themeController == null) themeController = FindFirstObjectByType<ArenaThemeController>();

        ApplyPhysicalState();
    }

    private void OnEnable()
    {
        if (totem != null)
        {
            totem.OnTotemConsumed += HandleTotemConsumed;
            totem.OnTotemReadyChanged += HandleTotemReadyChanged;
        }

        if (themeController != null)
        {
            themeController.OnPaletteApplied += HandlePaletteApplied;
            if (themeController.ActivePaletteIndex >= 0)
            {
                HandlePaletteApplied(themeController.ActivePalette);
            }
        }

        ApplyPhysicalState();
        UpdateVisuals();
    }

    private void Update()
    {
        if (totem != null && totem.IsReady)
        {
            float bob = Mathf.Sin(Time.time * totem.BobFrequency + initialPosition.x) * totem.BobAmplitude;
            transform.position = new Vector3(initialPosition.x, initialPosition.y + bob, initialPosition.z);
            transform.Rotate(Vector3.up, totem.RotationSpeed * Time.deltaTime, Space.World);
        }
    }

    private void OnDisable()
    {
        if (totem != null)
        {
            totem.OnTotemConsumed -= HandleTotemConsumed;
            totem.OnTotemReadyChanged -= HandleTotemReadyChanged;
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
        UpdateVisuals();
    }

    private void HandleTotemReadyChanged()
    {
        ApplyPhysicalState();
        UpdateVisuals();
    }

    private void HandleTotemConsumed()
    {
        ApplyPhysicalState();
        UpdateVisuals();
    }
    #endregion

    #region Visual Application
    private void ApplyPhysicalState()
    {
        bool isReady = (totem == null || totem.IsReady);
        Vector3 scaleMultiplier = totem != null ? totem.CooldownScaleMultiplier : new Vector3(0.75f, 0.35f, 0.75f);
        float heightMultiplier = totem != null ? totem.CooldownHeightMultiplier : 0.5f;

        transform.localScale = isReady
            ? initialScale
            : new Vector3(
                initialScale.x * scaleMultiplier.x,
                initialScale.y * scaleMultiplier.y,
                initialScale.z * scaleMultiplier.z);

        if (!isReady)
        {
            transform.position = new Vector3(initialPosition.x, initialPosition.y * heightMultiplier, initialPosition.z);
        }
    }

    private void UpdateVisuals()
    {
        if (themeController != null && themeController.ActivePaletteIndex < 0) return;

        bool isReady = (totem == null || totem.IsReady);
        Color c = isReady ? currentPalette.TotemColor : unreadyColor;
        float hdr = isReady ? readyHdr : unreadyHdr;

        if (totemRenderer != null)
        {
            Color hdrColor = c * hdr;
            totemRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor(BaseColorId, hdrColor);
            propBlock.SetColor(ColorId, hdrColor);
            propBlock.SetColor(EmissionColorId, hdrColor * emissionColorAttenuation);
            totemRenderer.SetPropertyBlock(propBlock);
        }

        if (totemLight != null)
        {
            totemLight.enabled = isReady;
            totemLight.color = currentPalette.TotemColor;
        }
    }
    #endregion
}

