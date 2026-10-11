using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Scripting;

public class GameManager : MonoBehaviour
{
    public int scoreP1 = 0;
    public int scoreP2 = 0;
    public int targetScore = 30;
    public float lastScoreTime = 0; 
    public float scoringTimer = 1f; // each 1s scoring happens
    public CaptureZone[] zones;
    public event System.Action<PlayerTeam> OnGameOver;
    private bool isGameOver = false;

    public void Start()
    {
        lastScoreTime = Time.time;
    }

    public void Update()
    {
       if(Time.time - lastScoreTime >= scoringTimer)
        {
            lastScoreTime = Time.time;

            foreach(CaptureZone zone in zones) 
            {
                if(zone.currentOwner == PlayerTeam.Player1) scoreP1++;
                if(zone.currentOwner == PlayerTeam.Player2) scoreP2++;
            }
        }

        if (!isGameOver)
        {
            if(scoreP1 >= targetScore) {
                OnGameOver?.Invoke(PlayerTeam.Player1);
                isGameOver = true;
            }
            
            if(scoreP2 >= targetScore)
            {
                OnGameOver?.Invoke(PlayerTeam.Player2);
                isGameOver = true;
            } 
        }
    }

    public void ResetMatch()
    {
        scoreP1 = 0;
        scoreP2 = 0;
        lastScoreTime = Time.time;
        isGameOver = false;
    }

}
