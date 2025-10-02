using UnityEngine;

public class TimerButton : MonoBehaviour
{
    public GamePhaseManager phaseManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // ghost has "Player" tag
        {
            // Remember the location of the button so the player can respawn there
            phaseManager.SetPlayerSpawnPoint(transform.position);
            phaseManager.BeginRecording(); // tell GamePhaseManager to start ghost phase
            Destroy(gameObject); // remove button after use
        }
    }
}
