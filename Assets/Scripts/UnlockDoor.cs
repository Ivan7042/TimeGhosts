using UnityEngine;

public class UnlockDoor : MonoBehaviour
{
    public Door door; // reference to the door to unlock

    private bool unlocked = false; // ensure single activation

    private void OnTriggerEnter(Collider other)
    {
        if (unlocked) return; // already unlocked
        if (!other.CompareTag("Player")) return; // only player or ghost can trigger

        door.Unlock();
        unlocked = true;

        // Optional: signal GamePhaseManager to move to next level
        // GamePhaseManager.Instance.MoveToNextLevel();

        Destroy(gameObject); // remove the trigger if you want
    }
}
