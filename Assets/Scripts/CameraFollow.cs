using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform target; // The target the camera will follow
    public Vector3 offset = new Vector3(0f, 10f, -10f);    // Offset from the target position
    public float smoothSpeed = 0.125f; // Smoothing speed

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
        transform.LookAt(target);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
