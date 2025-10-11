using UnityEngine;

public class TimerButton : MonoBehaviour
{
    public GamePhaseManager phaseManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ghost")) // only ghost can activate
        {
            // Remember the location of the button so the player can respawn there
            phaseManager.SetPlayerSpawnPoint(transform.position);
            phaseManager.BeginRecording(this.gameObject.transform.position); // tell GamePhaseManager to start ghost phase
            // Disable the button, not destroy, so it can be reset
            gameObject.SetActive(false);
        }
    }
}
