using System.Collections.Generic;
using UnityEngine;

public class GamePhaseManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject ghostPlayerPrefab;

    private GameObject playerInstance;
    private GameObject ghostPlayerInstance;

    public bool isGhostPhase = true;

    public CameraFollow camFollow;

    private float phaseTimer = 0f;
    private bool recordingActive = false;

    public CountdownTimer countdownTimer;

    [Header("Room Management")]
    public RoomManager currentRoom;

    private Vector3 playerSpawnPoint;

    public void SetPlayerSpawnPoint(Vector3 pos)
    {
        playerSpawnPoint = pos;
    }


    void Start()
    {
        playerInstance = Instantiate(playerPrefab, new Vector3(0, 1, 0), Quaternion.identity);

        // Spawn ghost at the room's designated spawn location
        Vector3 ghostSpawnPos = currentRoom.ghostSpawnLocation != null
            ? currentRoom.ghostSpawnLocation.position
            : new Vector3(0, 1, 0);

        ghostPlayerInstance = Instantiate(ghostPlayerPrefab, ghostSpawnPos, Quaternion.identity);

        var recorder = ghostPlayerInstance.GetComponent<PlayerRecorder>();
        if (recorder != null) recorder.enabled = false;

        // Start in ghost phase
        playerInstance.SetActive(false);
        camFollow.SetTarget(ghostPlayerInstance.transform);

    }

    void Update()
    {
        if (isGhostPhase && recordingActive)
        {
            phaseTimer -= Time.deltaTime;
            if (phaseTimer <= 0f)
            {
                SwitchPhase();
            }
        }
    }

    public void BeginRecording()
    {
        recordingActive = true;
        isGhostPhase = true;

        // setup timer
        phaseTimer = currentRoom.GetGhostPhaseDuration();

        var timer = currentRoom.GetCountdownTimer();
        if (timer != null)
        {
            timer.StartTimer(phaseTimer);
        }

        // start recording component on ghost
        var recorder = ghostPlayerInstance.GetComponent<PlayerRecorder>();
        if (recorder != null) recorder.enabled = true;
    }


    void SwitchPhase()
    {
        if (isGhostPhase)
        {
            // Stop physics on ghost so it won't keep moving after timer ends
            var rb = ghostPlayerInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = false; 
                rb.isKinematic = true;
            }

            // switch to ghost playback instead of recording
            var recorder = ghostPlayerInstance.GetComponent<PlayerRecorder>();
            var savedFrames = new List<PlayerRecorder.FrameData>(recorder.recordedFrames);

            Destroy(recorder);
            ghostPlayerInstance.AddComponent<GhostPlayback>().Init(savedFrames, playerSpawnPoint);
            var ghostContoller = ghostPlayerInstance.GetComponent<PlayerController>();
            if (ghostContoller != null) Destroy(ghostContoller); // remove player controls from ghost

            // remove collisions between ghost and player
            var ghostColliders = ghostPlayerInstance.GetComponentsInChildren<Collider>();
            var playerColliders = playerInstance.GetComponentsInChildren<Collider>();
            foreach (var gCol in ghostColliders)
            {
                foreach (var pCol in playerColliders)
                {
                    Physics.IgnoreCollision(gCol, pCol);
                }
            }

            // activate player
            playerInstance.SetActive(true);
            playerInstance.transform.position = playerSpawnPoint;

            // update camera to follow player
            camFollow.SetTarget(playerInstance.transform);
        }

        isGhostPhase = false; // switch phase
    }
}
