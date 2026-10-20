using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameHUDController : MonoBehaviour
{
    [Header("Player 1 UI Elements")]
    [FormerlySerializedAs("CubeCoherenceSlider")]
    [SerializeField] private Slider player1HealthSlider;
    [FormerlySerializedAs("CubeCoherenceText")]
    [SerializeField] private TMP_Text player1HealthText;
    [SerializeField] private Image p1SliderFill;

    [Header("Player 2 UI Elements")]
    [FormerlySerializedAs("SphereCoherenceSlider")]
    [SerializeField] private Slider player2HealthSlider;
    [FormerlySerializedAs("SphereCoherenceText")]
    [SerializeField] private TMP_Text player2HealthText;
    [SerializeField] private Image p2SliderFill;

    [Header("Match & Zone Display")]
    [FormerlySerializedAs("MatchTimerText")]
    [SerializeField] private TMP_Text matchTimerText;
    [FormerlySerializedAs("ScoreText")]
    [SerializeField] private TMP_Text scoreText;
    [FormerlySerializedAs("ZoneStatusText")]
    [SerializeField] private TMP_Text zoneStatusText;

    [Header("End Game Modal")]
    [FormerlySerializedAs("EndGamePanel")]
    [SerializeField] private GameObject endGamePanel;
    [FormerlySerializedAs("EndGameText")]
    [SerializeField] private TMP_Text endGameText;

    [Header("Gameplay References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private MovingZoneController movingZoneController;
    [SerializeField] private PlayerSurvival player1Survival;
    [SerializeField] private PlayerSurvival player2Survival;
    [SerializeField] private PlayerIdentity player1Identity;
    [SerializeField] private PlayerIdentity player2Identity;
    [SerializeField] private CaptureZone[] captureZones;
    [SerializeField] private MainMenuController mainMenuController;
    [SerializeField] private ArenaThemeController themeController;

    private ArenaPalette currentPalette;
    private bool isGameOver = false;

    #region UnityLifecycle
    private void Awake()
    {
        if (p1SliderFill == null && player1HealthSlider != null && player1HealthSlider.fillRect != null)
            player1HealthSlider.fillRect.TryGetComponent(out p1SliderFill);
        if (p2SliderFill == null && player2HealthSlider != null && player2HealthSlider.fillRect != null)
            player2HealthSlider.fillRect.TryGetComponent(out p2SliderFill);

        RefreshAllDisplays();
    }

    private void OnEnable()
    {
        SubscribeEvents();
        RefreshAllDisplays();
    }

    private void Start()
    {
        if (endGamePanel != null) endGamePanel.SetActive(false);
        RefreshAllDisplays();
    }

    private void Update()
    {
        if (!isGameOver)
        {
            UpdateTimerAndZonesDisplay();
        }
        HandleMatchShortcuts();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }
    #endregion

    #region Event Subscriptions & Handlers
    private void SubscribeEvents()
    {
        if (player1Survival != null) player1Survival.OnHealthChanged += HandleP1HealthChanged;
        if (player2Survival != null) player2Survival.OnHealthChanged += HandleP2HealthChanged;

        if (gameManager != null)
        {
            gameManager.OnScoresUpdated += HandleScoresUpdated;
            gameManager.OnGameWon += HandleGameWon;
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

    private void UnsubscribeEvents()
    {
        if (player1Survival != null) player1Survival.OnHealthChanged -= HandleP1HealthChanged;
        if (player2Survival != null) player2Survival.OnHealthChanged -= HandleP2HealthChanged;

        if (gameManager != null)
        {
            gameManager.OnScoresUpdated -= HandleScoresUpdated;
            gameManager.OnGameWon -= HandleGameWon;
        }

        if (themeController != null)
        {
            themeController.OnPaletteApplied -= HandlePaletteApplied;
        }
    }

    private void HandleP1HealthChanged(float current, float max)
    {
        if (player1HealthSlider != null)
        {
            player1HealthSlider.maxValue = max;
            player1HealthSlider.value = current;
        }
        if (player1HealthText != null)
        {
            player1HealthText.text = $"JOUEUR 1 : <b>{Mathf.RoundToInt((current / Mathf.Max(1f, max)) * 100f)} %</b>";
        }
    }

    private void HandleP2HealthChanged(float current, float max)
    {
        if (player2HealthSlider != null)
        {
            player2HealthSlider.maxValue = max;
            player2HealthSlider.value = current;
        }
        if (player2HealthText != null)
        {
            player2HealthText.text = $"JOUEUR 2 : <b>{Mathf.RoundToInt((current / Mathf.Max(1f, max)) * 100f)} %</b>";
        }
    }

    private void HandleScoresUpdated()
    {
        int p1Score = (gameManager != null && player1Identity != null) ? gameManager.GetScore(player1Identity) : 0;
        int p2Score = (gameManager != null && player2Identity != null) ? gameManager.GetScore(player2Identity) : 0;
        int maxScore = (gameManager != null && gameManager.MaxScore > 0) ? gameManager.MaxScore : 100;

        if (scoreText != null)
        {
            string p1Hex = !string.IsNullOrEmpty(currentPalette.PaletteName)
                ? ColorUtility.ToHtmlStringRGB(currentPalette.Player1Color)
                : "00EBFF";
            string p2Hex = !string.IsNullOrEmpty(currentPalette.PaletteName)
                ? ColorUtility.ToHtmlStringRGB(currentPalette.Player2Color)
                : "FF1F9E";

            scoreText.text = $"SCORE   <color=#{p1Hex}><b>JOUEUR 1 : {p1Score}</b></color> / {maxScore}     —     <color=#{p2Hex}><b>JOUEUR 2 : {p2Score}</b></color> / {maxScore}";
        }
    }

    private void HandleGameWon(PlayerIdentity winner)
    {
        isGameOver = true;
        Time.timeScale = 0f;

        if (endGamePanel != null)
        {
            endGamePanel.SetActive(true);
        }

        if (endGameText != null)
        {
            string winnerName = (winner != null && winner.Team == PlayerTeam.Team1) ? "JOUEUR 1" : "JOUEUR 2";
            string hex = (winner != null && winner.Team == PlayerTeam.Team1)
                ? ColorUtility.ToHtmlStringRGB(currentPalette.Player1Color)
                : ColorUtility.ToHtmlStringRGB(currentPalette.Player2Color);

            endGameText.text =
                $"<size=120%><b>VICTOIRE DU <color=#{hex}>{winnerName}</color> !</b></size>\n\n" +
                $"<size=80%><color=#94A3B8>[R] REJOUER     [ESC] MENU PRINCIPAL</color></size>";
        }
    }

    private void HandlePaletteApplied(ArenaPalette palette)
    {
        currentPalette = palette;

        if (p1SliderFill != null) p1SliderFill.color = palette.Player1Color;
        if (p2SliderFill != null) p2SliderFill.color = palette.Player2Color;
    }
    #endregion

    #region Helper & Refresh Methods
    private void RefreshAllDisplays()
    {
        if (player1Survival != null) HandleP1HealthChanged(player1Survival.CurrentHealth, player1Survival.MaxHealth);
        if (player2Survival != null) HandleP2HealthChanged(player2Survival.CurrentHealth, player2Survival.MaxHealth);
        HandleScoresUpdated();
        UpdateTimerAndZonesDisplay();
    }

    private void UpdateTimerAndZonesDisplay()
    {
        if (movingZoneController != null && matchTimerText != null)
        {
            int remaining = Mathf.CeilToInt(movingZoneController.RemainingTime);
            matchTimerText.text = $"ROTATION DES ZONES : <b>{remaining:00}s</b>";
        }

        if (zoneStatusText != null && captureZones != null && captureZones.Length > 0)
        {
            string status = "";
            for (int i = 0; i < captureZones.Length; i++)
            {
                var z = captureZones[i];
                if (z == null) continue;

                string badge = z.CurrentState switch
                {
                    CaptureState.Neutral => "<color=#94A3B8>NEUTRE</color>",
                    CaptureState.Capturing => "<color=#F59E0B>CAPTURE...</color>",
                    CaptureState.Contested => "<color=#EF4444>CONTESTÉE</color>",
                    CaptureState.Captured => z.CurrentOwner != null && z.CurrentOwner.Team == PlayerTeam.Team1
                        ? "<color=#00EBFF>J1</color>"
                        : "<color=#FF1F9E>J2</color>",
                    _ => "ZONE"
                };

                status += $"ZONE {i + 1} : [{badge}]     ";
            }
            zoneStatusText.text = status.TrimEnd();
        }
    }

    private void HandleMatchShortcuts()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.rKey.wasPressedThisFrame)
        {
            Time.timeScale = 1f;
            MainMenuController.SkipMenuOnNextLoad();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (kb.escapeKey.wasPressedThisFrame && mainMenuController != null)
        {
            mainMenuController.ShowMenuOverlay();
        }
    }
    #endregion
}
