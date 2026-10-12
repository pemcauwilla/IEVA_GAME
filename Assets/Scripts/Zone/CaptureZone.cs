using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

public enum ZoneState
{
    Neutral,
    Capturing,
    Contested,
    Captured
}
public class CaptureZone : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private float captureDuration = 3f;
    [SerializeField]
    private float currentProgress = 0f;
    
    public PlayerTeam currentOwner = PlayerTeam.None;
    private ZoneState currentState = ZoneState.Neutral;

    private List<PlayerIdentity> playersInZone = new();
    public event System.Action<PlayerTeam> OnZoneCaptured;

    public void OnTriggerEnter(Collider other)
    {
        if(!other.TryGetComponent<PlayerIdentity>(out var player)) return; 
        playersInZone.Add(player);
    }

    public void OnTriggerExit(Collider other)
    {
        if(!other.TryGetComponent<PlayerIdentity>(out var player)) return;
        playersInZone.Remove(player);
    }

    public void Update()
    {
        switch (playersInZone.Count)
        {
            case 0: HandleEmptyZone(); break;
            case 1: HandleCapturing(); break;
            case 2: HandleContesting(); break;
        }
    }

    private void HandleEmptyZone()
    {
        currentState = currentOwner == PlayerTeam.None ? ZoneState.Neutral : ZoneState.Captured;
        currentProgress = 0f;
    }

    private void HandleCapturing()
    {
        PlayerTeam zonePlayer = playersInZone[0].playerTeam;

        if(currentOwner == zonePlayer) {
            currentState = ZoneState.Captured;
            currentProgress = 0f;
            return;
        }

        // Nobody has captured it yet or Changing zone ownership
        currentProgress += Time.deltaTime;
        currentState = ZoneState.Capturing;
    
        if(currentProgress >= captureDuration)
        {
            if(zonePlayer != currentOwner) currentOwner = zonePlayer;
            OnZoneCaptured?.Invoke(currentOwner); 
            currentState = ZoneState.Captured;
            currentProgress = 0f;  
        }
    }

    private void HandleContesting()
    {
        currentState = ZoneState.Contested;
    }

}
