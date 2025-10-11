using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 3f, -6f);
    public float followSmoothSpeed = 0.1f;

    [Header("Rotation Settings")]
    public float mouseSensitivity = 100f;  // Mouse speed
    public float minPitch = -20f;           // Clamp limits for looking up/down
    public float maxPitch = 60f;
    public float rotationSmoothSpeed = 8f;

    private float yaw = 0f;   // Horizontal rotation
    private float pitch = 10f; // Vertical tilt (starts slightly downward)
    private float currentYaw;
    private float currentPitch;

    void Start()
    {
        // Optional: lock cursor to center for camera control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleMouseInput();

        // Smooth rotation interpolation
        currentYaw = Mathf.Lerp(currentYaw, yaw, Time.deltaTime * rotationSmoothSpeed);
        currentPitch = Mathf.Lerp(currentPitch, pitch, Time.deltaTime * rotationSmoothSpeed);

        // Calculate rotation
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;

        // Smoothly move the camera
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmoothSpeed);
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }

    private void HandleMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
