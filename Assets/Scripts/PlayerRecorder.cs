using System.Collections.Generic;
using UnityEngine;

public class PlayerRecorder : MonoBehaviour
{
    [System.Serializable]
    public struct FrameData
    {
        public Vector3 position;
        public Quaternion rotation;

        public FrameData(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
        }
    }
    // this will hold the frames from the last run
    [HideInInspector] public List<FrameData> recordedFrames = new List<FrameData>();

    private void Update()
    {
        // Record the player's position and rotation each frame
        recordedFrames.Add(new FrameData(transform.position, transform.rotation));
    }
}
