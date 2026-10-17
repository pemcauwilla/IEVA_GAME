using System;
using System.Collections.Generic;
using UnityEngine;

public class MovingZoneController : MonoBehaviour
{
    [Header("Managed Zones")]
    [Tooltip("The active CaptureZone instances on the map that will relocate periodically")]
    [SerializeField] private CaptureZone[] managedZones;

    [Header("Anchor Spawn Points")]
    [Tooltip("Pool of world positions where the active zones can spawn/relocate")]
    [SerializeField] private Vector3[] anchorPositions;

    [Header("Timing")]
    [Tooltip("Duration in seconds the zones stay at their current spawn points before swapping")]
    [SerializeField] private float zoneDuration = 18f;

    public float RemainingTime => Mathf.Max(0f, zoneDuration - timer);

    public event Action OnZonesRelocated;

    private int[] currentAnchorIndices;
    private float timer = 0f;

    #region UnityLifecycle
    private void Start()
    {
        InitializeZonePositions();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= zoneDuration)
        {
            timer = 0f;
            SwapZones();
        }
    }
    #endregion

    #region Public API

    public int GetCurrentAnchorIndex(CaptureZone zone)
    {
        if (currentAnchorIndices == null || zone == null) return -1;
        for (int i = 0; i < managedZones.Length; i++)
        {
            if (managedZones[i] == zone)
                return currentAnchorIndices[i];
        }
        return -1;
    }
    #endregion

    #region Helper Methods
    private void InitializeZonePositions()
    {
        if (managedZones == null || managedZones.Length == 0)
        {
            managedZones = FindObjectsByType<CaptureZone>(FindObjectsSortMode.None);
        }

        if (managedZones == null || managedZones.Length == 0) return;

        if (anchorPositions == null || anchorPositions.Length == 0)
        {
            anchorPositions = new Vector3[managedZones.Length];
            for (int i = 0; i < managedZones.Length; i++)
            {
                if (managedZones[i] != null) anchorPositions[i] = managedZones[i].transform.position;
            }
        }

        currentAnchorIndices = new int[managedZones.Length];

        for (int i = 0; i < managedZones.Length; i++)
        {
            currentAnchorIndices[i] = i;
            if (i < anchorPositions.Length && managedZones[i] != null)
            {
                managedZones[i].transform.position = anchorPositions[i];
            }
        }
    }

    private void SwapZones()
    {
        int zoneCount = managedZones.Length;
        int anchorCount = anchorPositions.Length;

        List<int> unoccupiedPool = new List<int>();
        List<int> occupiedPool = new List<int>();

        for (int i = 0; i < anchorCount; i++)
        {
            if (Array.IndexOf(currentAnchorIndices, i) >= 0)
                occupiedPool.Add(i);
            else
                unoccupiedPool.Add(i);
        }

        ShuffleList(unoccupiedPool);
        ShuffleList(occupiedPool);

        List<int> candidateAnchors = new List<int>(unoccupiedPool);
        candidateAnchors.AddRange(occupiedPool);

        for (int z = 0; z < zoneCount; z++)
        {
            int newAnchorIdx = candidateAnchors[z];
            currentAnchorIndices[z] = newAnchorIdx;

            CaptureZone zone = managedZones[z];
            zone.transform.position = anchorPositions[newAnchorIdx];
            zone.ResetZone();
        }

        OnZonesRelocated?.Invoke();
    }

    private static void ShuffleList(List<int> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            int j = UnityEngine.Random.Range(i, list.Count);
            int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
    #endregion
}

