using UnityEngine;

public enum PlayerTeam
{
    None,
    Player1,
    Player2
}
// 'Tag component'
[DisallowMultipleComponent]
public class PlayerIdentity : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Tag")]
    public PlayerTeam playerTeam;
    // Never leave Update and Awake methods empty, unity will call them anyway 
}
