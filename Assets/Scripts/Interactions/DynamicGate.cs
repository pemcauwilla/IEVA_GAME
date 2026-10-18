using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
[RequireComponent(typeof(Collider))]
public class DynamicGate : MonoBehaviour
{
    [Header("Gate Configuration")]
    [Tooltip("Pressure plate that controls this gate")]
    [SerializeField] private PressurePlate triggerPlate;
    [Tooltip("Should the gate be open (lowered) by default?")]
    [SerializeField] private bool openByDefault = true;
    [Tooltip("Delay in seconds after the player clears the plate before the barrier slams up behind them")]
    [SerializeField] private float activationDelay = 0.15f;
    [Tooltip("Seconds the gate stays raised after activating")]
    [SerializeField] private float holdDuration = 4.5f;
    [Tooltip("Height displacement when opened (moves down into the floor)")]
    [SerializeField] private float openHeightOffset = -3.5f;
    [Tooltip("Speed of opening/closing transition")]
    [SerializeField] private float transitionSpeed = 18f;

    public bool IsOpen { get; private set; }

    public event System.Action<DynamicGate, bool> OnGateStateChanged;

    private NavMeshObstacle obstacle;
    private Collider physicalCollider;
    private Vector3 closedPosition;
    private Vector3 openPosition;

    private bool isArmed = false;
    private float delayTimer = 0f;
    private float holdTimer = 0f;

    #region UnityLifecycle
    private void Awake()
    {
        obstacle = GetComponent<NavMeshObstacle>();
        physicalCollider = GetComponent<Collider>();

        closedPosition = transform.position;
        openPosition = closedPosition + Vector3.up * openHeightOffset;

        IsOpen = openByDefault;
        transform.position = IsOpen ? openPosition : closedPosition;
        OnStateChanged(notify: false);
    }

    private void OnEnable()
    {
        if (triggerPlate != null)
        {
            triggerPlate.OnPlatePressed += HandlePlatePressed;
        }
    }

    private void OnDisable()
    {
        if (triggerPlate != null)
        {
            triggerPlate.OnPlatePressed -= HandlePlatePressed;
        }
    }

    private void Update()
    {
        if (isArmed)
        {
            if (triggerPlate != null && !triggerPlate.IsPressed)
            {
                delayTimer += Time.deltaTime;
                if (delayTimer >= activationDelay)
                {
                    isArmed = false;
                    IsOpen = !openByDefault;
                    holdTimer = 0f;
                    OnStateChanged(notify: true);
                }
            }
            else
            {
                delayTimer = 0f;
            }
        }
        else if (IsOpen != openByDefault)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdDuration)
            {
                holdTimer = 0f;
                IsOpen = openByDefault;
                OnStateChanged(notify: true);
            }
        }

        Vector3 targetPos = IsOpen ? openPosition : closedPosition;
        if ((transform.position - targetPos).sqrMagnitude > 0.000001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, transitionSpeed * Time.deltaTime);
            if ((transform.position - targetPos).sqrMagnitude <= 0.000001f)
            {
                transform.position = targetPos;
            }
        }
    }
    #endregion

    #region Helper Methods
    private void HandlePlatePressed()
    {
        if (IsOpen == openByDefault)
        {
            isArmed = true;
            delayTimer = 0f;
        }
    }

    private void OnStateChanged(bool notify = true)
    {
        obstacle.enabled = !IsOpen;
        physicalCollider.enabled = !IsOpen;

        if (notify)
        {
            OnGateStateChanged?.Invoke(this, IsOpen);
        }
    }
    #endregion
}

