using System.Collections.Generic;
using UnityEngine;

public class GamePhaseManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject ghostPlayerPrefab;

    private GameObject playerInstance;
    private GameObject ghostPlayerInstance;

    public float phaseDuration = 10f;
    private float phaseTimer;
    public bool isGhostPhase = true;

    public CameraFollow camFollow;

    void Start()
    {
        playerInstance = Instantiate(playerPrefab, new Vector3(0, 1, 0), Quaternion.identity);
        ghostPlayerInstance = Instantiate(ghostPlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity);

        // Start in ghost phase
        playerInstance.SetActive(false);
        camFollow.SetTarget(ghostPlayerInstance.transform);
    }

    void Update()
    {
        phaseTimer += Time.deltaTime;

        if (phaseTimer >= phaseDuration)
        {
            SwitchPhase();
        }
    }

    void SwitchPhase()
    {
        phaseTimer = 0f;

        if (isGhostPhase)
        {
            // switch to ghost playback instead of recording
            var recorder = ghostPlayerInstance.GetComponent<PlayerRecorder>();
            var savedFrames = new List<PlayerRecorder.FrameData>(recorder.recordedFrames);

            Destroy(recorder);
            ghostPlayerInstance.AddComponent<GhostPlayback>().Init(savedFrames);
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

            // update camera to follow player
            camFollow.SetTarget(playerInstance.transform);
        }

        isGhostPhase = false; // switch phase
    }
}
