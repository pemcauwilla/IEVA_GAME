using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Score Parameters")]
    [Tooltip("Score needed to win the match")]
    [SerializeField] private int maxScore = 100;
    [Tooltip("The amount of points gained on each scoring tick")]
    [SerializeField] private int scoringValue = 10;
    [Tooltip("The time in seconds for the scoring tick")]
    [SerializeField] private float scoringInterval = 2f;

    [Header("Death Parameters")]
    [Tooltip("The time in seconds the player waits to respawn")]
    [SerializeField] private float respawnInterval = 3f;

    [Header("Zones")]
    [Tooltip("Reference to all the CaptureZones")]
    [SerializeField] private CaptureZone[] zones;
    [Tooltip("Reference to all the Players ")]
    [SerializeField] private PlayerSurvival[] players;

    public int MaxScore => maxScore;

    public event System.Action OnScoresUpdated;

    public event System.Action<PlayerIdentity> OnGameWon;

    private bool isGameOver = false;
    private Dictionary<PlayerIdentity, int> playerScoringMap = new();
    private float scoringTimer = 0f;

    #region UnityLifecycle
    private void Awake()
    {
        if (zones == null || zones.Length == 0)
        {
            zones = FindObjectsByType<CaptureZone>(FindObjectsSortMode.None);
        }
        if (players == null || players.Length == 0)
        {
            players = FindObjectsByType<PlayerSurvival>(FindObjectsSortMode.None);
        }

        if (players != null)
        {
            foreach(var survival in players)
            {
                if (survival != null) survival.OnPlayerDied += PlayerDeath;
            }
        }
    }

    private void Start()
    {
        OnScoresUpdated?.Invoke();
    }

    private void Update()
    {
        if(isGameOver) return;
        scoringTimer += Time.deltaTime;

        if(scoringTimer >= scoringInterval)
        {
            scoringTimer = 0f;
            bool anyScoreChanged = false;
            if (zones != null)
            {
                foreach(var zone in zones)
                {
                    if (zone == null) continue;
                    PlayerIdentity currentOwner = zone.CurrentOwner;

                    if(currentOwner != null)
                    {
                        if(!playerScoringMap.ContainsKey(currentOwner)) playerScoringMap[currentOwner] = 0;
                        playerScoringMap[currentOwner] += scoringValue;
                        anyScoreChanged = true;
                        if(playerScoringMap[currentOwner] >= maxScore)
                        {
                            OnGameWon?.Invoke(currentOwner);
                            isGameOver = true;
                        }
                    }
                }
            }

            if (anyScoreChanged)
            {
                OnScoresUpdated?.Invoke();
            }
        }
    }

    private void OnDestroy()
    {
        if (players != null)
        {
            foreach(var survival in players)
            {
                if (survival != null) survival.OnPlayerDied -= PlayerDeath;
            }
        }
    }
    #endregion

    #region PublicApi
    public int GetScore(PlayerIdentity player)
    {
        if(player != null && playerScoringMap.ContainsKey(player)) return playerScoringMap[player];
        return 0;
    }
    #endregion

    #region Helper Methods

    private void PlayerDeath(PlayerSurvival player)
    {
        StartCoroutine(RespawnRoutine(player));
    }

    private System.Collections.IEnumerator RespawnRoutine(PlayerSurvival player)
    {
        player.gameObject.SetActive(false);

        yield return new WaitForSeconds(respawnInterval);

        player.gameObject.SetActive(true);
        player.RevivePlayer();
    }
    #endregion
}
