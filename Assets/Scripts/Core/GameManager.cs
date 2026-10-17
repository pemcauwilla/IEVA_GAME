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

    // Public members
    /// <summary> Event triggered when a player reaches the maximum score/// </summary>
    public event System.Action<PlayerIdentity> OnGameWon;

    // Private members
    private bool isGameOver = false;
    private Dictionary<PlayerIdentity, int> playerScoringMap = new();
    private float scoringTimer = 0f;

    #region UnityLifecycle
    private void Awake()
    {
        foreach(var survival in players)
        {
            survival.OnPlayerDied += PlayerDeath;
        }
    }

    private void Update()
    {
        if(isGameOver) return;
        scoringTimer += Time.deltaTime;

        if(scoringTimer >= scoringInterval)
        {
            scoringTimer = 0f;
            foreach(var zone in zones)
            {
                PlayerIdentity currentOwner = zone.CurrentOwner;
                // Score update
                if(currentOwner != null)
                {
                    if(!playerScoringMap.ContainsKey(currentOwner)) playerScoringMap[currentOwner] = 0;
                    playerScoringMap[currentOwner] += scoringValue;
                    if(playerScoringMap[currentOwner] >= maxScore) 
                    {
                        OnGameWon?.Invoke(currentOwner);                
                        isGameOver = true;
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        foreach(var survival in players) // avoids calling methods that doesn't exist anymore with the event's '.Invoke()'
        {
            survival.OnPlayerDied -= PlayerDeath;
        }
    }
    #endregion

    #region PublicApi
    public int GetScore(PlayerIdentity player)
    {
        if(playerScoringMap.ContainsKey(player)) return playerScoringMap[player];
        return 0; 
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Method called after a player death.
    /// </summary>
    /// <param name="player">PlayerSurvival component of the dead player</param>
    private void PlayerDeath(PlayerSurvival player) 
    {
        StartCoroutine(RespawnRoutine(player));
    }

    /// <summary>
    /// Respawn coroutine. PS : Using coroutines so we don't have to store timers for each player individually
    /// </summary>
    /// <param name="player">PlayerIdentity component of the player who died.</param>
    /// <returns></returns>
    private System.Collections.IEnumerator RespawnRoutine(PlayerSurvival player)
    {
        player.gameObject.SetActive(false);

        yield return new WaitForSeconds(respawnInterval);

        player.gameObject.SetActive(true);
        player.RevivePlayer();
    }
    #endregion
}   