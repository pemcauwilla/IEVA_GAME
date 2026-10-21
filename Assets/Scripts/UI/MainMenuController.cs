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

    #region Public API
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
            Debug.LogError($"La scène '{gameSceneName}' doit être présente dans les Build Settings.");
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
        Debug.Log("QuitGame: Arrêt du mode Play dans l'éditeur.");
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
                $"<size=120%><b><color=#F8FAFC>COMMANDES & RÈGLES DU JEU</color></b></size>\n\n" +
                $"<color=#{p1Hex}><b>JOUEUR 1</b></color>  :  <b>ZQSD / WASD</b>   |   Dash : <b>ESPACE</b>\n" +
                $"<color=#{p2Hex}><b>JOUEUR 2</b></color>  :  <b>FLÈCHES</b>       |   Dash : <b>SHIFT DROIT / ENTRÉE</b>\n\n" +
                $"<size=90%><color=#CBD5E1>• <b>OBJECTIF :</b> Atteindre <b>100 points</b> en contrôlant les 3 zones mobiles.\n" +
                $"• <b>SUMO :</b> Utilisez le Dash pour expulser l'adversaire hors des zones.\n" +
                $"• <b>SURVIE :</b> Hors des zones, l'entropie vide votre vie. Esquivez les Gardiens !</color></size>\n\n" +
                $"<size=80%><color=#94A3B8><b>[ESC]</b> Pause / Menu      <b>[R]</b> Recommencer</color></size>";
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
