using UnityEngine;

public class Door : ReplayableObject
{
    private Collider doorCollider;

    private GamePhaseManager phaseManager;

    public RoomManager nextRoom;

    protected override void Awake()
    {
        base.Awake(); // registers with GamePhaseManager
        doorCollider = GetComponent<Collider>();
        phaseManager = FindAnyObjectByType<GamePhaseManager>();

        if (doorCollider != null)
            doorCollider.isTrigger = false; // start locked
    }

    public void Unlock()
    {
        if (doorCollider != null)
        {
            doorCollider.isTrigger = true;
        }
    }

    public void Lock()
    {
        if (doorCollider != null)
        {
            doorCollider.isTrigger = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has entered through the door.");
            if (phaseManager != null)
            {
                phaseManager.PlayerReachedDoor();
            }

            // Transition to next room
            if (phaseManager != null)
            {
                phaseManager.TransitionToNextRoom(nextRoom);
            }
        }
    }

    public override void ResetToInitialState()
    {
        base.ResetToInitialState();
        Lock(); // ensures the door is locked when reset
    }
}
