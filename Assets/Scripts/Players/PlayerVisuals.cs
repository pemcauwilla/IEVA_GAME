using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerIdentity playerIdentity;
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private Light playerLight;
    [SerializeField] private TrailRenderer playerTrail;
    [SerializeField] private ArenaThemeController themeController;

    [Header("Visual Parameters")]
    [SerializeField] private float hdrBoost = 2.1f;
    [SerializeField] private float lightIntensity = 5.5f;
    [SerializeField] private float emissionColorAttenuation = 0.45f;
    [SerializeField] private float trailHdrBoost = 1.8f;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private MaterialPropertyBlock propBlock;
    private Color currentTeamColor = Color.white;

    #region UnityLifecycle
    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();

        if (playerIdentity == null) playerIdentity = GetComponent<PlayerIdentity>();
        if (playerRenderer == null) playerRenderer = GetComponent<Renderer>();
        if (playerLight == null) playerLight = GetComponentInChildren<Light>();
        if (playerTrail == null) playerTrail = GetComponent<TrailRenderer>();
        if (themeController == null) themeController = FindFirstObjectByType<ArenaThemeController>();
    }

    private void OnEnable()
    {
        if (themeController != null)
        {
            themeController.OnPaletteApplied += ApplyPalette;
            if (themeController.ActivePaletteIndex >= 0)
            {
                ApplyPalette(themeController.ActivePalette);
            }
        }
    }

    private void OnDisable()
    {
        if (themeController != null)
        {
            themeController.OnPaletteApplied -= ApplyPalette;
        }
    }
    #endregion

    #region Visual Application
    public void ApplyPalette(ArenaPalette palette)
    {
        currentTeamColor = (playerIdentity != null && playerIdentity.Team == PlayerTeam.Team2)
            ? palette.Player2Color
            : palette.Player1Color;

        if (playerRenderer != null)
        {
            Color hdrColor = currentTeamColor * hdrBoost;
            playerRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor(BaseColorId, hdrColor);
            propBlock.SetColor(ColorId, hdrColor);
            propBlock.SetColor(EmissionColorId, hdrColor * emissionColorAttenuation);
            playerRenderer.SetPropertyBlock(propBlock);
        }

        if (playerLight != null)
        {
            playerLight.color = currentTeamColor;
            playerLight.intensity = lightIntensity;
        }

        if (playerTrail != null)
        {
            Color trailColor = currentTeamColor * trailHdrBoost;
            playerTrail.startColor = trailColor;
            playerTrail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
        }
    }
    #endregion
}
