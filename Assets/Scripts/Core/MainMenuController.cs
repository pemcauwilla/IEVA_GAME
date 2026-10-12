using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject ControlsPanel;
    public string GameSceneName = "Scene_Dev1";

    private void Start()
    {
        if (ControlsPanel != null)
            ControlsPanel.SetActive(false);
    }

    public void PlayGame()
    {
        if (!Application.CanStreamedLevelBeLoaded(GameSceneName))
        {
            Debug.LogError(
                $"La scène '{GameSceneName}' doit être ajoutée à la liste des scènes du build."
            );
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(GameSceneName);
    }

    public void OpenControls()
    {
        if (ControlsPanel != null)
            ControlsPanel.SetActive(true);
    }

    public void CloseControls()
    {
        if (ControlsPanel != null)
            ControlsPanel.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("Quitter : arrêt du mode Play dans Unity.");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}