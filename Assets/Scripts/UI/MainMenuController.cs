using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Element References")]
    [FormerlySerializedAs("ControlsPanel")]
    [SerializeField] private GameObject controlsPanel;

    [FormerlySerializedAs("HudObject")]
    [SerializeField] private GameObject hudObject;

    [FormerlySerializedAs("GameSceneName")]
    [SerializeField] private string gameSceneName = "Scene_LinearArena";

    [Header("Text & Button References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text controlsText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button closeControlsButton;
    [SerializeField] private ArenaThemeController themeController;

    public GameObject ControlsPanel { get => controlsPanel; set => controlsPanel = value; }
    public GameObject HudObject { get => hudObject; set => hudObject = value; }
    public string GameSceneName { get => gameSceneName; set => gameSceneName = value; }

    private static bool skipMenuOnNextLoad = false;

    public static void SkipMenuOnNextLoad() => skipMenuOnNextLoad = true;

    #region UnityLifecycle
    private void Awake()
    {
        SetupButtonListeners();
    }

    private void OnEnable()
    {
        if (themeController != null)
        {
            themeController.OnPaletteApplied += ApplyPaletteToMenu;
            if (!string.IsNullOrEmpty(themeController.ActivePalette.PaletteName))
            {
                ApplyPaletteToMenu(themeController.ActivePalette);
            }
        }
    }

    private void Start()
    {
        if (controlsPanel != null)
        {
            if (controlsPanel.TryGetComponent<Image>(out var img))
            {
                img.color = new Color(0.02f, 0.03f, 0.05f, 0.98f);
            }
            controlsPanel.SetActive(false);
        }

        if (SceneManager.GetActiveScene().name == gameSceneName)
        {
            if (skipMenuOnNextLoad)
            {
                skipMenuOnNextLoad = false;
                Time.timeScale = 1f;
                if (hudObject != null) hudObject.SetActive(true);
                gameObject.SetActive(false);
                return;
            }

            Time.timeScale = 0f;
            if (hudObject != null) hudObject.SetActive(false);
        }

        UpdateMenuRichTexts("00EBFF", "FF1F9E", "CYBER-SYNTH");
    }

    private void OnDisable()
    {
        if (themeController != null)
        {
            themeController.OnPaletteApplied -= ApplyPaletteToMenu;
        }
    }
    #endregion

    #region Public API (Ações de Botões)
    public void PlayGame()
    {
        Time.timeScale = 1f;

        if (SceneManager.GetActiveScene().name == gameSceneName)
        {
            if (hudObject != null) hudObject.SetActive(true);
            gameObject.SetActive(false);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            Debug.LogError($"A cena '{gameSceneName}' deve estar na lista de Build Settings.");
            return;
        }

        skipMenuOnNextLoad = true;
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowMenuOverlay()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (hudObject != null) hudObject.SetActive(false);

        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OpenControls()
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(true);
            if (controlsPanel.TryGetComponent<Image>(out var img))
            {
                img.color = new Color(0.02f, 0.03f, 0.05f, 0.98f);
            }
        }
    }

    public void CloseControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("QuitGame: Parando o modo Play no Editor.");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion

    #region Theming & Helper Methods
    private void ApplyPaletteToMenu(ArenaPalette palette)
    {
        string p1Hex = ColorUtility.ToHtmlStringRGB(palette.Player1Color);
        string p2Hex = ColorUtility.ToHtmlStringRGB(palette.Player2Color);
        string palName = string.IsNullOrEmpty(palette.PaletteName) ? "CYBER-SYNTH" : palette.PaletteName;

        UpdateMenuRichTexts(p1Hex, p2Hex, palName);
    }

    private void UpdateMenuRichTexts(string p1Hex, string p2Hex, string palName)
    {
        if (titleText != null)
        {
            titleText.text =
                $"<size=42%><color=#94A3B8>PROJET IEVA  //  PALETTE : <color=#E2E8F0>{palName}</color></color></size>\n" +
                $"<b><color=#{p1Hex}>PERCEPTION</color> <color=#F8FAFC>&</color> <color=#{p2Hex}>ENTROPIE</color></b>\n" +
                $"<size=44%><i><color=#CBD5E1>\"We don't see things as they are, we see them as we are.\"</color></i></size>";
        }

        if (controlsText != null)
        {
            controlsText.text =
                $"<size=115%><b><color=#F8FAFC>COMMANDES & RÈGLES D'ENGAGEMENT</color></b></size>\n\n" +
                $"<color=#{p1Hex}><b>JOUEUR 1 (OUEST)</b></color>   —   Déplacement : <b>WASD / ZQSD</b>   |   Dash : <b>ESPACE</b>\n" +
                $"<color=#{p2Hex}><b>JOUEUR 2 (EST)</b></color>     —   Déplacement : <b>FLÈCHES</b>       |   Dash : <b>SHIFT DROIT</b>\n\n" +
                $"<size=88%><color=#CBD5E1>• <b>OBJECTIF (100 PTS) :</b> Capturez les <b>3 Zones Mouvantes</b> pour contaminer l'arène\n" +
                $"  avec votre réalité chromatique e régénérer votre Santé / Vie.\n" +
                $"• <b>SURVIE :</b> Évitez les <b>4 Gardiens</b>, utilisez les <b>Portes Dynamiques</b> pour leur couper\n" +
                $"  la route e récupérez les <b>Totems de Soin</b> (+40 HP).</color></size>\n\n" +
                $"<size=80%><color=#94A3B8><b>[P]</b> Changer de Palette en direct      <b>[ESC]</b> Pause</color></size>";
        }
    }

    private void SetupButtonListeners()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(PlayGame);
            playButton.onClick.AddListener(PlayGame);
        }
        if (controlsButton != null)
        {
            controlsButton.onClick.RemoveListener(OpenControls);
            controlsButton.onClick.AddListener(OpenControls);
        }
        if (closeControlsButton != null)
        {
            closeControlsButton.onClick.RemoveListener(CloseControls);
            closeControlsButton.onClick.AddListener(CloseControls);
        }
        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
            quitButton.onClick.AddListener(QuitGame);
        }
    }
    #endregion
}
