using System.Collections.Generic;
using UnityEngine;

public class GhostPlayback : MonoBehaviour
{
    private List<PlayerRecorder.FrameData> frames;
    private int frameIndex = 0;

    public void Init(List<PlayerRecorder.FrameData> recordedFrames)
    {
        frames = recordedFrames;
        frameIndex = 0;
    }

    void Update()
    {
        if (frames == null || frames.Count == 0) return;
        if (frameIndex >= frames.Count) return; // finished playback

        // Set transform to recorded frame
        transform.position = frames[frameIndex].position;
        transform.rotation = frames[frameIndex].rotation;

        frameIndex++;
    }
}
