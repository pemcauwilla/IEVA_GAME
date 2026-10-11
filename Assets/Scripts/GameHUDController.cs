using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUDController : MonoBehaviour
{
    [Header("Références UI")]
    public Slider CubeCoherenceSlider;
    public Slider SphereCoherenceSlider;
    public TMP_Text CubeCoherenceText;
    public TMP_Text SphereCoherenceText;
    public TMP_Text MatchTimerText;
    public TMP_Text ZoneStatusText;
    public TMP_Text ScoreText;
    public GameObject EndGamePanel;
    public TMP_Text EndGameText;

    [Header("Valeurs de test temporaires")]
    [Range(0, 100)] public float CubeCoherence = 100;
    [Range(0, 100)] public float SphereCoherence = 100;
    [Min(0)] public float RemainingTime = 90;
    [Min(0)] public int CubeScore;
    [Min(0)] public int SphereScore;

    public string ZoneAStatus = "NEUTRE";
    public string ZoneBStatus = "NEUTRE";
    public string ZoneCStatus = "NEUTRE";

    public bool ShowEndPanel;
    public string EndMessage = "FIN DE PARTIE\nRésultat de test";

    private void Update()
    {
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        if (CubeCoherenceSlider != null)
            CubeCoherenceSlider.value = CubeCoherence;

        if (SphereCoherenceSlider != null)
            SphereCoherenceSlider.value = SphereCoherence;

        if (CubeCoherenceText != null)
            CubeCoherenceText.text =
                $"CUBE — COHÉRENCE TEST : {CubeCoherence:0} %";

        if (SphereCoherenceText != null)
            SphereCoherenceText.text =
                $"SPHÈRE — COHÉRENCE TEST : {SphereCoherence:0} %";

        int totalSeconds = Mathf.CeilToInt(Mathf.Max(0, RemainingTime));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        if (MatchTimerText != null)
            MatchTimerText.text = $"TEMPS TEST : {minutes:00}:{seconds:00}";

        if (ZoneStatusText != null)
            ZoneStatusText.text =
                $"A : {ZoneAStatus}     B : {ZoneBStatus}     C : {ZoneCStatus}";

        if (ScoreText != null)
            ScoreText.text =
                $"SCORE TEST — CUBE : {CubeScore}     SPHÈRE : {SphereScore}";

        if (EndGameText != null)
            EndGameText.text = EndMessage;

        if (EndGamePanel != null &&
            EndGamePanel.activeSelf != ShowEndPanel)
        {
            EndGamePanel.SetActive(ShowEndPanel);
        }
    }
}