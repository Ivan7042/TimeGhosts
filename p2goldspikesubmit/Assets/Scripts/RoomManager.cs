using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class RoomManager : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform ghostSpawnLocation;

    [Header("Phase Settings")]
    public float ghostPhaseDuration = 10f;

    [Header("Room Objects")]
    public GameObject timerButton;
    public GameObject unlockDoor;
    public GameObject door;
    public CountdownTimer countdownTimer;
    public Canvas roomCanvas;

    private bool isActive = false;
    private List<MonoBehaviour> roomBehaviours = new List<MonoBehaviour>();

    private void Awake()
    {
        // Collect all behaviours so we can disable them when room is frozen
        foreach (var behaviour in GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour != this)
                roomBehaviours.Add(behaviour);
        }

        // Room starts frozen
        SetRoomActive(false);

        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    public float GetGhostPhaseDuration() => ghostPhaseDuration;
    public CountdownTimer GetCountdownTimer() => countdownTimer;

    public void SetRoomActive(bool active)
    {
        isActive = active;

        foreach (var behaviour in roomBehaviours)
        {
            if (behaviour != null)
                behaviour.enabled = active;
        }

        if (roomCanvas != null)
            roomCanvas.gameObject.SetActive(active);

        if (timerButton != null)
            timerButton.SetActive(active);

        if (unlockDoor != null)
            unlockDoor.SetActive(active);

        if (door != null)
            door.SetActive(active);

        if (countdownTimer != null)
            countdownTimer.enabled = active;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive && (other.CompareTag("Player") || other.CompareTag("Ghost")))
        {
            ActivateRoom();
        }
    }

    public void ActivateRoom()
    {
        SetRoomActive(true);

        var phaseManager = FindAnyObjectByType<GamePhaseManager>();
        if (phaseManager != null)
        {
            phaseManager.currentRoom = this;
            phaseManager.countdownTimer = countdownTimer;
            phaseManager.SetPlayerSpawnPoint(ghostSpawnLocation.position);
        }

        Debug.Log($"Activated room: {gameObject.name}");
    }

    public void DeactivateRoom()
    {
        SetRoomActive(false);
    }
}
