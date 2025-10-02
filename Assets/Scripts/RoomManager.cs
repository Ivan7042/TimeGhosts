using UnityEngine;

public class RoomManager : MonoBehaviour
{

    public Transform ghostSpawnLocation;

    [Header("Phase Settings")]
    public float ghostPhaseDuration = 10f;

    [Header("References")]
    public CountdownTimer countdownTimer;

    public float GetGhostPhaseDuration()
    {
        return ghostPhaseDuration;
    }

    public CountdownTimer GetCountdownTimer()
    {
        return countdownTimer;
    }
}
