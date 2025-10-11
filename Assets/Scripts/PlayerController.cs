using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Jump Tuning")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    private Rigidbody rb;
    private bool isGrounded;

    private Transform cam; // Reference to the main camera

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.transform; // Cache main camera transform
        rb.freezeRotation = true; // Prevent physics from rotating the player
    }

    void Update()
    {
        if (cam == null) return;

        // movement input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Calculate movement direction relative to camera
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        // Flatten camera vectors (ignore tilt)
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // Combine input with camera direction
        Vector3 moveDir = (camForward * v + camRight * h).normalized;

        // Apply velocity, keeping Y from physics (e.g. gravity)
        Vector3 velocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);
        rb.linearVelocity = velocity;


        // Make ghost face movement direction without tipping
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (flatVel.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatVel);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }


        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        // Better jump feel
        if (rb.linearVelocity.y < 0) // falling
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space)) // rising but jump released
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        // Restart if player falls off map
        if (transform.position.y < -5f)
        {
            RestartLevel();
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
