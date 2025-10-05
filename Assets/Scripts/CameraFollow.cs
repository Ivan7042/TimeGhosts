using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target; // The target the camera will follow
    public Vector3 offset = new Vector3(0f, 5f, -1f); // Position offset
    public Vector3 rotationOffset = new Vector3(25f, 0f, 0f); // Rotation offset (Euler angles)
    public float smoothSpeed = 0.125f; // Position smoothing speed
    public float rotationSmoothSpeed = 5f; // Rotation smoothing speed

    void LateUpdate()
    {
        if (target == null) return;

        // Smooth position
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Smooth rotation
        Quaternion desiredRotation = Quaternion.Euler(rotationOffset);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSmoothSpeed);

        // Always face the target (optionally with tilt)
        transform.LookAt(target.position + Vector3.up * 1.5f); // Aim slightly above center if you want
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
