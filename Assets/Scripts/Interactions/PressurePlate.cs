using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PressurePlate : MonoBehaviour
{
    [Header("Visual Feedback")]
    [Tooltip("Amount the plate sinks down when pressed (careful with Z-fighting)")]
    [SerializeField] private float pressDepth = 0.02f;
    [Tooltip("Speed of the sinking / rising animation")]
    [SerializeField] private float animationSpeed = 12f;

    public bool IsPressed
    {
        get
        {
            CleanDeadEntities();
            return entitiesOnPlate.Count > 0;
        }
    }

    public event Action OnPlatePressed;

    private readonly List<Collider> entitiesOnPlate = new();
    private Vector3 unpressedPosition;
    private Vector3 pressedPosition;
    private const float MinPressDepth = 0.01f;

    #region UnityLifecycle
    private void Awake()
    {
        unpressedPosition = transform.position;
        float actualDepth = Mathf.Max(pressDepth, MinPressDepth);
        pressedPosition = unpressedPosition - Vector3.up * actualDepth;
    }

    private void Update()
    {
        Vector3 targetPos = IsPressed ? pressedPosition : unpressedPosition;
        if ((transform.position - targetPos).sqrMagnitude > 0.000001f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * animationSpeed);
            if ((transform.position - targetPos).sqrMagnitude <= 0.000001f)
            {
                transform.position = targetPos;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerSurvival>(out _) && !entitiesOnPlate.Contains(other))
        {
            entitiesOnPlate.Add(other);
            if (entitiesOnPlate.Count == 1)
            {
                OnPlatePressed?.Invoke();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        entitiesOnPlate.Remove(other);
    }
    #endregion

    #region Helper Methods
    private void CleanDeadEntities()
    {
        for (int i = entitiesOnPlate.Count - 1; i >= 0; i--)
        {
            if (entitiesOnPlate[i] == null || !entitiesOnPlate[i].gameObject.activeInHierarchy)
            {
                entitiesOnPlate.RemoveAt(i);
            }
        }
    }
    #endregion
}

