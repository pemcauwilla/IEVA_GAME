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
    [Header("Capture Parameters")]
    [Tooltip("Time taken (in seconds) for a player to capture a Zone")]
    [SerializeField] private float captureTime = 5.0f;

    public PlayerIdentity CurrentOwner {get; private set;}

    public PlayerIdentity CapturingPlayer => playersInZone.Count == 1 ? playersInZone[0] : null;

    public CaptureState CurrentState {get; private set;} = CaptureState.Neutral;

    public event System.Action<CaptureZone, PlayerIdentity> OnZoneCaptured;

    public float CaptureProgress => Mathf.Clamp01(captureTimer / captureTime);

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
            if(!playersInZone.Contains(pId)) playersInZone.Add(pId);
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

    public void ResetZone()
    {
        foreach (var pId in playersInZone)
        {
            if (pId != null && pId.TryGetComponent<PlayerSurvival>(out var survival))
            {
                survival.ExitSafeZone();
            }
        }
        playersInZone.Clear();
        CurrentOwner = null;
        CurrentState = CaptureState.Neutral;
        captureTimer = 0f;
    }
    #endregion
}
