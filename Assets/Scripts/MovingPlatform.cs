using UnityEngine;

public class MovingPlatformOrWall : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDistance = 3f;      // How far the object moves from its start point
    public float moveSpeed = 2f;         // How fast it moves
    public bool moveOnX = true;          // Move along X axis
    public bool moveOnZ = false;         // Move along Z axis
    public bool startMovingRight = true; // True = start moving right/forward, False = left/backward

    private Vector3 startPos;
    private float phaseOffset; // determines starting direction

    void Start()
    {
        startPos = transform.position;
        // phaseOffset = 0 means start going right; ? means start going left
        phaseOffset = startMovingRight ? 0f : Mathf.PI;
    }

    void Update()
    {
        // Sin oscillation with offset so we can start in either direction
        float movement = Mathf.Sin(Time.time * moveSpeed + phaseOffset) * moveDistance;

        Vector3 newPos = startPos;
        if (moveOnX)
            newPos.x = startPos.x + movement;
        if (moveOnZ)
            newPos.z = startPos.z + movement;

        transform.position = newPos;
    }
}
