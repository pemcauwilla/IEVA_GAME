using System.Collections.Generic;
using UnityEngine;

public enum CaptureState
{
    Neutral,
    Capturing,
    Contested,
    Captured
}

public class CaptureZone : MonoBehaviour
{
    

    [Header("Capture config")]
    [SerializeField] private float captureTime = 5.0f;
    private float captureTimer = 0f;

    private List<PlayerIdentity> playersInZone = new(); 
    public PlayerIdentity CurrentOwner {get; private set;}

    public CaptureState CurrentState {get; private set;} = CaptureState.Neutral;

    /// <summary>
    /// Fired when the zone is captured. Related to sound effects / visual effects
    /// </summary>
    public event System.Action OnZoneCaptured; 

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<PlayerIdentity>(out var pId))
        {
            if(!playersInZone.Contains(pId)) playersInZone.Add(pId); // in case the player has two collider components
        }
        UpdateState();
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent<PlayerIdentity>(out var pId))
        {
            playersInZone.Remove(pId);
        }
        UpdateState();
    }

    private void Update()
    {
        if(CurrentState == CaptureState.Capturing)
        {
            captureTimer += Time.deltaTime;

            if(captureTimer >= captureTime)
            {
                UpdateState();
                OnZoneCaptured?.Invoke();
            }
        }
    }

    /// <summary>
    /// Updates the new state of the zone according to how many players there are and calls <ref>ApplyState()</ref>.
    /// </summary>
    private void UpdateState()
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
            ApplyState();
        }
    }

    /// <summary>
    /// Update variables values according to the current state.
    /// </summary>
    private void ApplyState()
    {
        switch(CurrentState)
        {
            case CaptureState.Neutral: 
                captureTimer = 0f;

                break;

            case CaptureState.Captured:
                CurrentOwner = playersInZone[0];
                captureTimer = 0f;
                break;
        }
    }
}