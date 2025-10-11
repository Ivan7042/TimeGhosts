using UnityEngine;
using TMPro;
using System.Collections;

public class UnlockDoor : ReplayableObject
{
    public Door door;
    public TextMeshProUGUI doorUnlockedText;
    public GamePhaseManager phaseManager;

    private bool unlocked = false;

    private void Start()
    {
        if (phaseManager == null)
            phaseManager = Object.FindAnyObjectByType<GamePhaseManager>();

        if (doorUnlockedText != null)
            doorUnlockedText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (unlocked) return;
        if (!other.CompareTag("Ghost")) return;
        if (phaseManager == null) return;

        // allow interaction during either recording or playback
        if (!phaseManager.IsRecordingActive() && !phaseManager.IsPlaybackActive()) return;

        // perform unlock
        door.Unlock();
        unlocked = true;

        // show UI
        if (doorUnlockedText != null)
        {
            doorUnlockedText.gameObject.SetActive(true);
            StartCoroutine(HideTextAfterDelay());
        }

        // perform the replayable behaviour (disable, etc.)
        OnGhostInteraction();
    }

    private IEnumerator HideTextAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        if (doorUnlockedText != null)
            doorUnlockedText.gameObject.SetActive(false);
    }

    // ensure that when the room is reset, the unlock state is cleared
    public override void ResetToInitialState()
    {
        base.ResetToInitialState();
        unlocked = false;
        if (doorUnlockedText != null)
            doorUnlockedText.gameObject.SetActive(false);
    }

    // keep default OnGhostInteraction (which deactivates)
    public override void OnGhostInteraction()
    {
        base.OnGhostInteraction();
    }
}
