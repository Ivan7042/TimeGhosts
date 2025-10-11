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
        if (recordingActive || playbackActive)
        {
            phaseTimer -= Time.deltaTime;

            if (phaseTimer <= 0f)
            {
                if (isGhostPhase)
                    SwitchPhase();
                else
                    RestartLevel();
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


    private void RestartLevel()
    {
        Debug.Log(" Player ran out of time! Restarting level...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void TransitionToNextRoom(RoomManager nextRoom)
    {
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
        countdownTimer = nextRoom.countdownTimer;
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

        yield return new WaitForSeconds(0.5f);

    }

}
