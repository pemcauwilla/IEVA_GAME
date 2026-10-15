using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LowCoherenceFeedback : MonoBehaviour
{
    [Header("Références")]
    public Volume FeedbackVolume;
    public GameHUDController HUD;

    [Header("Réglages")]
    [Range(1f, 100f)]
    public float WarningThreshold = 25f;

    [Range(0f, 1f)]
    public float WarningIntensity = 0.35f;

    [Min(0.01f)]
    public float TransitionSpeed = 1f;

    [Header("Test indépendant du HUD")]
    public bool UseTestValues = true;

    [Range(0f, 100f)]
    public float TestCubeCoherence = 100f;

    [Range(0f, 100f)]
    public float TestSphereCoherence = 100f;

    [Header("Diagnostic pendant Play")]
    [SerializeField] private float cubeValue;
    [SerializeField] private float sphereValue;
    [SerializeField] private bool alertActive;
    [SerializeField] private float currentIntensity;

    private Vignette vignette;

    public bool IsAlertActive => alertActive;
    private void Start()
    {
        if (FeedbackVolume == null)
        {
            Debug.LogError("Feedback Volume est vide.", this);
            enabled = false;
            return;
        }

        VolumeProfile runtimeProfile = FeedbackVolume.profile;

        if (runtimeProfile == null ||
            !runtimeProfile.TryGet<Vignette>(out vignette))
        {
            Debug.LogError("Le Volume doit contenir une Vignette.", this);
            enabled = false;
            return;
        }

        vignette.active = true;
        vignette.intensity.overrideState = true;
        vignette.intensity.value = 0f;
    }

    private void Update()
    {
        bool gameEnded = false;

        if (UseTestValues)
        {
            cubeValue = TestCubeCoherence;
            sphereValue = TestSphereCoherence;
        }
        else
        {
            if (HUD == null)
            {
                Debug.LogError("HUD est vide : renseigne GameHUDController.", this);
                enabled = false;
                return;
            }

            cubeValue = HUD.CubeCoherence;
            sphereValue = HUD.SphereCoherence;
            gameEnded = HUD.ShowEndPanel;
        }

        alertActive = !gameEnded &&
            (cubeValue < WarningThreshold ||
             sphereValue < WarningThreshold);

        float targetIntensity = alertActive ? WarningIntensity : 0f;

        vignette.intensity.value = Mathf.MoveTowards(
            vignette.intensity.value,
            targetIntensity,
            Mathf.Max(0.01f, TransitionSpeed) * Time.deltaTime
        );

        currentIntensity = vignette.intensity.value;
    }

    private void OnDisable()
    {
        if (vignette != null)
            vignette.intensity.value = 0f;

        currentIntensity = 0f;
        alertActive = false;
    }
}