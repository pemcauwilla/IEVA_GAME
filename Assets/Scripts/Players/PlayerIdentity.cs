using UnityEngine;

public enum PlayerTeam
{
    Team1,
    Team2
}

public class PlayerIdentity : MonoBehaviour
{
    [SerializeField] private PlayerTeam team; 
}