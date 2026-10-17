using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enum that represents all the possible states of a <see cref="CaptureZone"/>
/// </summary>
public enum CaptureState
{
    Neutral,
    Capturing,
    Contested,
    Captured
}

/// <summary>
/// Class that implements the capturing zone state logic.
/// </summary>
public class CaptureZone : MonoBehaviour
{
    [Header("Capture Parameters")]
    [Tooltip("Time taken (in seconds) for a player to capture a Zone")]
    [SerializeField] private float captureTime = 5.0f;
    
    // Public members
    /// <summary> The last player who has captured this CaptureZone </summary>
    public PlayerIdentity CurrentOwner {get; private set;}
    /// <summary> This CaptureZone's current state</summary>
    public CaptureState CurrentState {get; private set;} = CaptureState.Neutral;
    /// <summary> Triggered when the zone is captured. Related to sound effects / visual effects </summary>
    public event System.Action<CaptureZone, PlayerIdentity> OnZoneCaptured; 
    /// <summary> Floating point number that represents the progress of the ongoing capture </summary>
    public float CaptureProgress => Mathf.Clamp01(captureTimer / captureTime);
  
    // Private members
    private List<PlayerIdentity> playersInZone = new();
    private float captureTimer = 0f;
    
    #region UnityLifecycle
    private void Update()
    {
        if(CurrentState == CaptureState.Capturing)
        {
            captureTimer += Time.deltaTime;

            if(captureTimer >= captureTime)
            {
                EvaluateState();
                if(CurrentOwner != null) OnZoneCaptured?.Invoke(this, CurrentOwner);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<PlayerIdentity>(out var pId))
        {
            if(!playersInZone.Contains(pId)) playersInZone.Add(pId); // Verifies if the player's already in the zone (they may have two colliders)
        }
        EvaluateState();
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent<PlayerIdentity>(out var pId))
        {
            playersInZone.Remove(pId);
        }
        EvaluateState();
    }
    #endregion

    #region Helper Methods
    private void EvaluateState()
    {
        CaptureState newState = CaptureState.Neutral;
        if(playersInZone.Count == 0)
        {
            newState = CurrentOwner == null ? CaptureState.Neutral : CaptureState.Captured;
        }

        if(playersInZone.Count == 1)
        {
            if(captureTimer < captureTime)
            { 
                newState = CurrentOwner != playersInZone[0] ? CaptureState.Capturing : CaptureState.Captured;
            }
            else 
            {
                newState = CaptureState.Captured;
            }
        }

        if(playersInZone.Count == 2)
        {
            newState = CaptureState.Contested;
        }

        if(CurrentState != newState) {
            CurrentState = newState;
            OnStateChanged();
        }
    }

    private void OnStateChanged()
    {
        captureTimer = 0f;
        switch(CurrentState)
        {
            case CaptureState.Captured:
                if(playersInZone.Count > 0)
                {
                    CurrentOwner = playersInZone[0];
                }
                break;
        }
    }
    #endregion
}   