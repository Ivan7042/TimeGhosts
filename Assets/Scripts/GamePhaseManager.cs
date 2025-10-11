using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GamePhaseManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject ghostPlayerPrefab;
    public CameraFollow camFollow;
    public RoomManager currentRoom;
    public CountdownTimer countdownTimer;

    public TextMeshProUGUI levelCompleteText;
    public TextMeshProUGUI timerRanOutText;

    private GameObject playerInstance;
    private GameObject ghostPlayerInstance;

    public bool recordingActive = false;
    public bool playbackActive = false;
    private bool isGhostPhase = true;
    private float phaseTimer = 0f;
    private Vector3 playerSpawnPoint;
    private Vector3 sharedSpawnPoint;

    public readonly List<ReplayableObject> replayableObjects = new();

    public bool IsRecordingActive() => recordingActive;
    public bool IsPlaybackActive() => playbackActive;

    public void RegisterReplayable(ReplayableObject obj)
    {
        if (!replayableObjects.Contains(obj))
            replayableObjects.Add(obj);
    }

    public void SetPlayerSpawnPoint(Vector3 pos) => playerSpawnPoint = pos;

    void Start()
    {
        // Activate only the starting room
        if (currentRoom != null)
        {
            currentRoom.SetRoomActive(true);
            Debug.Log($"Starting in room: {currentRoom.name}");
        }

        // Hide UI elements at start
        if (levelCompleteText != null)
            levelCompleteText.gameObject.SetActive(false);
        if (timerRanOutText != null)
            timerRanOutText.gameObject.SetActive(false);

        // Instantiate player and ghost
        playerInstance = Instantiate(playerPrefab, new Vector3(0, 1, 0), Quaternion.identity);

        Vector3 ghostSpawnPos = currentRoom?.ghostSpawnLocation != null
            ? currentRoom.ghostSpawnLocation.position
            : new Vector3(0, 1, 0);

        ghostPlayerInstance = Instantiate(ghostPlayerPrefab, ghostSpawnPos, Quaternion.identity);

        ghostPlayerInstance.GetComponent<PlayerRecorder>().enabled = false;
        playerInstance.SetActive(false);

        camFollow.SetTarget(ghostPlayerInstance.transform);


    }


    void Update()
    {

        // Room reset input
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCurrentRoom();
            return; // skip phase updates during reset
        }

        if (recordingActive || playbackActive)
        {
            phaseTimer -= Time.deltaTime;

            if (phaseTimer <= 0f)
            {
                if (isGhostPhase)
                {
                    SwitchPhase();
                }
                else
                {
                    // Show "Timer Ran Out!" only during player phase
                    StartCoroutine(HandleTimerRanOut());
                }
            }

        }
    }


    public void BeginRecording(Vector3 startPosition)
    {
        sharedSpawnPoint = startPosition;
        playerSpawnPoint = startPosition;

        recordingActive = true;
        isGhostPhase = true;
        phaseTimer = currentRoom.GetGhostPhaseDuration();

        countdownTimer.StartTimer(phaseTimer);

        ghostPlayerInstance.transform.position = sharedSpawnPoint;
        var recorder = ghostPlayerInstance.GetComponent<PlayerRecorder>();
        if (recorder != null) recorder.enabled = true;
    }


    private void SwitchPhase()
    {
        if (isGhostPhase)
        {
            recordingActive = false;
            playbackActive = true;

            // Stop ghost recording
            var rb = ghostPlayerInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            // Reset replayables
            foreach (var obj in replayableObjects)
                if (obj != null) obj.ResetToInitialState();

            // playback
            var recorder = ghostPlayerInstance.GetComponent<PlayerRecorder>();
            var savedFrames = new List<PlayerRecorder.FrameData>(recorder.recordedFrames);

            Destroy(recorder);
            ghostPlayerInstance.AddComponent<GhostPlayback>().Init(savedFrames, sharedSpawnPoint);

            // Disable ghost controller b/c it's now phase 2 
            var ghostCtrl = ghostPlayerInstance.GetComponent<PlayerController>();
            if (ghostCtrl != null)
                ghostCtrl.enabled = false;

            // Disable collisions between ghost and player
            foreach (var gCol in ghostPlayerInstance.GetComponentsInChildren<Collider>())
                foreach (var pCol in playerInstance.GetComponentsInChildren<Collider>())
                    Physics.IgnoreCollision(gCol, pCol);

            // Reposition both at the position of the button press
            ghostPlayerInstance.transform.position = sharedSpawnPoint;
            playerInstance.transform.position = sharedSpawnPoint;

            // Enable player phase
            playerInstance.SetActive(true);

            // Enable player controls
            var playerCtrl = playerInstance.GetComponent<PlayerController>();
            if (playerCtrl != null)
                playerCtrl.enabled = true;

            camFollow.SetTarget(playerInstance.transform);

            // Start player phase timer
            phaseTimer = currentRoom.GetGhostPhaseDuration();
            countdownTimer.StartTimer(phaseTimer);
        }

        isGhostPhase = false;
    }


    public void PlayerReachedDoor()
    {
        if (playbackActive)
        {
            Debug.Log(" Player reached the door! Level complete.");
            playbackActive = false;

            // stop the timer so RestartLevel doesnt get called 
            if (countdownTimer != null)
                countdownTimer.StopTimer();

            // display completion message
            var text = GameObject.Find("DoorUnlockedText")?.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "Level Complete!";
                text.gameObject.SetActive(true);
            }
        }
    }

    private IEnumerator HandleTimerRanOut()
    {
        // Only show message if still in player (playback) phase
        if (!playbackActive)
            yield break;

        // Stop the timer so it doesn’t continue running
        countdownTimer?.StopTimer();

        // Show “Timer Ran Out!” message
        if (timerRanOutText != null)
        {
            timerRanOutText.text = "Timer Ran Out!";
            timerRanOutText.gameObject.SetActive(true);
        }

        // Wait 2 seconds to show message
        yield return new WaitForSeconds(2f);

        // Change to “Press R to Reset Room”
        if (timerRanOutText != null)
        {
            timerRanOutText.text = "Press R to Reset Room";
        }

        // Do NOT reset automatically — let the player press R manually
        playbackActive = false;
    }



    private void RestartLevel()
    {
        ResetCurrentRoom();
    }


    public void TransitionToNextRoom(RoomManager nextRoom)
    {
        StopAllCoroutines();
        StartCoroutine(DelayedRoomTransition(nextRoom));
    }

    private IEnumerator DelayedRoomTransition(RoomManager nextRoom)
    {
        // Wait so the completion message has time to show
        yield return new WaitForSeconds(2f);

        // Deactivate the current room
        if (currentRoom != null)
        {
            currentRoom.SetRoomActive(false);
        }

        // Begin the actual transition
        yield return StartCoroutine(TransitionToNextRoomCoroutine(nextRoom));
    }


    private IEnumerator TransitionToNextRoomCoroutine(RoomManager nextRoom)
    {
        if (nextRoom == null)
        {
            Debug.LogWarning("Next room is null!");
            yield break;
        }

        Debug.Log($"Transitioning to {nextRoom.name}");

        if (timerRanOutText != null)
            timerRanOutText.gameObject.SetActive(false);

        if (countdownTimer != null)
        {
            countdownTimer.StopTimer();
            countdownTimer.ResetTimer();
        }

        // Disable player/ghost input during transition
        if (playerInstance.TryGetComponent(out PlayerController playerCtrl))
            playerCtrl.enabled = false;

        if (ghostPlayerInstance.TryGetComponent(out PlayerController ghostCtrl))
            ghostCtrl.enabled = false;

        Vector3 startPos = playerInstance.transform.position;
        Vector3 targetPos = nextRoom.ghostSpawnLocation.position;

        float duration = 4f;
        float t = 0f;

        // lerp both player and ghost to the new room's spawn point
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            Vector3 newPos = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
            playerInstance.transform.position = newPos;
            ghostPlayerInstance.transform.position = newPos;

            // move camera too
            camFollow.transform.position = Vector3.Lerp(
                camFollow.transform.position,
                targetPos + camFollow.offset,
                Time.deltaTime * 1.5f
            );

            yield return null;
        }

        // Update references
        currentRoom = nextRoom;

        // Update UI references for the new room
        if (nextRoom != null)
        {
            countdownTimer = nextRoom.countdownTimer;

            // Try to find the new room's TimerRanOutText and LevelCompleteText
            var newTimerRanOut = nextRoom.GetComponentInChildren<TextMeshProUGUI>(true);
            if (newTimerRanOut != null && newTimerRanOut.name.Contains("TimerRanOutText"))
                timerRanOutText = newTimerRanOut;

            var newLevelComplete = nextRoom.GetComponentInChildren<TextMeshProUGUI>(true);
            if (newLevelComplete != null && newLevelComplete.name.Contains("DoorUnlockedText"))
                levelCompleteText = newLevelComplete;
        }

        currentRoom.SetRoomActive(true); // unfreeze next room

        // Reset ghost player for new phase
        var rb = ghostPlayerInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // Restore ghost control
        var ghostController = ghostPlayerInstance.GetComponent<PlayerController>();
        if (ghostController == null)
            ghostController = ghostPlayerInstance.AddComponent<PlayerController>();
        ghostController.enabled = true;

        // Remove any playback component if it exists
        var playback = ghostPlayerInstance.GetComponent<GhostPlayback>();
        if (playback != null)
            Destroy(playback);

        // Ensure it has a recorder component for the next ghost phase
        var recorder = ghostPlayerInstance.GetComponent<PlayerRecorder>();
        if (recorder == null)
            recorder = ghostPlayerInstance.AddComponent<PlayerRecorder>();
        recorder.enabled = false; // defaulted to false

        // Hide the player on new levl start
        playerInstance.SetActive(false);

        // change camera to follow ghost
        camFollow.SetTarget(ghostPlayerInstance.transform);
        camFollow.transform.position = nextRoom.ghostSpawnLocation.position + camFollow.offset;

        // Move ghost to spawn point
        ghostPlayerInstance.transform.position = nextRoom.ghostSpawnLocation.position;

        // hide any UI messages
        if (levelCompleteText != null)
            levelCompleteText.gameObject.SetActive(false);
        if (timerRanOutText != null)
            timerRanOutText.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

    }

    public void ResetCurrentRoom()
    {
        Debug.Log($"Resetting current room: {currentRoom?.name ?? "None"}");

        if (currentRoom == null) return;

        // Stop active timers and flags
        recordingActive = false;
        playbackActive = false;
        countdownTimer?.StopTimer();

        // Reset replayable objects
        foreach (var obj in replayableObjects)
            if (obj != null)
                obj.ResetToInitialState();


        currentRoom.SetRoomActive(true);
        // Then reset keys
        var dualKeyDoor = FindAnyObjectByType<DualKeyDoor>();
        if (dualKeyDoor != null)
        {
            dualKeyDoor.ResetDoor(); // resets keys + door state
        }

        // Reactivate timer button if it was disabled
        if (currentRoom.timerButton != null)
            currentRoom.timerButton.SetActive(true);

        // Reset the timer logic
        if (currentRoom.countdownTimer != null)
            currentRoom.countdownTimer.ResetTimer();

        // Move both to shared spawn point (or room's default)
        Vector3 spawnPos = currentRoom.ghostSpawnLocation != null
            ? currentRoom.ghostSpawnLocation.position
            : Vector3.zero;

        if (playerInstance != null)
        {
            playerInstance.transform.position = spawnPos;
            // Disable player object and its controller so it cannot move during ghost phase
            playerInstance.SetActive(false);
            if (playerInstance.TryGetComponent(out PlayerController playerCtrl))
            {
                playerCtrl.enabled = false;
            }

            // zero out physics if it has a rigidbody
            if (playerInstance.TryGetComponent(out Rigidbody prb))
            {
                prb.linearVelocity = Vector3.zero;
                prb.angularVelocity = Vector3.zero;
            }
        }


        if (ghostPlayerInstance != null)
        {
            ghostPlayerInstance.SetActive(true);
            // get ghost player's playerControler and re-enable it
            var ghostCtrl = ghostPlayerInstance.GetComponent<PlayerController>();
            if (ghostCtrl != null)
                ghostCtrl.enabled = true;
            // zero out physics if it has a rigidbody
            if (ghostPlayerInstance.TryGetComponent(out Rigidbody grb))
            {
                grb.isKinematic = false;
                grb.useGravity = true;
                grb.linearVelocity = Vector3.zero;
                grb.angularVelocity = Vector3.zero;
            }
        }

        playerInstance.transform.position = spawnPos;
        ghostPlayerInstance.transform.position = spawnPos;

        // Reset ghost to recorder state
        var playback = ghostPlayerInstance.GetComponent<GhostPlayback>();
        if (playback != null) Destroy(playback);

        var recorder = ghostPlayerInstance.GetComponent<PlayerRecorder>();
        if (recorder == null) recorder = ghostPlayerInstance.AddComponent<PlayerRecorder>();
        recorder.enabled = false;

        // Re-enable ghost and camera follow
        ghostPlayerInstance.SetActive(true);
        camFollow.SetTarget(ghostPlayerInstance.transform);

        // Reset timer and phase state
        isGhostPhase = true;
        phaseTimer = 0f;
        countdownTimer.ResetTimer();


        // Hide any UI messages
        if (levelCompleteText != null)
            levelCompleteText.gameObject.SetActive(false);
        if (timerRanOutText != null)
            timerRanOutText.gameObject.SetActive(false);

        Debug.Log("Room reset complete. Ready to begin ghost phase again.");
    }

}
