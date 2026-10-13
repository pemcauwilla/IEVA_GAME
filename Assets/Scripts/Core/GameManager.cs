using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Scoring Config")]
    [SerializeField] private float scoringInterval = 2f;
    [SerializeField] private int scoringValue = 10;
    [SerializeField] private int maxScore = 100;

    [Header("Death config")]
    [SerializeField] private float respawnInterval = 3f;

    [Header("Zones and Players")]
    [SerializeField] private CaptureZone[] zones; 
    [SerializeField] private PlayerSurvival[] players;

    private float scoringTimer = 0f;
    private Dictionary<PlayerIdentity, int> playerScoringMap = new();

    public event System.Action<PlayerIdentity> OnGameWon;

    private void Awake()
    {
        foreach(var survival in players)
        {
            survival.OnPlayerDied += PlayerDeath;
        }
    }

    private void OnDestroy()
    {
        foreach(var survival in players) // avoids calling methods that doesn't exist anymore with the event's '.Invoke()'
        {
            survival.OnPlayerDied -= PlayerDeath;
        }
    }

    private void Update()
    {
        scoringTimer += Time.deltaTime;

        if(scoringTimer >= scoringInterval)
        {
            scoringTimer = 0f;
            foreach(var zone in zones)
            {
                if(zone.CurrentOwner != null){
                    if(!playerScoringMap.ContainsKey(zone.CurrentOwner)) playerScoringMap[zone.CurrentOwner] = 0;
                    
                    playerScoringMap[zone.CurrentOwner] += scoringValue;

                    if(playerScoringMap[zone.CurrentOwner] >= maxScore) OnGameWon?.Invoke(zone.CurrentOwner);                
                }
            }
        }
    }

    /// <summary>
    /// Method called after a player death.
    /// </summary>
    /// <param name="player">PlayerIdentity component of the player who died.</param>
    private void PlayerDeath(PlayerSurvival player) 
    {
        StartCoroutine(RespawnRoutine(player));
    }

    /// <summary>
    /// Respawn coroutine so it is not needed to store each players respawn timer individually.
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
}