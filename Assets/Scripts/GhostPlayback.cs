using System.Collections.Generic;
using UnityEngine;

public class GhostPlayback : MonoBehaviour
{
    private List<PlayerRecorder.FrameData> frames;
    private int frameIndex = 0;
    private Vector3 startOffset; // to spawn later at location of button press

    public void Init(List<PlayerRecorder.FrameData> recordedFrames, Vector3 spawnPoint)
    {
        frames = recordedFrames;
        frameIndex = 0;

        if (frames.Count > 0)
        {
            // Compute offset between spawn point and first recorded frame
            startOffset = spawnPoint - frames[0].position;
        }
        else
        {
            startOffset = Vector3.zero;
        }
    }

    void Update()
    {
        if (frames == null || frames.Count == 0) return;
        if (frameIndex >= frames.Count) return; // finished playback

        // Set transform to recorded frame
        transform.position = frames[frameIndex].position + startOffset;
        transform.rotation = frames[frameIndex].rotation;

        frameIndex++;
    }
}
